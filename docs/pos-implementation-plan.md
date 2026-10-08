# POS 機點餐 & 結帳 實作規劃（平板 Web UI）

> **專案**：EIVO2025（.NET 10 + Vue 3 SPA）
> **版本**：1.0（2026-10-04）
> **定位**：純餐飲（堂食 / 外帶 / 外賣外送），平板 Web UI，可離線結帳
> **發票策略**：雲端載具發票（F0401）＋ 紙本熱感應列印（C0401_POS）
> **輸入硬體**：實體條碼掃瞄器（HID 鍵盤）＋ 手動金額輸入

---

## 0. 總則與設計原則

### 0.1 落點與載體

- **前端載體**：`TaskCenter/ClientApp`（Vue 3 + Vite + TypeScript SPA，本系統「新架構」）。
  - POS 終端頁為**獨立全螢幕路由**（不套 `DefaultLayout` 管理側欄），路徑 `/Pos`，`requiresAuth`。
  - 選單/品項維護為管理頁（套 `DefaultLayout`），路徑 `/PosAdmin/Menu` 等。
  - 新增 PWA（`vite-plugin-pwa`）讓平板可「本機安裝、無網址列」。
- **後端載體**：`TaskCenter`（ASP.NET Core Web API，JWT Bearer，`ApiBaseController` 統一 `ApiResponse<T>` 回傳），新增 `Pos*` 一組 controller。
- **發票引擎**：**不另造**。直接複用既有 `CreateInvoiceController`（`/api/CreateInvoice/Commit`，`forPreview` 預覽/commit）與 `ModelCore` 發票實體。

### 0.2 可複用資產對照表（本規劃的前提）

| POS 能力 | 既有資產（直接複用 / 輕量擴充） |
|---|---|
| 商品/價格 | `ProductCatalog`（Barcode/Product/SalePrice/Spec/PieceUnit）+ `ProductCatalogDto` 快搜 |
| 訂單明細→發票 | `InvoiceItem` → `InvoiceProduct` → `InvoiceProductItem`（Brief/Piece/UnitCost/CostAmount/TaxType） |
| 發票開立 | `CreateInvoiceController.Commit`（F0401=41 存證/雲端、A0101=21 B2B；`forPreview` 預覽） |
| 課稅別 | `TaxType` 1–4（應稅5%/零稅率/免稅/特種），載具 `3J0002` 手機條碼 / `CQ0001` 自然人憑證 / Email |
| 載具/憑證 | `InvoiceCarrier`（CarrierType/CarrierNo）、`InvoiceByHousehold` |
| POS 機編碼 | `POSDevice`（POSNo/CompanyID）、`POSInvoiceNoSegment`（每機字軌）＋ `POSDeviceController`（`CommitPOS`/`EditPOSBooklets`/`CheckInvoiceNoAsync`） |
| 中獎 | `InvoiceWinningNumber` + `InquireWinningInvoice` |
| 收據列印 | 熱感應 `162×792`、`C0401_POS.cshtml`、PDF/QR 產生器（`ExternalPdfWrapper`/ZXing） |
| 權限 | 既有 JWT + 角色（SYS/SELLER…）+ `POSNo` 綁定店別資料範圍 |
| 前端約定 | `apiRequest<T>()`（永不拋錯、回 `ApiResponse<T>`）、`useCreateInvoice` 試算 `sum()`、`composables/`+`services/`+`components/` 慣例 |

### 0.3 核心設計決策（由需求 #1–#4 推出）

1. **獨立 POS/選單 schema**：不汙染既有 `ProductCatalog`（它服務 B2B/供應商等全平台）。新增 `Menu*` 選單域，`MenuItem` 自帶 Barcode/Price/TaxType/加料；結帳時把明細**攤平**成標準發票明細（brief/qty/unit/amount/taxType），再丟進發票引擎。
2. **每 POS 預分配字軌區段**（`POSInvoiceNoSegment`）→ 離線也能順序出號、不衝突。
3. **客戶端為離線期的「事實來源」**：訂單/付款先落本地 IndexedDB（Dexie），線上則同步提交；斷線時本地結帳+本地列印，上線後批次上傳+對帳。
4. **條碼掃瞄器 = HID 鍵盤楔**：前端只需全域 keydown 緩衝（快速輸入+Enter），無需驅動；金額用數字鍵盤元件手動輸入。

