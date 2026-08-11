<script setup>
import { RouterLink, RouterView, useRoute } from 'vue-router'
import NavMenu from './components/NavMenu.vue';

const route = useRoute();
</script>

<template>
  <div class="min-h-screen bg-bg-gray text-text-main font-sans">
    <NavMenu v-if="!route.meta?.hideNav" />
    <main>
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
</style>
