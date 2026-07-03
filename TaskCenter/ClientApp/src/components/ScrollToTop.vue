<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const visible = ref(false)

function onScroll() {
  visible.value = window.scrollY > 300
}

function scrollTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

onMounted(() => window.addEventListener('scroll', onScroll))
onUnmounted(() => window.removeEventListener('scroll', onScroll))
</script>

<template>
  <transition name="fade">
    <button
      v-if="visible"
      class="scroll-to-top"
      aria-label="回到頂部"
      @click="scrollTop"
    >
      <i class="fa fa-chevron-up"></i>
    </button>
  </transition>
</template>

<style scoped>
.scroll-to-top {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: 50%;
  background: linear-gradient(120deg, #3bc7ff, #2ee3c8);
  color: #0a1329;
  border: none;
  cursor: pointer;
  font-size: 1rem;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(59, 199, 255, 0.4);
  z-index: 999;
  transition: opacity 0.2s;
}
.scroll-to-top:hover { opacity: 0.85; }
.fade-enter-active, .fade-leave-active { transition: opacity 0.25s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
