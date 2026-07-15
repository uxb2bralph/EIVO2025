<!--
  Pager.vue – 共用分頁器（純呈現元件）。
  抽離自各查詢頁（OrganizationQueryIndex / UserAccountIndex / TrackCodeIndex）重複的分頁列。
  僅需 page / totalPages / totalCount，內部自算要顯示的頁碼清單與上下頁狀態，
  透過 change 事件回傳目標頁碼，換頁邏輯（載入資料）仍由父層 composable 負責。
-->
<script setup lang="ts">
import { computed } from 'vue'

const props = withDefaults(
  defineProps<{
    /** 目前頁碼（1-based） */
    page: number
    /** 總頁數 */
    totalPages: number
    /** 總筆數（顯示於頁尾資訊） */
    totalCount: number
    /** 分頁器一次顯示的頁碼數量 */
    windowSize?: number
  }>(),
  { windowSize: 10 },
)

const emit = defineEmits<{
  /** 使用者要求換頁，帶出目標頁碼 */
  (e: 'change', page: number): void
}>()

const hasPrev = computed(() => props.page > 1)
const hasNext = computed(() => props.page < props.totalPages)

// 以目前頁碼為中心，計算要顯示的頁碼清單（夾在 1 ~ totalPages 之間）
const pageNumbers = computed(() => {
  const total = props.totalPages
  const windowSize = props.windowSize
  if (total <= 0) return []
  let start = props.page - Math.floor(windowSize / 2)
  start = Math.max(1, Math.min(start, total - windowSize + 1))
  start = Math.max(1, start)
  const end = Math.min(total, start + windowSize - 1)
  const pages: number[] = []
  for (let p = start; p <= end; p++) pages.push(p)
  return pages
})

function go(target: number) {
  // 夾在 1 ~ totalPages 之間，讓「上/下 10 頁」在邊界附近仍可跳到最前/最後頁
  const clamped = Math.max(1, Math.min(target, props.totalPages))
  if (clamped !== props.page) {
    emit('change', clamped)
  }
}
</script>

<template>
  <nav v-if="totalPages > 1" class="pager">
    <button class="btn ghost" :disabled="!hasPrev" @click="go(1)">⏮</button>
    <button class="btn ghost" :disabled="!hasPrev" @click="go(page - 10)">⏪</button>
    <button class="btn ghost" :disabled="!hasPrev" @click="go(page - 1)">◀</button>
    <span class="page-numbers">
      <button
        v-for="p in pageNumbers"
        :key="p"
        class="btn ghost page-num"
        :class="{ active: p === page }"
        @click="go(p)"
      >
        {{ p }}
      </button>
    </span>
    <button class="btn ghost" :disabled="!hasNext" @click="go(page + 1)">▶</button>
    <button class="btn ghost" :disabled="!hasNext" @click="go(page + 10)">⏩</button>
    <button class="btn ghost" :disabled="!hasNext" @click="go(totalPages)">⏭</button>
    <span class="pager-info">第 {{ page }} / {{ totalPages }} 頁（共 {{ totalCount }} 筆）</span>
  </nav>
</template>

<style scoped>
.pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  margin-top: 1.25rem;
  padding-top: 1.25rem;
  border-top: 1px solid var(--color-border);
}
.pager-info {
  color: var(--color-muted);
  font-size: 0.9rem;
}
.page-numbers {
  display: flex;
  align-items: center;
  gap: 0.4rem;
}
.page-num {
  min-width: 2.25rem;
  padding: 0.4rem 0.6rem;
  text-align: center;
}
.page-num.active {
  background: var(--color-accent);
  border-color: var(--color-accent);
  color: #fff;
  cursor: default;
}
.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
.btn.ghost {
  background: transparent;
  border: 1px solid var(--color-border);
  color: var(--color-text);
}
.btn.ghost:hover:not(:disabled) {
  border-color: var(--color-accent);
}
</style>
