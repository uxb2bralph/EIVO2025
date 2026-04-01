# WebHome 重構工作執行計劃

> 目標：以資料庫應用為出發點，將 WebHome (MVC) 重構為前後端分離架構  
> 前端：Vue 3 (WebClient/ClientApp) | 後端：.NET Core Web API (WebClient) | 資料庫：MS SQL (EIVO03)

---

## 現況分析

| 項目 | 現狀 |
|------|------|
| **後端** | WebHome — ASP.NET Core MVC，混合 Razor Views + AJAX JSON 回傳 |
| **前端** | WebClient/ClientApp — Vue 3 + Vite + Bootstrap 5，已遷移：LoginForm、InvoiceQuery (6 元件)、OrganizationQuery (19 元件)、TrackCodeQuery |
| **ORM** | LINQ to SQL (DBML)，非 EF Core，`EIVOEntityDataContext` / `TurnKey2DataContext` |
| **權限** | Cookie Auth + 自訂 `RoleAuthorizeAttribute`，角色：SYS/SELLER/BUYER/GUEST/NETWORKSELLER 等 10 種 |
| **API** | 前端目前全部使用 Mock Data，尚未對接真實 API |
| **DTO** | ModelCore/DTOs 已有 `PagedResultDto<T>`、`BaseResponseDto`、`ResponseDto<T>` |

---

## 第一階段：基礎建設 — 資料存取層現代化

> 目標：建立穩固的資料庫存取層，為所有 API 提供基礎

### 1.1 EF Core 遷移（取代 LINQ to SQL）

| 步驟 | 工作內容 | 產出 |
|------|----------|------|
| 1.1.1 | 在 `ModelCore.EF` 專案中建立 `EIVODbContext`，使用 Database-First scaffolding 從 EIVO03 產生 EF Core 實體 | `EIVODbContext.cs`、`Models/` 目錄 |
| 1.1.2 | 為各 Schema 建立實體配置 (`IEntityTypeConfiguration<T>`) — `dbo`、`billing`、`center`、`proc`、`scm` | `Configurations/` 目錄 |
| 1.1.3 | 從 DBML 實體中提取自訂商業邏輯，搬移到 `Business.EF` 中的 Service 層 | Service classes |
| 1.1.4 | 建立 Repository 抽象介面 (`IRepository<T>`) + 通用實作 | `IRepository.cs`、`Repository.cs` |
| 1.1.5 | 設定 `DbContext` 連線字串與 DI 註冊 | `Program.cs` / `appsettings.json` |

**關鍵資料表對應優先序：**
```
第一批 (權限核心)：UserProfile, UserRole, UserRoleDefinition, UserMenu, MenuControl,
                   Organization, OrganizationCategory, CategoryDefinition,
                   OrganizationCategoryUserRole, OrganizationStatus
第二批 (發票核心)：InvoiceItem, InvoiceAmountType, InvoiceSeller, InvoiceBuyer,
                   InvoiceProductItem, InvoiceCancellation, InvoiceAllowance
第三批 (營運功能)：InvoiceTrackCode, InvoiceNoAllocation, InvoiceWinningNumber,
                   InvoiceCarrier, InvoiceDeliveryTracking
第四批 (帳務/其他)：billing.*, center.DocumentFlow*, Attachment, CDS_Document
```

### 1.2 共用 DTO / ViewModel 層

| 步驟 | 工作內容 |
|------|----------|
| 1.2.1 | 擴充 `ModelCore/DTOs/`：為每個 API 模組建立專用 DTO（Request / Response） |
| 1.2.2 | 定義共用 `ApiResponse<T>` 標準回傳格式 (與前端 `api-response.ts` 對齊) |
| 1.2.3 | 建立 AutoMapper Profile 或手動 Mapping 擴充方法 (Entity ↔ DTO) |

---

## 第二階段：權限系統設計與實作

> 目標：將隱式的角色判斷改為明確的權限定義，支援 JWT 認證

### 2.1 權限模型定義

基於現有資料庫的權限結構：

```
UserProfile (使用者)
    └── UserRole (使用者角色指派)
            ├── RoleID → UserRoleDefinition (角色定義)
            └── OrgaCateID → OrganizationCategory (組織分類)
                                └── OrganizationCategoryUserRole (角色-選單權限)
                                        └── MenuControl (選單項目)
```

| 步驟 | 工作內容 |
|------|----------|
| 2.1.1 | 整理並文件化所有角色 (`RoleID`) 與其對應功能權限 |
| 2.1.2 | 定義功能權限列舉 `Permission` Enum（對應 `MenuControl` + `UserMenu`） |
| 2.1.3 | 建立 `RolePermissionMapping` 靜態配置或資料表，明確定義角色→權限對應 |

