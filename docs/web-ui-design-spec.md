# WebHome Web UI 設計規格指引

> **適用專案**：`WebHome`（ASP.NET Core MVC, net10.0, Razor Views）  
> **目標 UI 框架**：Bootstrap 5.3  
> **版本**：1.0（2026-09-27）  
> **定位**：過渡期標準 — 本系統為 Vue 3 遷移（見 `docs/refactoring-plan.md`）完成前的過渡期 UI。新頁面與重構頁面須遵循本規格。

---

## 目錄

- [Part A — 設計規格指引](#part-a--設計規格指引)
  - [A.1 總則與設計原則](#a1-總則與設計原則)
  - [A.2 設計令牌 (Design Tokens)](#a2-設計令牌-design-tokens)
  - [A.3 Layout 規格](#a3-layout-規格)
  - [A.4 元件規格](#a4-元件規格)
  - [A.5 互動與狀態規格](#a5-互動與狀態規格)
  - [A.6 無障礙 (a11y) 規格](#a6-無障礙-a11y-規格)
  - [A.7 響應式規格](#a7-響應式規格)
  - [A.8 命名與 class 慣例](#a8-命名與-class-慣例)
  - [A.9 資產與建置規格](#a9-資產與建置規格)
  - [A.10 Razor 端慣例](#a10-razor-端慣例)
- [Part B — UI 優化路線圖](#part-b--ui-優化路線圖)
- [附錄 A — BS3/4 → BS5 Class 對照表](#附錄-a--bs34--bs5-class-對照表)
- [附錄 B — Font Awesome 4 → 6 圖示對照表](#附錄-b--font-awesome-4--6-圖示對照表)

---

# Part A — 設計規格指引

## A.1 總則與設計原則

### 適用範圍

| 適用 | 不適用 |
|------|--------|
| `WebHome/Views/` 下所有 `.cshtml` | `WebClient/`（Vue 3 SPA，遵循 `.github/instructions/vue.instructions.md`） |
| `WebHome/wwwroot/` 下所有靜態資產 | `Web/`（legacy 參考，唯讀） |
| `WebHome/Components/`（Razor Components） | `HNB.Web/`（新 Vue 專案） |

### 設計原則

1. **一致性** — 同一類元件全站只有一種寫法；禁止頁面自創樣式解決通用問題。
2. **可維護** — 樣式集中於 SCSS 令牌 + 少量元件 class；JS 集中於 `wwwroot/ts/` 編譯產物；禁止散落 inline style 與 inline script 邏輯。
3. **可存取** — 鍵盤可操作、螢幕閱讀器可理解（見 A.6）。
4. **響應式** — 以 BS5 斷點為基礎，桌面優先（本系統主要使用情境為辦公桌面），mobile 可用但不強求完美。
5. **過渡期定位** — 新元件設計應考慮未來可對應到 Vue 3 元件（命名、結構語意化），降低遷移成本。

### 技術基準

| 項目 | 標準 | 現況 | 差距 |
|------|------|------|------|
| CSS 框架 | Bootstrap **5.3**（自架，禁 CDN） | BS3 + BS4.6 混用 | 需升級 |
| 圖示 | Font Awesome **6 Free**（自架） | FA 4.2.0 CDN | 需升級 + 自架 |
| JS 基礎 | jQuery **3.7.x**（過渡期保留） | jQuery 3.7.1 | ✅ |
| 日期選擇 | jQuery UI datepicker（CSS/JS 版本一致） | CSS 1.8.14 / JS 1.13.2 | 需對齊 |
| 樣式預處理 | SCSS（esbuild 編譯） | `site.scss` 空檔 | 需建立 |
| 型別 | TypeScript（`wwwroot/ts/`，esbuild bundle） | 僅 stub | 需啟用 |
| 表格 | 原生 `<table>` + BS5 class + 自訂排序 JS | jsGrid / DataTables / rwd-table 混雜 | 需標準化 |
| 對話框 | BS5 Modal（統一） | jQuery UI dialog + BS modal 混用 | 需標準化 |

---

## A.2 設計令牌 (Design Tokens)

### 落點

```
WebHome/wwwroot/scss/
├── _tokens.scss        ← 所有設計令牌（SCSS 變數）
├── _bootstrap.scss     ← BS5 變數覆寫（@use "bootstrap" with (...)）
├── _layout.scss        ← shell / navbar / sidebar / tab
├── _components.scss    ← 元件樣式（表格、對話框、表單等）
├── _utilities.scss     ← 自訂 utility class
└── site.scss           ← 主入口（@use 各 partial）→ 編譯為 wwwroot/css/site.css
```

編譯產物由 `WebHome/package.json` 的 esbuild 管線產生（見 A.9）。

### 色彩令牌

沿用現有品牌色並正規化（來源：`App_Themes/Visitor/table.css`、`CommonScriptInclude.cshtml`、`App_Themes/Visitor/header.css`）：

```scss
// _tokens.scss — 色彩

// ─── 品牌色（沿用現有） ───
$eivo-brand:         #c99040;  // 表頭橘（現 .table01 th 背景）
$eivo-brand-dark:    #a8762e;  // 品牌色 hover/active（由 #c99040 加深 ~20%）
$eivo-brand-light:   #e8c99b;  // 品牌色淺色（選單 hover 背景）
$eivo-accent:        #eb5e00;  // 強調橘（現 header 連結 hover 色）
$eivo-info:          #3276B1;  // 資訊藍（現 loading overlay 色）

// ─── 狀態色（對齊 BS5 語意） ───
$eivo-success:       #198754;  // 成功 / 啟用 / 已完成
$eivo-warning:       #ffc107;  // 警告 / 待處理 / 進行中
$eivo-danger:        #dc3545;  // 錯誤 / 停用 / 刪除 / 作廢
$eivo-info-color:    #0dcaf0;  // 資訊 / 提示

// ─── 中性色 ───
$eivo-text:          #212529;  // 主要文字
$eivo-text-secondary:#495057;  // 次要文字
$eivo-text-muted:    #6c757d;  // 輔助文字 / disabled
$eivo-border:        #dee2e6;  // 邊框
$eivo-bg:            #f8f9fa;  // 頁面背景（現 sb-admin-2 body #f8f8f8）
$eivo-bg-content:    #ffffff;  // 內容區 / 卡片背景
$eivo-bg-stripe:     #FDF5E6;  // 表格斑马紋（現 .itemList tr:nth-child(even)）
$eivo-bg-hover:      #f1f3f5;  // 列 hover / 選單 hover
$eivo-bg-disabled:   #e9ecef;  // disabled 背景

// ─── 映射到 BS5 變數（_bootstrap.scss） ───
// @use "bootstrap" with (
//   $primary: $eivo-brand,
//   $info:    $eivo-info,
//   $success: $eivo-success,
//   $warning: $eivo-warning,
//   $danger:  $eivo-danger,
//   $font-size-base: 14px,
//   $border-radius: 4px,
// );
```

**使用規則**

| 規則 | 說明 |
|------|------|
| 禁止 hardcode | 元件顏色一律引用令牌（SCSS 變數或 CSS custom property），禁止直接寫 hex |
| 品牌色用途限定 | `$eivo-brand` 僅用於：表頭背景、主要按鈕、選單 active 態；**不得**用於一般文字（對比度不足） |
| 狀態色語意固定 | success=成功/啟用、warning=警告/待處理、danger=錯誤/停用/刪除、info=資訊/進行中 |
| 文字對比度 | 所有文字與背景組合須滿足 WCAG AA（4.5:1），品牌橘 `#c99040` 上僅放白色文字 |

### 字體令牌

```scss
// _tokens.scss — 字體
$eivo-font-family: "Microsoft JhengHei", "PingFang TC", "Noto Sans TC", Arial, sans-serif;
$eivo-font-size-base:  14px;  // 表格密集型系統基準（BS5 預設 16px 過大）
$eivo-font-size-sm:    12px;  // 表格內容、輔助文字、badge
$eivo-font-size-md:    14px;  // 表單 label、一般文字
$eivo-font-size-lg:    16px;  // 區塊標題（h3）
$eivo-font-size-xl:    20px;  // 頁面標題（h2）
$eivo-font-size-xxl:   24px;  // 登入頁標題（h1）
$eivo-line-height:     1.5;
$eivo-line-height-tight: 1.2; // 標題
```

### 間距 / 圓角 / 陰影

```scss
// 間距：直接使用 BS5 間距階梯（$spacer = 0.25rem 基準），不自訂
// 常用：ms-1(4px) ms-2(8px) ms-3(16px) ms-4(24px) ms-5(48px)

// 圓角
$eivo-radius:       4px;   // 元件圓角（按鈕、輸入框、卡片）
$eivo-radius-lg:    8px;   // 對話框、大卡片
$eivo-radius-pill:  50rem; // badge、chip

// 陰影
$eivo-shadow-sm:    0 1px 2px rgba(0,0,0,.05);   // 卡片、選單
$eivo-shadow:       0 .5rem 1rem rgba(0,0,0,.15); // modal、dropdown
$eivo-shadow-lg:    0 1rem 3rem rgba(0,0,0,.175); // offcanvas
```

### 斷點（沿用 BS5）

| 名稱 | 寬度 | 用途 |
|------|------|------|
| sm | ≥576px | 表單欄位換行 |
| md | ≥768px | 選單展開/收合分界、表單雙欄 |
| lg | ≥992px | **主 shell 分界**（sidebar 常駐 vs offcanvas） |
| xl | ≥1200px | 表格完整顯示、多欄表單 |
| xxl | ≥1400px | 寬螢幕留白 |

### 圖示

- **Font Awesome 6 Free**（自架 `wwwroot/vendor/fontawesome/`）。
- 命名規則：`<i class="fa-solid fa-magnifying-glass" aria-hidden="true"></i>`
- FA4 → FA6 名稱對照見 [附錄 B](#附錄-b--font-awesome-4--6-圖示對照表)。
- 圖示必須搭配 `aria-hidden="true"`（純裝飾）或 `aria-label`（獨立語意按鈕）。
- 禁止使用 FA4 舊名稱（如 `fa-search` → 應為 `fa-magnifying-glass`）。

---

## A.3 Layout 規格

### 3.1 標準 Shell（主功能頁）

現況參照：`Views/Template/MvcMainPage.cshtml`

```
┌──────────────────────────────────────────────────────────────┐
│ Topbar (56px): [品牌 Logo+名稱]  [公告 alert]   [🔔] [👤▼]  │
├────────────┬─────────────────────────────────────────────────┤
│ Sidebar    │  Tab bar (nav-tabs, 40px)                       │
│ (250px)    │  ┌───────────────────────────────────────────┐  │
│            │  │ 內容區 (container-fluid, padding 24px)    │  │
│ 角色選單    │  │                                           │  │
│ - 系統設定  │  │  頁面標題 (h2, mb-4)                      │  │
│ - 發票開立  │  │  表單 / 表格 / 內容                       │  │
│ - 發票查詢  │  │                                           │  │
│ - ...      │  │                                           │  │
│            │  └───────────────────────────────────────────┘  │
├────────────┴─────────────────────────────────────────────────┤
│ Footer: text-muted small — Powered by UXB2B                  │
└──────────────────────────────────────────────────────────────┘
```

#### Topbar 規格

| 區域 | 規格 |
|------|------|
| 容器 | BS5 `navbar navbar-expand-lg`，背景 `$eivo-brand`，高度 56px，`sticky-top` |
| 品牌 | 左側：系統名稱「電子發票系統」，`navbar-brand`，連結 `Home/MainPage`，白色文字 |
| 公告 | 品牌右側：BS5 `alert alert-light`（靜態，非 marquee），顯示最新一則 `SystemMessage`；多則以「更多 (n)」連結至公告頁；無公告時不顯示 |
| 通知 | 右側：bell 圖示 `fa-bell`，badge 顯示未讀數，dropdown 連結 `ProcessRequest/QueryIndex` |
| 使用者 | 最右：`fa-user` + 帳號名，dropdown：帳號管理 / 登出 |
| Hamburger | `<lg` 時左側顯示 `navbar-toggler`，開啟 sidebar offcanvas |

**遷移要點**：
- 移除 `<marquee>` → 改用 `alert`（P2）
- 移除 `<font color>` → 改用 BS5 text utility
- 移除大量註解掉的 dropdown（messages/tasks）→ 清理 HTML

#### Sidebar 規格

| 項目 | 規格 |
|------|------|
| 寬度 | 250px（lg 以上常駐；<lg 為 `offcanvas-start`，由 hamburger 開啟） |
| 背景 | `$eivo-bg-content`（白色），右邊框 `$eivo-border` |
| 選單來源 | `Views/SiteAction/*Menu.cshtml`（依 `UserProfile.CurrentSiteMenu` 動態載入，**機制保留**） |
| 結構 | BS5 巢狀 `nav` + `collapse`（二級選單摺疊）；或保留 metisMenu JS 但樣式對齊 BS5 |
| 選單項 | `<a class="nav-link">` + FA6 圖示（`me-2`）+ 文字；字體 14px |
| Active 態 | 背景 `$eivo-brand`，文字白色，圓角 `$eivo-radius` |
| Hover 態 | 背景 `$eivo-bg-hover` |
| 層級 | 最多 3 級（一級=功能群組、二級=功能、三級=子功能） |
| 縮排 | 一級 16px、二級 32px、三級 48px |

**遷移要點**：
- 移除 `App_Themes/Visitor/left-menu.css` 的 WebForms 時代樣式
- 選單 HTML 結構從 `<ul class="nav">` 改為 BS5 `nav` + `nav-link`
- 移除 `fa fa-angle-double-right` 等 FA4 圖示 → FA6

#### Master Tab 規格

保留現有 tab 機制（`$global.createTab/removeTab/showTab`），樣式改用 BS5 `nav-tabs`。

| 項目 | 規格 |
|------|------|
| 容器 | BS5 `nav nav-tabs`，高度 40px，border-bottom `$eivo-border` |
| Tab 項 | `nav-link`，active 態：文字 `$eivo-brand`、border-bottom 2px `$eivo-brand` |
| 關閉按鈕 | 每個 tab（除首頁）右側 `fa-xmark` 小圖示，`btn-close` 樣式 |
| 首頁 tab | 標題「首頁」，不可關閉，永遠存在 |
| Tab 上限 | 8 個；超過時自動關閉最舊的非首頁 tab |
| 標題長度 | ≤ 12 字，超過以 `text-truncate` + `title` 屬性 |
| 開啟時機 | 查詢結果（`createTab('queryResult', ...)`）、預覽、編輯 |
| URL 對應 | tab 開啟時 `history.pushState` 更新 fragment（如 `#tab=queryResult`）；重新整理恢復首頁 tab（完整狀態恢復列 P3） |

**遷移要點**：
- 移除 `nav nav-tabs` 的 BS3 樣式 → BS5
- `fa fa-times` → `fa-solid fa-xmark`
- `fa fa-angle-double-right` → `fa-solid fa-angle-right`

#### Footer 規格

| 項目 | 規格 |
|------|------|
| 結構 | 簡單 `<footer>` 元素，`text-muted small`，padding 12px 24px |
| 內容 | 「Powered by UXB2B」右對齊 |
| 禁止 | 使用 `<table>` 排版（現況 `<table id="footer">` 須移除） |

### 3.2 頁面類型分類

| 類型 | 模板 | 使用情境 | 現況參照 |
|------|------|----------|----------|
| 獨立頁 | `ContentPage.cshtml` | 登入、忘記密碼、公開查詢 | `Views/Account/Login.cshtml` |
| 主功能頁 | `MvcMainPage.cshtml` | 所有需 shell 的頁面 | `Views/Home/MainPage.cshtml` |
| 查詢頁 | `QueryIndex.cshtml` → `QueryActionTemplate.cshtml` | 查詢表單 + 結果表格 | `Views/AllowanceProcess/Index.cshtml` |
| 表單/編輯頁 | 主功能頁 + 表單元件 | 新增/編輯資料 | `Views/IndividualProcess/EditInvoiceBuyer.cshtml` |
| 對話框內容 | `Shared/Module/MessageDialog.cshtml` | AJAX 載入的彈窗內容 | `Views/Dialog/GetInputValue.cshtml` |

**新頁面規則**：一律從上述 5 類中選定模板，禁止自創 layout 結構。

### 3.3 登入頁規格（獨立頁）

| 項目 | 規格 |
|------|------|
| 布局 | 置中卡片（BS5 `card`，max-width 480px），背景 `$eivo-bg`，min-height 100vh flex 置中 |
| 標題 | 系統名稱 + 簡短描述，`h4` 置中 |
| 表單 | 帳號（`form-control`）、密碼（`form-control`）、驗證圖碼（`CaptchaImg.cshtml` 保留） |
| 按鈕 | `btn btn-primary w-100`（取代現況 GIF 按鈕 `login_button_up.gif`） |
| 公告 | 卡片下方 `alert alert-info`（取代 marquee） |
| 連結 | 忘記密碼、發票查詢、用戶端下載 — `btn btn-link` 置於卡片底部 |
| 禁止 | `<table>` 排版、GIF 按鈕、`<marquee>` |

---

## A.4 元件規格

> 每節格式：用途 / 結構 / 規則 / 現況參照 / 遷移要點。

### 4.1 按鈕 (Button)

**用途**：所有操作觸發。

**結構**（BS5）：

```html
<!-- 主要操作（每視圖最多 1 個 primary） -->
<button type="button" class="btn btn-primary">
  <i class="fa-solid fa-floppy-disk me-1" aria-hidden="true"></i>儲存
</button>

<!-- 次要 -->
<button type="button" class="btn btn-outline-secondary">取消</button>

<!-- 危險操作（刪除/作廢/註銷） -->
<button type="button" class="btn btn-outline-danger">
  <i class="fa-solid fa-trash-can me-1" aria-hidden="true"></i>刪除
</button>

<!-- 純圖示按鈕（表格列操作） -->
<button type="button" class="btn btn-sm btn-outline-secondary" title="編輯" aria-label="編輯">
  <i class="fa-solid fa-pen" aria-hidden="true"></i>
</button>

<!-- 查詢/搜尋 -->
<button type="button" class="btn btn-primary">
  <i class="fa-solid fa-magnifying-glass me-1" aria-hidden="true"></i>查詢
</button>
```

**規則**

| 規則 | 說明 |
|------|------|
| 數量限制 | 每視圖最多 1 個 `btn-primary`；其餘用 `outline-*` |
| 尺寸 | 預設 `btn`；表格列內 `btn-sm`；**禁止** `btn-lg`（表格密集系統） |
| 圖示位置 | 圖示在文字左側，間距 `me-1`；純圖示按鈕必須有 `title` + `aria-label` |
| Loading | `btn` 加 `disabled` + 內嵌 `<span class="spinner-border spinner-border-sm me-1"></span>`（見 A.5.1） |
| 禁止 | `<input type="button">` 裸用（現況靠 `addClass('btn')` 補救，新代碼直接寫 class） |
| 順序 | 表單底部：[主要] [次要] [危險]，右對齊 |
| 危險操作 | 必須搭配確認對話框（見 A.5.4） |

**現況參照**：`GlobalScript.cshtml` 的 `$('input[type="button"]').addClass('btn')`；`QueryPaging.cshtml` 的查詢按鈕。

### 4.2 表單 (Form)

**用途**：資料輸入、查詢條件。

#### 查詢表單（horizontal，標準）

```html
<form id="queryForm" class="row g-3 mb-3">
  <div class="col-md-3">
    <label for="invoiceNo" class="form-label">發票號碼</label>
    <input type="text" class="form-control" id="invoiceNo" name="InvoiceNo" />
  </div>
  <div class="col-md-3">
    <label for="issueDate" class="form-label">開立日期</label>
    <input type="text" class="form-control form_date" id="issueDate" name="IssueDate" />
  </div>
  <div class="col-md-3">
    <label for="sellerId" class="form-label">開立人</label>
    <select class="form-select" id="sellerId" name="SellerId">
      <option value="">全部</option>
    </select>
  </div>
  <div class="col-md-3 d-flex align-items-end">
    <button type="button" class="btn btn-primary me-2">
      <i class="fa-solid fa-magnifying-glass me-1" aria-hidden="true"></i>查詢
    </button>
    <button type="button" class="btn btn-outline-secondary">重設</button>
  </div>
</form>
```

#### 編輯表單（vertical，標準）

```html
<form id="editForm" class="row g-3">
  <div class="col-md-6">
    <label for="fieldName" class="form-label">
      欄位名稱 <span class="text-danger" aria-hidden="true">*</span>
    </label>
    <input type="text" class="form-control" id="fieldName" name="FieldName" required />
    <div class="invalid-feedback">請輸入欄位名稱</div>
  </div>
  <!-- 更多欄位... -->
  <div class="col-12 mt-4">
    <button type="button" class="btn btn-primary me-2">
      <i class="fa-solid fa-floppy-disk me-1" aria-hidden="true"></i>儲存
    </button>
    <button type="button" class="btn btn-outline-secondary">取消</button>
  </div>
</form>
```

**規則**

| 規則 | 說明 |
|------|------|
| Label | 每個輸入框必須有 `<label for="...">`；required 欄位加 `<span class="text-danger">*</span>` |
| 驗證錯誤 | BS5 `is-invalid` class + `<div class="invalid-feedback">`；取代現況 `label.error` 自訂 |
| 日期 | `form_date` class 保留（jQuery UI datepicker 初始化選擇器） |
| 下拉 | BS5 `form-select`（取代 BS3 `form-control` 用於 select） |
| 欄位寬度 | 查詢表單：`col-md-3`（4 欄）；編輯表單：`col-md-6`（2 欄）或 `col-12`（1 欄） |
| 禁止 | `<table>` 排版表單、inline style、無 label 的輸入框 |

**現況參照**：`Views/Account/Login.cshtml`（table 排版）、`QueryPaging.cshtml`（查詢表單）。

### 4.3 資料表格 (Data Table)

**用途**：查詢結果、列表資料。

**結構**（BS5 table + 自訂排序/分頁）：

```html
<div class="card">
  <div class="card-header d-flex justify-content-between align-items-center">
    <span class="fw-bold">查詢結果</span>
    <span class="text-muted small">共 @recordCount 筆</span>
  </div>
  <div class="table-responsive">
    <table class="table table-striped table-hover align-middle eivo-data-table" id="resultTable">
      <thead class="table-light">
        <tr>
          <th class="eivo-sortable" data-sort="0">
            發票號碼 <i class="fa-solid fa-sort ms-1" aria-hidden="true"></i>
          </th>
          <th class="eivo-sortable" data-sort="1">
            開立日期 <i class="fa-solid fa-sort ms-1" aria-hidden="true"></i>
          </th>
          <th>金額</th>
          <th class="text-end">操作</th>
        </tr>
      </thead>
      <tbody>
        <tr>
          <td>AB12345678</td>
          <td>2026-09-01</td>
          <td class="text-end">1,234.00</td>
          <td class="text-end">
            <button class="btn btn-sm btn-outline-secondary me-1" title="查看" aria-label="查看">
              <i class="fa-solid fa-eye" aria-hidden="true"></i>
            </button>
            <button class="btn btn-sm btn-outline-danger" title="作廢" aria-label="作廢">
              <i class="fa-solid fa-ban" aria-hidden="true"></i>
            </button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
  <!-- 空資料 -->
  <div class="text-center text-muted py-4 d-none" id="emptyState">
    <i class="fa-solid fa-inbox fa-2x mb-2 d-block" aria-hidden="true"></i>
    查無資料
  </div>
  <!-- 分頁 -->
  <div class="card-footer d-flex justify-content-between align-items-center">
    <div class="small text-muted">
      每頁：
      <select class="form-select form-select-sm d-inline-block" style="width:auto" id="pageSizeSelect">
        <option value="10">10</option>
        <option value="30">30</option>
        <option value="50">50</option>
        <option value="100">100</option>
      </select>
    </div>
    <nav aria-label="表格分頁">
      <ul class="pagination pagination-sm mb-0" id="tablePagination"></ul>
    </nav>
  </div>
</div>
```

**規則**

| 規則 | 說明 |
|------|------|
| 容器 | 必須包在 `card` 內；`table-responsive` 包裹 table（橫向捲動） |
| 表頭 | `table-light` 背景；可排序欄位加 `eivo-sortable` class + `fa-sort` 圖示 |
| 排序 | 點擊表頭循環：無 → asc（`fa-sort-up`）→ desc（`fa-sort-down`）→ 無；`aria-sort` 屬性同步 |
| 斑马紋 | `table-striped`（BS5 內建，背景色由令牌 `$eivo-bg-stripe` 覆寫） |
| Hover | `table-hover` |
| 數字對齊 | 金額/數字欄位 `text-end` |
| 操作欄 | 最右欄 `text-end`，`btn-sm` 圖示按鈕，間距 `me-1` |
| 空資料 | `emptyState` div，`d-none` 預設隱藏，無資料時顯示 |
| 分頁 | BS5 `pagination`，置於 `card-footer`；每頁筆數 select 在左 |
| 載入中 | table 上加 `eivo-loading` class → CSS 顯示 spinner overlay（見 A.5.1） |
| 列選取 | 需要時第一欄加 `<input type="checkbox" class="form-check-input">`；表頭全選 checkbox |
| 禁止 | jsGrid、DataTables、rwd-table（新頁面）；`<table border="1">` |

**現況參照**：`Common/Module/TableBody.cshtml`、`SortableHeader.cshtml`、`QueryPaging.cshtml`、`buildSort/initSortable`（`CommonScriptInclude.cshtml`）。

### 4.4 對話框 (Modal)

**用途**：訊息提示、確認操作、表單編輯、內容預覽。

**統一使用 BS5 Modal**（取代 jQuery UI dialog + BS modal 混用）。

#### 四類對話框

| 類型 | 尺寸 | 用途 | 按鈕 |
|------|------|------|------|
| 訊息 (Message) | `modal-sm` (320px) | 成功/錯誤/資訊提示 | [確定] |
| 確認 (Confirm) | `modal-sm` (320px) | 危險操作確認 | [取消] [確認] |
| 表單 (Form) | `modal-lg` (800px) | 新增/編輯資料 | [取消] [儲存] |
| 預覽 (Preview) | `modal-xl` (1140px) | 發票預覽、報表預覽 | [關閉] [列印] |

**結構**：

```html
<!-- 確認對話框（標準） -->
<div class="modal fade" id="confirmModal" tabindex="-1" aria-labelledby="confirmModalLabel" aria-hidden="true">
  <div class="modal-dialog modal-sm">
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title" id="confirmModalLabel">
          <i class="fa-solid fa-triangle-exclamation me-2 text-warning" aria-hidden="true"></i>確認操作
        </h5>
        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="關閉"></button>
      </div>
      <div class="modal-body">
        <p id="confirmMessage">確定要執行此操作嗎？</p>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">取消</button>
        <button type="button" class="btn btn-danger" id="confirmOkBtn">
          <i class="fa-solid fa-check me-1" aria-hidden="true"></i>確認
        </button>
      </div>
    </div>
  </div>
</div>
```

**JS 初始化**（`wwwroot/ts/modal.ts`）：

```typescript
// 全域對話框管理
export const EivoModal = {
  // 訊息對話框
  message(title: string, body: string, type: 'success' | 'danger' | 'warning' | 'info' = 'info'): void {
    // 設定 #messageModal 的 title/body/icon 後 bootstrap.Modal.getOrCreateInstance('#messageModal').show()
  },
  // 確認對話框（回傳 Promise<boolean>）
  confirm(message: string, title = '確認操作'): Promise<boolean> {
    // 設定 #confirmModal 後 show，resolve(true/false)
  },
  // 載入遠端內容到對話框
  load(url: string, modalId: string, width?: string): void {
    // $.postJSON(url) → 填入 modal body → show
  },
};
```

**規則**

| 規則 | 說明 |
|------|------|
| 統一 BS5 Modal | 新頁面禁止使用 jQuery UI `$(html).dialog()`；現況 `alertModal`、`actionHandler` 逐步遷移 |
| 標題圖示 | 訊息型：`fa-circle-check`(success) / `fa-circle-xmark`(danger) / `fa-triangle-exclamation`(warning) / `fa-circle-info`(info) |
| 焦點管理 | Modal 開啟時焦點移至第一個可操作元素；關閉時返回觸發元素（BS5 內建） |
| ESC 關閉 | 預設允許；表單型 modal 若有未儲存變更需攔截（見 A.5.5） |
| 背景點擊 | 訊息/確認型：允許關閉；表單型：`backdrop: 'static'` 禁止 |
| 巢狀 | 禁止巢狀 modal（如需預覽中再編輯，改用 tab 或新頁面） |
| 禁止 | jQuery UI dialog、`window.alert()`、`window.confirm()` |

**現況參照**：`alertModal`（`CommonScriptInclude.cshtml`）、`actionHandler`（`GlobalScript.cshtml`）、`Shared/Module/MessageDialog.cshtml`。

### 4.5 通知 (Notification)

**用途**：操作結果反饋、系統公告。

#### Toast（操作反饋，右上角）

```html
<!-- 容器（body 末尾，shell 中統一放置） -->
<div class="position-fixed top-0 end-0 p-3" style="z-index: 9999" id="toastContainer"></div>
```

```typescript
// wwwroot/ts/toast.ts
export function showToast(message: string, type: 'success' | 'danger' | 'warning' | 'info' = 'info', duration = 3000): void {
  const iconMap = { success: 'fa-circle-check', danger: 'fa-circle-xmark', warning: 'fa-triangle-exclamation', info: 'fa-circle-info' };
  const $toast = $(`
    <div class="toast align-items-center text-bg-${type} border-0" role="alert" aria-live="assertive" aria-atomic="true">
      <div class="d-flex">
        <div class="toast-body">
          <i class="fa-solid ${iconMap[type]} me-2" aria-hidden="true"></i>${message}
        </div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="關閉"></button>
      </div>
    </div>
  `);
  $('#toastContainer').append($toast);
  const toast = new bootstrap.Toast($toast[0], { delay: duration });
  toast.show();
  $toast.on('hidden.bs.toast', () => $toast.remove());
}
```

**規則**

| 規則 | 說明 |
|------|------|
| 使用時機 | 操作成功（儲存/刪除/提交）、非阻塞性警告 |
| 位置 | 右上角，堆疊，自動消失（3 秒） |
| 禁止 | `window.alert()` 用於操作成功反饋（alert 僅用於不可忽略的錯誤） |
| 錯誤 | 表單驗證錯誤用 inline `invalid-feedback`；API 錯誤用 toast danger + 詳細訊息 |

#### 系統公告（取代 marquee）

| 項目 | 規格 |
|------|------|
| 位置 | Topbar 內（品牌右側）或內容區頂部 |
| 結構 | BS5 `alert alert-info alert-dismissible`，靜態顯示 |
| 多則 | 顯示最新一則 +「更多公告 (n)」連結 |
| 關閉 | 可關閉（session 內不再顯示，`sessionStorage` 記錄） |
| 禁止 | `<marquee>`、`<font color>` |

### 4.6 分頁 (Pagination)

**用途**：表格分頁導航。

**結構**（BS5 pagination）：

```html
<nav aria-label="表格分頁">
  <ul class="pagination pagination-sm" id="tablePagination">
    <li class="page-item disabled"><a class="page-link" href="#" tabindex="-1">首頁</a></li>
    <li class="page-item disabled"><a class="page-link" href="#" tabindex="-1">上一頁</a></li>
    <li class="page-item active"><a class="page-link" href="#">1</a></li>
    <li class="page-item"><a class="page-link" href="#">2</a></li>
    <li class="page-item"><a class="page-link" href="#">3</a></li>
    <li class="page-item"><a class="page-link" href="#">下一頁</a></li>
    <li class="page-item"><a class="page-link" href="#">末頁</a></li>
  </ul>
</nav>
```

**規則**

| 規則 | 說明 |
|------|------|
| 樣式 | BS5 `pagination`，品牌色 active 態（`--bs-pagination-active-bg: $eivo-brand`） |
| 文字 | 首頁/上一頁/下一頁/末頁（繁體中文，現況 twbsPagination 格式保留） |
| 頁碼上限 | 顯示最多 7 個頁碼 + 省略號 |
| 總筆數 | 分頁旁顯示「共 N 筆」（`text-muted small`） |
| 每頁筆數 | select 置於分頁左側（見 4.3 表格結構） |
| 禁止 | twbsPagination 自訂樣式（新頁面）；JS 動態產生時 class 須對齊 BS5 |

**現況參照**：`QueryPaging.cshtml`、`jquery.twbsPagination.js`。

### 4.7 標籤 / 徽章 / 狀態 (Badge / Chip)

**用途**：狀態標示、分類標籤。

```html
<!-- 狀態 badge -->
<span class="badge text-bg-success">已完成</span>
<span class="badge text-bg-warning">待處理</span>
<span class="badge text-bg-danger">已作廢</span>
<span class="badge text-bg-secondary">已停用</span>
<span class="badge text-bg-info">進行中</span>

<!-- 分類 chip（可點擊） -->
<span class="badge rounded-pill text-bg-light border eivo-chip">A0101</span>
```

**規則**

| 規則 | 說明 |
|------|------|
| 狀態色映射 | 成功/啟用=success、待處理/警告=warning、錯誤/停用/作廢=danger、進行中=info、中性=secondary |
| 字體 | 12px（`$eivo-font-size-sm`） |
| 禁止 | `<font color>`、inline style 設色 |

### 4.8 檔案上傳 (File Upload)

**用途**：附件上傳、匯入。

**結構**：

```html
<div class="mb-3">
  <label for="fileInput" class="form-label">附件</label>
  <input class="form-control" type="file" id="fileInput" name="Attachment"
         accept=".pdf,.xlsx,.xls,.zip" />
  <div class="form-text">支援 PDF、Excel、ZIP，最大 10MB</div>
</div>
```

**規則**

| 規則 | 說明 |
|------|------|
| 元件 | BS5 `form-control` type="file"（不用自訂 dropzone，過渡期保持簡單） |
| 驗證 | 前端 `accept` + 大小檢查；後端二次驗證 |
| 上傳中 | 按鈕 loading 狀態 + 禁止重複提交 |
| 多檔 | 需要時加 `multiple`；顯示已選檔名列表 |
| 現況 | `uploadFile`（`CommonScriptInclude.cshtml`，ajaxForm）保留，樣式對齊 BS5 |

### 4.9 列印 (Print)

**用途**：發票預覽列印、報表列印。

**規格**

| 項目 | 規格 |
|------|------|
| 機制 | 保留現況 iframe 列印（`$global.printContent`），但標準化觸發方式 |
| 觸發 | 按鈕 `btn btn-outline-secondary` + `fa-print` 圖示 |
| 預覽 | 先開啟預覽 modal（4.4 Preview 型），modal 內有 [列印] 按鈕 |
| 列印區域 | 目標元素加 `eivo-print-area` class；CSS `@media print` 隱藏其他元素 |
| 禁止 | 直接 `window.print()` 無預覽（除快速列印場景） |

**CSS**（`_components.scss`）：

```scss
@media print {
  body * { visibility: hidden; }
  .eivo-print-area, .eivo-print-area * { visibility: visible; }
  .eivo-print-area { position: absolute; left: 0; top: 0; width: 100%; }
}
```

---

## A.5 互動與狀態規格

### 5.1 Loading 狀態

**標準**：取代 `$.blockUI`（jquery.blockUI）為 BS5 spinner + overlay。

```scss
// _components.scss
.eivo-loading {
  position: relative;
  &::after {
    content: "";
    position: absolute;
    inset: 0;
    background: rgba(255,255,255,.75);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10;
  }
  &::before {
    content: "";
    position: absolute;
    inset: 0;
    z-index: 11;
    width: 2rem;
    height: 2rem;
    margin: auto;
    border: 3px solid $eivo-border;
    border-top-color: $eivo-brand;
    border-radius: 50%;
    animation: eivo-spin .75s linear infinite;
  }
}
@keyframes eivo-spin { to { transform: rotate(360deg); } }
```

**規則**

| 情境 | 方式 |
|------|------|
| 表格載入 | table 容器加 `eivo-loading` class |
| 按鈕提交 | 按鈕 `disabled` + `spinner-border spinner-border-sm` |
| 全頁載入（AJAX 頁面） | 內容區加 `eivo-loading`（過渡期保留 blockUI 亦可，新頁面用 spinner） |
| 對話框載入 | modal body 加 `eivo-loading` |
| 自動隱藏 | 超過 10 秒未回傳 → toast warning「載入時間過長」 |

### 5.2 錯誤處理

**統一錯誤回傳格式**（後端 `$.postJSON` 回傳）：

```json
{ "message": "錯誤描述文字" }
```

**前端處理規則**

| 錯誤類型 | 顯示方式 |
|----------|----------|
| 表單驗證錯誤 | inline `invalid-feedback`（欄位旁）+ 表單頂部 `alert alert-danger` 摘要 |
| API 業務錯誤 | toast danger（3 秒）+ 若需詳細資訊則 modal message |
| 網路錯誤 / 500 | toast danger「系統錯誤，請稍後重試」+ console.error |
| 401 未授權 | 重定向至登入頁（現況 Cookie Auth 機制保留） |
| 403 無權限 | toast warning「您沒有權限執行此操作」 |

**禁止**：`alert(data.message)` 裸用（現況大量使用，逐步遷移至 toast/modal）。

### 5.3 成功訊息

| 情境 | 方式 |
|------|------|
| 儲存/提交成功 | toast success「儲存成功」+ 關閉 modal / 刷新表格 |
| 刪除成功 | toast success「刪除成功」+ 刷新表格 |
| 匯出成功 | toast success「匯出完成」+ 觸發下載 |
| 批量操作 | toast success「成功處理 N 筆」+ 若有失敗附「M 筆失敗」 |

### 5.4 確認對話

**必須使用確認對話框的情境**：
- 刪除（單筆/批量）
- 作廢發票
- 註銷發票
- 停用組織/使用者
- 任何不可逆操作

**格式**：`EivoModal.confirm('確定要作廢發票 AB12345678 嗎？此操作不可復原。', '確認作廢')`

### 5.5 未儲存變更警示

| 情境 | 行為 |
|------|------|
| 表單 modal 有變更時按 ESC / 背景 / 關閉 | 攔截 → `EivoModal.confirm('有未儲存的變更，確定要關閉嗎？')` |
| 表單有變更時離開頁面（beforeunload） | `e.preventDefault()` + 瀏覽器原生確認 |
| 表單有變更時切換 tab | 不攔截（tab 內容保留），但 tab 標題加 `*` 標記 |

**實作**：表單 `input/change` 事件設 `dirty` flag；`hidden` 事件檢查。

---

## A.6 無障礙 (a11y) 規格

### 鍵盤導航

| 元素 | 鍵盤操作 |
|------|----------|
| 選單 | Tab 進入、Arrow Up/Down 移動、Enter 開啟、Escape 關閉子選單 |
| 表格 | Tab 進入操作按鈕；排序表頭可 Tab 聚焦 + Enter 觸發 |
| Modal | Tab 循環（BS5 內建）、Escape 關閉 |
| Toast | 不需聚焦（aria-live="assertive" 自動讀取） |
| 分頁 | Tab 進入、Arrow Left/Right 移動 |

### ARIA 屬性

| 元素 | 必要 ARIA |
|------|-----------|
| 排序表頭 | `aria-sort="none|ascending|descending"` |
| 圖示按鈕 | `aria-label="操作名稱"` + 圖示 `aria-hidden="true"` |
| Modal | `aria-labelledby` + `aria-hidden`（BS5 內建） |
| Toast | `role="alert"` + `aria-live="assertive"` |
| 分頁 | `aria-label="表格分頁"` + `aria-current="page"`（active 項） |
| 表格 | `scope="col"`（th）、`caption`（sr-only 描述） |
| 選單 | `aria-expanded`（可展開項）、`aria-haspopup` |

### 對比度

| 組合 | 最低對比度 |
|------|-----------|
| 一般文字 / 背景 | 4.5:1 (WCAG AA) |
| 大字（≥18px 或 14px bold） | 3:1 |
| 品牌橘 `#c99040` / 白色文字 | 3.2:1（僅用於大字/圖示背景） |
| 白色文字 / 品牌橘背景 | 同上 |

### 螢幕閱讀器

- 所有互動元素有可讀 label（`aria-label` 或 `<label>`）。
- 表格有 `caption`（可 `sr-only`）。
- 動態內容更新（表格刷新）用 `aria-live="polite"` 區域通知。
- 圖片有 `alt`（裝飾圖 `alt=""`）。

---

## A.7 響應式規格

### 斷點行為

| 斷點 | Shell | 表單 | 表格 |
|------|------|------|------|
| <lg (<992px) | Sidebar 收合為 offcanvas（hamburger 開啟） | 單欄（col-12） | 橫向捲動（table-responsive） |
| lg–xl | Sidebar 常駐 250px | 查詢 4 欄 / 編輯 2 欄 | 完整顯示 |
| ≥xl | Sidebar 常駐 | 查詢 4 欄 / 編輯 2 欄 | 完整顯示 + 留白 |

### 規則

| 規則 | 說明 |
|------|------|
| 桌面優先 | 本系統主要使用情境為辦公桌面（≥lg）；mobile 為可用級別，非完美級別 |
| 表格 | 一律 `table-responsive` 包裹；欄位過多時橫向捲動，不壓縮欄寬 |
| 選單 | <lg 時 sidebar 為 `offcanvas-start`；開啟時背景遮罩；選單項點擊後自動關閉 offcanvas |
| 表單 | 查詢表單 <md 時單欄堆疊；編輯表單 <md 時單欄 |
| 對話框 | modal 在 <sm 時 `modal-fullscreen`（BS5 內建） |
| 禁止 | 固定 px 寬度（除 sidebar 250px、modal 尺寸）；使用 `col-*` 響應式欄位 |

---

## A.8 命名與 class 慣例

### Class 命名

| 規則 | 說明 |
|------|------|
| 自訂 class 前綴 | `eivo-`（如 `eivo-data-table`、`eivo-loading`、`eivo-print-area`、`eivo-chip`） |
| BS5 class 優先 | 能用 BS5 utility / 元件 class 解決的，不自訂 class |
| 禁止 inline style | 新代碼禁止 `style="..."`（現況大量存在，逐步清除）；例外：動態寬度（JS 計算） |
| 禁止已棄用 HTML | `<font>`、`<marquee>`、`<center>`、`<table border="1">`（非資料表格） |
| 語意化 | 結構用語意標籤（`nav`、`main`、`footer`、`section`），非全 `div` |

### 自訂 Class 清單（標準化）

| Class | 用途 | 定義位置 |
|-------|------|----------|
| `eivo-data-table` | 資料表格（斑马紋色覆寫） | `_components.scss` |
| `eivo-sortable` | 可排序表頭 | `_components.scss` |
| `eivo-loading` | Loading overlay | `_components.scss` |
| `eivo-print-area` | 列印區域 | `_components.scss` |
| `eivo-chip` | 分類 chip | `_components.scss` |
| `form_date` | 日期選擇器初始化選擇器（保留現況） | JS 選擇器 |
| `eivo-sidebar` | Sidebar 容器 | `_layout.scss` |
| `eivo-topbar` | Topbar 容器 | `_layout.scss` |

### JS 命名

| 規則 | 說明 |
|------|------|
| 全域物件 | `$global`（保留現況）、`$inquiryAgent`（保留現況）、`EivoModal`、`EivoToast`（新增） |
| 函式 | camelCase（`showLoading`、`createTab`） |
| 常量 | UPPER_SNAKE（`MAX_TAB_COUNT`） |
| 禁止 | 全域變數裸掛 `window`（除上述標準化物件）；inline `<script>` 邏輯（應移至 `wwwroot/ts/`） |

---

## A.9 資產與建置規格

### 資產目錄結構（目標）

```
WebHome/wwwroot/
├── css/
│   └── site.css              ← SCSS 編譯產物（fingerprinted）
├── scss/
│   ├── _tokens.scss
│   ├── _bootstrap.scss
│   ├── _layout.scss
│   ├── _components.scss
│   ├── _utilities.scss
│   └── site.scss             ← 主入口
├── js/
│   └── main.js               ← TS 編譯產物（fingerprinted）
├── ts/
│   ├── index.ts              ← 入口
│   ├── modal.ts              ← EivoModal
│   ├── toast.ts              ← EivoToast / showToast
│   ├── table.ts              ← 表格排序/分頁
│   ├── form.ts               ← 表單驗證/dirty tracking
│   └── inquiry-agent.ts      ← $inquiryAgent 標準化
├── vendor/
│   ├── bootstrap/            ← BS5.3（bootstrap.bundle.min.js + css 由 SCSS 編譯）
│   ├── jquery/               ← jQuery 3.7.x
│   ├── jquery-ui/            ← jQuery UI 1.13.x（CSS+JS 同版本）
│   ├── fontawesome/          ← FA6 Free（css + webfonts）
│   └── jquery-plugins/       ← jquery.form、jquery.blockUI（過渡期）
└── images/
    ├── logo.png
    ├── loading.gif           ← 過渡期保留
    └── ...
```

### 建置管線

| 項目 | 規格 |
|------|------|
| SCSS 編譯 | esbuild（`package.json` 已有）或 `sass` CLI；產物 `wwwroot/css/site.css` |
| TS 編譯 | esbuild bundle（`package.json` 已有 `build:es` script）；產物 `wwwroot/js/main.js` |
| Fingerprinting | ASP.NET Core Static Web Assets 內建（`Cache-Control: max-age=31536000, immutable`） |
| 開發 | `npm run watch:es`（TS）+ `sass --watch`（SCSS）；或整合為單一 watch script |
| MSBuild 整合 | `WebHome.csproj` 的 `BuildTypeScript` target（`-p:TypeScriptBuild=true`）保留 |
| CDN | **禁止**（現況 FA 4.2.0 走 maxcdn CDN → 自架） |
| 外部 webroot | **目標移除**：資產搬入 `WebHome/wwwroot` 後，`ASPNETCORE_WEBROOT` 環境變數不再需要 |

### 資產搬遷策略（P0）

1. 從 `C:\Project\Github\IFS-EIVO03\eIVOGo\` 複製必要資產至 `WebHome/wwwroot/`。
2. 升級：BS3/4 → BS5.3、FA4 → FA6、jQuery UI CSS 對齊 JS 版本。
3. 移除：不再使用的資產（jsGrid、DataTables、rwd-table、sb-admin-2、App_Themes/Visitor/*）。
4. 驗證：所有頁面正常載入、無 404。
5. 移除 `ASPNETCORE_WEBROOT` 環境變數（`RunWebHome.bat`、`.vscode/launch.json`）。

---

## A.10 Razor 端慣例

### Partial 使用規則

| 規則 | 說明 |
|------|------|
| 共用 partial 位置 | `Views/Shared/`（全域）、`Views/Common/Module/`（查詢/表格相關）、`Views/{Feature}/Module/`（功能特定） |
| 呼叫方式 | `await Html.RenderPartialAsync("~/Views/...")` 或 `@Html.PartialAsync(...)` |
| 禁止 | 跨功能資料夾引用 partial（如 InvoiceProcess 引用 AllowanceProcess 的 partial） |
| 新 partial | 須有明確用途註解（首行 `@* 用途：... *@`） |

### `@section` 使用

| Section | 用途 | 定義於 |
|---------|------|--------|
| `headContent` | 頁面特定 CSS/JS | Template 各模板 |
| `formContent` | 表單內容（shell 的 `<form>` 內） | `MvcMainPage.cshtml` |
| `QueryForm` | 查詢表單 | `QueryIndex.cshtml` |
| `PrepareAgent` | `$inquiryAgent` 初始化 | `InquiryAgentActionTemplate.cshtml` |

### `ViewBag.ViewModel` 模式

```csharp
// Controller
ViewBag.ViewModel = new QueryViewModel { ... };
ViewBag.ModelState = this.ModelState;
return View("~/Views/Feature/Action.cshtml");
```

```cshtml
@* View *@
@{
    QueryViewModel _viewModel = (QueryViewModel)ViewBag.ViewModel;
    ModelStateDictionary _modelState = (ModelStateDictionary)ViewBag.ModelState;
}
```

**規則**：
- ViewModel 型別放 `WebHome/Models/ViewModel/` 或 `ModelCore/Models/ViewModel/`。
- 禁止在 View 中直接存取 `HttpContext.Items["Models"]`（應透過 `ViewBag` 或 `@Model`）。
- 現況 `ModelSource<InvoiceItem> models = (ModelSource<InvoiceItem>)ViewContext.HttpContext.Items["Models"]!` 模式保留（過渡期），新頁面優先 `@Model`。

### `$inquiryAgent` 查詢模式標準化

```cshtml
@* 查詢頁標準結構（QueryIndex 型） *@
@section QueryForm {
    <form id="queryArea" class="row g-3 mb-3">
        <!-- 查詢欄位 -->
    </form>
}

@section PrepareAgent {
    <script>
        $inquiryAgent.viewModel = @Html.Raw(_viewModel.JsonStringify());
        $inquiryAgent.inquiryAction = '@Html.Raw(Url.Action("QueryResult", "FeatureController"))';
        $inquiryAgent.loadAction = '@Html.Raw(Url.Action("LoadItem", "FeatureController"))';
        $inquiryAgent.editAction = '@Html.Raw(Url.Action("EditItem", "FeatureController"))';
        $inquiryAgent.commitAction = '@Html.Raw(Url.Action("CommitItem", "FeatureController"))';
    </script>
}
```

**規則**：
- 查詢頁一律使用 `QueryIndex.cshtml` 模板 + `$inquiryAgent` 模式。
- `inquiryAction` 回傳 HTML partial（表格 tbody 內容）或 JSON（`{message: "..."}` 表示錯誤）。
- 結果顯示於 master tab（`createTab`）。

### `[FromJsonOrForm]` API 呼叫模式

```csharp
// Controller Action
[HttpPost]
public ActionResult DoSomething([FromJsonOrForm] MyViewModel viewModel)
{
    if (!ModelState.IsValid)
        return Json(new { message = ModelState.ErrorMessage() });
    // 業務邏輯...
    return Json(new { result = true });
}
```

```javascript
// 前端呼叫
$.postJSON('@Url.Action("DoSomething", "Feature")', { field1: val1 }, function (data) {
    if ($.isPlainObject(data)) {
        if (data.result) {
            showToast('操作成功', 'success');
        } else {
            showToast(data.message, 'danger');
        }
    } else {
        // HTML 回傳（partial）
        $(data).appendTo('#target');
    }
});
```

**規則**：
- 所有 AJAX Action 使用 `[FromJsonOrForm]`（全域 using 已設於 csproj）。
- 回傳格式：成功 `{ result: true }` 或 HTML partial；錯誤 `{ message: "..." }`。
- 前端統一用 `$.postJSON`（POST + JSON），禁止裸 `$.ajax`。

---

# Part B — UI 優化路線圖

> 依優先級排列，供後續執行階段參考。每項標註：影響範圍、風險、驗證方式。

## P0 — 基礎建設（前置條件）

| # | 工作項 | 影響範圍 | 風險 | 驗證方式 |
|---|--------|----------|------|----------|
| P0.1 | 資產搬入 repo：從 IFS-EIVO03 複製 Scripts/Content/css/images 至 `WebHome/wwwroot/` | `wwwroot/`、`RunWebHome.bat`、`.vscode/launch.json` | 中（路徑引用需逐一確認） | 所有頁面正常載入、無 404 |
| P0.2 | 建立 SCSS 管線：`_tokens.scss` + `site.scss` + esbuild/sass 編譯 | `wwwroot/scss/`、`package.json` | 低 | `npm run build:es` 產出 `css/site.css` |
| P0.3 | BS5.3 升級：引入 BS5.3 SCSS source、建立 `_bootstrap.scss` 變數覆寫 | `wwwroot/scss/`、所有引用 BS class 的 `.cshtml` | **高**（BS3→5 大量 class 變更） | 逐頁視覺比對；見附錄 A 對照表 |
| P0.4 | FA6 自架：下載 FA6 Free 至 `wwwroot/vendor/fontawesome/`、移除 CDN link | `MvcMainPage.cshtml`、`ContentPage.cshtml`、所有引用 FA 的 view | 低 | 圖示正常顯示、無 CDN 請求 |
| P0.5 | jQuery UI 版本對齊：CSS 升級至 1.13.x（與 JS 一致） | `wwwroot/vendor/jquery-ui/` | 低 | datepicker 正常顯示 |
| P0.6 | 移除 `ASPNETCORE_WEBROOT` 環境變數 | `RunWebHome.bat`、`.vscode/launch.json` | 低 | 啟動後資產正常 |

**P0 完成標準**：系統以 BS5.3 + FA6 + 自架資產正常運行，功能無回歸。

## P1 — 核心元件標準化

| # | 工作項 | 影響範圍 | 風險 | 驗證方式 |
|---|--------|----------|------|----------|
| P1.1 | 標準 Shell 重寫：`MvcMainPage.cshtml` 改用 BS5 navbar/offcanvas/nav-tabs | `Views/Template/MvcMainPage.cshtml`、`_layout.scss` | **高**（影響所有頁面） | 所有角色選單正常、tab 功能正常 |
| P1.2 | 表格標準化：`TableBody`/`SortableHeader`/`QueryPaging` partial 改用 BS5 table + 自訂排序 JS | `Views/Common/Module/`、`_components.scss`、`ts/table.ts` | 中 | 查詢頁排序/分頁/斑马紋正常 |
| P1.3 | 對話框標準化：建立 `EivoModal`（BS5 Modal）、遷移 `alertModal`/`actionHandler` | `ts/modal.ts`、`CommonScriptInclude.cshtml`、`GlobalScript.cshtml` | 中 | 所有對話框場景正常 |
| P1.4 | 表單驗證標準化：BS5 `is-invalid` + `invalid-feedback`、遷移 `validateForm`/`clearErrors` | `ts/form.ts`、`CommonScriptInclude.cshtml` | 低 | 表單驗證錯誤顯示正常 |
| P1.5 | 登入頁重寫：BS5 card 布局、移除 table/GIF/marquee | `Views/Account/Login.cshtml`、`App_Themes/Login/` | 低 | 登入流程正常 |

**P1 完成標準**：核心元件（shell/表格/對話框/表單）全站統一為 BS5 標準。

## P2 — 一致性清理

| # | 工作項 | 影響範圍 | 風險 | 驗證方式 |
|---|--------|----------|------|----------|
| P2.1 | 圖示全面 FA6：批次替換 FA4 class → FA6 class（見附錄 B） | 所有 `.cshtml` | 低（純替換） | 無 `fa fa-*` 殘留（grep 驗證） |
| P2.2 | 通知標準化：建立 `EivoToast`、遷移 `alert()` 呼叫 | `ts/toast.ts`、所有 `alert(...)` 呼叫處 | 中 | 操作反饋正常 |
| P2.3 | Loading 標準化：`eivo-loading` class 取代 blockUI（新頁面） | `_components.scss`、新頁面 | 低 | Loading 顯示正常 |
| P2.4 | 移除遺留 HTML：`<marquee>`→alert、`<font color>`→text utility、inline style 清除 | 所有 `.cshtml` | 低 | grep 無 `<marquee`/`<font ` 殘留 |
| P2.5 | 系統公告標準化：Topbar alert 取代 marquee、`SystemMessage` 顯示邏輯 | `MvcMainPage.cshtml`、`Login.cshtml` | 低 | 公告正常顯示/關閉 |
| P2.6 | Footer 標準化：移除 `<table id="footer">` → `<footer>` | `MvcMainPage.cshtml` | 低 | Footer 正常顯示 |

**P2 完成標準**：全站無 FA4 圖示、無 `<marquee>`/`<font>`、操作反饋統一為 toast。

## P3 — 品質提升

| # | 工作項 | 影響範圍 | 風險 | 驗證方式 |
|---|--------|----------|------|----------|
| P3.1 | a11y 補強：aria 屬性、鍵盤導航、focus 管理 | 所有元件 | 中 | 鍵盤走查 + screen reader 測試 |
| P3.2 | 響應式補強：offcanvas sidebar、modal fullscreen、表格捲動 | `_layout.scss`、`_components.scss` | 低 | 各斷點視覺測試 |
| P3.3 | TS 化：`wwwroot/ts/` 取代 inline script（`$global`、`$inquiryAgent` 標準化） | `wwwroot/ts/`、`CommonScriptInclude.cshtml`、`GlobalScript.cshtml` | **高**（核心 JS 重構） | 所有 AJAX 功能正常 |
| P3.4 | E2E 可測性：`data-testid` 慣例、關鍵流程 Playwright 測試 | 所有 `.cshtml`、`E2E/` | 低 | E2E 測試通過 |
| P3.5 | Tab 狀態恢復：`sessionStorage` 保存開啟 tab、重新整理恢復 | `ts/tab.ts`、`MvcMainPage.cshtml` | 低 | 重新整理後 tab 恢復 |
| P3.6 | 效能：CSS/JS minify + gzip、圖片優化、lazy load | `wwwroot/`、`Startup.cs` | 低 | Lighthouse 評分 |

**P3 完成標準**：a11y 達 WCAG AA、TS 化完成、E2E 覆蓋主要流程。

## 執行時程建議

```
階段      週期      前置依賴
─────────────────────────────────────
P0 基礎    1-2 週    無
P1 核心    2-3 週    P0 完成
P2 一致性  1-2 週    P1 完成
P3 品質    2-3 週    P1 完成（可與 P2 並行）
```

## 與 Vue 遷移的對齊

| 項目 | WebHome（過渡期） | Vue 3（目標） | 對齊策略 |
|------|-------------------|---------------|----------|
| UI 框架 | Bootstrap 5.3 | Vuetify 3（見 `vue.instructions.md`） | 元件結構語意化，降低遷移改寫量 |
| 狀態管理 | jQuery + 全域變數 | Pinia | TS 化時以 composable 模式封裝 |
| 路由 | MVC Controller/Action | Vue Router（file-based） | 路由路徑命名對齊 |
| API 呼叫 | `$.postJSON` | Axios（`src/api/http.ts`） | 回傳格式統一 `{result, message, data}` |
| 權限 | Cookie + RoleAuthorizeAttribute | JWT + Policy-based | Claims 結構對齊 |

> **注意**：WebHome 的 UI 優化投入應以「維持穩定 + 降低遷移成本」為目標，避免過度投資於即將被 Vue 3 取代的頁面。優先優化高頻使用頁面（登入、發票查詢、發票開立）。

---

# 附錄 A — BS3/4 → BS5 Class 對照表

> 完整對照參考 [Bootstrap 5 Migration Guide](https://getbootstrap.com/docs/5.3/migration/)。以下列出本系統常用變更。

## 按鈕

| BS3/4 | BS5 | 備註 |
|--------|-----|------|
| `btn-primary` | `btn btn-primary` | 需加 `btn` 基礎 class |
| `btn-default` | `btn btn-secondary` | 改名 |
| `btn-info` | `btn btn-info` | 不變 |
| `btn-link` | `btn btn-link` | 不變 |
| `btn-block` | `w-100` | 改用 utility |
| `btn-xs` | `btn-sm` | 移除 xs |
| `disabled` (btn) | `disabled` | 不變 |

## 表單

| BS3/4 | BS5 | 備註 |
|--------|-----|------|
| `form-control` (input) | `form-control` | 不變 |
| `form-control` (select) | `form-select` | **select 改用 form-select** |
| `form-inline` | `row g-2` + `col-auto` | 結構變更 |
| `has-error` | `is-invalid` | 驗證 class 改名 |
| `help-block` | `invalid-feedback` / `form-text` | 改名 |
| `input-group` | `input-group` | 不變 |
| `checkbox` / `radio` | `form-check` + `form-check-input` | **結構變更** |
| `form-horizontal` | `row` + `col` + `form-label` | 結構變更 |

## 表格

| BS3/4 | BS5 | 備註 |
|--------|-----|------|
| `table` | `table` | 不變 |
| `table-striped` | `table-striped` | 不變 |
| `table-bordered` | `table-bordered` | 不變 |
| `table-hover` | `table-hover` | 不變 |
| `table-condensed` | `table-sm` | 改名 |
| `active` (tr) | `table-active` | 改名 |
| `success` (tr) | `table-success` | 改名 |
| `warning` (tr) | `table-warning` | 改名 |
| `danger` (tr) | `table-danger` | 改名 |
| `info` (tr) | `table-info` | 改名 |
| `table-responsive` | `table-responsive` | 不變 |

## 對話框 (Modal)

| BS3/4 | BS5 | 備註 |
|--------|-----|------|
| `modal` | `modal` | 不變 |
| `modal-dialog` | `modal-dialog` | 不變 |
| `modal-sm` | `modal-sm` | 不變 |
| `modal-lg` | `modal-lg` | 不變 |
| `modal-xl` | `modal-xl` | BS5 新增 |
| `modal-fullscreen` | `modal-fullscreen` | BS5 新增（含斷點變體） |
| `data-toggle="modal"` | `data-bs-toggle="modal"` | **data 屬性加 bs- 前綴** |
| `data-target` | `data-bs-target` | 同上 |
| `data-dismiss="modal"` | `data-bs-dismiss="modal"` | 同上 |
| `modal-backdrop` | 自動管理 | BS5 自動建立 |
| `$(...).modal('show')` | `bootstrap.Modal.getOrCreateInstance(...).show()` | JS API 變更 |

## 其他元件

| BS3/4 | BS5 | 備註 |
|--------|-----|------|
| `alert` | `alert` | 不變 |
| `alert-dismissible` | `alert-dismissible` | 不變 |
| `badge` | `badge` | 不變 |
| `badge-default` | `badge text-bg-secondary` | 改名 |
| `badge-primary` | `badge text-bg-primary` | 改名 |
| `breadcrumb` | `breadcrumb` | 不變 |
| `pagination` | `pagination` | 不變 |
| `navbar` | `navbar` | 不變 |
| `navbar-default` | 移除（用 `navbar-light`/`navbar-dark`） | 改名 |
| `navbar-toggle` | `navbar-toggler` | 改名 |
| `navbar-collapse` | `navbar-collapse` | 不變 |
| `nav-tabs` | `nav-tabs` | 不變 |
| `nav-pills` | `nav-pills` | 不變 |
| `data-toggle="tab"` | `data-bs-toggle="tab"` | data 屬性加 bs- |
| `data-toggle="collapse"` | `data-bs-toggle="collapse"` | 同上 |
| `dropdown` | `dropdown` | 不變 |
| `data-toggle="dropdown"` | `data-bs-toggle="dropdown"` | 同上 |
| `caret` | `caret`（FA 圖示取代） | 建議用 FA |
| `text-center` | `text-center` | 不變 |
| `text-muted` | `text-muted` | 不變 |
| `pull-right` | `ms-auto` / `text-end` | **改名** |
| `pull-left` | `me-auto` / `text-start` | **改名** |
| `col-xs-*` | `col-*` | 移除 xs |
| `col-sm-*` | `col-sm-*` | 不變 |
| `hidden-xs` | `d-sm-none` / `d-none d-sm-block` | **改用 display utility** |
| `visible-print` | `d-print-block` | 改用 display utility |
| `sr-only` | `visually-hidden` | 改名 |
| `close` (btn) | `btn-close` | 改名 |
| `well` | `card` 或 `border rounded p-3` | 移除 |
| `panel` | `card` | **改名** |
| `panel-default` | `card` | 改名 |
| `panel-heading` | `card-header` | 改名 |
| `panel-body` | `card-body` | 改名 |
| `panel-footer` | `card-footer` | 改名 |
| `thumbnail` | `card` + `img-fluid` | 改名 |
| `media` | `media`（BS5 仍支援）或 flex | 不變 |
| `list-group` | `list-group` | 不變 |
| `progress` | `progress` | 不變 |
| `progress-bar` | `progress-bar` | 不變 |
| `tooltip` | `tooltip`（`data-bs-toggle="tooltip"`） | data 屬性加 bs- |
| `popover` | `popover`（`data-bs-toggle="popover"`） | 同上 |
| `tooltipster`/自訂 | `tooltip` | 統一 BS5 |

## JS API 變更

| BS3/4 | BS5 |
|--------|-----|
| `$.fn.modal` | `bootstrap.Modal` |
| `$.fn.tab` | `bootstrap.Tab` |
| `$.fn.collapse` | `bootstrap.Collapse` |
| `$.fn.dropdown` | `bootstrap.Dropdown` |
| `$.fn.tooltip` | `bootstrap.Tooltip` |
| `$.fn.popover` | `bootstrap.Popover` |
| `$.fn.alert` | `bootstrap.Alert` |
| `$.fn.carousel` | `bootstrap.Carousel` |
| `$.fn.scrollspy` | `bootstrap.ScrollSpy` |
| `$.fn.toast` | `bootstrap.Toast`（BS5 新增） |

> **重要**：BS5 不再依賴 jQuery。所有 `$.fn.*` 改為 `bootstrap.*` 靜態 API。過渡期 jQuery 仍保留（供自訂 JS 使用），但 BS5 元件本身不需 jQuery。

---

# 附錄 B — Font Awesome 4 → 6 圖示對照表

> 完整對照參考 [FA Migration Guide](https://fontawesome.com/upgrading/from-4)。以下列出本系統常用圖示。

## 命名規則變更

| FA4 | FA6 | 備註 |
|-----|-----|------|
| `fa fa-*` | `fa-solid fa-*` | 加 `fa-solid` 前綴（solid 風格） |
| `fa fa-*` (outline) | `fa-regular fa-*` | regular 風格 |
| `fa fa-*` (brand) | `fa-brands fa-*` | brand 風格 |

## 常用圖示對照

| FA4 名稱 | FA6 名稱 | 用途 |
|----------|----------|------|
| `fa-search` | `fa-magnifying-glass` | 搜尋 |
| `fa-times` | `fa-xmark` | 關閉 |
| `fa-plus` | `fa-plus` | 新增（不變） |
| `fa-pencil` / `fa-pencil-square-o` | `fa-pen` | 編輯 |
| `fa-trash` / `fa-trash-o` | `fa-trash-can` | 刪除 |
| `fa-eye` | `fa-eye` | 查看（不變） |
| `fa-print` | `fa-print` | 列印（不變） |
| `fa-download` | `fa-download` | 下載（不變） |
| `fa-upload` | `fa-upload` | 上傳（不變） |
| `fa-refresh` | `fa-rotate-right` | 刷新 |
| `fa-check` | `fa-check` | 確認（不變） |
| `fa-check-square-o` | `fa-square-check` | 勾選框 |
| `fa-square-o` | `fa-square` | 方框 |
| `fa-exclamation-triangle` | `fa-triangle-exclamation` | 警告 |
| `fa-info-circle` | `fa-circle-info` | 資訊 |
| `fa-check-circle` / `fa-check-circle-o` | `fa-circle-check` | 成功 |
| `fa-times-circle` / `fa-times-circle-o` | `fa-circle-xmark` | 錯誤 |
| `fa-question-circle` | `fa-circle-question` | 說明 |
| `fa-bell` | `fa-bell` | 通知（不變） |
| `fa-envelope` | `fa-envelope` | 郵件（不變） |
| `fa-user` | `fa-user` | 使用者（不變） |
| `fa-users` | `fa-users` | 使用者群組（不變） |
| `fa-sign-out` | `fa-right-from-bracket` | 登出 |
| `fa-sign-in` | `fa-right-to-bracket` | 登入 |
| `fa-cog` / `fa-gear` | `fa-gear` | 設定 |
| `fa-cogs` | `fa-gears` | 設定（複數） |
| `fa-folder` | `fa-folder` | 資料夾（不變） |
| `fa-folder-open` | `fa-folder-open` | 開啟資料夾（不變） |
| `fa-file` | `fa-file` | 檔案（不變） |
| `fa-file-text-o` | `fa-file-lines` | 文字檔案 |
| `fa-file-pdf-o` | `fa-file-pdf` | PDF |
| `fa-file-excel-o` | `fa-file-excel` | Excel |
| `fa-file-zip-o` | `fa-file-zipper` | ZIP |
| `fa-image` | `fa-image` | 圖片（不變） |
| `fa-calendar` / `fa-calendar-o` | `fa-calendar` | 日曆（不變） |
| `fa-clock-o` | `fa-clock` | 時鐘 |
| `fa-history` | `fa-clock-rotate-left` | 歷史 |
| `fa-sort` | `fa-sort` | 排序（不變） |
| `fa-sort-asc` | `fa-sort-up` | 升冪 |
| `fa-sort-desc` | `fa-sort-down` | 降冪 |
| `fa-sort-alpha-asc` | `fa-arrow-down-a-z` | A-Z |
| `fa-angle-double-right` | `fa-angles-right` | 雙箭頭右 |
| `fa-angle-double-left` | `fa-angles-left` | 雙箭頭左 |
| `fa-angle-right` | `fa-angle-right` | 單箭頭右（不變） |
| `fa-angle-left` | `fa-angle-left` | 單箭頭左（不變） |
| `fa-angle-down` | `fa-angle-down` | 單箭頭下（不變） |
| `fa-angle-up` | `fa-angle-up` | 單箭頭上（不變） |
| `fa-hand-o-right` | `fa-hand-point-right` | 手指標右（選單子項） |
| `fa-language` | `fa-language` | 語言（不變） |
| `fa-tasks` | `fa-list-check` | 工作清單 |
| `fa-inbox` | `fa-inbox` | 收件匣（不變） |
| `fa-ban` | `fa-ban` | 禁止/作廢（不變） |
| `fa-floppy-o` | `fa-floppy-disk` | 儲存 |
| `fa-money` | `fa-money-bill` | 金額 |
| `fa-barcode` | `fa-barcode` | 條碼（不變） |
| `fa-qrcode` | `fa-qrcode` | QR Code（不變） |
| `fa-truck` | `fa-truck` | 運送（不變） |
| `fa-building` / `fa-building-o` | `fa-building` | 機構 |
| `fa-home` | `fa-house` | 首頁 |
| `fa-external-link` | `fa-up-right-from-square` | 外部連結 |
| `fa-arrow-circle-o-right` | `fa-circle-arrow-right` | 圓箭頭右 |
| `fa-ellipsis-v` | `fa-ellipsis-vertical` | 垂直省略號 |
| `fa-ellipsis-h` | `fa-ellipsis` | 水平省略號 |
| `fa-filter` | `fa-filter` | 篩選（不變） |
| `fa-columns` | `fa-table-columns` | 欄位設定 |
| `fa-th-list` | `fa-list` | 列表檢視 |
| `fa-table` | `fa-table` | 表格檢視（不變） |
| `fa-expand` | `fa-expand` | 展開（不變） |
| `fa-compress` | `fa-compress` | 收合（不變） |
| `fa-maximize` | `fa-expand` | 最大化 |
| `fa-minimize` | `fa-compress` | 最小化 |
| `fa-wrench` | `fa-screwdriver-wrench` | 工具 |
| `fa-bug` | `fa-bug` | 除錯（不變） |
| `fa-database` | `fa-database` | 資料庫（不變） |
| `fa-server` | `fa-server` | 伺服器（不變） |
| `fa-shield` | `fa-shield-halved` | 安全 |
| `fa-lock` / `fa-lock-o` | `fa-lock` | 鎖定 |
| `fa-unlock` / `fa-unlock-o` | `fa-lock-open` | 解鎖 |
| `fa-key` | `fa-key` | 金鑰（不變） |
| `fa-star` / `fa-star-o` | `fa-star` | 星號 |
| `fa-heart` / `fa-heart-o` | `fa-heart` | 愛心 |
| `fa-flag` | `fa-flag` | 旗標（不變） |
| `fa-tag` | `fa-tag` | 標籤（不變） |
| `fa-tags` | `fa-tags` | 標籤（複數）（不變） |
| `fa-bookmark` / `fa-bookmark-o` | `fa-bookmark` | 書籤 |
| `fa-bell-o` | `fa-bell` | 通知（輪廓） |
| `fa-comment` / `fa-comment-o` | `fa-comment` | 留言 |
| `fa-comments` | `fa-comments` | 留言（複數）（不變） |
| `fa-share` | `fa-share-nodes` | 分享 |
| `fa-link` / `fa-chain` | `fa-link` | 連結（不變） |
| `fa-unlink` / `fa-chain-broken` | `fa-link-slash` | 斷開連結 |
| `fa-copy` / `fa-files-o` | `fa-copy` | 複製 |
| `fa-cut` / `fa-scissors` | `fa-scissors` | 剪下 |
| `fa-paste` / `fa-clipboard` | `fa-clipboard` | 貼上 |
| `fa-save` | `fa-floppy-disk` | 儲存 |
| `fa-undo` | `fa-rotate-left` | 復原 |
| `fa-redo` | `fa-rotate-right` | 重做 |
| `fa-cut` | `fa-scissors` | 剪下 |
| `fa-bolt` / `fa-flash` | `fa-bolt` | 閃電（不變） |
| `fa-fire` | `fa-fire` | 火焰（不變） |
| `fa-magic` | `fa-wand-magic-sparkles` | 魔法 |
| `fa-random` | `fa-shuffle` | 隨機 |
| `fa-retweet` | `fa-retweet` | 轉發（不變） |
| `fa-resize-vertical` | `fa-up-down` | 垂直調整 |
| `fa-resize-horizontal` | `fa-left-right` | 水平調整 |
| `fa-move` | `fa-up-down-left-right` | 移動 |
| `fa-arrows` | `fa-up-down-left-right` | 四向箭頭 |
| `fa-arrows-v` | `fa-up-down` | 垂直箭頭 |
| `fa-arrows-h` | `fa-left-right` | 水平箭頭 |
| `fa-plus-circle` / `fa-plus-circle-o` | `fa-circle-plus` | 圓加號 |
| `fa-minus-circle` / `fa-minus-circle-o` | `fa-circle-minus` | 圓減號 |
| `fa-times-circle-o` | `fa-circle-xmark` | 圓叉號 |
| `fa-check-circle-o` | `fa-circle-check` | 圓勾號 |
| `fa-question-circle-o` | `fa-circle-question` | 圓問號 |
| `fa-exclamation-circle` / `fa-exclamation-circle-o` | `fa-circle-exclamation` | 圓驚嘆號 |
| `fa-minus` | `fa-minus` | 減號（不變） |
| `fa-external-link-o` | `fa-up-right-from-square` | 外部連結（輪廓） |
| `fa-share-square-o` | `fa-share-from-square` | 分享方框 |
| `fa-share-alt` | `fa-share-nodes` | 分享 |
| `fa-caret-right` | `fa-caret-right` | 小箭頭右（不變） |
| `fa-caret-left` | `fa-caret-left` | 小箭頭左（不變） |
| `fa-caret-down` | `fa-caret-down` | 小箭頭下（不變） |
| `fa-caret-up` | `fa-caret-up` | 小箭頭上（不變） |
| `fa-chevron-right` | `fa-chevron-right` | V 箭頭右（不變） |
| `fa-chevron-left` | `fa-chevron-left` | V 箭頭左（不變） |
| `fa-chevron-down` | `fa-chevron-down` | V 箭頭下（不變） |
| `fa-chevron-up` | `fa-chevron-up` | V 箭頭上（不變） |
| `fa-long-arrow-right` | `fa-arrow-right` | 長箭頭右 |
| `fa-long-arrow-left` | `fa-arrow-left` | 長箭頭左 |
| `fa-long-arrow-down` | `fa-arrow-down` | 長箭頭下 |
| `fa-long-arrow-up` | `fa-arrow-up` | 長箭頭上 |
| `fa-arrow-right` | `fa-arrow-right` | 箭頭右（不變） |
| `fa-arrow-left` | `fa-arrow-left` | 箭頭左（不變） |
| `fa-arrow-down` | `fa-arrow-down` | 箭頭下（不變） |
| `fa-arrow-up` | `fa-arrow-up` | 箭頭上（不變） |
| `fa-hand-o-up` | `fa-hand-point-up` | 手指標上 |
| `fa-hand-o-down` | `fa-hand-point-down` | 手指標下 |
| `fa-hand-o-left` | `fa-hand-point-left` | 手指標左 |
| `fa-th` | `fa-table-cells` | 網格檢視 |
| `fa-th-large` | `fa-table-cells-large` | 大網格 |
| `fa-list-ul` | `fa-list-ul` | 無序列表（不變） |
| `fa-list-ol` | `fa-list-ol` | 有序列表（不變） |
| `fa-outdent` | `fa-outdent` | 減少縮排（不變） |
| `fa-indent` | `fa-indent` | 增加縮排（不變） |
| `fa-align-left` | `fa-align-left` | 左對齊（不變） |
| `fa-align-center` | `fa-align-center` | 置中（不變） |
| `fa-align-right` | `fa-align-right` | 右對齊（不變） |
| `fa-bold` | `fa-bold` | 粗體（不變） |
| `fa-italic` | `fa-italic` | 斜體（不變） |
| `fa-underline` | `fa-underline` | 底線（不變） |
| `fa-strikethrough` | `fa-strikethrough` | 刪除線（不變） |
| `fa-superscript` | `fa-superscript` | 上標（不變） |
| `fa-subscript` | `fa-subscript` | 下標（不變） |
| `fa-text-height` | `fa-text-height` | 字高（不變） |
| `fa-text-width` | `fa-text-width` | 字寬（不變） |
| `fa-font` | `fa-font` | 字型（不變） |
| `fa-link` | `fa-link` | 連結（不變） |
| `fa-unlink` | `fa-link-slash` | 斷開連結 |
| `fa-table` | `fa-table` | 表格（不變） |
| `fa-columns` | `fa-table-columns` | 欄位 |
| `fa-sliders` | `fa-sliders` | 滑桿（不變） |
| `fa-magic` | `fa-wand-magic-sparkles` | 魔法 |
| `fa-tasks` | `fa-list-check` | 工作清單 |
| `fa-tachometer` | `fa-gauge-high` | 儀表板 |
| `fa-calendar` | `fa-calendar` | 日曆（不變） |
| `fa-calendar-check-o` | `fa-calendar-check` | 日曆勾選 |
| `fa-calendar-plus-o` | `fa-calendar-plus` | 日曆加號 |
| `fa-calendar-minus-o` | `fa-calendar-minus` | 日曆減號 |
| `fa-calendar-times-o` | `fa-calendar-xmark` | 日曆叉號 |
| `fa-clock-o` | `fa-clock` | 時鐘 |
| `fa-hourglass` | `fa-hourglass` | 沙漏（不變） |
| `fa-hourglass-o` | `fa-hourglass` | 沙漏（輪廓） |
| `fa-hourglass-start` | `fa-hourglass-start` | 沙漏開始（不變） |
| `fa-hourglass-half` | `fa-hourglass-half` | 沙漏一半（不變） |
| `fa-hourglass-end` | `fa-hourglass-end` | 沙漏結束（不變） |
| `fa-bell` | `fa-bell` | 通知（不變） |
| `fa-bell-o` | `fa-bell` | 通知（輪廓） |
| `fa-bell-slash` | `fa-bell-slash` | 通知關閉（不變） |
| `fa-bell-slash-o` | `fa-bell-slash` | 通知關閉（輪廓） |
| `fa-certificate` | `fa-certificate` | 證書（不變） |
| `fa-cogs` | `fa-gears` | 齒輪（複數） |
| `fa-comments` | `fa-comments` | 留言（不變） |
| `fa-compass` | `fa-compass` | 指南針（不變） |
| `fa-credit-card` | `fa-credit-card` | 信用卡（不變） |
| `fa-cutlery` | `fa-utensils` | 餐具 |
| `fa-dashboard` | `fa-gauge-high` | 儀表板 |
| `fa-dot-circle-o` | `fa-circle-dot` | 圓點 |
| `fa-download` | `fa-download` | 下載（不變） |
| `fa-flask` | `fa-flask` | 燒瓶（不變） |
| `fa-floppy-o` | `fa-floppy-disk` | 儲存 |
| `fa-gift` | `fa-gift` | 禮物（不變） |
| `fa-bullhorn` | `fa-bullhorn` | 擴音器（不變） |

> **批次替換建議**：FA4→FA6 替換時，先處理 `fa fa-*` → `fa-solid fa-*` 前綴，再依上表替換個別圖示名稱。可用 regex 批次處理：
> - `fa fa-` → `fa-solid fa-`（前綴）
> - 個別名稱依上表對照（注意部分名稱不變，如 `fa-plus`、`fa-eye`、`fa-print`）
> - 替換後 grep 驗證無 `fa fa-` 殘留。

---

## 文件維護

| 項目 | 說明 |
|------|------|
| 維護者 | 前端團隊 |
| 更新時機 | 元件規格變更、BS5 版本升級、新元件加入 |
| 版本記錄 | 見文件頭部版本欄位 |
| 相關文件 | `docs/refactoring-plan.md`（Vue 遷移計劃）、`.github/instructions/vue.instructions.md`（Vue 慣例） |

### 版本記錄

| 版本 | 日期 | 變更 |
|------|------|------|
| 1.0 | 2026-09-27 | 初版：Part A 設計規格（A.1–A.10）+ Part B 路線圖（P0–P3）+ 附錄 A/B |