---

## 1. 資料模型（Data Model）

> 新增實體放於 `ModelCore.EF/DataEntity/`（新 sub-namespace `Pos` / `Menu`），以 EF Core **code-first 遷移**產出（既有為 database-first；新域用 migration 增量較安全，不重跑全量 scaffold）。
> 金額一律 `decimal`；快照欄位（`Name`/`ModifierText`）保證歷史訂單不受選單後續更動影響。

### 1.1 選單域（Menu）

| 實體 | 欄位 | 說明 |
|---|---|---|
| **MenuCategory** | `CategoryID` PK, `CompanyID`, `Name`, `SortOrder`, `IsEnabled`, `ImageUrl?` | 分類（主菜/飲/甜點…） |
| **MenuItem** | `ItemID` PK, `CompanyID`, `CategoryID`, `Name`, `Barcode?`, `Price` decimal, `TaxType`(1–4), `Spec?`, `Description?`, `ImageUrl?`, `IsAvailable`, `IsCombo`, `SortOrder`, `ProductCatalogID?` | 品項。`ProductCatalogID` 選填，供會計/既有商品對接 |
| **MenuItemOptionGroup** | `GroupID` PK, `ItemID`, `Name`, `IsRequired`, `MinSelect`, `MaxSelect` | 加料群組（甜度/冰量/加料） |
| **MenuItemOptionValue** | `ValueID` PK, `GroupID`, `Name`, `PriceDelta` decimal | 加料項（加糖 +0、加料 +15） |
| **MenuItemComboMember** | `MemberID` PK, `ItemID`(combo), `MemberItemID`, `Qty` | 套餐組合（set meal） |

### 1.2 訂單域（Order）

| 實體 | 欄位 | 說明 |
|---|---|---|
| **POSSession**（shift） | `SessionID` PK, `CompanyID`, `DeviceID`(=POSNo), `UserProfileID`, `OpenCash`, `StartDT`, `EndDT?`, `CloseCash?`, `Status` | 班次/現金匣，供 Z-report |
| **POSOrder** | `OrderID` PK, `CompanyID`, `DeviceID`, `SessionID`, `OrderNo`（人讀）, `ClientID`（UUID，**冪等鍵**）, `OrderType`（DineIn/Takeout/Delivery）, `TableNo?`, `OrderStatus`（Open/Held/Paid/Void）, `CreatedBy`, `CreatedAt`, `PaidAt?`, `InvoiceID?`, `Subtotal`, `DiscountTotal`, `TaxTotal`, `GrandTotal`, `AmountTendered`, `Change`, `Remark?`, `DeliveryName?/Phone?/Addr?` | 訂單主檔 |
| **POSOrderLine** | `LineID` PK, `OrderID`, `MenuItemID`, `ItemName`（快照）, `ModifierText`（快照）, `Qty` decimal, `UnitPrice` decimal, `LineTotal`, `DiscountAmount`, `TaxType`, `Remark?` | 明細（對應發票明細一行或多行） |
| **POSOrderPayment** | `PaymentID` PK, `OrderID`, `PayMethod`（Cash/Debit/Credit/QRWallet/EVoucher/Other）, `Amount`, `RefNo?`, `PaidAt` | 多筆/混合付款 |
| **POSRefund** | `RefundID` PK, `OrderID`, `VoidType`（Full/Partial）, `Amount`, `Reason`, `CreatedBy`, `CreatedAt`, `AllowanceID?/CancellationID?` | 退貨/退款（對接既有折讓 D0401 / 作廢 C0501） |

### 1.3 直接複用（不新增）

