<script setup>
import { ref } from 'vue';
import { computed } from 'vue';
import { submitPurchase } from '@/services/investment';

// Mock Data
const totalDeployed = ref(85000.00);
const currentValue = ref(102450.50);
const unrealizedPnL = computed(() => currentValue.value - totalDeployed.value);
const unrealizedPnLPercent = computed(() => ((currentValue.value - totalDeployed.value) / totalDeployed.value) * 100);

// Asset Allocation Mock Data
const assets = ref([
  { name: 'Bitcoin (BTC)', color: '#fcb814', percentage: 45 },
  { name: 'Ethereum (ETH)', color: '#749ab6', percentage: 35 },
  { name: 'Solana (SOL)', color: '#c5d7e5', percentage: 20 },
]);

// DCA Campaigns Mock Data
const dcaCampaigns = ref([
  { id: 1, asset: 'Bitcoin', symbol: 'BTC', holdings: '1.25', avgCost: 42000, currentPrice: 58400, phase: 'Markup' },
  { id: 2, asset: 'Ethereum', symbol: 'ETH', holdings: '15.4', avgCost: 2100, currentPrice: 3100, phase: 'Accumulation' },
  { id: 3, asset: 'Solana', symbol: 'SOL', holdings: '145.0', avgCost: 85, currentPrice: 142, phase: 'Markup' },
  { id: 4, asset: 'Chainlink', symbol: 'LINK', holdings: '500.0', avgCost: 15.5, currentPrice: 13.2, phase: 'Accumulation' }
]);

// New Purchase Modal State
const isPurchaseModalOpen = ref(false);
const newPurchase = ref({
  asset: '',
  symbol: '',
  amount: null,
  price: null,
  fees: null,
  date: new Date().toISOString().split('T')[0]
});

const totalPurchaseCost = computed(() => {
  return ((newPurchase.value.amount || 0) * (newPurchase.value.price || 0)) + ((newPurchase.value.fees || 0) * (newPurchase.value.price || 0));
});

const isSubmitting = ref(false);
const errorMessage = ref('');
const successMessage = ref('');

const closePurchaseModal = () => {
  isPurchaseModalOpen.value = false;
  errorMessage.value = '';
  newPurchase.value = {
    asset: '',
    symbol: '',
    amount: null,
    price: null,
    date: new Date().toISOString().split('T')[0]
  };
};

const handlePurchase = async () => {
  isSubmitting.value = true;
  errorMessage.value = '';

  try {
    const payload = {
      name: newPurchase.value.asset,
      symbol: newPurchase.value.symbol,
      amountTokens: parseFloat(newPurchase.value.amount),
      purchasePrice: parseFloat(newPurchase.value.price),
      fees: parseFloat(newPurchase.value.fees),
      executionDate: new Date(newPurchase.value.date).toISOString()
    };
    const res = await submitPurchase(payload);
    
    if (res.status === 200) {
      successMessage.value = `Successfully logged ${payload.amountTokens} ${payload.symbol} to your DCA Campaign!`;
      closePurchaseModal();
      
      // Auto-hide the toast after 4 seconds
      setTimeout(() => {
        successMessage.value = '';
      }, 4000);
    }
  } catch (ex) {
    if(ex.response && ex.response.data) {
      if(ex.response.data.error) {
        errorMessage.value = ex.response.data.error;
      } else if (ex.response.data.errors) {
        errorMessage.value = Object.values(ex.response.data.errors).flat().join(', ');
      } else {
        errorMessage.value = "Failed to log purchase. Please check your inputs.";
      }
    } else {
      errorMessage.value = "An unexpected error occurred. Please try again.";
    }
    console.error(ex);
  } finally {
    isSubmitting.value = false;
  }
};

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);
};

const formatPercent = (value) => {
  return new Intl.NumberFormat('en-US', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value / 100);
};

