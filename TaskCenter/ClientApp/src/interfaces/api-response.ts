/**
 * api-response.ts — 後端統一 API 回應格式
 *
 * 對應後端 `ResponseDto<T>` / `BaseResponseDto`（已透過 [JsonPropertyName] 統一為 camelCase）。
 * 詳見 backend-api-spec.md「7. Response 風格」。
 */

/**
 * 統一 API 回應結構。`apiRequest` 永遠回傳此型別（成功或失敗皆然）。
 */
export interface ApiResponse<T = unknown> {
  /** 操作是否成功 */
  success: boolean
  /** 人類可讀的訊息（後端已翻譯） */
  message: string
  /** 回傳資料，失敗時為 undefined */
  data?: T
  /** 驗證或業務錯誤明細，成功時為 undefined */
  errors?: string[]
}

/**
 * 分頁回應結構，對應後端 `PagedResultDto<T>`。
 */
export interface PagedResult<T = unknown> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}