`ProductCatalog`（選填對接）· `POSDevice` / `POSInvoiceNoSegment`（每機字軌）· `InvoiceItem` + `InvoiceProduct` + `InvoiceProductItem`（發票）· `InvoiceCarrier` / `InvoiceByHousehold`（載具/憑證）· `Organization`（賣方）· `UserProfile`/`UserRole`（權限）· `InvoiceWinningNumber`（中獎）· `InvoiceAllowance`/`InvoiceCancellation`（折讓/作廢）。

### 1.4 遷移與 Seed

- 新增 `ModelCore.EF` 遷移檔建立上述表；`POSDevice`/`POSInvoiceNoSegment` 為既有表，**不改建**。
- Seed：每 `CompanyID` 的選單範本、`PayMethod`/`OrderType` 代碼表、預設稅率設定（放 `OrganizationCustomSetting` 或新 `POSSetting`：含稅/未稅、四捨五入規則、收據頁尾、店名/統編/電話）。
- **版本化選單同步**：`MenuCategory`/`MenuItem` 加 `Version`/`ChangedAt`，供離線端增量同步（見 §4 離線）。

---

## 2. 資料庫遷移（Migration）步驟

1. `dotnet ef migrations add AddPosMenu` → 建立 `Menu*` 五表。
2. `dotnet ef migrations add AddPosOrder` → 建立 `POSSession/POSOrder/POSOrderLine/POSOrderPayment/POSRefund`。
3. 索引：`POSOrder(DeviceID, OrderStatus)`、`POSOrder(ClientID)` UNIQUE、`POSOrderLine(OrderID)`、`MenuItem(CompanyID, Barcode)`、`MenuItem(CompanyID, IsAvailable)`。
4. `POSSetting`（或擴充 `OrganizationCustomSetting`）放店別結算設定。
5. CI 已含 `db-migration-test`（`.github/workflows/db-migration-test.yml`）→ 遷移須過該測試。

---

## 3. 後端 API（ASP.NET Core, `/api`，`ApiResponse<T>` 統一回傳）

> 全部走 `ApiBaseController` + JWT；資料範圍以 `POSDevice.CompanyID` 守門（同 `CanAccessSeller` 思路）。
> 命名/風格沿用既有（camelCase 欄位、`forPreview` 模式、加密 `sellerKey`）。

### 3.1 選單 `PosMenuController`

| Method | Path | 用途 |
|---|---|---|
| GET | `/api/Pos/Menu?device={posNo}` | 取整店選單（分類+品項+加料+套餐），含 `Version`；終端啟動/離線快取來源 |
| GET | `/api/Pos/Menu/Delta?since={version}` | 離線增量同步選單異動 |
| POST/PUT/DELETE | `/api/Pos/Menu/Category`、`/api/Pos/Menu/Item` | 管理端 CRUD（品項/加料/套餐） |
| POST | `/api/Pos/Menu/Item/{id}/Availability` | 上下架（售完/時段） |

### 3.2 班次 / POS 機 `PosSessionController`

| Method | Path | 用途 |
|---|---|---|
| POST | `/api/Pos/Session/Open` | 登入+開班：回 店別設定、賣方 `sellerKey`、選單版本、**預分配字軌區段**、目前中獎號、收據頁尾 |
| POST | `/api/Pos/Session/Close` | 關班→ Z-report 快照 |
| GET | `/api/Pos/Session/ZReport?session={id}` | 日結（銷售/付款方式/退貨/現金匣差額） |
| POST | `/api/Pos/InvoiceNumber/Allocate` | 依 `POSNo` 補發字軌區段（接 `POSInvoiceNoSegment`/`EditPOSBooklets`） |

### 3.3 訂單 `PosOrderController`

