<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <div class="min-h-screen flex flex-col bg-bg-gray relative overflow-hidden">
    <!-- Decorative background elements -->
    <div class="absolute top-[-10%] left-[-10%] w-[40%] h-[40%] bg-primary rounded-full blur-[120px] opacity-20"></div>
    <div class="absolute bottom-[-10%] right-[-10%] w-[40%] h-[40%] bg-secondary rounded-full blur-[120px] opacity-20"></div>

    <header class="w-full py-6 px-8 flex justify-between items-center z-10 relative">
      <div class="text-2xl font-black text-text-main tracking-tight flex items-center gap-2">
        <div class="w-8 h-8 bg-primary rounded-lg flex items-center justify-center text-white font-bold">T</div>
        TradeSync<span class="text-primary">.</span>
      </div>
      <nav class="flex gap-4">
        <RouterLink to="/auth/login" class="px-6 py-2 rounded-full text-text-main font-medium hover:bg-light-blue transition-colors duration-300">Log In</RouterLink>
        <RouterLink to="/auth/register" class="px-6 py-2 rounded-full bg-primary text-white font-medium hover:bg-opacity-90 shadow-lg hover:shadow-primary/30 transition-all duration-300 transform hover:-translate-y-0.5">Get Started</RouterLink>
      </nav>
    </header>

    <main class="flex-grow flex flex-col justify-center items-center px-4 sm:px-6 lg:px-8 z-10 relative text-center">
      <span class="px-4 py-1.5 rounded-full bg-light-blue text-primary text-sm font-bold tracking-wide uppercase mb-6 shadow-sm border border-primary/20">The Dual-Engine Approach</span>
      
      <h1 class="text-5xl md:text-7xl font-extrabold text-text-main max-w-4xl leading-tight tracking-tight mb-6">
        Master your <span class="text-transparent bg-clip-text bg-gradient-to-r from-primary to-blue-600">Investments</span> &<br/> Perfect your <span class="text-transparent bg-clip-text bg-gradient-to-r from-secondary to-orange-500">Trades</span>.
      </h1>
      
      <p class="text-lg md:text-xl text-text-muted max-w-2xl mb-10 leading-relaxed">
        A self-hosted, full-stack application designed to completely separate your long-term asset accumulation from your active, mechanical trading strategies.
      </p>

      <div class="flex flex-col sm:flex-row gap-4 mb-16">
        <RouterLink to="/auth/register" class="px-8 py-4 rounded-full bg-primary text-white font-bold text-lg hover:bg-opacity-90 shadow-xl hover:shadow-primary/40 transition-all duration-300 transform hover:-translate-y-1 flex items-center justify-center gap-2">
          Start Journaling Free
          <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
            <path fill-rule="evenodd" d="M10.293 3.293a1 1 0 011.414 0l6 6a1 1 0 010 1.414l-6 6a1 1 0 01-1.414-1.414L14.586 11H3a1 1 0 110-2h11.586l-4.293-4.293a1 1 0 010-1.414z" clip-rule="evenodd" />
          </svg>
        </RouterLink>
        <a href="#features" class="px-8 py-4 rounded-full bg-white text-text-main font-bold text-lg border border-gray-200 hover:border-light-blue hover:bg-gray-50 transition-all duration-300 flex items-center justify-center">
          Explore Features
        </a>
      </div>

      <!-- Feature Cards -->
      <div id="features" class="grid grid-cols-1 md:grid-cols-2 gap-8 max-w-5xl w-full mt-10 mb-16">
        <div class="bg-white/60 backdrop-blur-xl p-8 rounded-3xl shadow-xl border border-white/40 text-left group hover:-translate-y-2 transition-all duration-300 hover:shadow-primary/20">
          <div class="w-14 h-14 bg-light-blue rounded-2xl flex items-center justify-center mb-6 text-primary group-hover:scale-110 transition-transform duration-300">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6" />
            </svg>
          </div>
          <h3 class="text-2xl font-bold text-text-main mb-3">Investing Tracker</h3>
          <p class="text-text-muted">Built for long-term portfolio growth. Log asset purchases, group into DCA campaigns, and track lifecycle phases automatically.</p>
        </div>

        <div class="bg-white/60 backdrop-blur-xl p-8 rounded-3xl shadow-xl border border-white/40 text-left group hover:-translate-y-2 transition-all duration-300 hover:shadow-secondary/20">
          <div class="w-14 h-14 bg-yellow/20 rounded-2xl flex items-center justify-center mb-6 text-secondary group-hover:scale-110 transition-transform duration-300">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </div>
          <h3 class="text-2xl font-bold text-text-main mb-3">Active Trading Journal</h3>
          <p class="text-text-muted">Isolated environment for strict mechanical setups. Measure realized PnL, win rates, and attach charts for rigorous review.</p>
        </div>
      </div>
    </main>
  </div>
</template>

<style scoped>
</style>