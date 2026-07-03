# Frontend API Service Generator

## Task
Create a frontend API service file that follows established patterns in the HNB.Web project for making HTTP requests to backend endpoints.

## Template Structure
```typescript
import { apiRequest } from '@/services/api-service'
import type { {DTO_TYPE} } from '@/types/{MODULE_NAME}'

export async function {FUNCTION_NAME}(id: number): Promise<{DTO_TYPE}> {
  const res = await apiRequest<{DTO_TYPE}>(`/{ENDPOINT_PATH}/{id}`)
  if (!res.data) throw new Error(res.message ?? '查無資料')
  return res.data
}
```

## Key Points
- Use the existing `apiRequest` service for all HTTP calls
- Import DTO types from the appropriate module path (`@/types/{MODULE_NAME}`)
- All functions should take an `id: number` parameter
- Use consistent error handling with fallback message '查無資料'
- Validate that `res.data` exists before returning
- Follow the naming convention: `get{EntityName}Detail` for detail functions

## Example Implementation
```typescript
import { apiRequest } from '@/services/api-service'
import type { AmendAppDetailDto } from '@/types/lcApp'

export async function getAmendAppDetail(id: number): Promise<AmendAppDetailDto> {
  const res = await apiRequest<AmendAppDetailDto>(`/query/amend-app/${id}`)
  if (!res.data) throw new Error(res.message ?? '查無修改申請書明細')
  return res.data
}
```

## Usage
Provide the following information:
- DTO type name
- Module name (for type import path)
- Function name (should describe what data is being fetched)
- API endpoint path (without the ID, as ID is appended automatically)