| Method | Path | 用途 |
|---|---|---|
| POST | `/api/Pos/Order` | 建立/暫存訂單（**以 `ClientID` 冪等**）；回 `OrderID` |
| GET | `/api/Pos/Order?status=Open|Held&table={n}` | 列取開單中/暫存單（桌位狀態） |
| POST | `/api/Pos/Order/{id}/Hold` · `/Resume` | 掛單/取回 |
| POST | `/api/Pos/Order/Merge` · `/Split` | 合併 / 拆帳（split bill） |
| POST | `/api/Pos/Order/{id}/Checkout` | **結帳核心**：原子建立 POSOrder + 開立發票（呼叫發票引擎）+ 記付款 + 回 `invoiceNo/randomNo/printPayload/winning` |
| POST | `/api/Pos/Order/{id}/Void` · `/Refund` | 作廢/退貨（對接 C0501 / D0401） |
| POST | `/api/Pos/Order/{id}/Print` | 重新列印（reprint） |

### 3.4 離線同步 `PosSyncController`

| Method | Path | 用途 |
|---|---|---|
| POST | `/api/Pos/Sync/Upload` | **批次上傳**離線期訂單+結帳+發票開立請求；伺服器驗證→開立發票→回校正結果 |
| POST | `/api/Pos/InvoiceNumber/Consume` | 回報已用字軌區段（標記 used） |
| GET | `/api/Pos/Config` | 店別/稅/列印/收據設定 + 中獎號 + 選單版本 |

### 3.5 發票（複用既有）

- **預覽/開立**：POS 呼叫既有 `POST /api/CreateInvoice/Commit`（`forPreview=true/false`）。
  - 雲端載具：`processType=F0401(41)` + `carrierType/carrierId1/npoban`（`3J0002`/`CQ0001`/Email）。
  - 紙本熱感應：開立標準 B2C 發票後，收據端以 `paperStyle="POS"` → `C0401_POS.cshtml`（162×792）渲染/列印。
- 明細攤平規則：每筆 `POSOrderLine` → 一或多行 `{ itemNo, brief, piece, unitCost, costAmount, taxType }`；加料併入 `brief`（如「拿鐵 中杯(加糖) +10」）。
- 若需「收銀機發票」獨立 `processType`，為**小配置項**（擴充 `Naming.InvoiceProcessType`），不影響主流程。

---

## 4. 離線 / 冪等 / 對帳策略（需求 #3 關鍵）

**目標**：斷網仍可完成整單點餐+結帳+印收據；上線後無遺漏、無重複發票。

### 4.1 本地事實來源（客戶端）

- `dexie`（IndexedDB）存 `orders / orderLines / payments / pendingInvoiceOpens / numberCursor`。
- 一切結帳**先落本地**（含離線期與線上期的「提交前快照」），再嘗試上傳 → 天然冪等。
- `ClientID = UUIDv4`（前端生成）作為跨端冪等鍵；`POSOrder.ClientID` 設 UNIQUE。

### 4.2 離線發票號（字軌）

- 開班時由 `/Session/Open` 預分配**一段連續發票號**到 `POSDevice`（`POSInvoiceNoSegment`）。
- 本地 `numberCursor` 順序消耗；**低於閾值（如剩 50）自動再向伺服器申請新段**（可延後到上線）。
- 上線後 `Consume` 回報已用範圍，伺服器標記 used 並與已開 `InvoiceItem` 核對。

### 4.3 防偽隨機碼（randomNo）

- 20 位唯一隨機數。**離線**：本地安全 RNG 生成，記錄入 `pendingInvoiceOpens`，確保店別內不重複（可用字軌段派生 + 校驗）。
- 上線 `Sync/Upload` 時伺服器驗證唯一性；衝突（極低機率）以「補發新號」並回校正。

### 4.4 上傳與對帳

- `Sync/Upload` 為**批次 + 冪等**（以 `ClientID` 去重）。每筆回 `orderStatus / invoiceNo / randomNo / winner? / rejectReason?`。
- 伺服器失敗/中獎/補號 → 客戶端顯示「待校正」清單，可重印。
- 中獎號於**上傳成功後**判定並提示（離線期先標「中獎待確認」）。
- 選單離線：`/Menu` 全量 + `/Menu/Delta` 增量快取到 Dexie；品項價/加料隨選單快取。

