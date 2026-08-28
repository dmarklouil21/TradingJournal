<script setup>
import { ref, onMounted } from 'vue';
import { fetchStrategies } from '@/services/settings';
import { submitNewTrade, fetchTrades } from '@/services/activetrading';

// KPIs
const totalRealizedPnL = ref(0);
const overallWinRate = ref(0);
const profitFactor = ref(0.00);

// Strategy Performance
const strategies = ref([]);

// Available Strategies from Backend
const availableStrategies = ref([]);

const loadStrategies = async () => {
  try {
    const res = await fetchStrategies();
    availableStrategies.value = res.data;
  } catch (ex) {
    console.error("Failed to load strategies:", ex);
  }
};

// Trade Review Queue
const reviewQueue = ref([]);

const loadTrades = async () => {
  try {
    const res = await fetchTrades();
    const rawTrades = res.data;
    
    reviewQueue.value = rawTrades.map(t => ({
      ...t,
      date: new Date(t.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
    }));

    // Calculate KPIs
    // const closedTrades = rawTrades.filter(t => t.status !== 'Open');
    
    totalRealizedPnL.value = rawTrades.reduce((sum, t) => sum + (t.pnl || 0), 0);
    
    const wins = rawTrades.filter(t => t.pnl > 0).length;
    overallWinRate.value = rawTrades.length > 0 ? ((wins / rawTrades.length) * 100).toFixed(1) : 0;
    
    const grossProfit = rawTrades.filter(t => t.pnl > 0).reduce((sum, t) => sum + t.pnl, 0);
    const grossLoss = Math.abs(rawTrades.filter(t => t.pnl < 0).reduce((sum, t) => sum + t.pnl, 0));
    profitFactor.value = grossLoss > 0 ? (grossProfit / grossLoss).toFixed(2) : (grossProfit > 0 ? '∞' : '0.00');

    // Calculate Strategy Performance
    const strategyMap = {};
    rawTrades.forEach(t => {
      if(!strategyMap[t.strategy]) {
        strategyMap[t.strategy] = { name: t.strategy, wins: 0, pnl: 0, count: 0 };
      }
      strategyMap[t.strategy].count++;
      strategyMap[t.strategy].pnl += (t.pnl || 0);
      if(t.pnl > 0) strategyMap[t.strategy].wins++;
    });

    strategies.value = Object.values(strategyMap).map(s => ({
      name: s.name,
      winRate: s.count > 0 ? Math.round((s.wins / s.count) * 100) : 0,
      pnl: s.pnl,
      count: s.count
    })).sort((a, b) => b.pnl - a.pnl);
    
  } catch (ex) {
    console.error("Failed to load trades:", ex);
  }
};

onMounted(() => {
  loadStrategies();
  loadTrades();
});

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
};

// Log Trade Modal State
const isLogTradeModalOpen = ref(false);
const isSubmitting = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
const newTrade = ref({
  instrument: '',
  positionType: 'Long',
  entryDate: new Date().toISOString().split('T')[0],
  exitDate: new Date().toISOString().split('T')[0],
  entryPrice: null,
  exitPrice: null,
  positionSize: null,
  realizedPnL: null,
  strategyId: '',
  reviewNotes: '',
  chartImage: null,
  chartPreview: null,
});

const openLogTradeModal = () => {
  isLogTradeModalOpen.value = true;
};

const handleFileUpload = (event) => {
  const file = event.target.files[0];
  if (file) {
    newTrade.value.chartImage = file;
    newTrade.value.chartPreview = URL.createObjectURL(file);
  }
};

const removeFile = () => {
  newTrade.value.chartImage = null;
  newTrade.value.chartPreview = null;
};

// Review Modal State
const isReviewModalOpen = ref(false);
const selectedTrade = ref(null);

const openReviewModal = (trade) => {
  selectedTrade.value = trade;
  isReviewModalOpen.value = true;
};

const closeReviewModal = () => {
  isReviewModalOpen.value = false;
  setTimeout(() => { selectedTrade.value = null; }, 300); // Wait for animation
};