**角色清單與權限矩陣：**

| RoleID | 角色 | 核心權限範圍 |
|--------|------|-------------|
| 1 | ROLE_SYS (系統管理員) | 全部功能 |
| 51 | ROLE_SELLER (開立人) | 發票開立/作廢/折讓、字軌管理、組織設定 |
| 52 | ROLE_BUYER (買受人) | 發票查詢、進項發票 |
| 53 | ROLE_GUEST (訪客) | 唯讀查詢 |
| 54 | ROLE_NETWORKSELLER | 網路開立發票 |
| 55 | ROLE_GOOGLETW | Google TW 專用 |
| 61 | 集團成員 | 集團內發票查詢 |
| 62 | 相對營業人 | 對口廠商發票 |
| 64 | 資料稽核員 | 資料審查/稽核 |

### 2.2 JWT 認證系統

| 步驟 | 工作內容 | 說明 |
|------|----------|------|
| 2.2.1 | 在 WebClient 後端加入 JWT Bearer 認證 middleware | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| 2.2.2 | 建立 `AuthController` — `POST /api/auth/login`（帳密驗證→發 JWT） | 對接現有 `UserProfileFactory.CreateInstance()` |
| 2.2.3 | 建立 `POST /api/auth/refresh` — Token 刷新 | RefreshToken 儲存於 `UserToken` 表 |
| 2.2.4 | JWT Payload 攜帶：`uid`, `pid`, `roles[]`, `companyId`, `orgaCateId` | Claims 設計 |
| 2.2.5 | 建立自訂 `PermissionAuthorizationHandler` — 取代 `RoleAuthorizeAttribute` | Policy-based Authorization |
| 2.2.6 | 定義 Authorization Policy (例如 `Policy.InvoiceManage`, `Policy.OrgAdmin`) | 可在 Controller/Action 級別套用 |

### 2.3 前端權限整合

| 步驟 | 工作內容 |
|------|----------|
| 2.3.1 | 登入 API 回傳使用者角色、權限列表、可用選單 |
| 2.3.2 | 前端建立 `useAuth` composable — 管理 JWT Token、角色狀態 |
| 2.3.3 | Vue Router 加入 `beforeEach` guard — 根據權限控制路由存取 |
| 2.3.4 | 建立 `v-permission` directive — 控制 UI 元素顯示 |
| 2.3.5 | 動態選單生成 — 依 `UserMenu` + `MenuControl` 產生左側導航 |

---

## 第三階段：WebClient API 層建立

> 目標：將 WebHome Controller 中的商業邏輯轉為 RESTful API

### 3.1 API 架構設計

```
WebClient/
├── Controllers/
│   └── Api/
│       ├── AuthController.cs          ← 認證
│       ├── UserProfileController.cs   ← 使用者管理
│       ├── OrganizationController.cs  ← 組織管理
│       ├── InvoiceController.cs       ← 發票 CRUD
│       ├── InvoiceQueryController.cs  ← 發票查詢
│       ├── AllowanceController.cs     ← 折讓管理
│       ├── TrackCodeController.cs     ← 字軌管理
│       ├── InvoiceNumberController.cs ← 發票號碼配號
│       ├── POSDeviceController.cs     ← POS 設備管理
│       ├── NotificationController.cs  ← 通知
│       ├── ReportController.cs        ← 報表/匯出
│       └── MenuController.cs          ← 動態選單
├── Services/
│   ├── IAuthService.cs / AuthService.cs
│   ├── IUserService.cs / UserService.cs
│   ├── IOrganizationService.cs / OrganizationService.cs
│   ├── IInvoiceService.cs / InvoiceService.cs
│   ├── IAllowanceService.cs / AllowanceService.cs
│   ├── ITrackCodeService.cs / TrackCodeService.cs
│   └── IReportService.cs / ReportService.cs
└── Mappings/
    └── MappingProfile.cs
```

### 3.2 API 遷移優先序與對應