### 4.5 收據（離線列印）

- 收據 HTML 以**前端模板**（`C0401_POS` 的 SPA 版）本地渲染 → 熱感應（ESC/POS 藍牙）或 PDF/QR。
- 雲端發票 QR（QR 金鑰）於**開立成功**（含線上回傳）後才最終生效；離線收據標「雲端發票待上傳」，QR 補印於校正後。

---

## 5. 前端（Vue 3）架構

### 5.1 新增檔案結構（`TaskCenter/ClientApp/src`）

```
components/pos/
├─ PosTerminal.vue                 ← 路由 /Pos（全螢幕、無側欄、requiresAuth）
├─ pos/
│  ├─ MenuPanel.vue                ← 分類 tab + 品項 grid（觸控）
│  ├─ MenuItemCard.vue
│  ├─ ModifierDialog.vue           ← 規格/加料/套餐 選擇
│  ├─ CartPanel.vue                ← 当前訂單明細
│  ├─ CartLine.vue
│  ├─ OrderTypeBar.vue             ← 堂食/外帶/外送 + 桌號
│  ├─ TablePicker.vue              ← 桌位選擇（occupied/vacant）
│  ├─ NumberPad.vue                ← 數字鍵盤（金額/數量/桌號）
│  ├─ PaymentDialog.vue            ← 付款方式/混合/找零
│  ├─ InvoiceChoiceDialog.vue      ← 雲端載具 vs 紙本熱感應
│  ├─ CarrierDialog.vue            ← 手機條碼/自然人憑證/Email
│  ├─ ReceiptView.vue              ← C0401_POS(162×792) + QR
│  ├─ HeldOrderList.vue
│  ├─ RefundDialog.vue
│  ├─ BarcodeScanner.vue           ← HID 鍵盤楔 listener（可複用於全域）
│  └─ OfflineBadge.vue             ← 離線/待上傳指示
├─ pos-admin/
│  ├─ MenuManageIndex.vue          ← 選單/品項/加料/套餐維護（套 DefaultLayout）
│  └─ MenuItemForm.vue
composables/pos/
├─ usePosTerminal.ts
├─ useMenu.ts
├─ useCart.ts                       ← 明細+金額試算（沿用 sum() 邏輯）
├─ useCheckout.ts
├─ useBarcodeScanner.ts
└─ useOfflineQueue.ts               ← Dexie 讀寫 + Sync/Upload
services/
├─ pos-menu-api.ts
├─ pos-order-api.ts
├─ pos-session-api.ts
└─ pos-sync-api.ts                  ← 全走 apiRequest<T>()
store/  (Pinia 或 auth 既有 store 慣例)
└─ pos-store.ts                     ← 当前班次/訂單/離線狀態
db/
└─ pos-db.ts                        ← Dexie schema
router/index.ts                     ← 新增 /Pos（無 layout）+ /PosAdmin/*（有 layout）
```

### 5.2 關鍵元件行為

- **`useBarcodeScanner`**：全域 `keydown` 緩衝；偵測「短時間內純數字/條碼 + Enter」→ 發 `barcode` 事件；`MenuPanel`/`PaymentDialog`/`CarrierDialog` 訂閱。手動金額：`NumberPad` 點按。
- **`useCart`**：品項+加料→明細行；即時 `subtotal/discount/tax/grandTotal`（沿用 `useCreateInvoice.sum()` 含稅/未稅邏輯）；含稅/未稅與四捨五入規則由店別設定驅動。
- **`useCheckout`**：組合 `POSOrder`（快照明細）→ 依「雲端/紙本」選 `processType`+載具 → 線上走 `/Checkout`（或 `/CreateInvoice/Commit`）；離線落 Dexie + 本地列印。
- **`useOfflineQueue`**：管理 `pending*` 佇列；`navigator.onLine` + 定時/手動重試；調 `/Sync/Upload`；處理校正（補號/中獎/重印）。
- **PWA**：`vite-plugin-pwa`（workbox）→ `installable`、`offline fallback`；靜態資產+Dexie 快取選單/模板。

