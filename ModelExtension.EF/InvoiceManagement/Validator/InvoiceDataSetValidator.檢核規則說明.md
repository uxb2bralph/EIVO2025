# 發票匯入 (Xlsx/DataSet) 資料格式檢核規則說明

> 對象程式：[InvoiceDataSetValidator.cs](InvoiceDataSetValidator.cs)（繼承自 [InvoiceRootInvoiceValidator.cs](InvoiceRootInvoiceValidator.cs)）
> 適用來源：Excel（Xlsx）逐列匯入的開立發票（B2B / B2C / 跨境電商 CBE）
> 訊息來源：`ModelCore.Resource.MessageResources`（[MessageResources.resx](../../../ModelCore.EF/Resource/MessageResources.resx)）

---

## 一、整體檢核流程

進入點：`Validate(DataRow dataItem, IEnumerable<DataRow> details)`

| 順序 | 檢核方法 | 檢核目的 |
|----|---------|---------|
| 1 | `checkBusiness()` | 賣方 / 買方 / 隨機碼 / 買方明細 |
| 2 | `checkDataNumber()`（自動配號時）／ `buildDataNumber()`（自帶發票號時） | 單據號碼、單據日期 |
| 3 | `checkAmount()` | 金額、課稅別、稅率、通關方式、幣別 |
| 4 | `checkInvoiceDelivery()` | 列印 / 載具 / 捐贈 交付方式組合 |
| 5 | `checkMandatoryFields()` | 捐贈註記、列印註記、發票類別 |
| 6 | `checkInvoiceProductItems()` | 品項明細 |
| 7 | `checkInvoice()` | 發票號碼、發票日期、重複、字軌 |

> 任一步驟回傳 `Exception` 即中止並回報該筆錯誤；全部通過回傳 `null`。
> `_isAutoTrackNo` = `InvoiceNo()` 是否為空。自動配號模式會嚴格檢查單據號碼（步驟 2），自帶號碼模式僅組裝單據資料。

---

## 二、支援的匯入格式（processType）與欄位對應

`resetFieldIndex()` 依 `processType` 決定各欄位在 DataRow 的欄位索引。

| processType | 說明 | 是否含發票號欄 | 明細稅別欄 |
|-------------|------|:---:|:---:|
| `F0401_Xlsx_Allocation_ByVAC` | VAC 配號（系統自動配號） | 否（自動配號） | 有 |
| `F0401_Xlsx_CBE` | 跨境電商（欄位精簡，多數欄位由程式帶固定值） | 否（自動配號） | 無（固定為「應稅」） |
| `A0101_Xlsx_Allocation_ByIssuer` / `F0401_Xlsx_Allocation_ByIssuer` | 開立人自帶發票號碼 | 有 | 有 |

### CBE（跨境電商）特別行為
`processType == F0401_Xlsx_CBE` 時多欄位改帶固定值：
- `BuyerID()` = `"0000000000"`、`TaxType()` = 應稅、`InvoiceType()` = 一般稅額、`TaxRate()` = `0.05`、`PrintMark()` = `"N"`、`DonateMark()` = `"0"`
- `CarrierType()` = `5G0001`（跨境電商載具）、載具號取 `Carrier_Id1`、`EMail()` 取 `Carrier_Id1`
- `randomNo` 以現在時間毫秒 `ffff` 產生

---

## 二之一、完整 Excel 欄位順序對照表

> 欄位序號 = DataRow 的欄位索引（0-based），即 Excel 由左至右第 1 欄對應索引 0。
> 未列出的欄位 = 該格式無此欄，由程式帶固定值或不使用（見上「CBE 特別行為」）。

### 格式 A：`F0401_Xlsx_Allocation_ByVAC`（系統自動配號 / B2C、B2B 皆可）

