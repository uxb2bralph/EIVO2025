<script setup lang="ts">
import ScrollToTop from '@/components/ScrollToTop.vue'
</script>

<template>
  <!--
    VLocaleProvider: 提供全域語言環境 (zh-TW)。
    VApp: 應用程式根層級殼層，套用全域變數與背景。
  -->
  <div class="v-locale-provider" lang="zh-TW">
    <div class="v-application">
      <RouterView />
      <ScrollToTop />
    </div>
  </div>
</template>

<style>
@import url('https://fonts.googleapis.com/css2?family=Prompt:wght@400;500;600&family=Noto+Sans+TC:wght@400;500;700&display=swap');
@import url('https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css');

/* ── Design tokens ──────────────────────────────────── */
:root {
  --color-bg: #0b1225;
  --color-surface: rgba(9, 18, 45, 0.85);
  --color-border: rgba(255, 255, 255, 0.08);
  --color-text: #e8edf4;
  --color-muted: rgba(255, 255, 255, 0.6);
  --color-accent: #3bc7ff;
  --color-accent-2: #2ee3c8;
  --color-nav-bg: rgba(9, 14, 29, 0.9);
  --nav-width: 280px;
  --header-height: 72px;
}

/* ── Reset ──────────────────────────────────────────── */
*,
*::before,
*::after {
  box-sizing: border-box;
}

body {
  margin: 0;
  font-family: 'Noto Sans TC', 'Prompt', 'Segoe UI', sans-serif;
  background: var(--color-bg);
  color: var(--color-text);
}

h1, h2, h3, h4, h5, h6 { margin: 0; }
p { margin: 0; }
ul { margin: 0; padding: 0; list-style: none; }
button { font-family: inherit; }

/* ── App shell ──────────────────────────────────────── */
.v-application {
  min-height: 100vh;
  background-image:
    radial-gradient(110% 80% at 10% 20%, rgba(51, 98, 255, 0.35), transparent),
    radial-gradient(140% 90% at 90% 0%, rgba(56, 200, 175, 0.25), transparent),
    linear-gradient(135deg, #0b1225 0%, #101a34 60%, #050a18 100%);
  color: var(--color-text);
}

/* ── Shared button utilities ────────────────────────── */
.btn {
  border: 0;
  border-radius: 999px;
  padding: 0.55rem 1.4rem;
  cursor: pointer;
  font-weight: 600;
  font-size: 0.9rem;
  font-family: inherit;
  transition: opacity 0.2s;
}

.btn:hover { opacity: 0.85; }

.btn.ghost {
  background: transparent;
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: var(--color-text);
}

.btn.primary {
  background: linear-gradient(120deg, #3bc7ff, #2ee3c8);
  color: #0a1329;
}

.btn.solid {
  background: linear-gradient(120deg, #6c63ff, #5de0e6);
  color: #0a1329;
}

.btn.link {
  background: transparent;
  color: #7dd3fc;
  padding: 0;
}

.muted {
  color: var(--color-muted);
  font-size: 0.875rem;
}
</style>