### 5.3 路由變更（`router/index.ts`）

```js
// 全螢幕終端（不套 DefaultLayout）
{ path: '/Pos', name: 'PosTerminal', component: () => import('@/components/pos/PosTerminal.vue'), meta: { requiresAuth: true } }
// 管理端（套 DefaultLayout，加入 children）
{ path: 'PosAdmin/Menu', name: 'PosMenuManage', component: () => import('@/components/pos/pos-admin/MenuManageIndex.vue'), alias: ['/PosAdminMenu'] }
```

---

## 6. 權限 / 多店 / 多裝置

- **綁定**：POS 終端登入後綁定一個 `POSDevice`（POSNo→CompanyID）→ 自動決定賣方、字軌、選單、資料範圍。
- **角色**：`Cashier`（點餐/結帳）vs `Manager`（退貨/void/改價/關班/維護選單）→ 擴充 `UserRole`/`MenuControl`（既有權限機制）。
- **多店**：以 `CompanyID` 隔離；`POSNo` 唯一。
- **多裝置**：同店多台平板各綁獨立 `POSNo`；桌位/暫存單以 `CompanyID+DeviceID` 維度列取，可跨裝置取回暫存單。
- **換手/排班**：開班/關班（`POSSession`）對應班別與現金匣。

---

## 7. 報表 / 稽核（Report / Audit）

- **Z-report（日結）**：`POSSession` + `POSOrder/POSOrderPayment` 聚合 → 總銷售、依付款方式、依品項/類別、退貨、稅額、現金匣開/關/差額。
- **銷售統計**：可複用 `InvoiceDataReportExtensions`（發票端）+ 新增 POS 端聚合（`/api/Pos/Report/Sales?from&to&groupBy`）。
- **稽核**：`POSOrder`/`POSRefund` 記錄 `CreatedBy/CreatedAt/DeviceID`；改價/折扣/void 寫獨立 `POSOrderAudit`（who/when/what/delta）。
- **中獎**：複用 `WinningInvoiceReport`（發票端）+ POS 端即時提示。

---

## 8. 測試策略

- **單元**：金額試算（含/未稅、四捨五入、整額）、明細→發票明細攤平、找零、split/merge、字軌消耗與閾值、`randomNo` 唯一。
- **API 整合**：`/Checkout` 原子性（訂單+發票+付款同 transaction）、`Sync/Upload` 冪等（重複 `ClientID` 不重開發票）、字軌 `Consume` 核對。
- **離線 E2E**：斷網→點餐→結帳→本地列印→上線→上傳→校核（無重複發票、中獎判定、補號）。
- **UI/E2E**：Playwright（平板 viewport）— 條碼楔、數字鍵盤、混合付款、重印、離線 badge。
- **遷移**：過 `.github/workflows/db-migration-test.yml`。

---

## 9. 里程碑 / 分期（Milestones）

> 每里程碑可獨立上線；離線（M3）與報表（M4）可並行。

### M0 — 基礎（1–2 週）
選單域 schema+seed、`POSSetting`、POS 機/字軌分配（`Session/Open`）、權限（Cashier/Manager）、PWA scaffold + Dexie 離線骨架、`/Pos` 路由與空白終端頁。
**驗收**：平板可登入並開班，取得選單+字軌；PWA 可安裝。

### M1 — 點餐 MVP（2 週）
分類/品項 grid、加料/規格/套餐、條碼掃瞄、手動金額、堂食/外帶/外送+桌位、明細+金額試算、掛單/取回/合併。
**驗收**：完整點一單（含加料/套餐/條碼/手動金額）並暫存/取回；金額與試算規則正確。

### M2 — 結帳 + 發票（2 週）
付款方式（含現金找零）+ 混合付款、雲端載具發票（F0401：手機條碼/自然人憑證/Email）、紙本熱感應（C0401_POS 162×792）+QR、重印、中獎提示（線上）。
**驗收**：一單結帳→出雲端發票（載具 QR）或紙本收據→可重印；混合付款找零正確。