**發票主檔（共 29 欄）**

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Data_ID | 單據號碼 |
| 1 | Data_Date | 單據日期 |
| 2 | Seller_ID | 賣方統編 |
| 3 | Buyer_Name | 買方名稱 |
| 4 | Buyer_ID | 買方統編（空=B2C `0000000000`） |
| 5 | Buyer_Mark | 買方註記 |
| 6 | Customs_Clearance_Mark | 通關方式 |
| 7 | Customer_ID | 客戶編號 |
| 8 | Contact_Name | 聯絡人 |
| 9 | EMail | 電子郵件 |
| 10 | Address | 地址 |
| 11 | Phone | 電話 |
| 12 | Sales_Amount | 應稅銷售額 |
| 13 | Free_Tax_Sales_Amount | 免稅銷售額 |
| 14 | Zero_Tax_Sales_Amount | 零稅率銷售額 |
| 15 | Invoice_Type | 發票類別 |
| 16 | Tax_Type | 課稅別 |
| 17 | Tax_Rate | 稅率 |
| 18 | Tax_Amount | 稅額 |
| 19 | Total_Amount | 總金額 |
| 20 | Currency | 幣別 |
| 21 | Print_Mark | 列印註記 |
| 22 | Carrier_Type | 載具類別 |
| 23 | Carrier_Id1 | 載具顯碼 |
| 24 | Carrier_Id2 | 載具隱碼 |
| 25 | Donate_Mark | 捐贈註記 |
| 26 | NPOBAN | 捐贈對象 |
| 27 | Random_Number | 隨機碼 |
| 28 | Main_Remark | 主要備註 |

**明細（共 8 欄）**

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Data_ID | 單據號碼（對應主檔） |
| 1 | Description | 品名 |
| 2 | Quantity | 數量 |
| 3 | Unit | 單位 |
| 4 | Unit_Price | 單價 |
| 5 | Amount | 金額 |
| 6 | Item_Tax_Type | 品項課稅別 |
| 7 | Remark | 備註 |

---

### 格式 B：`A0101_Xlsx_Allocation_ByIssuer` / `F0401_Xlsx_Allocation_ByIssuer`（開立人自帶發票號碼）

**發票主檔（共 30 欄）**　※較格式 A 多首欄發票號碼與發票日期，且**無 Data_Date 欄**

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Invoice_No | 發票號碼（2碼字軌+8碼） |
| 1 | Invoice_Date | 發票日期 |
| 2 | Data_ID | 單據號碼 |
| 3 | Seller_ID | 賣方統編 |
| 4 | Buyer_Name | 買方名稱 |
| 5 | Buyer_ID | 買方統編 |
| 6 | Buyer_Mark | 買方註記 |
| 7 | Customs_Clearance_Mark | 通關方式 |
| 8 | Customer_ID | 客戶編號 |
| 9 | Contact_Name | 聯絡人 |
| 10 | EMail | 電子郵件 |
| 11 | Address | 地址 |
| 12 | Phone | 電話 |
| 13 | Sales_Amount | 應稅銷售額 |
| 14 | Free_Tax_Sales_Amount | 免稅銷售額 |
| 15 | Zero_Tax_Sales_Amount | 零稅率銷售額 |
| 16 | Invoice_Type | 發票類別 |
| 17 | Tax_Type | 課稅別 |
| 18 | Tax_Rate | 稅率 |
| 19 | Tax_Amount | 稅額 |
| 20 | Total_Amount | 總金額 |
| 21 | Currency | 幣別 |
| 22 | Print_Mark | 列印註記 |
| 23 | Carrier_Type | 載具類別 |
| 24 | Carrier_Id1 | 載具顯碼 |
| 25 | Carrier_Id2 | 載具隱碼 |
| 26 | Donate_Mark | 捐贈註記 |
| 27 | NPOBAN | 捐贈對象 |
| 28 | Random_Number | 隨機碼 |
| 29 | Main_Remark | 主要備註 |

**明細（共 8 欄）**　※首欄改為 Invoice_No 對應主檔

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Invoice_No | 發票號碼（對應主檔） |
| 1 | Description | 品名 |
| 2 | Quantity | 數量 |
| 3 | Unit | 單位 |
| 4 | Unit_Price | 單價 |
| 5 | Amount | 金額 |
| 6 | Item_Tax_Type | 品項課稅別 |
| 7 | Remark | 備註 |

---

### 格式 C：`F0401_Xlsx_CBE`（跨境電商，欄位精簡）

**發票主檔（共 10 欄）**　※其餘欄位由程式帶固定值（見「CBE 特別行為」）

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Data_ID | 單據號碼 |
| 1 | Data_Date | 單據日期 |
| 2 | Seller_ID | 賣方統編 |
| 3 | Customer_ID | 客戶編號 |
| 4 | Sales_Amount | 應稅銷售額 |
| 5 | Tax_Amount | 稅額 |
| 6 | Total_Amount | 總金額 |
| 7 | Currency | 幣別 |
| 8 | Carrier_Id1 | 載具號（同時作為 EMail 使用；載具類別固定 `5G0001`） |
| 9 | Main_Remark | 主要備註 |

**明細（共 7 欄）**　※無品項課稅別欄（固定為「應稅」）

