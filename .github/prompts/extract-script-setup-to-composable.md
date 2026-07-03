# Prompt: 將 Vue `<script setup>` 抽離為 Composable

## 使用時機

當一個 `.vue` 頁面元件的 `<script setup>` 過長，想將所有邏輯封裝到獨立 `.ts` 檔，以 composable 形式供元件調用。

---

## 指令模板

```
請將 [檔案路徑] 中的 <script setup> 代碼抽離為獨立的 composable .ts 檔。

規則：
1. Composable 命名為 `use[ComponentName]`，放在 src/components 目錄下
2. 將所有 reactive state、computed、function、onMounted 移入 composable
3. `provide()` 呼叫也移入 composable（在 setup context 中呼叫 composable 時，provide 完全合法）
4. Vue component 保留最精簡的 <script setup>：只 import composable 並解構其回傳值
5. Template 完全不改動
6. 不需要顯式 import 已在 auto-import 範圍內的組件（src/components/、@core/components/）
```

---

## 執行步驟

### Step 1 — 建立 `use[ComponentName].ts`

移入以下內容：

| 類型 | 說明 |
|------|------|
| Vue composable | `useRouter()`, `useRoute()`, `useI18n()`, `useXxxStore()` 等 |
| Reactive state | `ref()`, `reactive()` 宣告的所有狀態 |
| Template refs | `ref<InstanceType<typeof SomeComponent>>()` 型別的 ref |
| Computed | 所有 `computed()` |
| Functions | 所有事件處理器、業務邏輯函式 |
| Lifecycle | `onMounted()` 等 |
| `provide()` | **移入 composable**（不留在元件），放在 `modelValue` 建立後立即呼叫 |

回傳物件包含所有 template 需要存取的 binding。

### Step 2 — 精簡 `.vue` 元件

```vue
<script setup lang="ts">
import { useXxx } from './useXxx'

const {
  // 解構所有 template 用到的 binding
} = useXxx()
</script>

<!-- template 保持不動 -->
```

---

## 已知陷阱與解法

### `provide()` 位置
`provide()` 必須在 Vue 的 setup context 中呼叫。由於 composable 是在 `<script setup>` 期間被呼叫，因此在 composable 內部呼叫 `provide()` 完全合法。**不需要**保留在元件中。

### Template ref（`ref="xxx"`）
Template ref 變數（如 `const paymentSetupRef = ref<InstanceType<typeof Foo>>()`）必須存在於 `<script setup>` 的作用域內。從 composable 解構後帶入 scope 即可，template 的 `ref="paymentSetupRef"` 會正常運作。TypeScript 可能顯示「declared but never read」hint（6133），這是已知限制，不影響功能。

### Import 順序（ESLint `import/order`）
相對路徑 import（`./useXxx`）在 ESLint 排序規則中需放在 `@/...` alias import **之前**（或之後，依專案設定）。若出現 import order error，調整排序後重試。

### Auto-imported 組件
本專案透過 `unplugin-vue-components` 自動 import `src/components/` 與 `@core/components/` 內的元件。在精簡後的 `.vue` 元件中，**不需要**顯式 import 這些元件。

### `@/constants/...` 無副檔名 import
在 `.vue` 檔中 import `@/constants/` 下的 `.ts` 檔時，ESLint 的 `import/extensions` 規則行為可能矛盾（有時要求加 `.ts`，有時又禁止）。最佳解法是將該 import 移入 composable 的 `.ts` 檔，在那裡通常不會觸發此規則。

---

## 產出結構範例

```
src/pages/sales-operations/
  process-receivable-payment.vue    ← 只剩 import + 解構 + template
src/components  
  useReceivablePayment.ts           ← 所有邏輯（含 provide）
```
