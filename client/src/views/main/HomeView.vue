<script setup>
import { ref } from 'vue';

// Mock data for the dashboard
const totalNetWorth = ref(124500.75);
const trendPercentage = ref(2.4);

const recentActivities = ref([
  { id: 1, type: 'active', action: 'Closed Long', asset: 'NQ1!', amount: '+$450.00', date: '2 hours ago', icon: 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6' },
  { id: 2, type: 'dca', action: 'Bought', asset: '0.05 BTC', amount: '-$3,120.50', date: '5 hours ago', icon: 'M12 6v6m0 0v6m0-6h6m-6 0H6' },
  { id: 3, type: 'active', action: 'Logged Setup', asset: 'ORB Strategy', amount: '', date: '1 day ago', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
  { id: 4, type: 'dca', action: 'Bought', asset: '10 ETH', amount: '-$22,500.00', date: '2 days ago', icon: 'M12 6v6m0 0v6m0-6h6m-6 0H6' },
  { id: 5, type: 'active', action: 'Stopped Out', asset: 'ES1!', amount: '-$150.00', date: '3 days ago', icon: 'M6 18L18 6M6 6l12 12' },
]);

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
};
</script>

<template>
  <div class="p-6 md:p-10 max-w-7xl mx-auto space-y-8 relative z-10 w-full">
    <header class="mb-8 mt-4">
      <h1 class="text-3xl font-extrabold text-text-main tracking-tight">Command Center</h1>
      <p class="text-text-muted mt-1">Your high-level portfolio overview.</p>
    </header>

    <!-- Top Section: Net Worth & Quick Actions -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Total Portfolio Net Worth -->
      <div class="lg:col-span-2 bg-white rounded-3xl p-8 shadow-sm border border-gray-100 flex flex-col justify-center relative overflow-hidden group">
        <!-- Subtle decorative background -->
        <div class="absolute top-0 right-0 -mr-16 -mt-16 w-64 h-64 bg-light-blue rounded-full opacity-20 group-hover:scale-110 transition-transform duration-700 pointer-events-none"></div>
        
        <p class="text-sm font-semibold text-text-muted uppercase tracking-wider mb-2">Total Portfolio Net Worth</p>
        <div class="flex flex-wrap items-end gap-4">
          <h2 class="text-5xl md:text-6xl font-black text-text-main tracking-tighter">{{ formatCurrency(totalNetWorth) }}</h2>
          <div class="flex items-center bg-green-50 text-green-700 px-3 py-1 rounded-lg text-sm font-bold mb-2">
            <svg class="w-4 h-4 mr-1" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"></path></svg>
            +{{ trendPercentage }}%
          </div>
        </div>
      </div>

      <!-- Quick Actions -->
      <div class="flex flex-col gap-4">
        <button class="flex-1 bg-primary text-white rounded-2xl p-6 shadow-md hover:shadow-lg hover:bg-opacity-95 transition-all transform hover:-translate-y-1 flex flex-col items-start justify-between group">
          <div class="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center mb-4 text-white group-hover:scale-110 transition-transform">
             <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6"></path></svg>
          </div>
          <span class="font-bold text-lg">Log DCA Purchase</span>
        </button>
        
        <button class="flex-1 bg-primary text-white rounded-2xl p-6 shadow-md hover:shadow-lg hover:bg-opacity-95 transition-all transform hover:-translate-y-1 flex flex-col items-start justify-between group">
          <div class="w-10 h-10 bg-white/20 rounded-xl flex items-center justify-center mb-4 text-white group-hover:scale-110 transition-transform">
             <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"></path></svg>
          </div>
          <span class="font-bold text-lg">Log Active Trade</span>
        </button>
      </div>
    </div>

    <!-- Recent Activity Feed -->
    <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-6 md:px-8 py-6 border-b border-gray-50 flex justify-between items-center">
        <h3 class="text-xl font-bold text-text-main">Recent Activity</h3>
        <button class="text-sm font-medium text-primary hover:text-opacity-80 transition-colors">View All</button>
      </div>
      
      <div class="divide-y divide-gray-50">
        <div v-for="activity in recentActivities" :key="activity.id" class="px-6 md:px-8 py-5 hover:bg-gray-50 transition-colors flex items-center justify-between group">
          <div class="flex items-center gap-4">
            <div :class="[
              'w-12 h-12 rounded-xl flex items-center justify-center shrink-0', 
              activity.type === 'dca' ? 'bg-light-blue text-primary' : 'bg-yellow/20 text-secondary'
            ]">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" :d="activity.icon"></path>
              </svg>
            </div>
            <div>
              <p class="font-bold text-text-main">{{ activity.action }} <span class="text-text-muted font-medium ml-1">{{ activity.asset }}</span></p>
              <p class="text-sm text-text-muted mt-0.5">{{ activity.date }} &bull; <span class="capitalize">{{ activity.type === 'dca' ? 'Investing Tracker' : 'Active Trading' }}</span></p>
            </div>
          </div>
          <div class="text-right pl-2">
            <span :class="[
              'font-bold',
              activity.amount.startsWith('+') ? 'text-green-600' : (activity.amount.startsWith('-') ? 'text-red-500' : 'text-text-main')
            ]">
              {{ activity.amount }}
            </span>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<style scoped>
</style>
