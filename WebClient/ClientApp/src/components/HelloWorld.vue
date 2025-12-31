<template>
  <section>
    <h1>{{ msg }}</h1>
    <p>This is a small starter component.</p>
  </section>
</template>

<script setup>
defineProps({ msg: String })
</script>

<style scoped>
h1 { color: #42b983; }
</style>