| 優先 | 模組 | WebHome Controller → WebClient API | 資料表 |
|------|------|-----------------------------------|--------|
| P0 | 認證 | `AccountController` → `Api/AuthController` | UserProfile, UserToken, UserRole |
| P0 | 選單 | (新建) → `Api/MenuController` | UserMenu, MenuControl, UserRoleDefinition |
| P1 | 組織查詢 | `OrganizationQueryController` → `Api/OrganizationController` | Organization, OrganizationCategory, OrganizationStatus |
| P1 | 組織管理 | `OrganizationController` → 合併入上方 | Organization, OrganizationExtension |
| P1 | 使用者 | `UserProfileController` → `Api/UserProfileController` | UserProfile, UserProfileExtension, UserRole |
| P2 | 發票查詢 | `InvoiceQueryController` → `Api/InvoiceQueryController` | InvoiceItem, InvoiceAmountType, InvoiceSeller, InvoiceBuyer |
| P2 | 發票處理 | `InvoiceProcessController` → `Api/InvoiceController` | InvoiceItem, InvoiceProductItem |
| P2 | 折讓 | `AllowanceProcessController` → `Api/AllowanceController` | InvoiceAllowance, AllowanceCancellation |
| P3 | 字軌管理 | `TrackCodeController` → `Api/TrackCodeController` | InvoiceTrackCode, InvoiceNoAllocation |
| P3 | 配號 | `InvoiceNumberApplyController` → `Api/InvoiceNumberController` | InvoiceNoAllocation |
| P3 | POS | `POSDeviceController` → `Api/POSDeviceController` | POSDevice |
| P4 | 稽核 | `InvoiceAuditController` → `Api/InvoiceAuditController` | InvoiceItem + audit tables |
| P4 | 通知 | `NotificationController` → `Api/NotificationController` | 通知相關表 |
| P4 | 中獎 | `WinningNumberController` → `Api/WinningNumberController` | InvoiceWinningNumber |
| P5 | 帳務 | `Accounting/*` → `Api/BillingController` | billing.* tables |
| P5 | 資料交換 | `DataExchangeController` → `Api/DataExchangeController` | CDS_Document, DocumentFlow |

### 3.3 API 標準化規範

```csharp
// 統一回傳格式
{
  "success": true/false,
  "message": "...",
  "data": { ... },        // 單筆
  "errors": [...]          // 驗證錯誤
}

// 分頁查詢
GET /api/invoices?page=1&pageSize=20&sortBy=invoiceDate&sortDesc=true&filters...
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 1234,
    "pageNumber": 1,
    "pageSize": 20,
    "hasNext": true,
    "hasPrevious": false
  }
}

// RESTful 動詞
GET    /api/invoices          → 查詢列表
GET    /api/invoices/{id}     → 取得單筆
POST   /api/invoices          → 新增
PUT    /api/invoices/{id}     → 更新
DELETE /api/invoices/{id}     → 刪除
POST   /api/invoices/export   → 匯出 (特殊操作用 POST)
```

---

## 第四階段：前端 Vue 模組開發

> 目標：將 Mock 切換為真實 API，逐步遷移剩餘頁面

### 4.1 前端基礎建設

| 步驟 | 工作內容 |
|------|----------|
| 4.1.1 | 建立 `src/stores/` (Pinia) 管理全局狀態：`useAuthStore`, `useMenuStore`, `useUserStore` |
| 4.1.2 | 重構 `src/api/` — 將 mock import 切換為真實 `$api` 呼叫 |
| 4.1.3 | 統一 API Error Handling（Toast 通知 + 表單驗證錯誤顯示） |
| 4.1.4 | 建立 TypeScript 介面 (`src/interfaces/`) 對齊後端 DTO |

### 4.2 模組遷移 (對應 WebHome Views)

| 優先 | Vue 模組 | WebHome Views 來源 | 狀態 |
|------|----------|-------------------|------|
| ✅ | LoginForm | Views/Account/ | 已完成 (Mock) → 接真實 API |
| ✅ | InvoiceQuery (6 元件) | Views/InvoiceQuery/ | 已完成 (Mock) → 接真實 API |
| ✅ | OrganizationQuery (19 元件) | Views/OrganizationQuery/ | 已完成 (Mock) → 接真實 API |
| ✅ | TrackCodeQuery | Views/TrackCode/ | 已完成 (Mock) → 接真實 API |
| P1 | UserProfile | Views/UserProfile/ | 待遷移 |
| P2 | InvoiceProcess | Views/InvoiceProcess/ | 待遷移 |
| P2 | AllowanceProcess | Views/AllowanceProcess/ | 待遷移 |
| P3 | InvoiceNumberApply | Views/InvoiceNumberApply/ | 待遷移 |
| P3 | InvoiceAudit | Views/InvoiceAudit/ | 待遷移 |
| P3 | POSDevice | Views/POSDevice/ | 待遷移 |
| P4 | BusinessRelationship | Views/BusinessRelationship/ | 待遷移 |
| P4 | DataExchange | Views/DataExchange/ | 待遷移 |
| P4 | DonatedInvoice | Views/DonatedInvoice/ | 待遷移 |
| P4 | WinningNumber | Views/WinningNumber/ | 待遷移 |
| P5 | Notification | Views/Notification/ | 待遷移 |
| P5 | Handling | Views/Handling/ | 待遷移 |
| P5 | DataFlow | Views/DataFlow/ | 待遷移 |