### M3 — 離線結帳（2 週，可並行 M2 後半）
本地事實來源、字軌本地消耗+`Consume`、`randomNo` 本地生成、`Sync/Upload` 批次+冪等+對帳、離線收據渲染、待校正清單。
**驗收**：斷網結帳+印收據；上線後上傳成功、無重複發票、中獎/補號正確。

### M4 — 報表/退貨/多店（1–2 週）
Z-report、銷售統計、退貨/void（D0401/C0501 對接）、改價稽核、多店/多裝置、拆帳進階。
**驗收**：日結 Z-report 與銷售統計與明細一致；退貨走既有折讓/作廢；多店資料隔離。

### M5 — 硬化與驗收（1 週）
實體掃瞄器 HW 實測、ESC/POS 藍牙列印實測、效能（選單/明細大單）、PWA 離線完整度、安全（JWT/權限/冪等）複審、上線。

---

## 10. 開放問題 / 風險（需確認）

| # | 議題 | 建議 / 待決 |
|---|---|---|
| 1 | `randomNo` 離線唯一性與平台接受度 | 本地 RNG + 字軌派生 + 上傳校正；**需與發票平台/主計處規則確認**紙本/憑條 randomNo 規範 |
| 2 | 紙本 C0401 是否為合法「收銀機發票」類型 | 確認 統一發票 類別（電子計算機/收銀機）要件；影響是否需獨立 `processType` |
| 3 | 字軌段補發失敗（上線時伺服器不可達） | 本地保留多餘段緩衝；上線後補段並 `Consume` |
| 4 | 選單規模與離線快取 | 用 `Version` 增量同步；圖檔 CDN/快取策略 |
| 5 | 付款方式範圍（QR 錢包/簽帳機） | 純軟體記錄 or 接收款閘道？影響 `PayMethod` 與是否需收據/對帳金流 |
| 6 | 四捨五入/整額規則 | 確認店別含稅/未稅與進位規則（影響 `sum()` 參數） |
| 7 | 平板熱感應列印路徑 | 藍牙 ESC/POS 直接印 vs 後端 PDF；影響收據渲染位置 |
| 8 | 外送配送欄位深度 | 派送員/時段/配送費/平台（外送平台 API?）待釐清 |

---

## 附錄 A — 結帳請求 ↔ 發票引擎 對應

```
POSOrder(快照) ──攤平──▶ CreateInvoiceRequest
  POSOrderLine[]  ──▶ lines[]: { itemNo, brief(含加料), piece, unitCost, costAmount }
  POSOrder.Subtotal/TaxTotal/GrandTotal ──▶ salesAmount/taxAmount/totalAmount
  TaxType ──▶ taxType(1–4) ; taxRate 依店別
  載具(雲端) ──▶ processType=F0401 + carrierType/carrierId1/npoban
  紙本 ──▶ processType(收銀機/或標準) + print: paperStyle="POS" (C0401_POS, 162×792)
  POSDevice ──▶ sellerKey(加密 CompanyID) + TrackID/字軌(POSInvoiceNoSegment)
  前端 genRandomNo() ──▶ randomNo
回傳 CreateInvoiceResult: invoiceNo / trackCode / randomNo / printMark / hasCarrier / processType
```

## 附錄 B — 離線資料流

```
[線上]  開班→取選單+字軌  ──▶  點餐(本地 cart)  ──▶  結帳→POST /Checkout ──▶ 印收據
[離線]  本地 cart  ──▶  結帳→落 Dexie(orders/pays/pendingInvoiceOpens)+本地出號+本地列印
                    └──▶ 上線: POST /Sync/Upload(批次,ClientID 冪等)
                              伺服器: 驗證→開立 InvoiceItem→標字軌 used→中獎/補號
                              回: 校正清單(中獎/補號/重印)
```
