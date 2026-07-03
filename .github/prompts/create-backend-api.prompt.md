# 創建後端API (Create Backend API)

## 概述
此提示用於自動生成標準後端API的完整結構，包括：
- 控制器 (Controller)
- 服務介面 (Service Interface) 
- 服務實現 (Service Implementation)
- DI 註冊

## 使用情境
當需要快速建立符合項目規範的後端API時使用。特別適用於：
- REST API 端點
- 使用 C# 和 ASP.NET Core
- 依照 IUnitOfWork 架構
- 支援認證和授權

## 輸出規格
1. **Controller 檔案**（含路由、認證屬性、using）：根據提供的 HTTP 方法與路由模板產生控制器
2. **IService 介面**：定義服務介面以提供依賴注入
3. **Service 實作**（含 IUnitOfWork、ILogger、字串截斷）：實現服務，使用 IUnitOfWork 持久化資料；當 IUnitOfWork 操作拋出例外時，使用 ILogger 記錄 LogError（含例外物件），然後重新拋出例外（throw;），不在 Service 層吞掉例外
4. **Program.cs DI 註冊片段**：在 Program.cs 中加入服務註冊

## 模板輸入
- 服務名稱 (例如：NegoDraft)
- 資料模型 (例如：InvoiceUploadDto)
- 目標實體 (例如：NegoInvoice)
- HTTP 方法與路由（例如：POST /api/negodraft/upload-invoice）
- 單一操作方法名稱 (例如：UploadInvoice)
- 資料長度限制 (例如：InvoiceNo: 16, LadingNo: 6)

## 預設行為
- 檔案將放置在正確的資料夾結構中（Controllers、Services、Interfaces）
- 服務命名符合 C# 撰寫習慣（INegoDraftService）
- 控制器認證方式：在控制器類別層級加上 [Authorize]（無 Policy）。若需要特定 Role 或 Policy，由使用者在模板輸入中提供，否則預設僅套用 [Authorize]
- 當輸入字串超過指定長度限制時，靜默截斷至最大長度（例如 value.Length > 16 ? value[..16] : value），不拋出例外
- 若使用者未提供資料長度限制，則不產生任何字串截斷邏輯
- 控制器方法回傳 Task<ActionResult<ApiResponse<T>>>，使用 [ProducesResponseType] 標註，命名空間格式為 CompanyName.ProjectName.Controllers

## 輸出示例
- 在輸出末尾加入一段註解範例，展示如何在控制器建構子中注入 INegoDraftService