| 索引 | 欄位 | 說明 |
|:--:|------|------|
| 0 | Data_ID | 單據號碼（對應主檔） |
| 1 | Description | 品名 |
| 2 | Quantity | 數量 |
| 3 | Unit | 單位 |
| 4 | Unit_Price | 單價 |
| 5 | Amount | 金額 |
| 6 | Remark | 備註 |

---

### 三格式主檔欄位差異速查

| 欄位 | A：ByVAC | B：ByIssuer | C：CBE |
|------|:--:|:--:|:--:|
| Invoice_No | — | 0 | —（自動配號） |
| Invoice_Date | — | 1 | — |
| Data_ID | 0 | 2 | 0 |
| Data_Date | 1 | **—** | 1 |
| Seller_ID | 2 | 3 | 2 |
| Buyer_Name | 3 | 4 | —（固定 null） |
| Buyer_ID | 4 | 5 | —（固定 `0000000000`） |
| Buyer_Mark | 5 | 6 | — |
| Customs_Clearance_Mark | 6 | 7 | —（固定 null） |
| Customer_ID | 7 | 8 | 3 |
| Contact_Name | 8 | 9 | — |
| EMail | 9 | 10 | 取 Carrier_Id1(8) |
| Address | 10 | 11 | — |
| Phone | 11 | 12 | — |
| Sales_Amount | 12 | 13 | 4 |
| Free_Tax_Sales_Amount | 13 | 14 | —（固定 null） |
| Zero_Tax_Sales_Amount | 14 | 15 | —（固定 null） |
| Invoice_Type | 15 | 16 | —（固定 一般稅額） |
| Tax_Type | 16 | 17 | —（固定 應稅） |
| Tax_Rate | 17 | 18 | —（固定 0.05） |
| Tax_Amount | 18 | 19 | 5 |
| Total_Amount | 19 | 20 | 6 |
| Currency | 20 | 21 | 7 |
| Print_Mark | 21 | 22 | —（固定 N） |
| Carrier_Type | 22 | 23 | —（固定 5G0001） |
| Carrier_Id1 | 23 | 24 | 8 |
| Carrier_Id2 | 24 | 25 | —（固定 null） |
| Donate_Mark | 25 | 26 | —（固定 0） |
| NPOBAN | 26 | 27 | —（固定 null） |
| Random_Number | 27 | 28 | —（時間 ffff 產生） |
| Main_Remark | 28 | 29 | 9 |

> — 表示該格式無此欄位。CBE 之「固定 xxx」為程式內帶入的預設值，不需在 Excel 提供。

---

## 三、逐項檢核規則

### 1. 賣方 / 買方 — `checkBusiness()`

| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 賣方統編 | 依 `SellerID()` 查得 `Organization`，查無 → 錯誤 | `AlertInvalidSeller` |
| 代理開立 | 有 `_owner` 且賣方非本人、且未登記為其代理開立人 | `InvalidSellerOrAgent` |
| 賣方狀態 | 賣方被註記停用（`Mark_To_Delete`） | （硬編字串）`開立人已註記停用…<SellerId/>` |
| 買方統編為空 | 自動補 `"0000000000"`（B2C） | — |
| 買方統編格式 | 非 `0000000000` 時須符合 `^[0-9]{8}$` | `InvalidBuyerId` |
| 買方統編檢核碼 | 賣方啟用 `EnableBuyerIDValidation` 時，須通過 `CheckRegno()` | `InvalidReceiptNo` |
| 買方名稱長度 | `BuyerName().Length > 60` | `InvalidBuyerNameLengthLimit` |
| 隨機碼 | 空 → 自動以時間 `ffff` 產生；有值須符合 `^[0-9]{4}$` | `InvalidRandomNumber` |

接著呼叫 `checkBusinessDetails()` 組裝買方資料（含 B2C 名稱正規化、關係人資料回填），此步驟不產生檢核錯誤。

### 2. 單據號碼 — `checkDataNumber()`（僅自動配號模式）

| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 單據號碼必填 | `DataID()` 為空 | `AlertDataNumber` |
| 單據號碼長度 | `> 60` 碼 | `AlertDataNumberLimitedLength` |
| 強制查核號重複 | 賣方 `ForcedAuditNo()` 時建立稽核號失敗 | `AlertDataNumberDuplicated` |
| 單據號碼重複 | 同賣方已存在相同 `OrderNo` | `AlertDataNumberDuplicated`（`DuplicateDataNumberException`） |
| 單據日期必填 | `DataDate()` 無值 | `AlertDataDate` |

