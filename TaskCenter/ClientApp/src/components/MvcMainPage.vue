<!-- MvcMainPage.vue – embeds the legacy MVC main page via iframe -->
<script setup>
import { computed } from 'vue'

const src = computed(() => '/Home/MainPage')
</script>

<template>
  <div class="mvc-wrapper">
    <iframe
      :src="src"
      class="mvc-frame"
      title="電子發票系統主頁"
      frameborder="0"
      allowfullscreen
    ></iframe>
  </div>
</template>

<style scoped>
.mvc-wrapper {
  width: 100%;
  height: calc(100vh - var(--header-height) - 3rem);
  min-height: 400px;
}
.mvc-frame {
  width: 100%;
  height: 100%;
  border: none;
  border-radius: 0.5rem;
  background: #fff;
}
</style>
