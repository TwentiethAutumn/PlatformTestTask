<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="page">
    <header>
      <div class="wrapper">
      </div>
    </header>

    <RouterView />
  </div>
</template>

<style scoped>
.page {
  width: 100%;
  height: 100%;
}
</style>