> 自帶發票號模式改走 `buildDataNumber()`：僅在 `DataID()` 有值時建立單據，不做上述檢核。

### 3. 金額 / 課稅 / 幣別 — `checkAmount()`

| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 應稅銷售額 | `SalesAmount() < 0` | `InvalidSellingPrice` |
| 免稅銷售額 | `FreeTaxSalesAmount() < 0` | `InvalidFreeTaxAmount` |
| 零稅率銷售額 | `ZeroTaxSalesAmount() < 0` | `InvalidZeroTaxAmount` |
| 稅額 | `TaxAmount() < 0` | `InvalidTaxAmount` |
| 總金額 | `TotalAmount() < 0` | `InvalidTotalAmount` |
| 課稅別 | 須為 `TaxTypeDefinition` 定義值（1應稅/2零稅率/3免稅/4特種稅率/9混合稅率） | `InvalidTaxType` |
| 稅率 | `TaxRate() < 0` | `InvalidTaxRate` |
| 零稅率通關方式 | 課稅別=零稅率時，`CustomsClearanceMark` 必填 | `AlertClearanceMarkZeroTax` |
| 通關方式值 | 有值時僅允許 `1`（非經海關）或 `2`（經海關） | `AlertClearanceMarkExport` |
| 幣別 | `Currency()` 有值但查無對應 `CurrencyType` | （硬編字串）`Invalid currency code…<Currency/>` |

### 4. 交付方式（列印 / 載具 / 捐贈）— `checkInvoiceDelivery()`

以 4 個布林維度查 `_deliveryCheck[列印, 載具, B2C, 捐贈]` 決策表：
- 列印 = `PrintMark() == "Y"`
- 載具 = `CarrierType` 非空且 `CarrierId1/2` 至少一個有值
- B2C = `BuyerID() == "0000000000"`
- 捐贈 = `DonateMark() == "1"`

| 情境 | 錯誤訊息代碼 |
|------|-------------|
| 列印 + 有載具（B2C） | `AlertPrintedInvoiceCarrierType`（列印時載具須留空） |
| 列印 + 有載具 + 捐贈（B2B） | `AlertDonationInvoiceCarryType`（捐贈時載具須留空） |
| 列印 + 捐贈（B2C） | `AlertPrintedInvoiceDonation`（列印不得捐贈） |
| 未列印 + 捐贈（B2C） | 捐贈對象 `NPOBAN` 空或長度不在 3~7 → `InvalidDonationTaker` |
| 未列印 + 有載具 + 共通性載具 | 載具驗證失敗 → `InvalidPublicCarrierType`（見下） |

#### 載具驗證（`usePublicCarrier` / `checkPublicCarrier`）
| 載具類別 | 規則 | 失敗訊息 |
|---------|------|---------|
| `3J0002` 手機條碼 | 長度 8、開頭 `/`、符合 `^/[A-Z0-9+-.]{7}$` | `InvalidPublicCarrierType` |
| `CQ0001` 自然人憑證 | 符合 `^[A-Z]{2}[0-9]{14}$` | `InvalidPublicCarrierType` |
| `5G0001` 跨境電商載具 | 有 `CarrierId1` 或 `CarrierId2` 即可 | `InvalidPublicCarrierType` |

#### 載具完整性（`checkCarrierDataIsComplete`）
| 檢核項 | 錯誤訊息代碼 |
|-------|-------------|
| 載具類別空、或顯碼/隱碼皆空 | `AlertInvoiceCarrierComplete` |
| 載具類別 > 6 碼、顯碼/隱碼 > 64 碼 | `AlertInvoiceCarrierLength` |

### 5. 必填欄位 — `checkMandatoryFields()`

| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 捐贈註記 | B2C 時 `DonateMark` 僅允許 `0` / `1` | `InvalidDonationMark` |
| 列印註記 | 空 → 補 `N`；有值須為 `Y` / `N` | `InvalidPrintMark` |
| 發票類別 | 須為有效類別（僅 `07` 一般稅額、`08` 特種稅額） | `InvalidInvoiceType` |

### 6. 品項明細 — `checkInvoiceProductItems()`

| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 明細筆數 | `_details` 為空 | `InvalidInvoiceDetails` |
| 品名 | 空或 `> 256` 碼 | `InvalidProductDescription` |
| 單位 | 有值且 `> 6` 碼 | `InvalidPieceUnit` |

> 單價/金額格式、金額 = 單價×數量 等檢核目前為註解停用狀態。

