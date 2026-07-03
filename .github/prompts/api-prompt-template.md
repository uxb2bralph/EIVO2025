# Frontend API Service Generator Prompt

## Task
Create a frontend API service file that follows the established patterns in the HNB.Web project for making HTTP requests to backend endpoints.

## Template
```typescript
import { apiRequest } from '@/services/api-service'
import type { {DTO_TYPE} } from '@/types/{MODULE_NAME}'

export async function {FUNCTION_NAME}(id: number): Promise<{DTO_TYPE}> {
  const res = await apiRequest<{DTO_TYPE}>(`/{ENDPOINT_PATH}/{id}`)
  if (!res.data) throw new Error(res.message ?? '查無資料')
  return res.data
}
```

## Parameters to Replace
- `{DTO_TYPE}`: The data transfer object type being returned
- `{MODULE_NAME}`: The module/type category (e.g., lcApp, credit, etc.)
- `{FUNCTION_NAME}`: The function name describing what data is being fetched
- `{ENDPOINT_PATH}`: The API endpoint path

## Example Usage
For a function that fetches amendment application details:
```typescript
import { apiRequest } from '@/services/api-service'
import type { AmendAppDetailDto } from '@/types/lcApp'

export async function getAmendAppDetail(id: number): Promise<AmendAppDetailDto> {
  const res = await apiRequest<AmendAppDetailDto>(`/query/amend-app/${id}`)
  if (!res.data) throw new Error(res.message ?? '查無修改申請書明細')
  return res.data
}
```

## Best Practices
- Use consistent error messages with fallback '查無資料'
- Follow existing import patterns from the project
- Use the `apiRequest` service for all HTTP calls
- Always validate response data exists before returning
- Import DTO types from the appropriate module path
- Use proper TypeScript typing