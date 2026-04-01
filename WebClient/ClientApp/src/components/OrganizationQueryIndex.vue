<!-- OrganizationQueryIndex.vue – organization query stub (migrated from OrganizationQueryController) -->
<script setup>
// TODO: migrate full OrganizationQuery UI from MVC to Vue 3
</script>

<template>
  <div class="stub-page">
    <div class="stub-card">
      <i class="fa fa-building-o stub-icon"></i>
      <h2>營業人資料管理</h2>
      <p class="muted">此頁面正在遷移中，請暫時使用舊版介面。</p>
      <a href="/OrganizationQuery/Index" class="btn primary">前往舊版介面</a>
    </div>
  </div>
</template>

<style scoped>
.stub-page {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
}
.stub-card {
  text-align: center;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: 1rem;
  padding: 3rem 2rem;
  max-width: 400px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
}
.stub-icon { font-size: 3rem; color: var(--color-accent); }
h2 { font-size: 1.25rem; }
.muted { color: var(--color-muted); font-size: 0.9rem; }
.btn.primary {
  background: linear-gradient(120deg, #3bc7ff, #2ee3c8);
  color: #0a1329;
  font-weight: 600;
  border-radius: 999px;
  padding: 0.55rem 1.4rem;
  text-decoration: none;
  font-size: 0.9rem;
}
</style>
