<script setup>
import { ref } from 'vue';

const activeTab = ref('active-trading'); // 'investing' | 'active-trading'

// Mock Data for Strategies
const strategies = ref([
  { id: 1, name: 'Opening Range Breakout', description: 'Trading the breakout of the first 15m candle.' },
  { id: 2, name: 'VWAP Rejection', description: 'Shorting or going long when price rejects VWAP.' }
]);

const newStrategy = ref({
  name: '',
  description: ''
});

const isSubmitting = ref(false);
const successMessage = ref('');
const errorMessage = ref('');

const handleAddStrategy = async () => {
  if (isSubmitting.value) return;
  if (!newStrategy.value.name) {
    errorMessage.value = 'Strategy Name is required.';
    return;
  }
  
  isSubmitting.value = true;
  errorMessage.value = '';
  
  try {
    // Mock backend call
    await new Promise(resolve => setTimeout(resolve, 600));
    
    strategies.value.push({
      id: Date.now(),
      name: newStrategy.value.name,
      description: newStrategy.value.description
    });
    
    successMessage.value = `Strategy "${newStrategy.value.name}" added successfully.`;
    
    newStrategy.value.name = '';
    newStrategy.value.description = '';
    
    setTimeout(() => { successMessage.value = ''; }, 4000);
  } catch (error) {
    errorMessage.value = 'Failed to add strategy.';
  } finally {
    isSubmitting.value = false;
  }
};
</script>

<template>
  <div class="p-6 md:p-10 max-w-7xl mx-auto space-y-8 w-full relative">
    <!-- Global Success Toast -->
    <div v-if="successMessage" class="fixed top-8 right-8 z-50 bg-green-50 border border-green-200 text-green-800 px-6 py-4 rounded-2xl shadow-xl shadow-green-900/5 flex items-center gap-3 animate-[fadeIn_0.3s_ease-out]">
      <svg class="w-6 h-6 text-green-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
      <p class="font-bold text-sm">{{ successMessage }}</p>
    </div>

    <header class="mb-8 mt-4">
      <h1 class="text-3xl font-extrabold text-text-main tracking-tight">System Settings</h1>
      <p class="text-text-muted mt-1">Configure your modules and mechanical strategies.</p>
    </header>

    <!-- Tabs Navigation -->
    <div class="flex gap-4 border-b border-gray-200 mb-6">
      <button 
        @click="activeTab = 'investing'" 
        class="pb-4 px-2 font-bold text-sm transition-colors border-b-2"
        :class="activeTab === 'investing' ? 'border-primary text-primary' : 'border-transparent text-text-muted hover:text-text-main hover:border-gray-300'"
      >
        Investing Tracker
      </button>
      <button 
        @click="activeTab = 'active-trading'" 
        class="pb-4 px-2 font-bold text-sm transition-colors border-b-2"
        :class="activeTab === 'active-trading' ? 'border-secondary text-secondary' : 'border-transparent text-text-muted hover:text-text-main hover:border-gray-300'"
      >
        Active Trading
      </button>
    </div>

    <!-- Investing Tracker Settings -->
    <div v-if="activeTab === 'investing'" class="animate-[fadeIn_0.2s_ease-out]">
      <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100 flex items-center justify-center min-h-[300px]">
        <div class="text-center">
          <svg class="mx-auto h-12 w-12 text-gray-300 mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"></path>
          </svg>
          <h3 class="text-lg font-bold text-text-main mb-1">Investing Settings</h3>
          <p class="text-sm text-text-muted">No configuration required for Module A at this time.</p>
        </div>
      </div>
    </div>

    <!-- Active Trading Settings -->
    <div v-if="activeTab === 'active-trading'" class="space-y-8 animate-[fadeIn_0.2s_ease-out]">
      
      <!-- Strategy Manager -->
      <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100">
        <div class="flex items-center gap-3 mb-6">
          <div class="w-10 h-10 rounded-full bg-secondary/10 text-secondary flex items-center justify-center">
             <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
          </div>
          <div>
            <h2 class="text-xl font-bold text-text-main">Strategy Manager</h2>
            <p class="text-sm text-text-muted mt-0.5">Define strict, mechanical strategies to tag your trades.</p>
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-10">
          
          <!-- Add New Strategy Form -->
          <div>
            <h3 class="text-sm font-bold text-text-main uppercase tracking-wider mb-4 border-b border-gray-100 pb-2">Add New Strategy</h3>
            
            <div v-if="errorMessage" class="mb-4 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3">
              <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
              <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
            </div>

            <form @submit.prevent="handleAddStrategy" class="space-y-4">
              <div>
                <label class="block text-sm font-semibold text-text-main mb-1.5">Strategy Name</label>
                <input v-model="newStrategy.name" type="text" required placeholder="e.g. Mean Reversion" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted" />
              </div>
              
              <div>
                <label class="block text-sm font-semibold text-text-main mb-1.5">Description (Optional)</label>
                <textarea v-model="newStrategy.description" rows="3" placeholder="Brief mechanical rules for this strategy..." class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted"></textarea>
              </div>

              <button type="submit" class="w-full py-3.5 rounded-xl bg-secondary text-white font-bold shadow-md shadow-secondary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all disabled:opacity-70 disabled:hover:translate-y-0" :disabled="isSubmitting">
                {{ isSubmitting ? 'Saving...' : 'Save Strategy' }}
              </button>
            </form>
          </div>

          <!-- Existing Strategies List -->
          <div>
            <h3 class="text-sm font-bold text-text-main uppercase tracking-wider mb-4 border-b border-gray-100 pb-2">Active Strategies</h3>
            <div class="space-y-3 max-h-[400px] overflow-y-auto pr-2 custom-scrollbar">
              <div v-for="strategy in strategies" :key="strategy.id" class="p-4 border border-gray-100 rounded-2xl bg-gray-50/50 hover:bg-white hover:border-gray-200 transition-colors group">
                <div class="flex justify-between items-start">
                  <div>
                    <h4 class="font-bold text-text-main text-sm">{{ strategy.name }}</h4>
                    <p class="text-xs text-text-muted mt-1 leading-relaxed">{{ strategy.description || 'No description provided.' }}</p>
                  </div>
                  <button class="text-gray-400 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-opacity" title="Archive Strategy">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"></path></svg>
                  </button>
                </div>
              </div>
              <div v-if="strategies.length === 0" class="text-center py-8 text-text-muted text-sm italic">
                No strategies defined yet.
              </div>
            </div>
          </div>
          
        </div>
      </div>
      
    </div>
  </div>
</template>

<style scoped>
/* Premium thin scrollbar for the modal */
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background-color: #e5e7eb;
  border-radius: 10px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background-color: #d1d5db;
}
</style>
