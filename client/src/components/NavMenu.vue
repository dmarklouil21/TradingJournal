<script setup>
import { computed } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';

const route = useRoute();
const router = useRouter();

const navItems = [
  {
    name: 'Dashboard',
    path: '/home',
    icon: 'M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6'
  },
  {
    name: 'Investing Tracker',
    path: '/investing',
    icon: 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6'
  },
  {
    name: 'Active Trading',
    path: '/trading',
    icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z'
  },
  {
    name: 'Settings',
    path: '/settings',
    icon: 'M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z'
  }
];

const handleLogout = () => {
  // Add actual logout logic here later
  router.push('/');
};
</script>

<template>
  <aside class="w-64 bg-white h-screen sticky top-0 border-r border-gray-100 flex flex-col z-20 shrink-0">
    <!-- Logo -->
    <div class="h-24 flex items-center px-6 border-b border-gray-50">
      <RouterLink to="/home" class="text-xl font-black text-text-main tracking-tight flex items-center gap-2 group">
        <div class="w-8 h-8 bg-primary rounded-lg flex items-center justify-center text-white font-bold group-hover:scale-105 transition-transform">T</div>
        TradeSync<span class="text-primary">.</span>
      </RouterLink>
    </div>

    <!-- Navigation Links -->
    <nav class="flex-grow px-4 py-8 space-y-1.5 overflow-y-auto">
      <p class="px-3 text-xs font-bold text-text-muted uppercase tracking-wider mb-4">Menu</p>
      
      <RouterLink 
        v-for="item in navItems" 
        :key="item.path" 
        :to="item.path"
        class="flex items-center gap-3 px-3 py-3 rounded-xl font-semibold transition-all duration-200 group"
        :class="[
          route.path === item.path 
            ? 'bg-light-blue text-primary' 
            : 'text-text-muted hover:bg-gray-50 hover:text-text-main'
        ]"
      >
        <svg 
          class="w-5 h-5 transition-colors duration-200 shrink-0" 
          :class="route.path === item.path ? 'text-primary' : 'text-gray-400 group-hover:text-primary'"
          fill="none" 
          stroke="currentColor" 
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="item.icon"></path>
          <path v-if="item.name === 'Settings'" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"></path>
        </svg>
        {{ item.name }}
      </RouterLink>
    </nav>

    <!-- Bottom Section / Logout -->
    <div class="p-4 border-t border-gray-50 mb-2">
      <button 
        @click="handleLogout"
        class="w-full flex items-center gap-3 px-3 py-3 rounded-xl font-semibold text-text-muted hover:bg-red-50 hover:text-red-500 transition-colors duration-200 group"
      >
        <svg class="w-5 h-5 text-gray-400 group-hover:text-red-500 transition-colors shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1"></path>
        </svg>
        Log Out
      </button>
    </div>
  </aside>
</template>

<style scoped>
</style>
