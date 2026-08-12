<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
import NavMenu from './components/NavMenu.vue';

const route = useRoute();
</script>

<template>
  <div class="min-h-screen bg-bg-gray text-text-main font-sans flex">
    <NavMenu v-if="!route.meta?.hideNav" class="shrink-0" />
    <main class="flex-grow min-w-0 flex flex-col">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
</style>