const closeLogTradeModal = () => {
  isLogTradeModalOpen.value = false;
  errorMessage.value = '';
  newTrade.value = {
    instrument: '',
    positionType: 'Long',
    entryDate: new Date().toISOString().split('T')[0],
    exitDate: new Date().toISOString().split('T')[0],
    entryPrice: null,
    exitPrice: null,
    positionSize: null,
    realizedPnL: null,
    strategyId: '',
    reviewNotes: '',
    chartImage: null,
    chartPreview: null,
  };
};

const handleLogTrade = async () => {
  if (isSubmitting.value) return;
  
  // Here we would submit the trade to the backend
  isSubmitting.value = true;
  errorMessage.value = '';
  
  try {
    const formData = new FormData();
    formData.append("instrument", newTrade.value.instrument);
    formData.append("positionType", newTrade.value.positionType);
    formData.append("entryDate", new Date(newTrade.value.entryDate).toISOString());
    formData.append("exitDate", newTrade.value.exitDate ? new Date(newTrade.value.exitDate).toISOString() : "");
    if (newTrade.value.entryPrice) formData.append("entryPrice", parseFloat(newTrade.value.entryPrice));
    if (newTrade.value.exitPrice) formData.append("exitPrice", parseFloat(newTrade.value.exitPrice));
    if (newTrade.value.positionSize) formData.append("positionSize", parseFloat(newTrade.value.positionSize));
    if (newTrade.value.realizedPnL) formData.append("realizedPnL", parseFloat(newTrade.value.realizedPnL));
    formData.append("strategyId", parseInt(newTrade.value.strategyId));
    if (newTrade.value.reviewNotes) formData.append("reviewNotes", newTrade.value.reviewNotes);
    
    if (newTrade.value.chartImage) {
      formData.append("file", newTrade.value.chartImage);
    }
    
    const res = await submitNewTrade(formData);
    
    if(res.status == 200) {
      successMessage.value = `Successfully logged trade for ${newTrade.value.instrument}!`;
      await loadTrades();
    }
    closeLogTradeModal();
    
    setTimeout(() => { successMessage.value = ''; }, 4000);
  } catch (error) {
    if (error.response && error.response.data) {
      if (error.response.data.error) {
        errorMessage.value = error.response.data.error;
      } else if (error.response.data.errors) {
        errorMessage.value = Object.values(error.response.data.errors).flat().join(', ');
      } else {
        errorMessage.value = 'Failed to log trade.';
      }
    } else {
      errorMessage.value = 'An unexpected error occurred.';
    }
  } finally {
    isSubmitting.value = false;
  }
};
// Chart View Modal State
const isChartModalOpen = ref(false);
const activeChartUrl = ref('');

const openChartModal = (url) => {
  if (!url) return;
  activeChartUrl.value = url;
  isChartModalOpen.value = true;
};

const closeChartModal = () => {
  isChartModalOpen.value = false;
  setTimeout(() => { activeChartUrl.value = ''; }, 300);
};
</script>