### 4.3 前端路由與權限規劃

```javascript
// router/index.ts 目標結構
const routes = [
  { path: '/login', component: LoginForm, meta: { public: true } },
  {
    path: '/',
    component: DefaultLayout,
    meta: { requiresAuth: true },
    children: [
      // P0 - 基礎
      { path: 'dashboard', component: MainPage },
      { path: 'profile', component: UserProfile },
      
      // P1 - 組織
      { path: 'organization', component: OrgQuery, meta: { permission: 'org.view' } },
      { path: 'organization/:id', component: OrgEdit, meta: { permission: 'org.edit' } },
      
      // P2 - 發票
      { path: 'invoices', component: InvoiceQuery, meta: { permission: 'invoice.view' } },
      { path: 'invoices/process', component: InvoiceProcess, meta: { permission: 'invoice.manage' } },
      { path: 'allowances', component: AllowanceProcess, meta: { permission: 'allowance.manage' } },
      
      // P3 - 管理
      { path: 'track-codes', component: TrackCode, meta: { permission: 'trackcode.manage' } },
      { path: 'invoice-numbers', component: InvoiceNumberApply, meta: { permission: 'invoiceno.manage' } },
      { path: 'pos-devices', component: POSDevice, meta: { permission: 'pos.manage' } },
      { path: 'audit', component: InvoiceAudit, meta: { permission: 'audit.view' } },

      // P4 - 進階
      { path: 'relationships', component: BusinessRelationship, meta: { permission: 'relationship.manage' } },
      { path: 'data-exchange', component: DataExchange, meta: { permission: 'exchange.manage' } },
      { path: 'winning-numbers', component: WinningNumber, meta: { permission: 'winning.view' } },
      
      // P5 - 系統
      { path: 'notifications', component: Notification },
      { path: 'admin/users', component: UserManagement, meta: { permission: 'user.manage' } },
    ]
  }
]
```

---

## 第五階段：測試與品質保障

| 步驟 | 工作內容 |
|------|----------|
| 5.1 | 後端 API 整合測試 — 每個 Controller 的 CRUD 路徑 |
| 5.2 | 前端 Component 測試 (Vitest) — 核心 composables 與表單驗證 |
| 5.3 | E2E 測試 — 登入→查詢→操作 主要流程 |
| 5.4 | API 文件 — 啟用 Swagger/OpenAPI (`Swashbuckle` 已在 WebHome 引用) |
| 5.5 | 資安檢查 — CORS 設定、JWT 有效期、SQL Injection 防護、XSS 防護 |

---

## 第六階段：部署與上線切換

| 步驟 | 工作內容 |
|------|----------|
| 6.1 | WebClient `Program.cs` 整合 SPA + API hosting |
| 6.2 | 設定 CORS policy (開發/生產環境) |
| 6.3 | 資料庫 Migration 策略 — 確保 EF Core Model 與現有 schema 相容 |
| 6.4 | 並行運行期 — WebHome (舊) 與 WebClient (新) 同時運作，逐步切流量 |
| 6.5 | WebHome 停用計劃 — 確認所有功能已遷移後關閉 |

---

## 執行時程建議

```
階段              週期      前置依賴
─────────────────────────────────────
1. 資料存取層       2-3 週    無
2. 權限系統         2 週      階段 1.1 (EF Core)
3. API 層 (P0-P1)  2-3 週    階段 1 + 2
3. API 層 (P2-P3)  3-4 週    P0-P1 完成
4. 前端接 API       與階段 3 並行
5. 測試             持續進行
6. 部署切換         1-2 週    階段 3-5 完成
```

---

## 技術決策摘要

| 決策項目 | 選擇 | 理由 |
|----------|------|------|
| ORM | EF Core (Database-First scaffold) | 現代化、支援 Migration、取代 LINQ to SQL |
| 認證 | JWT Bearer Token | 前後端分離標準做法，對接現有 UserProfile 表 |
| 狀態管理 | Pinia | Vue 3 官方推薦，取代 localStorage 直接操作 |
| API 格式 | RESTful + 統一 `ApiResponse<T>` | 與前端 `api-response.ts` 介面一致 |
| 權限控制 | Policy-based Authorization | .NET Core 正規做法，取代自訂 Filter |
| 專案結構 | WebClient 統一承載 API + SPA | 簡化部署，已有 Vite proxy 設定 |