### 7. 發票號碼 / 日期 / 重複 — `checkInvoice()`

**自動配號模式**：由 `TrackNoManager` 取號，取號失敗 → `AlertNullTrackNoInterval`。

**自帶號碼模式**：
| 檢核項 | 規則 | 錯誤訊息代碼 |
|-------|------|-------------|
| 發票日期 | `InvoiceDate()` 無值 | `AlertInvoiceDate` |
| 發票號碼格式 | 須符合 `^[a-zA-Z]{2}[0-9]{8}$` | `AlertInvoiceNumber` |
| 字軌檢核 | `CheckInvoiceNo()`（啟用時查 `InvoiceTrackCode`） | `InvalidTrackCode` |
| 發票重複 | 同字軌號碼於同期別已存在 | `AlertInvoiceDuplicated` |
| 字軌區間 | 啟用 `EnableTrackCodeInvoiceNoValidation` 時查無核配區間 | （硬編字串）`發票號碼錯誤…<InvoicNumber/>` |

---

## 四、錯誤訊息代碼總表

> `{0}...{n}` 為執行期帶入的實際上傳值；`TAG` 標示對應的資料欄位。

| 代碼 (Resource Key) | 訊息內容 |
|---------------------|---------|
| `AlertInvalidSeller` | 賣方為非註冊營業人,開立人統一編號:{0}，TAG:&lt;SellerId/&gt; |
| `InvalidSellerOrAgent` | 發票開立人為非註冊營業人:{0}…也未註冊於機構:{1}，代理發票傳送 |
| `InvalidBuyerId` | 買方識別碼錯誤，傳送資料：{0}，TAG:&lt;BuyerId/&gt; |
| `InvalidReceiptNo` | 公司統一編號錯誤: {0}, Incorrect TAG:&lt;BuyerId/&gt; |
| `InvalidBuyerNameLengthLimit` | B2B買方名稱不能空白，長度最多60碼，傳送資料：{0}，TAG:&lt;BuyerName/&gt; |
| `InvalidRandomNumber` | 交易隨機碼應由4位數值構成，上傳資料：{0}，TAG:&lt;RandomNumber/&gt; |
| `AlertDataNumber` | 單據號碼錯誤，TAG:&lt;DataNumber/&gt; |
| `AlertDataNumberLimitedLength` | 單據號碼資料長度最長為60碼；傳送資料:{0}，TAG:&lt;DataNumber/&gt; |
| `AlertDataNumberDuplicated` | 單據號碼不可重複；傳送資料:{0}，TAG:&lt;DataNumber/&gt; |
| `AlertDataDate` | 單據日期錯誤，TAG:&lt;DataDate/&gt; |
| `InvalidSellingPrice` | 應稅銷售額合計(新台幣)不可為負數且為整數，上傳資料：{0},TAG:&lt;SalesAmount/&gt; |
| `InvalidFreeTaxAmount` | 免稅銷售額合計(新台幣)不可為負數且為整數，上傳資料：{0},TAG:&lt;FreeTaxSalesAmount/&gt; |
| `InvalidZeroTaxAmount` | 零稅率銷售額合計(新台幣)不可為負數且為整數，上傳資料：{0},TAG:&lt;ZeroTaxSalesAmount/&gt; |
| `InvalidTaxAmount` | 營業稅額不可為負數且為整數，上傳資料：{0},TAG:&lt;TaxAmount/&gt; |
| `InvalidTotalAmount` | 總金額不可為負數且為整數，上傳資料：{0},TAG:&lt;TaxAmount/&gt; |
| `InvalidTaxType` | 課稅別格式錯誤，上傳資料：{0},TAG:&lt;TaxType/&gt; |
| `InvalidTaxRate` | 稅率格式錯誤，上傳資料：{0},TAG:&lt;TaxRate/&gt; |
| `AlertClearanceMarkZeroTax` | 若為零稅率發票，通關方式註記為必填欄位，上傳資料：{0},TAG:&lt;CustomsClearanceMark/&gt; |
| `AlertClearanceMarkExport` | 通關方式註記格式錯誤，限填 "1"(非經海關) 或 "2"(經海關)，上傳資料：{0},TAG:&lt;CustomsClearanceMark/&gt; |
| `AlertPrintedInvoiceCarrierType` | 註記列印時載具類別請留空白，傳送資料：{0}，TAG:&lt;CarrierType/&gt; |
| `AlertDonationInvoiceCarryType` | 註記捐贈時載具類別請留空白，傳送資料：{0}，TAG:&lt;CarrierType/&gt; |
| `AlertPrintedInvoiceDonation` | 列印或設定全列印時，不得標註捐贈，傳送資料：{0}，TAG:&lt;DonateMark/&gt; |
| `InvalidDonationTaker` | 發票捐贈對象錯誤，傳送資料：{0}，TAG:&lt;NPOBAN/&gt; |
| `InvalidPublicCarrierType` | 載具資料檢查（1.非共通性載具 2.顯碼/隱碼開頭為'/' 3.含'/'共8碼），TAG:&lt;CarrierType/CarrierId1/CarrierId2/&gt; |
| `AlertInvoiceCarrierComplete` | 上傳載具資料不完全（1.載具類別 2.顯碼或隱碼至少填一） |
| `AlertInvoiceCarrierLength` | 載具類別(6碼)或顯碼/隱碼ID(64碼)長度超出限制，載具類別{0}、顯碼{1}、隱碼{2} |
| `InvalidDonationMark` | 捐贈註記錯誤，上傳資料：{0}，TAG:&lt;DonateMark/&gt; |
| `InvalidPrintMark` | 電子發票證明聯已列印註記錯誤，TAG:&lt;PrintMark/&gt; |
| `InvalidInvoiceType` | 發票類別格式錯誤，請填相應代號「07」或「08」，上傳資料：{0}，TAG:&lt;InvoiceType/&gt; |
| `InvalidInvoiceDetails` | 無發票品項明細，TAG:&lt;InvoiceItem/&gt; |
| `InvalidProductDescription` | 品項名稱不可空白且長度不得大於256，傳送資料：{0}，TAG:&lt;Description/&gt; |
| `InvalidPieceUnit` | 單位格式錯誤，傳送資料：{0}，TAG:&lt;Unit/&gt; |
| `AlertNullTrackNoInterval` | 未設定發票字軌或發票號碼已用完，發票開立人統編：{0} |
| `AlertInvoiceDate` | 發票日期，TAG：&lt;InvoiceDate/&gt; |
| `AlertInvoiceNumber` | 發票號碼，傳送資料:{0}，TAG：&lt;InvoicNumber/&gt; |
| `InvalidTrackCode` | 發票號碼字軌錯誤，傳送資料:{0}，TAG：&lt;InvoicNumber/&gt; |
| `AlertInvoiceDuplicated` | 發票號碼重複，已存在的發票資料。 |