<template>
  <div class="p-6 md:p-10 max-w-7xl mx-auto space-y-8 w-full relative">
    
    <!-- Global Success Toast -->
    <div v-if="successMessage" class="fixed top-8 right-8 z-50 bg-green-50 border border-green-200 text-green-800 px-6 py-4 rounded-2xl shadow-xl shadow-green-900/5 flex items-center gap-3 animate-[fadeIn_0.3s_ease-out]">
      <svg class="w-6 h-6 text-green-500 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
      <p class="font-bold text-sm">{{ successMessage }}</p>
    </div>

    <header class="flex justify-between items-end mb-8 mt-4">
      <div>
        <h1 class="text-3xl font-extrabold text-text-main tracking-tight">Active Trading Journal</h1>
        <p class="text-text-muted mt-1">Module B: Strict mechanical execution and realized performance.</p>
      </div>
      <button @click="openLogTradeModal" class="bg-secondary text-white px-6 py-2.5 rounded-xl font-bold shadow-md shadow-secondary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all flex items-center gap-2">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
        Log Trade
      </button>
    </header>

    <!-- Top Grid: KPIs -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      
      <!-- Total Realized PnL -->
      <div class="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 relative overflow-hidden group">
        <div class="absolute -bottom-10 -right-10 w-32 h-32 bg-green-50 rounded-full opacity-50 group-hover:scale-125 transition-transform duration-500 pointer-events-none"></div>
        <div class="flex items-center gap-3 mb-4 text-green-600">
           <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
           <span class="text-sm font-bold uppercase tracking-wider text-text-muted">Total Realized PnL</span>
        </div>
        <h2 class="text-4xl font-black text-text-main tracking-tighter">{{ formatCurrency(totalRealizedPnL) }}</h2>
      </div>

      <!-- Overall Win Rate -->
      <div class="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 relative overflow-hidden group">
        <div class="absolute -bottom-10 -right-10 w-32 h-32 bg-primary rounded-full opacity-10 group-hover:scale-125 transition-transform duration-500 pointer-events-none"></div>
        <div class="flex items-center gap-3 mb-4 text-primary">
           <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
           <span class="text-sm font-bold uppercase tracking-wider text-text-muted">Overall Win Rate</span>
        </div>
        <h2 class="text-4xl font-black text-text-main tracking-tighter">{{ overallWinRate }}%</h2>
      </div>

      <!-- Profit Factor -->
      <div class="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 relative overflow-hidden group">
        <div class="absolute -bottom-10 -right-10 w-32 h-32 bg-secondary rounded-full opacity-10 group-hover:scale-125 transition-transform duration-500 pointer-events-none"></div>
        <div class="flex items-center gap-3 mb-4 text-secondary">
           <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 7h8m0 0v8m0-8l-8 8-4-4-6 6"></path></svg>
           <span class="text-sm font-bold uppercase tracking-wider text-text-muted">Profit Factor</span>
        </div>
        <h2 class="text-4xl font-black text-text-main tracking-tighter">{{ profitFactor }}</h2>
      </div>
      
    </div>

    <!-- Charts Grid -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      
      <!-- The Equity Curve -->
      <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100 flex flex-col">
        <div class="flex justify-between items-center mb-6">
          <h3 class="text-lg font-bold text-text-main">The Equity Curve</h3>
          <span class="text-xs font-bold text-green-600 bg-green-50 px-2 py-1 rounded-md">New All-Time High</span>
        </div>
        <!-- Mock Line Chart for Equity Curve -->
        <div class="flex-grow w-full relative flex items-end">
          <svg class="w-full h-full text-secondary drop-shadow-md" preserveAspectRatio="none" viewBox="0 0 100 100" fill="none" stroke="currentColor">
            <path d="M0,90 L10,85 L20,95 L30,60 L40,65 L50,45 L60,50 L70,25 L80,35 L90,15 L100,5" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="animate-[dash_2s_ease-out_forwards]" stroke-dasharray="300" stroke-dashoffset="0"/>
            <path d="M0,90 L10,85 L20,95 L30,60 L40,65 L50,45 L60,50 L70,25 L80,35 L90,15 L100,5 L100,100 L0,100 Z" fill="currentColor" class="opacity-10" stroke="none" />
          </svg>
        </div>
      </div>

      <!-- Strategy Performance Breakdown -->
      <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100 flex flex-col">
        <h3 class="text-lg font-bold text-text-main mb-6">Strategy Performance Breakdown</h3>
        <div class="flex-grow space-y-6">
          
          <div v-for="strategy in strategies" :key="strategy.name" class="space-y-2">
            <div class="flex justify-between items-end">
              <span class="font-bold text-text-main text-sm">{{ strategy.name }} <span class="text-text-muted font-normal">({{ strategy.count }} trades)</span></span>
              <span class="font-bold text-green-600 text-sm">{{ formatCurrency(strategy.pnl) }}</span>
            </div>
            <div class="h-3 w-full bg-bg-gray rounded-full overflow-hidden relative">
              <div class="h-full bg-primary transition-all duration-1000" :style="`width: ${strategy.winRate}%`"></div>
            </div>
            <div class="text-xs font-bold text-text-muted text-right">{{ strategy.winRate }}% Win Rate</div>
          </div>
          
        </div>
      </div>

    </div>

    <!-- Trade Review Queue -->
    <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-8 py-6 border-b border-gray-50 flex justify-between items-center">
        <h3 class="text-xl font-bold text-text-main">Trade Review Queue</h3>
        <button class="text-sm font-medium text-secondary hover:text-opacity-80 transition-colors">View All Trades</button>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-bg-gray/50 text-xs uppercase tracking-wider text-text-muted">
              <th class="px-8 py-4 font-semibold">Instrument & Date</th>
              <th class="px-8 py-4 font-semibold">Strategy Tag</th>
              <th class="px-8 py-4 font-semibold text-center">Chart</th>
              <th class="px-8 py-4 font-semibold text-right">Realized PnL</th>
              <th class="px-8 py-4 font-semibold text-center">Action</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="trade in reviewQueue" :key="trade.id" class="hover:bg-gray-50/50 transition-colors group">
              <td class="px-8 py-4">
                <div class="flex items-center gap-3">
                  <div :class="[
                    'w-2 h-10 rounded-full',
                    trade.positionType === 'Long' ? 'bg-green-400' : 'bg-red-400'
                  ]"></div>
                  <div>
                    <p class="font-bold text-text-main">{{ trade.instrument }} <span class="text-text-muted font-normal ml-1">({{ trade.positionType }})</span></p>
                    <p class="text-xs text-text-muted mt-0.5">{{ trade.date }}</p>
                  </div>
                </div>
              </td>
              <td class="px-8 py-4">
                <span class="px-3 py-1 text-xs font-bold bg-light-blue text-primary rounded-md">
                  {{ trade.strategy }}
                </span>
              </td>
              <td class="px-8 py-4 text-center">
                <div v-if="trade.hasChart" @click.stop="openChartModal(trade.chartImageUrl)" class="inline-flex items-center justify-center w-8 h-8 rounded-lg bg-gray-100 text-gray-400 hover:text-primary hover:bg-light-blue cursor-pointer transition-colors" title="View attached chart">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>
                </div>
                <span v-else class="text-text-muted text-xs italic">No chart</span>
              </td>
              <td class="px-8 py-4 text-right font-bold">
                <!-- <span v-if="trade.status === 'Open'" class="text-text-muted italic text-sm font-medium">Open</span> -->
                <span :class="trade.pnl > 0 ? 'text-green-600' : (trade.pnl < 0 ? 'text-red-500' : 'text-text-muted')">
                  {{ trade.pnl > 0 ? '+' : '' }}{{ formatCurrency(trade.pnl) }}
                </span>
              </td>
              <td class="px-8 py-4 text-center">
                <button @click="openReviewModal(trade)" class="text-sm font-bold text-primary hover:text-secondary transition-colors">Review</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
    
    <!-- Log Trade Modal -->
    <div v-if="isLogTradeModalOpen" class="fixed inset-0 bg-gray-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl w-full max-w-lg p-8 shadow-2xl relative animate-[fadeIn_0.2s_ease-out] max-h-[90vh] overflow-y-auto custom-scrollbar">
        <button @click="closeLogTradeModal" class="absolute top-6 right-6 text-gray-400 hover:text-red-500 transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        
        <h2 class="text-2xl font-extrabold text-text-main mb-2">Log Active Trade</h2>
        <p class="text-text-muted text-sm font-medium mb-6">Record a completed mechanical trade execution.</p>

        <!-- Error Message Alert -->
        <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3 transition-all">
          <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
          <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
        </div>

        <form @submit.prevent="handleLogTrade" class="space-y-5">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Instrument</label>
              <input v-model="newTrade.instrument" type="text" required placeholder="NQ1!" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted uppercase" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Position Type</label>
              <select v-model="newTrade.positionType" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main font-bold appearance-none">
                <option value="Long" class="text-green-600 font-bold">Long</option>
                <option value="Short" class="text-red-500 font-bold">Short</option>
              </select>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Entry Date</label>
              <input v-model="newTrade.entryDate" type="date" required class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main text-sm" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Exit Date</label>
              <input v-model="newTrade.exitDate" type="date" required class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main text-sm" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Entry Price</label>
              <input v-model="newTrade.entryPrice" type="number" step="any" required placeholder="18500.50" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Exit Price</label>
              <input v-model="newTrade.exitPrice" type="number" step="any" required placeholder="18550.00" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
          </div>
          
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Quantity / Size</label>
              <input v-model="newTrade.positionSize" type="number" step="any" required placeholder="2" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Realized PnL</label>
              <input v-model="newTrade.realizedPnL" type="number" step="any" required placeholder="125.50" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
          </div>

          <div>
            <label class="block text-sm font-semibold text-text-main mb-1.5">Strategy Tag</label>
            <select v-model="newTrade.strategyId" required class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main font-bold appearance-none">
              <option value="" disabled selected>Select a strategy</option>
              <option v-for="strategy in availableStrategies" :key="strategy.id" :value="strategy.id">{{ strategy.name }}</option>
            </select>
          </div>

          <div>
            <label class="block text-sm font-semibold text-text-main mb-1.5">Review Notes</label>
            <textarea v-model="newTrade.reviewNotes" rows="3" placeholder="What went well? Did you follow your rules?" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-secondary focus:ring-2 focus:ring-secondary/20 transition-all outline-none text-text-main placeholder-text-muted custom-scrollbar"></textarea>
          </div>

          <div>
            <label class="block text-sm font-semibold text-text-main mb-1.5">Chart Screenshot</label>
            <div class="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-xl hover:border-secondary transition-colors relative" :class="{ 'bg-gray-50': !newTrade.chartPreview, 'bg-gray-100': newTrade.chartPreview }">
              <div v-if="!newTrade.chartPreview" class="space-y-1 text-center">
                <svg class="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true">
                  <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
                <div class="flex text-sm text-text-muted justify-center">
                  <label for="file-upload" class="relative cursor-pointer bg-transparent rounded-md font-bold text-secondary hover:text-opacity-80 focus-within:outline-none">
                    <span>Upload a file</span>
                    <input id="file-upload" name="file-upload" type="file" class="sr-only" accept="image/*" @change="handleFileUpload" />
                  </label>
                  <p class="pl-1">or drag and drop</p>
                </div>
                <p class="text-xs text-gray-500">PNG, JPG, GIF up to 5MB</p>
              </div>
              <div v-else class="relative w-full">
                <img :src="newTrade.chartPreview" class="max-h-48 rounded-lg mx-auto object-contain" />
                <button type="button" @click.prevent="removeFile" class="absolute -top-3 -right-3 bg-red-500 text-white rounded-full p-1 hover:bg-red-600 shadow-md transition-colors">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
                </button>
              </div>
            </div>
          </div>

          <div class="flex gap-4 pt-2">
            <button type="button" @click="closeLogTradeModal" class="flex-1 py-3.5 rounded-xl bg-gray-100 text-text-muted font-bold hover:bg-gray-200 transition-colors" :disabled="isSubmitting">Cancel</button>
            <button type="submit" class="flex-1 py-3.5 rounded-xl bg-secondary text-white font-bold shadow-md shadow-secondary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all disabled:opacity-70 disabled:hover:translate-y-0" :disabled="isSubmitting">
              {{ isSubmitting ? 'Logging...' : 'Log Trade' }}
            </button>
          </div>
        </form>
      </div>
    </div>
    <!-- Review Trade Modal -->
    <div v-if="isReviewModalOpen && selectedTrade" class="fixed inset-0 bg-gray-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl w-full max-w-2xl p-8 shadow-2xl relative animate-[fadeIn_0.2s_ease-out] max-h-[90vh] overflow-y-auto custom-scrollbar">
        <button @click="closeReviewModal" class="absolute top-6 right-6 text-gray-400 hover:text-red-500 transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        
        <div class="flex items-center gap-3 mb-6">
          <div :class="[
            'w-3 h-12 rounded-full',
            selectedTrade.positionType === 'Long' ? 'bg-green-400' : 'bg-red-400'
          ]"></div>
          <div>
            <h2 class="text-2xl font-extrabold text-text-main leading-tight">{{ selectedTrade.instrument }} <span class="text-text-muted text-lg font-normal ml-1">({{ selectedTrade.positionType }})</span></h2>
            <p class="text-text-muted text-sm font-medium mt-0.5">{{ selectedTrade.date }} &bull; {{ selectedTrade.strategy }}</p>
          </div>
        </div>

        <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-8">
          <div class="p-4 rounded-xl bg-bg-gray/50 border border-gray-100">
            <p class="text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Status</p>
            <p class="font-extrabold text-text-main">{{ selectedTrade.status }}</p>
          </div>
          <div class="p-4 rounded-xl bg-bg-gray/50 border border-gray-100">
            <p class="text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Position Size</p>
            <p class="font-extrabold text-text-main">{{ selectedTrade.positionSize }}</p>
          </div>
          <div class="p-4 rounded-xl bg-bg-gray/50 border border-gray-100 md:col-span-2">
            <p class="text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Realized PnL</p>
            <p class="font-extrabold text-xl" :class="selectedTrade.pnl > 0 ? 'text-green-600' : (selectedTrade.pnl < 0 ? 'text-red-500' : 'text-text-main')">
              <!-- <span v-if="selectedTrade.status === 'Open'" class="text-text-muted italic text-base font-medium">Open Position</span> -->
              <span>{{ selectedTrade.pnl > 0 ? '+' : '' }}{{ formatCurrency(selectedTrade.pnl) }}</span>
            </p>
          </div>
          <div class="p-4 rounded-xl bg-bg-gray/50 border border-gray-100 md:col-span-2">
            <p class="text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Entry Price</p>
            <p class="font-extrabold text-text-main">{{ formatCurrency(selectedTrade.entryPrice) }}</p>
          </div>
          <div class="p-4 rounded-xl bg-bg-gray/50 border border-gray-100 md:col-span-2">
            <p class="text-xs font-bold text-text-muted uppercase tracking-wider mb-1">Exit Price</p>
            <p class="font-extrabold text-text-main" :class="!selectedTrade.exitPrice ? 'text-text-muted italic font-normal' : ''">
              {{ selectedTrade.exitPrice ? formatCurrency(selectedTrade.exitPrice) : 'Not Exited' }}
            </p>
          </div>
        </div>

        <div class="mb-8">
          <h3 class="text-sm font-bold text-text-main uppercase tracking-wider mb-3">Review Notes</h3>
          <div class="p-5 rounded-xl bg-bg-gray/30 border border-gray-100 min-h-[100px]">
            <p v-if="selectedTrade.reviewNotes" class="text-text-main leading-relaxed whitespace-pre-line">{{ selectedTrade.reviewNotes }}</p>
            <p v-else class="text-text-muted italic text-sm">No review notes provided for this trade.</p>
          </div>
        </div>

        <div v-if="selectedTrade.hasChart">
          <h3 class="text-sm font-bold text-text-main uppercase tracking-wider mb-3">Attached Chart</h3>
          <div class="rounded-xl overflow-hidden border border-gray-200 bg-gray-50 flex items-center justify-center p-4">
            <!-- If we had actual image URLs we would use: <img :src="selectedTrade.chartImageUrl" class="max-w-full rounded-lg shadow-sm" /> -->
            <!-- For now, showing placeholder since images are simulated or URL not full -->
            <div class="text-center py-10 text-gray-400">
              <!-- <svg class="mx-auto h-12 w-12 mb-3" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true">
                <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
              </svg>
              <p class="text-sm">Chart image would be displayed here.</p> -->
              <img :src="selectedTrade.chartImageUrl" @click.stop="openChartModal(selectedTrade.chartImageUrl)" class="max-w-full rounded-lg shadow-sm cursor-pointer hover:opacity-90 transition-opacity" title="Click to view full screen" />
            </div>
          </div>
        </div>

      </div>
    </div>
    <!-- Chart View Modal -->
    <div v-if="isChartModalOpen && activeChartUrl" class="fixed inset-0 bg-gray-900/80 backdrop-blur-md z-[60] flex items-center justify-center p-4">
      <div class="relative w-full max-w-5xl animate-[fadeIn_0.2s_ease-out]">
        <button @click="closeChartModal" class="absolute -top-12 right-0 md:-right-12 text-white/70 hover:text-white transition-colors p-2 bg-gray-900/50 hover:bg-gray-900 rounded-full">
          <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        <img :src="activeChartUrl" class="w-full max-h-[85vh] object-contain rounded-xl shadow-2xl" alt="Full size chart view" />
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
