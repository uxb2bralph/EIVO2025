# HNB.Api Backend API 實作規範

> 適用版本：ASP.NET Core 10 + EF Core 10  
> 適用專案：所有繼承 HNB.Api 架構的後台 API 模組

---

## 目錄

1. [架構分層總覽](#1-架構分層總覽)
2. [Controller 規範](#2-controller-規範)
3. [Service 規範](#3-service-規範)
4. [Repository 規範](#4-repository-規範)
5. [DTO 規範](#5-dto-規範)
6. [Request 風格](#6-request-風格)
7. [Response 風格](#7-response-風格)
8. [分頁規範](#8-分頁規範)
9. [驗證規範](#9-驗證規範)
10. [AutoMapper / Mapperly 規範](#10-automapper--mapperly-規範)
11. [多語系規範](#11-多語系規範)
12. [權限規範](#12-權限規範)
13. [錯誤處理規範](#13-錯誤處理規範)
14. [命名規範](#14-命名規範)
15. [完整範例](#15-完整範例)

---

## 1. 架構分層總覽

```
HTTP Request
    ↓
[Authorization Filter]   ConditionalAuthorize / RequirePagePermission
    ↓
[Model Validation Filter] ValidateModel attribute
    ↓
Controller               {TableName}Controller : LocalizedControllerBase
    ↓
Service                  {TableName}Service : I{TableName}Service
    ↓
Repository               {TableName}Repository : GenericRepository<T>
    ↓
DbContext                WisDbContext (EF Core, global query filters apply)
```

**嚴格命名慣例：** `{TableName}Controller` → `{TableName}Service` → `{TableName}Repository`

---

## 2. Controller 規範

### 2.1 基本結構

```csharp
[ApiController]
[Route("api/[controller]")]
[ConditionalAuthorize]          // 使用此而非 [Authorize]，開發環境可 bypass
[Produces("application/json")]
public class ProductsController : LocalizedControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }
}
```

### 2.2 必要規則

- 繼承 `LocalizedControllerBase`，**不要繼承 `ControllerBase`**
- 使用 `[ConditionalAuthorize]`，**不要使用 `[Authorize]`**
- 所有 action 必須是 `async Task<IActionResult>`
- Response 統一透過 `LocalizedControllerBase` 的 helper 方法輸出
- 業務邏輯**不放在 Controller**，全部委派給 Service
- 使用 `[ValidateModel]` attribute，**不要手寫 `if (!ModelState.IsValid)`**

### 2.3 Response Helper 方法

| 方法 | HTTP 狀態碼 | 使用時機 |
|------|------------|---------|
| `CreateSuccessResponse<T>(data, messageKey)` | 200 | 查詢、更新成功並回傳資料 |
| `CreateSuccessResponse(messageKey)` | 200 | 操作成功、無需回傳資料 |
| `CreateCreatedResponse<T>(data, messageKey)` | 201 | 新增資源成功 |
| `CreateNotFoundResponse(messageKey)` | 404 | 資源不存在 |
| `CreateBadRequestResponse(messageKey, errors?)` | 400 | 請求格式或業務邏輯錯誤 |
| `CreateErrorResponse(statusCode, messageKey, errors?)` | 自訂 | 其他錯誤（401、403、500）|

### 2.4 標準 Action 模板

```csharp
[HttpGet]
[ValidateModel]
public async Task<IActionResult> GetList([FromQuery] ProductQueryDto queryDto)
{
    try
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return CreateErrorResponse(401, "User.Unauthorized");

        var result = await _productService.GetPagedAsync(queryDto);
        return CreateSuccessResponse(result, "Common.Retrieved");
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Resource not found");
        return CreateNotFoundResponse("Common.NotFound");
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Invalid operation: {Message}", ex.Message);
        return CreateBadRequestResponse(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in {Action}", nameof(GetList));
        return CreateErrorResponse(500, "Common.RetrieveError");
    }
}

[HttpGet("{id}")]
public async Task<IActionResult> GetById(int id)
{
    try
    {
        var result = await _productService.GetByIdAsync(id);
        return result is null
            ? CreateNotFoundResponse("Common.NotFound")
            : CreateSuccessResponse(result, "Common.Retrieved");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting product {Id}", id);
        return CreateErrorResponse(500, "Common.RetrieveError");
    }
}

[HttpPost]
[ValidateModel]
public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
{
    try
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return CreateErrorResponse(401, "User.Unauthorized");

        var result = await _productService.CreateAsync(dto, userId);
        return CreateCreatedResponse(result, "Common.Created");
    }
    catch (InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Create validation failed");
        return CreateBadRequestResponse(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating product");
        return CreateErrorResponse(500, "Common.CreateError");
    }
}

[HttpPut("{id}")]
[ValidateModel]
public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
{
    try
    {
        await _productService.UpdateAsync(id, dto);
        return CreateSuccessResponse("Common.Updated");
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Product {Id} not found for update", id);
        return CreateNotFoundResponse("Common.NotFound");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating product {Id}", id);
        return CreateErrorResponse(500, "Common.UpdateError");
    }
}

[HttpDelete("{id}")]
public async Task<IActionResult> Delete(int id)
{
    try
    {
        await _productService.DeleteAsync(id);
        return CreateSuccessResponse("Common.Deleted");
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Product {Id} not found for delete", id);
        return CreateNotFoundResponse("Common.NotFound");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error deleting product {Id}", id);
        return CreateErrorResponse(500, "Common.DeleteError");
    }
}
```

---

## 3. Service 規範

### 3.1 基本結構

```csharp
public interface IProductService
{
    Task<PagedResultDto<ProductDatatableDto>> GetPagedAsync(ProductQueryDto queryDto);
    Task<ProductDto?> GetByIdAsync(int id);
    Task<ProductDto> CreateAsync(CreateProductDto dto, int createdByUserId);
    Task UpdateAsync(int id, UpdateProductDto dto);
    Task DeleteAsync(int id);
}

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly WisMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IUnitOfWork unitOfWork,
        WisMapper mapper,
        ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }
}
```

### 3.2 必要規則

- Service 一律實作對應 interface
- 所有業務邏輯（含 AutoMapper/Mapperly 呼叫）在 Service 執行，**不在 Controller**
- 使用 `IUnitOfWork` 而非直接注入 `DbContext`
- 需要交易時呼叫 `await _unitOfWork.BeginTransactionAsync()`
- 拋出有語意的例外：`KeyNotFoundException`（找不到）、`InvalidOperationException`（業務邏輯錯誤）

### 3.3 查詢模式

```csharp
public async Task<PagedResultDto<ProductDatatableDto>> GetPagedAsync(ProductQueryDto queryDto)
{
    var query = _unitOfWork.Context.Set<Product>()
        .AsNoTracking()
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(queryDto.Search))
        query = query.Where(p => p.Name.Contains(queryDto.Search));

    if (queryDto.CategoryId.HasValue)
        query = query.Where(p => p.CategoryId == queryDto.CategoryId.Value);

    var totalCount = await query.CountAsync();
    var items = await query
        .OrderByDescending(p => p.CreateDate)
        .Skip(queryDto.Skip)
        .Take(queryDto.PageSize)
        .Select(p => _mapper.MapToProductDatatableDto(p))
        .ToListAsync();

    return new PagedResultDto<ProductDatatableDto>
    {
        Items = items,
        TotalCount = totalCount,
        PageNumber = queryDto.Page,
        PageSize = queryDto.PageSize
    };
}
```

---

## 4. Repository 規範

### 4.1 繼承 GenericRepository

```csharp
public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(WisDbContext context) : base(context) { }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        => await FindAsync(p => p.CategoryId == categoryId);
}
```

### 4.2 GenericRepository 可用方法

```csharp
// 基本 CRUD
Task<T?> GetByIdAsync(object id)
Task<IEnumerable<T>> GetAllAsync()
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
Task AddAsync(T entity)
Task AddRangeAsync(IEnumerable<T> entities)
void Update(T entity)
void Remove(T entity)

// 分頁
Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, ...)
Task<(IEnumerable<T> Items, int TotalCount)> GetPagedValidAsync(...)

// 僅取 IsValid = true 的資料
Task<IEnumerable<T>> GetAllValidAsync()
Task<IEnumerable<T>> FindValidAsync(Expression<Func<T, bool>> predicate)

// 計數
Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)

// 原始 DbSet（複雜查詢用）
DbSet<T> DbSet
```

### 4.3 Unit of Work 交易模式

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Products.AddAsync(product);
    await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

---

## 5. DTO 規範

### 5.1 DTO 分類

| DTO 類型 | 用途 | 命名範例 |
|---------|------|---------|
| `{Entity}Dto` | 單筆完整詳情（GET by id） | `ProductDto` |
| `{Entity}DatatableDto` | 列表 / 表格顯示 | `ProductDatatableDto` |
| `Create{Entity}Dto` | 新增請求 body | `CreateProductDto` |
| `Update{Entity}Dto` | 更新請求 body | `UpdateProductDto` |
| `{Entity}QueryDto` | 列表查詢參數（繼承 `PagedRequestDto`） | `ProductQueryDto` |
| `{Entity}FormDto` | 含檔案上傳的表單 | `ProductFormDto` |

### 5.2 欄位規則

- 使用 `interface` 而非 `class`（Vue 前端消費時）；後端 DTO 使用 `class`
- 屬性加上適當 Data Annotations 供 `[ValidateModel]` 使用
- 非必填欄位使用 `?` nullable
- **不在 DTO 放業務邏輯**，複雜驗證才實作 `IValidatableObject`

```csharp
public class CreateProductDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Validation.PriceMustBePositive")]
    public decimal Price { get; set; }

    public int? CategoryId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
```

### 5.3 多語系 ErrorMessage 鍵

validation attribute 的 `ErrorMessage` 使用 `SharedResource` 的 key，讓 `ValidateModel` filter 能自動翻譯：

```csharp
[Required(ErrorMessage = "Validation.Required")]
[StringLength(100, ErrorMessage = "Validation.StringLength")]
[Range(1, 100, ErrorMessage = "Validation.Range")]
```

---

## 6. Request 風格

### 6.1 路由慣例

```
GET    /api/{resource}           列表（含分頁過濾）
GET    /api/{resource}/{id}      單筆詳情
POST   /api/{resource}           新增
PUT    /api/{resource}/{id}      完整更新
PATCH  /api/{resource}/{id}      部分更新
DELETE /api/{resource}/{id}      刪除
POST   /api/{resource}/{action}/{id?}  業務操作（非 CRUD）
```

### 6.2 查詢參數（Query String）

列表 / 查詢端點一律使用 `[FromQuery]`，繼承 `PagedRequestDto`：

```
GET /api/products?page=1&pageSize=10&search=abc&sortBy=name&sortDescending=false
```

```csharp
public class ProductQueryDto : PagedRequestDto
{
    public int? CategoryId { get; set; }
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }
}
```

### 6.3 Request Body

- 新增 / 更新：`[FromBody]`，`Content-Type: application/json`
- 含檔案上傳：`[FromForm]`，`Content-Type: multipart/form-data`，JSON 欄位以 string 傳遞後在 DTO 反序列化
- 路由參數：使用 `[FromRoute]`（預設即是）

---

## 7. Response 風格

### 7.1 統一 Response 結構

所有 API 回傳統一格式：

```json
{
  "success": true,
  "message": "Products retrieved successfully",
  "data": { ... },
  "errors": null
}
```

| 欄位 | 型別 | 說明 |
|------|------|------|
| `success` | `bool` | 操作是否成功 |
| `message` | `string` | 人類可讀的訊息（已翻譯） |
| `data` | `T?` | 回傳資料，失敗時為 `null` |
| `errors` | `string[]?` | 驗證或業務錯誤明細，成功時為 `null` |

### 7.2 成功回應範例

**單筆資源：**
```json
{
  "success": true,
  "message": "查詢成功",
  "data": {
    "id": 1,
    "name": "Product A",
    "price": 100.00
  },
  "errors": null
}
```

**分頁列表：**
```json
{
  "success": true,
  "message": "查詢成功",
  "data": {
    "items": [ ... ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10,
    "hasPrevious": false,
    "hasNext": true
  },
  "errors": null
}
```

**無資料的成功操作（刪除、更新）：**
```json
{
  "success": true,
  "message": "刪除成功",
  "data": null,
  "errors": null
}
```

### 7.3 錯誤回應範例

**驗證失敗（400）：**
```json
{
  "success": false,
  "message": "輸入資料無效",
  "data": null,
  "errors": [
    "名稱為必填欄位",
    "價格必須大於 0"
  ]
}
```

**資源不存在（404）：**
```json
{
  "success": false,
  "message": "找不到指定資源",
  "data": null,
  "errors": null
}
```

**未授權（401）：**
```json
{
  "success": false,
  "message": "未授權的操作",
  "data": null,
  "errors": null
}
```

**伺服器錯誤（500）：**
```json
{
  "success": false,
  "message": "系統發生錯誤，請稍後再試",
  "data": null,
  "errors": null
}
```

### 7.4 HTTP 狀態碼規範

| 情境 | 狀態碼 | Helper 方法 |
|------|--------|------------|
| 查詢 / 更新成功 | 200 | `CreateSuccessResponse<T>()` |
| 新增成功 | 201 | `CreateCreatedResponse<T>()` |
| 驗證失敗 | 400 | `CreateBadRequestResponse()` |
| 未登入 | 401 | `CreateErrorResponse(401, ...)` |
| 無權限 | 403 | `CreateErrorResponse(403, ...)` |
| 資源不存在 | 404 | `CreateNotFoundResponse()` |
| 伺服器錯誤 | 500 | `CreateErrorResponse(500, ...)` |

---

## 8. 分頁規範

### 8.1 Request DTO

```csharp
public class PagedRequestDto
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;

    public int Skip => (Page - 1) * PageSize;
}
```

### 8.2 Response DTO

```csharp
public class PagedResultDto<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
```

### 8.3 Service 中分頁查詢模式

```csharp
var totalCount = await query.CountAsync();
var items = await query
    .OrderByDescending(x => x.CreateDate)
    .Skip(queryDto.Skip)
    .Take(queryDto.PageSize)
    .ToListAsync();

return new PagedResultDto<TDto>
{
    Items = items.Select(_mapper.MapToDto),
    TotalCount = totalCount,
    PageNumber = queryDto.Page,
    PageSize = queryDto.PageSize
};
```

---

## 9. 驗證規範

### 9.1 基本 Data Annotations

```csharp
[Required(ErrorMessage = "Validation.Required")]
[StringLength(100, MinimumLength = 2, ErrorMessage = "Validation.StringLength")]
[Range(1, int.MaxValue, ErrorMessage = "Validation.Range")]
[EmailAddress(ErrorMessage = "Validation.Email")]
[Phone(ErrorMessage = "Validation.Phone")]
[RegularExpression(@"^\d{4}$", ErrorMessage = "Validation.Format")]
```

### 9.2 複雜驗證（IValidatableObject）

當驗證涉及多欄位互相依賴時：

```csharp
public class ProductQueryDto : PagedRequestDto, IValidatableObject
{
    public decimal? PriceMin { get; set; }
    public decimal? PriceMax { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PriceMin.HasValue && PriceMax.HasValue && PriceMin > PriceMax)
            yield return new ValidationResult(
                "Validation.PriceRangeInvalid",
                new[] { nameof(PriceMin), nameof(PriceMax) });
    }
}
```

### 9.3 ValidateModel Attribute

- 在每個需要驗證的 action 加上 `[ValidateModel]`
- Filter 自動翻譯 ErrorMessage key，並回傳標準 `BaseResponseDto` 格式
- **不要手動寫 `if (!ModelState.IsValid)` 區塊**

---

## 10. AutoMapper / Mapperly 規範

### 10.1 優先使用 Mapperly（WisMapper）

新功能一律使用 Mapperly 的 `WisMapper`，不新增 AutoMapper profile：

```csharp
// WisMapper 中宣告 partial method，讓 source generator 實作
public partial ProductDto MapToProductDto(Product source);
public partial IEnumerable<ProductDto> MapToProductDtos(IEnumerable<Product> source);

// 忽略特定欄位
[MapperIgnoreTarget(nameof(Product.Id))]
[MapperIgnoreTarget(nameof(Product.CreateDate))]
private partial Product MapToProductFromCreate(CreateProductDto source);
```

### 10.2 Mapping 呼叫位置

- Mapping **在 Service 執行**，不在 Controller 或 Repository
- Controller 只呼叫 Service，取到 DTO 後直接回傳

### 10.3 更新模式（只更新非 null 欄位）

```csharp
public void ApplyUpdate(UpdateProductDto source, Product target)
{
    if (source.Name != null) target.Name = source.Name;
    if (source.Price.HasValue) target.Price = source.Price.Value;
    if (source.Description != null) target.Description = source.Description;
}
```

---

## 11. 多語系規範

### 11.1 Resource Key 慣例

```
Common.Retrieved         查詢成功
Common.RetrieveError     查詢失敗
Common.Created           新增成功
Common.CreateError       新增失敗
Common.Updated           更新成功
Common.UpdateError       更新失敗
Common.Deleted           刪除成功
Common.DeleteError       刪除失敗
Common.NotFound          找不到資源
User.Unauthorized        未授權

{Module}.Retrieved       模組專屬查詢成功（如 Payments.Retrieved）
{Module}.NotFound        模組專屬找不到
{Module}.CreateError     模組專屬錯誤
```

### 11.2 新增 Key 流程

新增 message key 時同步更新兩個檔案：

- `Resources/SharedResource.resx`（英文）
- `Resources/SharedResource.zh-TW.resx`（繁體中文）

### 11.3 使用方式

```csharp
// Controller 中（透過 helper 方法自動 resolve）
return CreateSuccessResponse(result, "Common.Retrieved");

// 手動取得翻譯字串
var message = GetLocalizedString("Common.Retrieved");
```

---

## 12. 權限規範

### 12.1 Controller 層級授權

```csharp
[ConditionalAuthorize]  // 所有需要登入的 controller 加此 attribute
```

### 12.2 頁面 / 功能權限

```csharp
[RequirePagePermission("Products")]          // 需要 Products 頁面權限
[RequirePagePermission("Products", "Admin")] // 多個頁面任一即可
```

### 12.3 角色權限

```csharp
[RequireRole("Admin")]
[RequireRole("Admin", "Manager")]
```

### 12.4 從 Claims 取 UserId

```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
    return CreateErrorResponse(401, "User.Unauthorized");
```

---

## 13. 錯誤處理規範

### 13.1 例外類型對應

| 例外類型 | 意義 | Controller 處理 |
|---------|------|----------------|
| `KeyNotFoundException` | 資源不存在 | `CreateNotFoundResponse()` |
| `InvalidOperationException` | 業務邏輯錯誤 | `CreateBadRequestResponse(ex.Message)` |
| `UnauthorizedAccessException` | 無操作權限 | `CreateErrorResponse(403, ...)` |
| `Exception` | 非預期錯誤 | `CreateErrorResponse(500, "Common.Error")` + `LogError` |

### 13.2 Logging 規範

```csharp
// 預期性錯誤（業務邏輯）用 Warning
_logger.LogWarning(ex, "Product {Id} not found", id);

// 非預期錯誤用 Error
_logger.LogError(ex, "Unexpected error in {Action}", nameof(GetById));

// 重要操作記錄用 Information
_logger.LogInformation("Product {Id} created by user {UserId}", productId, userId);
```

### 13.3 Service 拋出例外

Service **不 catch** 非預期例外，讓它向上傳播到 Controller：

```csharp
// Service 中
var product = await _unitOfWork.Products.GetByIdAsync(id)
    ?? throw new KeyNotFoundException($"Product {id} not found");

if (product.IsLocked)
    throw new InvalidOperationException("Product.Locked");
```

---

## 14. 命名規範

### 14.1 檔案與類別

| 類型 | 命名格式 | 範例 |
|------|---------|------|
| Controller | `{TableName}Controller` | `ProductsController` |
| Service Interface | `I{TableName}Service` | `IProductService` |
| Service | `{TableName}Service` | `ProductService` |
| Repository Interface | `I{TableName}Repository` | `IProductRepository` |
| Repository | `{TableName}Repository` | `ProductRepository` |
| DTO（詳情） | `{TableName}Dto` | `ProductDto` |
| DTO（列表） | `{TableName}DatatableDto` | `ProductDatatableDto` |
| DTO（新增） | `Create{TableName}Dto` | `CreateProductDto` |
| DTO（更新） | `Update{TableName}Dto` | `UpdateProductDto` |
| DTO（查詢） | `{TableName}QueryDto` | `ProductQueryDto` |

### 14.2 Route 命名

- Route 使用 `kebab-case`（複數）：`/api/product-categories`
- Action 名稱使用 PascalCase：`GetList`、`GetById`、`Create`、`Update`、`Delete`

### 14.3 Localization Key

- 通用：`Common.{Action}` / `Common.{Action}Error`
- 模組：`{ModuleName}.{Action}` / `{ModuleName}.{Action}Error`
- 驗證：`Validation.{Rule}`

---

## 15. 完整範例

完整的 CRUD 功能實作（Categories 模組）：

### Controller

```csharp
[ApiController]
[Route("api/[controller]")]
[ConditionalAuthorize]
[Produces("application/json")]
public class CategoriesController : LocalizedControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    [ValidateModel]
    public async Task<IActionResult> GetList([FromQuery] CategoryQueryDto queryDto)
    {
        try
        {
            var result = await _categoryService.GetPagedAsync(queryDto);
            return CreateSuccessResponse(result, "Common.Retrieved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return CreateErrorResponse(500, "Common.RetrieveError");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _categoryService.GetByIdAsync(id);
            return result is null
                ? CreateNotFoundResponse("Common.NotFound")
                : CreateSuccessResponse(result, "Common.Retrieved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {Id}", id);
            return CreateErrorResponse(500, "Common.RetrieveError");
        }
    }

    [HttpPost]
    [ValidateModel]
    [RequirePagePermission("Categories")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return CreateErrorResponse(401, "User.Unauthorized");

            var result = await _categoryService.CreateAsync(dto, userId);
            return CreateCreatedResponse(result, "Common.Created");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Category create validation failed");
            return CreateBadRequestResponse(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return CreateErrorResponse(500, "Common.CreateError");
        }
    }

    [HttpPut("{id}")]
    [ValidateModel]
    [RequirePagePermission("Categories")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            await _categoryService.UpdateAsync(id, dto);
            return CreateSuccessResponse("Common.Updated");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Category {Id} not found", id);
            return CreateNotFoundResponse("Common.NotFound");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", id);
            return CreateErrorResponse(500, "Common.UpdateError");
        }
    }

    [HttpDelete("{id}")]
    [RequirePagePermission("Categories")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoryService.DeleteAsync(id);
            return CreateSuccessResponse("Common.Deleted");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Category {Id} not found", id);
            return CreateNotFoundResponse("Common.NotFound");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", id);
            return CreateErrorResponse(500, "Common.DeleteError");
        }
    }
}
```

### Service

```csharp
public interface ICategoryService
{
    Task<PagedResultDto<CategoryDatatableDto>> GetPagedAsync(CategoryQueryDto queryDto);
    Task<CategoryDto?> GetByIdAsync(int id);
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto, int userId);
    Task UpdateAsync(int id, UpdateCategoryDto dto);
    Task DeleteAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly WisMapper _mapper;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, WisMapper mapper, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResultDto<CategoryDatatableDto>> GetPagedAsync(CategoryQueryDto queryDto)
    {
        var query = _unitOfWork.Context.Set<Category>().AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryDto.Search))
            query = query.Where(c => c.Name.Contains(queryDto.Search));

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.DisplayOrder)
            .Skip(queryDto.Skip)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return new PagedResultDto<CategoryDatatableDto>
        {
            Items = items.Select(_mapper.MapToCategoryDatatableDto),
            TotalCount = totalCount,
            PageNumber = queryDto.Page,
            PageSize = queryDto.PageSize
        };
    }

    public async Task<CategoryDto?> GetByIdAsync(int id)
    {
        var entity = await _unitOfWork.Context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        return entity is null ? null : _mapper.MapToCategoryDto(entity);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto, int userId)
    {
        var entity = _mapper.MapToCategory(dto);
        await _unitOfWork.Categories.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.MapToCategoryDto(entity);
    }

    public async Task UpdateAsync(int id, UpdateCategoryDto dto)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");
        _mapper.ApplyCategoryUpdate(dto, entity);
        _unitOfWork.Categories.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found");
        entity.IsValid = false;
        _unitOfWork.Categories.Update(entity);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

---

*最後更新：2026-04-29*