### 硬編（未走 Resource）之錯誤訊息
| 觸發位置 | 訊息 |
|---------|------|
| `checkBusiness()` 賣方停用 | 開立人已註記停用,開立人統一編號:{0}，TAG:&lt;SellerId/&gt; |
| `checkAmount()` 幣別 | Invalid currency code：{0}，TAG：&lt;Currency/&gt; |
| `checkInvoice()` 字軌區間 | 發票號碼錯誤:{0}，TAG:&lt;InvoicNumber/&gt; |
| `checkPrintAll()` 跨境載具 | EMail as Carrier ID limits to 64 characters, "{0}" |

---

## 五、關鍵格式規則（Regex / 常數）

| 項目 | 規則 |
|------|------|
| 買方統編 | `^[0-9]{8}$`（另有 `CheckRegno()` 檢核碼驗證） |
| 隨機碼 | `^[0-9]{4}$` |
| 發票號碼 | `^[a-zA-Z]{2}[0-9]{8}$`（2 碼字軌 + 8 碼號碼） |
| 手機條碼載具 | `^/[A-Z0-9+-.]{7}$`，長度 8、開頭 `/`（`__CELLPHONE_BARCODE = 3J0002`） |
| 自然人憑證載具 | `^[A-Z]{2}[0-9]{14}$`（`__自然人憑證 = CQ0001`） |
| 跨境電商載具 | `__CROSS_BORDER_MURCHANT = 5G0001` |
| 捐贈碼 NPOBAN | 長度 3~7 碼 |
| 單據號碼 | 最長 60 碼 |
| 買方名稱 | 最長 60 碼 |
| 品名 | 最長 256 碼 |
| 單位 | 最長 6 碼 |
| 載具類別 | 最長 6 碼；載具號 最長 64 碼 |
| 有效發票類別 | `07`（一般稅額）、`08`（特種稅額） |
| 有效課稅別 | `1` 應稅、`2` 零稅率、`3` 免稅、`4` 特種稅率、`9` 混合稅率 |
| 通關方式 | `1` 非經海關、`2` 經海關（零稅率時必填） |
