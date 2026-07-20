/**
 * app-base.ts — 執行期取得應用程式部署根路徑
 *
 * 值來自伺服器在回傳 index.html 時依 `Request.PathBase` 注入的 `<base href>`：
 *   - 部署於 IIS 子應用程式 /TaskCenter/  → '/TaskCenter/'
 *   - 部署於站台根 / 或本機 dev（無 <base>）→ '/'
 *
 * 前端不需知道實際子路徑名稱，換部署路徑無須重新 build。
 */
export const appBase = import.meta.env.VITE_BASE_URL ?? new URL(document.baseURI).pathname
