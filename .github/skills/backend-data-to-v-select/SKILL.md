---
name: backend-data-to-v-select
description: '將後端 API 資料綁定至 Vuetify v-select 下拉選單。當要串接後端資料到 v-select、設定 item-value/item-title、或建立 select 選項物件時使用。Use when: 串接後端 API 資料到 v-select, 建立下拉選單選項, select 綁定 item-value item-title, 後端資料給前端下拉, 動態選項載入'
user-invocable: true
---

# Backend Data → Vuetify v-select 串接流程

將後端 API 的資料（通常是多筆紀錄）轉換為 Vuetify `<v-select>` 可用的選項格式，正確指定 `item-value` 與 `item-title`，並在切換篩選條件時重新載入。

## 適用情境

- 從後端 API 取得清單資料作為下拉選單選項
- 選項需要顯示文字（item-title）與實際值（item-value）分離
- 選項內容會隨上層篩選條件變動（如選擇類別後載入對應選項）
- 使用 `watch` 監控條件變數，變更時重新向 API 取值

## 不適用情境

- 固定不變的選項（請直接寫死在程式碼或列舉中）
- 單純字串陣列的選項（不須 item-value / item-title 分離）

## 流程

### 步驟 1：定義後端 DTO

在 `HNB.Api/Models/DTOs/` 下新增 DTO class，包含：

| 屬性 | 用途 | 對應 item |
|------|------|-----------|
| `int Id` | 選項實際值 | `item-value` |
| `string ...` | 顯示文字所需的欄位 | `item-title` 組合來源 |

```csharp
public class XxxOptionDto
{
    public int Id { get; set; }          // 對應 item-value
    public string DisplayField { get; set; } = string.Empty; // 顯示文字
}
```

### 步驟 2：定義後端 Service Interface

在 `HNB.Api/Interfaces/` 下新增介面，方法簽章範例：

```csharp
Task<List<XxxOptionDto>> GetXxxByCategoryAsync(int categoryId, CancellationToken cancellationToken);
```

### 步驟 3：實作 Service

在 `HNB.Api/Services/` 下實作，查詢方式使用 `HNBDbContext`（或 `IUnitOfWork.DbSet`），投影到 DTO：

```csharp
var result = _context.SomeEntities
    .AsNoTracking()
    .Where(e => e.CategoryId == categoryId)
    .Select(e => new XxxOptionDto
    {
        Id = e.Id,
        DisplayField = e.Name,  // 或組合多欄位
    })
    .Distinct()
    .OrderBy(e => e.DisplayField);

return await result.ToListAsync(cancellationToken);
```

### 步驟 4：註冊 DI

在 `Program.cs` 加入：

```csharp
builder.Services.AddScoped<IXxxService, XxxService>();
```

### 步驟 5：新增 Controller Endpoint

在既有或新的 Controller 中新增端點，遵循 `try-catch` 與 `ApiResult<T>` 包裝模式：

```csharp
[HttpGet("xxx/{categoryId:int}")]
[ProducesResponseType(typeof(ApiResult<List<XxxOptionDto>>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetXxxByCategory(
    [FromRoute] int categoryId,
    CancellationToken cancellationToken)
{
    try
    {
        var data = await _xxxService.GetXxxByCategoryAsync(categoryId, cancellationToken);
        return Ok(new ApiResult<List<XxxOptionDto>> { Data = data });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in {Action}", nameof(GetXxxByCategory));
        return StatusCode(500, new ProblemDetails { Status = 500, Title = "查詢失敗，請稍後再試。" });
    }
}
```

### 步驟 6：定義前端 API 函式

在 `HNB.Web/src/api/` 下新增 API 檔案：

```typescript
import http from '@/api/http'

export interface XxxOption {
  id: number
  displayField: string
}

export async function getXxxByCategory(categoryId: number): Promise<XxxOption[]> {
  const { data } = await http.get<{ data: XxxOption[] }>(`/query/xxx/${categoryId}`)
  return data.data ?? []
}
```

### 步驟 7：Composable 中定義選項型別與狀態

在 composable 中（或直接在元件中）：

```typescript
// 選項物件介面
export interface SelectOptionItem {
  id: number
  itemValue: string
}

// 狀態
const options = ref<SelectOptionItem[]>([])
const loading = ref(false)
const selectedValue = ref<number | ''>('')
```

### 步驟 8：watch 條件變數，動態載入選項

```typescript
watch(conditionVar, async (val) => {
  selectedValue.value = ''     // 切換條件時重置已選值
  options.value = []
  if (!val) return
  loading.value = true
  try {
    const list: XxxOption[] = await getXxxByCategory(val)
    options.value = list.map((item) => ({
      id: item.id,
      itemValue: `${item.displayField}`,  // 組合顯示文字
    }))
  } catch {
    snackbar.error('取得資料失敗')
  } finally {
    loading.value = false
  }
})
```

### 步驟 9：模板中設定 v-select

```vue
<v-select
  v-model="selectedValue"
  :items="[{ id: 0, itemValue: '= 請選擇 =' }, ...options]"
  item-title="itemValue"
  item-value="id"
  :loading="loading"
  density="compact"
  hide-details
/>
```

- `item-title="itemValue"`：下拉顯示的文字來自物件的 `itemValue` 屬性
- `item-value="id"`：v-model 綁定的值是物件的 `id`（數字型別）
- 手動加入 `{ id: 0, itemValue: '= 請選擇 =' }` 作為預設提示項
- 條件式判斷使用 `selectedValue !== 0` 而非字串比對

## 資料型別要點

| 位置 | 型別 | 說明 |
|------|------|------|
| 後端 DTO.Id | `int` | 資料庫主鍵 |
| 前端 SelectOptionItem.id | `number` | TypeScript 對應 |
| v-model (`selectedValue`) | `number \| ''` | 初始為空字串，選取後為數字 |
| v-select item-value | `"id"` | 字串指向物件屬性名稱 |
| v-select item-title | `"itemValue"` | 字串指向物件屬性名稱 |

## 常見錯誤

- **TS2367** (`selectedBene !== 0`): `selectedBene` 型別若為 `string` 無法與 `number` 比較 → 應改為 `ref<number | ''>('')`
- **item-title / item-value 拼錯**: Vuetify 不會報錯但下拉顯示空白 → 確認字串完全比對物件屬性名
- **watch 未重置 selectedValue**: 切換條件時若不清空，會殘留上一個條件的選取值