const getPnLColor = (cost, current) => {
  return current >= cost ? 'text-green-600' : 'text-red-500';
};
const getPnLBg = (cost, current) => {
  return current >= cost ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700';
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
        <h1 class="text-3xl font-extrabold text-text-main tracking-tight">Investing Tracker</h1>
        <p class="text-text-muted mt-1">DCA Module A: Long-term asset accumulation cycle.</p>
      </div>
      <button @click="isPurchaseModalOpen = true" class="bg-primary text-white px-6 py-2.5 rounded-xl font-bold shadow-md shadow-primary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all flex items-center gap-2">
        <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path></svg>
        New Purchase
      </button>
    </header>

    <!-- Top Grid: Deployed Capital & Allocation -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      
      <!-- Capital Deployed vs Current Value -->
      <div class="lg:col-span-2 bg-white rounded-3xl p-8 shadow-sm border border-gray-100 relative overflow-hidden group flex flex-col justify-between">
        <div class="absolute -bottom-24 -right-24 w-64 h-64 bg-primary rounded-full opacity-5 group-hover:scale-110 transition-transform duration-700"></div>
        
        <h3 class="text-lg font-bold text-text-main mb-6">Capital Deployed vs. Current Value</h3>
        
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8 mb-4">
          <div>
            <p class="text-sm font-semibold text-text-muted uppercase tracking-wider mb-1">Total Deployed</p>
            <p class="text-4xl font-black text-text-main">{{ formatCurrency(totalDeployed) }}</p>
          </div>
          <div>
            <p class="text-sm font-semibold text-text-muted uppercase tracking-wider mb-1">Current Value</p>
            <div class="flex items-end gap-3">
              <p class="text-4xl font-black text-text-main">{{ formatCurrency(currentValue) }}</p>
              <div :class="[getPnLBg(totalDeployed, currentValue), 'px-2.5 py-1 rounded-lg text-sm font-bold mb-1']">
                {{ unrealizedPnLPercent > 0 ? '+' : '' }}{{ formatPercent(unrealizedPnLPercent) }}
              </div>
            </div>
          </div>
        </div>

        <!-- Progress bar comparison visual -->
        <div class="mt-4">
          <div class="flex justify-between text-xs font-bold text-text-muted mb-2">
            <span>Deployed Base</span>
            <span :class="getPnLColor(totalDeployed, currentValue)">{{ unrealizedPnL > 0 ? '+' : '' }}{{ formatCurrency(unrealizedPnL) }} Unrealized</span>
          </div>
          <div class="h-4 w-full bg-bg-gray rounded-full overflow-hidden flex relative">
             <div class="h-full bg-light-blue" style="width: 100%"></div>
             <div class="h-full bg-primary absolute top-0 left-0 transition-all duration-1000" :style="`width: ${Math.min((totalDeployed / currentValue) * 100, 100)}%`"></div>
          </div>
        </div>
      </div>

      <!-- Asset Allocation (CSS Pie Chart) -->
      <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100 flex flex-col items-center justify-center">
        <h3 class="text-lg font-bold text-text-main w-full mb-6 text-center">Asset Allocation</h3>
        
        <div class="relative w-40 h-40 rounded-full shadow-inner mb-6 transform hover:scale-105 transition-transform duration-500"
             style="background: conic-gradient(#fcb814 0% 45%, #749ab6 45% 80%, #c5d7e5 80% 100%);">
          <!-- Inner circle for donut effect -->
          <div class="absolute inset-4 bg-white rounded-full shadow-sm flex items-center justify-center">
            <span class="font-bold text-text-muted text-sm">3 Assets</span>
          </div>
        </div>

        <div class="w-full space-y-3">
          <div v-for="asset in assets" :key="asset.name" class="flex items-center justify-between text-sm">
            <div class="flex items-center gap-2">
              <div class="w-3 h-3 rounded-full" :style="`background-color: ${asset.color}`"></div>
              <span class="font-medium text-text-main">{{ asset.name }}</span>
            </div>
            <span class="font-bold text-text-muted">{{ asset.percentage }}%</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Growth Over Time Mock Chart Area -->
    <div class="bg-white rounded-3xl p-8 shadow-sm border border-gray-100">
      <div class="flex justify-between items-center mb-6">
        <h3 class="text-lg font-bold text-text-main">Asset Growth Over Time</h3>
        <select class="bg-bg-gray border-none text-sm font-semibold rounded-lg px-3 py-1.5 focus:ring-0 outline-none text-text-main">
          <option>YTD</option>
          <option>1 Year</option>
          <option>All Time</option>
        </select>
      </div>
      <!-- Mock Line Chart -->
      <div class="h-48 w-full relative flex items-end">
        <!-- SVG Mock Line -->
        <svg class="w-full h-full text-primary drop-shadow-md" preserveAspectRatio="none" viewBox="0 0 100 100" fill="none" stroke="currentColor">
          <path d="M0,90 Q10,85 20,70 T40,65 T60,40 T80,30 T100,10" stroke-width="3" stroke-linecap="round" class="animate-[dash_2s_ease-out_forwards]" stroke-dasharray="200" stroke-dashoffset="0"/>
          <!-- Gradient Fill below line -->
          <path d="M0,90 Q10,85 20,70 T40,65 T60,40 T80,30 T100,10 L100,100 L0,100 Z" fill="currentColor" class="opacity-10" stroke="none" />
        </svg>
      </div>
    </div>

    <!-- Active DCA Campaigns Table -->
    <div class="bg-white rounded-3xl shadow-sm border border-gray-100 overflow-hidden">
      <div class="px-8 py-6 border-b border-gray-50">
        <h3 class="text-xl font-bold text-text-main">Active DCA Campaigns</h3>
      </div>
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-bg-gray/50 text-xs uppercase tracking-wider text-text-muted">
              <th class="px-8 py-4 font-semibold">Asset Name</th>
              <th class="px-8 py-4 font-semibold">Total Holdings</th>
              <th class="px-8 py-4 font-semibold">True Avg Cost</th>
              <th class="px-8 py-4 font-semibold">Current Price</th>
              <th class="px-8 py-4 font-semibold text-right">Unrealized PnL</th>
              <th class="px-8 py-4 font-semibold text-center">Phase</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-50">
            <tr v-for="campaign in dcaCampaigns" :key="campaign.id" class="hover:bg-gray-50/50 transition-colors group">
              <td class="px-8 py-5">
                <div class="font-bold text-text-main flex items-center gap-2">
                  <div class="w-8 h-8 rounded-full bg-light-blue flex items-center justify-center text-primary text-xs shrink-0">
                    {{ campaign.symbol[0] }}
                  </div>
                  {{ campaign.asset }}
                  <span class="text-text-muted font-medium text-sm ml-1">{{ campaign.symbol }}</span>
                </div>
              </td>
              <td class="px-8 py-5 font-semibold text-text-main">
                {{ campaign.holdings }}
              </td>
              <td class="px-8 py-5 font-semibold text-text-muted">
                {{ formatCurrency(campaign.avgCost) }}
              </td>
              <td class="px-8 py-5 font-semibold text-text-main">
                {{ formatCurrency(campaign.currentPrice) }}
              </td>
              <td class="px-8 py-5 text-right font-bold">
                <span :class="getPnLColor(campaign.avgCost, campaign.currentPrice)">
                  {{ campaign.currentPrice >= campaign.avgCost ? '+' : '' }}{{ formatPercent(((campaign.currentPrice - campaign.avgCost) / campaign.avgCost) * 100) }}
                </span>
              </td>
              <td class="px-8 py-5 text-center">
                <span class="px-3 py-1 text-xs font-bold rounded-full border"
                  :class="{
                    'bg-green-50 text-green-700 border-green-200': campaign.phase === 'Markup',
                    'bg-blue-50 text-blue-700 border-blue-200': campaign.phase === 'Accumulation',
                    'bg-orange-50 text-orange-700 border-orange-200': campaign.phase === 'Distribution'
                  }">
                  {{ campaign.phase }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- New Purchase Modal -->
    <div v-if="isPurchaseModalOpen" class="fixed inset-0 bg-gray-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl w-full max-w-lg p-8 shadow-2xl relative animate-[fadeIn_0.2s_ease-out]">
        <button @click="closePurchaseModal" class="absolute top-6 right-6 text-gray-400 hover:text-red-500 transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        
        <h2 class="text-2xl font-extrabold text-text-main mb-2">Log New Purchase</h2>
        <p class="text-text-muted text-sm font-medium mb-6">Add a new execution to your long-term DCA tracking.</p>

        <!-- Error Message Alert -->
        <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3 transition-all">
          <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
          <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
        </div>

        <form @submit.prevent="handlePurchase" class="space-y-5">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Asset Name</label>
              <input v-model="newPurchase.asset" type="text" required placeholder="Bitcoin" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Symbol</label>
              <input v-model="newPurchase.symbol" type="text" required placeholder="BTC" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted uppercase" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Amount (Units)</label>
              <input v-model="newPurchase.amount" type="number" step="any" min="0" required placeholder="0.05" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Execution Price</label>
              <input v-model="newPurchase.price" type="number" step="any" min="0" required placeholder="65000" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Fees (Token)</label>
              <input v-model="newPurchase.fees" type="number" step="any" min="0" required placeholder="0.001" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main placeholder-text-muted" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Purchase Date</label>
              <input v-model="newPurchase.date" type="date" required class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main text-sm" />
            </div>
          </div>

          <!-- Dynamic Total Cost Calculator -->
          <div class="p-4 bg-light-blue rounded-xl flex justify-between items-center border border-primary/10">
            <span class="text-sm font-bold text-text-main">Total Cost</span>
            <span class="text-xl font-black text-primary">{{ formatCurrency(totalPurchaseCost) }}</span>
          </div>

          <div class="flex gap-4 pt-2">
            <button type="button" @click="closePurchaseModal" class="flex-1 py-3.5 rounded-xl bg-gray-100 text-text-muted font-bold hover:bg-gray-200 transition-colors" :disabled="isSubmitting">Cancel</button>
            <button type="submit" class="flex-1 py-3.5 rounded-xl bg-primary text-white font-bold shadow-md shadow-primary/30 hover:bg-opacity-90 hover:-translate-y-0.5 transition-all disabled:opacity-70 disabled:hover:translate-y-0" :disabled="isSubmitting">
              {{ isSubmitting ? 'Logging...' : 'Log Purchase' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
</style>
