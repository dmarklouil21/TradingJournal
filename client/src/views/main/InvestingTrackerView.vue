<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import { submitPurchase, fetchCryptoPrice, fetchCampaigns, submitSale, submitPhaseUpdate } from '@/services/investment';
import { getAssetIcon } from '@/utils/getAssetIcon';

// Active DCA Campaigns mapped from the Database
const dcaCampaigns = ref([]);

// Live Coins.ph Price State
const livePrices = ref({});
let pollingInterval = null;

const loadCampaigns = async () => {
  try {
    const res = await fetchCampaigns();
    dcaCampaigns.value = res.data;
    fetchPrices(); 
  } catch (ex) {
    console.error("Failed to load campaigns:", ex);
  }
};

const fetchPrices = async () => {
  if (dcaCampaigns.value.length === 0) return;
  
  try {
    const promises = dcaCampaigns.value.map(async (campaign) => {
      const symbol = campaign.symbol.toUpperCase() + "PHP";
      
      const response = await fetchCryptoPrice(symbol);
      
      const json = response.data;
      
      if (json && json.price) {
        livePrices.value[campaign.symbol] = parseFloat(json.price);
      }
    });
    
    await Promise.all(promises);
  } catch (error) {
    console.error("Failed to fetch live prices:", error);
  }
};

onMounted(() => {
  loadCampaigns();
  pollingInterval = setInterval(fetchPrices, 15000);
});

onUnmounted(() => {
  if (pollingInterval) clearInterval(pollingInterval);
});

// Summary Statistics
const totalDeployed = computed(() => {
  return dcaCampaigns.value.reduce((total, campaign) => {
    return total + (parseFloat(campaign.holdings) * campaign.avgCost);
  }, 0);
});

const currentValue = computed(() => {
  return dcaCampaigns.value.reduce((total, campaign) => {
    const price = getLivePrice(campaign.symbol, campaign.currentPrice);
    return total + (parseFloat(campaign.holdings) * price);
  }, 0);
});

const unrealizedPnL = computed(() => currentValue.value - totalDeployed.value);
const unrealizedPnLPercent = computed(() => {
  if (totalDeployed.value === 0) return 0;
  return ((currentValue.value - totalDeployed.value) / totalDeployed.value) * 100;
});

// Dynamic Asset Allocation Logic
const generateColor = (index) => {
  const hue = (index * 137.5) % 360;
  return `hsl(${hue}, 70%, 60%)`;
};

const assetAllocation = computed(() => {
  if (dcaCampaigns.value.length === 0 || currentValue.value === 0) return [];
  
  let currentPercentage = 0;
  
  return dcaCampaigns.value
    .map(campaign => {
      const price = getLivePrice(campaign.symbol, campaign.currentPrice);
      const value = parseFloat(campaign.holdings) * price;
      return {
        name: campaign.symbol,
        value: value,
        rawPercentage: (value / currentValue.value) * 100
      };
    })
    .sort((a, b) => b.value - a.value)
    .map((asset, index) => {
      asset.percentage = asset.rawPercentage.toFixed(1);
      asset.color = generateColor(index);
      
      const start = currentPercentage;
      currentPercentage += asset.rawPercentage;
      asset.gradientStop = `${asset.color} ${start}% ${currentPercentage}%`;
      return asset;
    });
});

const pieChartStyle = computed(() => {
  if (assetAllocation.value.length === 0) return 'background: #f3f4f6;';
  const gradients = assetAllocation.value.map(a => a.gradientStop).join(', ');
  return `background: conic-gradient(${gradients});`;
});

// Native Charting Logic for Asset Growth Over Time
const chartPoints = computed(() => {
  if (dcaCampaigns.value.length === 0) return [];
  
  // Flatten and aggregate all logs
  const allLogs = [];
  dcaCampaigns.value.forEach(campaign => {
    if (campaign.logs) {
      campaign.logs.forEach(log => {
        allLogs.push({
          timestamp: new Date(log.executionDate).getTime(),
          amountTokens: parseFloat(log.amountTokens),
          purchasePrice: parseFloat(log.purchasePrice),
          fees: parseFloat(log.fees)
        });
      });
    }
  });
  
  if (allLogs.length === 0) return [];
  
  allLogs.sort((a, b) => a.timestamp - b.timestamp);
  
  const startTime = allLogs[0].timestamp;
  const endTime = Date.now();
  const timeSpan = endTime - startTime || 1; // Prevent division by 0
  
  // Calculate max possible value for Y-axis scaling
  let maxTotal = totalDeployed.value * 1.2; 
  if (currentValue.value > maxTotal) maxTotal = currentValue.value * 1.2;
  if (maxTotal === 0) maxTotal = 1;
  
  let runningTotal = 0;
  const points = [];
  
  // Start the chart at the very first transaction
  points.push(`0,${100 - (0 / maxTotal * 100)}`);
  
  allLogs.forEach(log => {
    // Add transaction to running total. Sales natively drop this down.
    runningTotal += (log.amountTokens * log.purchasePrice) + log.fees;
    
    // X is percentage of time passed. Y is percentage of max value.
    const x = ((log.timestamp - startTime) / timeSpan) * 100;
    const y = 100 - (runningTotal / maxTotal * 100);
    
    points.push(`${x.toFixed(1)},${y.toFixed(1)}`);
  });
  
  // Final dot shoots up (or down) to the exact Live Value right now
  const finalLiveY = 100 - (currentValue.value / maxTotal * 100);
  points.push(`100,${finalLiveY.toFixed(1)}`);
  
  return points;
});

const chartPath = computed(() => {
  const points = chartPoints.value;
  if (points.length === 0) return "M0,100 L100,100";
  return `M${points.join(' L')}`;
});

const chartAreaPath = computed(() => {
  const points = chartPoints.value;
  if (points.length === 0) return "M0,100 L100,100 Z";
  return `M${points.join(' L')} L100,100 L0,100 Z`;
});

// New Purchase Modal State
const isPurchaseModalOpen = ref(false);
const isSubmitting = ref(false);
const errorMessage = ref('');
const successMessage = ref('');
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

const openPurchaseModal = () => {
  isPurchaseModalOpen.value = true;
};

const closePurchaseModal = () => {
  isPurchaseModalOpen.value = false;
  errorMessage.value = '';
  newPurchase.value = {
    asset: '',
    symbol: '',
    amount: null,
    price: null,
    fees: null,
    date: new Date().toISOString().split('T')[0]
  };
};

// Manage Campaign (Sell / Phase) Modal State
const isManageModalOpen = ref(false);
const selectedCampaign = ref(null);
const manageTab = ref('sell');
const newSale = ref({
  amount: null,
  price: null,
  fees: null,
  date: new Date().toISOString().split('T')[0]
});
const newPhase = ref('');

const openManageModal = (campaign) => {
  selectedCampaign.value = campaign;
  newPhase.value = campaign.phase;
  manageTab.value = 'sell';
  isManageModalOpen.value = true;
};

const closeManageModal = () => {
  isManageModalOpen.value = false;
  selectedCampaign.value = null;
  errorMessage.value = '';
  newSale.value = {
    amount: null,
    price: null,
    fees: null,
    date: new Date().toISOString().split('T')[0]
  };
};

const totalSaleProceeds = computed(() => {
  return ((newSale.value.amount || 0) * (newSale.value.price || 0)) - ((newSale.value.fees || 0) * (newSale.value.price || 0));
});

const handleSale = async () => {
  if (isSubmitting.value) return;
  
  if (!newSale.value.amount || !newSale.value.price || !newSale.value.date) {
    errorMessage.value = 'Please fill in all required fields.';
    return;
  }
  
  if (newSale.value.amount > parseFloat(selectedCampaign.value.holdings)) {
    errorMessage.value = 'Cannot sell more than your current holdings.';
    return;
  }
  
  try {
    isSubmitting.value = true;
    errorMessage.value = '';
    
    const payload = {
      campaignId: selectedCampaign.value.id,
      amountTokens: parseFloat(newSale.value.amount),
      sellPrice: parseFloat(newSale.value.price),
      fees: parseFloat(newSale.value.fees || 0),
      executionDate: new Date(newSale.value.date).toISOString()
    };
    
    const res = await submitSale(payload);
    
    if (res.status === 200) {
      successMessage.value = `Successfully logged sale of ${payload.amountTokens} ${selectedCampaign.value.symbol}!`;
      closeManageModal();
      loadCampaigns(); 
      setTimeout(() => { successMessage.value = ''; }, 4000);
    }
  } catch (error) {
    errorMessage.value = error.response?.data?.error || 'Failed to log sale.';
  } finally {
    isSubmitting.value = false;
  }
};

const handleChangePhase = async () => {
  if (isSubmitting.value) return;
  if (newPhase.value === selectedCampaign.value.phase) return;
  
  try {
    isSubmitting.value = true;
    errorMessage.value = '';
    console.log(`Campaign ID: ${selectedCampaign.value.id}`)
    console.log(`New Phase: ${newPhase.value}`);
    const payload = {
      campaignId: selectedCampaign.value.id,
      newPhase: newPhase.value
    };
    
    const res = await submitPhaseUpdate(payload);
    
    if (res.status === 200) {
      successMessage.value = `Successfully updated phase to ${newPhase.value}!`;
      closeManageModal();
      loadCampaigns(); 
      setTimeout(() => { successMessage.value = ''; }, 4000);
    }
  } catch (error) {
    errorMessage.value = error.response?.data?.error || 'Failed to update phase.';
  } finally {
    isSubmitting.value = false;
  }
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
      loadCampaigns(); 
      
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

const getLivePrice = (symbol, fallbackPrice) => {
  return livePrices.value[symbol] || fallbackPrice;
};

const formatCurrency = (value) => {
  return new Intl.NumberFormat('en-PH', { style: 'currency', currency: 'PHP' }).format(value);
};

const formatPercent = (value) => {
  return new Intl.NumberFormat('en-PH', { style: 'percent', minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value / 100);
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
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
      
      <!-- Capital Deployed vs Current Value -->
      <div class="lg:col-span-2 bg-white rounded-3xl p-8 shadow-sm border border-gray-100 relative overflow-hidden group flex flex-col justify-between h-full">
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
      <div class="bg-white rounded-3xl p-6 shadow-sm border border-gray-100 flex flex-col h-full min-h-[320px]">
        <h3 class="text-lg font-bold text-text-main w-full">Asset Allocation</h3>
        
        <div class="flex items-center justify-between gap-6 flex-1">
          <!-- Scaled down Pie Chart on the Left -->
          <div class="relative w-32 h-32 rounded-full shadow-inner transform hover:scale-105 transition-transform duration-500 shrink-0"
               :style="pieChartStyle">
            <!-- Inner circle for donut effect -->
            <div class="absolute inset-4 bg-white rounded-full shadow-sm flex items-center justify-center">
              <span class="font-bold text-text-muted text-xs">{{ assetAllocation.length }} Assets</span>
            </div>
          </div>

          <!-- Condensed Legend on the Right -->
          <div class="w-full space-y-2 flex-1">
            <div v-for="asset in assetAllocation" :key="asset.name" class="flex items-center justify-between text-xs">
              <div class="flex items-center gap-1.5">
                <div class="w-2.5 h-2.5 rounded-full shrink-0" :style="`background-color: ${asset.color}`"></div>
                <span class="font-semibold text-text-main truncate max-w-[60px]" :title="asset.name">{{ asset.name }}</span>
              </div>
              <span class="font-bold text-text-muted shrink-0">{{ asset.percentage }}%</span>
            </div>
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
      <!-- Native SVG Real-Time Line Chart -->
      <div class="h-48 w-full relative flex items-end">
        <svg class="w-full h-full drop-shadow-md" preserveAspectRatio="none" viewBox="0 0 100 100" fill="none">
          <!-- Area Fill Gradient -->
          <path :d="chartAreaPath" fill="url(#chartGradient)" class="opacity-30 transition-all duration-1000 ease-out" stroke="none" />
          
          <!-- Data Path -->
          <path :d="chartPath" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round" class="text-primary transition-all duration-1000 ease-out" stroke="currentColor" />
          
          <!-- Glowing End Dot (Live Value) -->
          <circle cx="100" :cy="chartPoints.length ? chartPoints[chartPoints.length-1].split(',')[1] : 100" r="2.5" class="text-primary transition-all duration-1000 animate-pulse" fill="currentColor" />

          <defs>
            <linearGradient id="chartGradient" x1="0%" y1="0%" x2="0%" y2="100%">
              <stop offset="0%" stop-color="#3b82f6" stop-opacity="1" />
              <stop offset="100%" stop-color="#3b82f6" stop-opacity="0" />
            </linearGradient>
          </defs>
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
            <tr v-for="campaign in dcaCampaigns" @click="openManageModal(campaign)" :key="campaign.id" class="hover:bg-gray-50/50 transition-colors group">
              <td class="px-8 py-5">
                <div class="font-bold text-text-main flex items-center gap-2">
                  <img 
                    :src="getAssetIcon(campaign.symbol)" 
                    :alt="campaign.asset"
                    class="w-8 h-8 shrink-0 drop-shadow-sm"
                    @error="$event.target.style.display='none'; $event.target.nextElementSibling.style.display='flex'"
                  />
                  <div class="w-8 h-8 rounded-full bg-light-blue items-center justify-center text-primary text-xs shrink-0 hidden">
                    {{ campaign.symbol[0] }}
                  </div>
                  <div class="flex flex-col">
                    {{ campaign.symbol }}
                    <span class="text-text-muted font-medium text-xs">{{ campaign.asset }}</span>
                  </div>
                </div>
              </td>
              <td class="px-8 py-5 font-semibold text-text-main">
                {{ campaign.holdings }}
              </td>
              <td class="px-8 py-5 font-semibold text-text-muted">
                {{ formatCurrency(campaign.avgCost) }}
              </td>
              <td class="px-8 py-5 font-semibold text-text-main transition-all duration-300">
                {{ formatCurrency(getLivePrice(campaign.symbol, campaign.currentPrice)) }}
              </td>
              <td class="px-8 py-5 text-right font-bold transition-all duration-300">
                <span :class="getPnLColor(campaign.avgCost, getLivePrice(campaign.symbol, campaign.currentPrice))">
                  {{ getLivePrice(campaign.symbol, campaign.currentPrice) >= campaign.avgCost ? '+' : '' }}{{ formatPercent(((getLivePrice(campaign.symbol, campaign.currentPrice) - campaign.avgCost) / campaign.avgCost) * 100) }}
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
    <!-- Manage Campaign Modal -->
    <div v-if="isManageModalOpen" class="fixed inset-0 bg-gray-900/40 backdrop-blur-sm z-50 flex items-center justify-center p-4">
      <div class="bg-white rounded-3xl w-full max-w-lg p-8 shadow-2xl relative animate-[fadeIn_0.2s_ease-out]">
        <button @click="closeManageModal" class="absolute top-6 right-6 text-gray-400 hover:text-red-500 transition-colors">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path></svg>
        </button>
        
        <div class="flex items-center gap-3 mb-6">
          <img :src="getAssetIcon(selectedCampaign?.symbol)" class="w-10 h-10 drop-shadow-sm" />
          <div>
            <h2 class="text-2xl font-extrabold text-text-main leading-tight">Manage {{ selectedCampaign?.asset }}</h2>
            <p class="text-text-muted text-sm font-medium">{{ selectedCampaign?.holdings }} Tokens currently deployed</p>
          </div>
        </div>

        <!-- Tab Navigation -->
        <div class="flex bg-bg-gray/50 rounded-xl p-1 mb-6">
          <button @click="manageTab = 'sell'" class="flex-1 py-2 text-sm font-bold rounded-lg transition-all" :class="manageTab === 'sell' ? 'bg-white shadow-sm text-primary' : 'text-text-muted hover:text-text-main'">Log Sale</button>
          <button @click="manageTab = 'phase'" class="flex-1 py-2 text-sm font-bold rounded-lg transition-all" :class="manageTab === 'phase' ? 'bg-white shadow-sm text-primary' : 'text-text-muted hover:text-text-main'">Change Phase</button>
        </div>

        <!-- Error Message -->
        <div v-if="errorMessage" class="mb-6 p-4 rounded-xl bg-red-50 border border-red-100 flex items-start gap-3 transition-all">
          <svg class="w-5 h-5 text-red-500 mt-0.5 shrink-0" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
          <p class="text-sm font-medium text-red-700 leading-tight">{{ errorMessage }}</p>
        </div>

        <!-- Sell Form -->
        <form v-if="manageTab === 'sell'" @submit.prevent="handleSale" class="space-y-5">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Amount (Units)</label>
              <input v-model="newSale.amount" type="number" step="any" min="0" :max="selectedCampaign?.holdings" required placeholder="0.05" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Sell Price</label>
              <input v-model="newSale.price" type="number" step="any" min="0" required placeholder="65000" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Fees (Token)</label>
              <input v-model="newSale.fees" type="number" step="any" min="0" required placeholder="0.001" class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main" />
            </div>
            <div>
              <label class="block text-sm font-semibold text-text-main mb-1.5">Execution Date</label>
              <input v-model="newSale.date" type="date" required class="w-full px-4 py-3 rounded-xl bg-bg-gray/50 border border-gray-200 focus:border-primary focus:ring-2 focus:ring-primary/20 transition-all outline-none text-text-main text-sm" />
            </div>
          </div>

          <!-- Proceeds Calculator -->
          <div class="p-4 bg-green-50 rounded-xl flex justify-between items-center border border-green-100">
            <span class="text-sm font-bold text-green-800">Total Proceeds</span>
            <span class="text-xl font-black text-green-600">{{ formatCurrency(totalSaleProceeds) }}</span>
          </div>

          <div class="flex gap-4 pt-2">
            <button type="button" @click="closeManageModal" class="flex-1 py-3.5 rounded-xl bg-gray-100 text-text-muted font-bold hover:bg-gray-200 transition-colors" :disabled="isSubmitting">Cancel</button>
            <button type="submit" class="flex-1 py-3.5 rounded-xl bg-primary text-white font-bold shadow-md shadow-primary/30 hover:bg-opacity-90 transition-all disabled:opacity-70" :disabled="isSubmitting">
              {{ isSubmitting ? 'Processing...' : 'Log Sale' }}
            </button>
          </div>
        </form>

        <!-- Phase Form -->
        <form v-if="manageTab === 'phase'" @submit.prevent="handleChangePhase" class="space-y-6">
          <div>
            <label class="block text-sm font-semibold text-text-main mb-3">System Phase</label>
            <div class="grid grid-cols-2 gap-3">
              <button type="button" @click="newPhase = 'PhaseOne'" class="py-3 px-2 border-2 rounded-xl text-sm font-bold transition-all" :class="newPhase === 'PhaseOne' ? 'border-green-500 bg-green-50 text-green-700' : 'border-gray-200 text-text-muted hover:border-gray-300'">Phase 1<br/><span class="text-[10px] font-semibold opacity-70">The Accumulation Engine</span></button>
              
              <button type="button" @click="newPhase = 'PhaseTwo'" class="py-3 px-2 border-2 rounded-xl text-sm font-bold transition-all" :class="newPhase === 'PhaseTwo' ? 'border-blue-500 bg-blue-50 text-blue-700' : 'border-gray-200 text-text-muted hover:border-gray-300'">Phase 2<br/><span class="text-[10px] font-semibold opacity-70">The House Money Milestone</span></button>
              
              <button type="button" @click="newPhase = 'PhaseThree'" class="py-3 px-2 border-2 rounded-xl text-sm font-bold transition-all" :class="newPhase === 'PhaseThree' ? 'border-orange-500 bg-orange-50 text-orange-700' : 'border-gray-200 text-text-muted hover:border-gray-300'">Phase 3<br/><span class="text-[10px] font-semibold opacity-70 text-center block">The Technical Overextension Warning</span></button>
              
              <button type="button" @click="newPhase = 'PhaseFour'" class="py-3 px-2 border-2 rounded-xl text-sm font-bold transition-all" :class="newPhase === 'PhaseFour' ? 'border-red-500 bg-red-50 text-red-700' : 'border-gray-200 text-text-muted hover:border-gray-300'">Phase 4<br/><span class="text-[10px] font-semibold opacity-70">The Cool-Down & Restart Rule</span></button>
            </div>
          </div>

          <div class="flex gap-4 pt-2">
            <button type="button" @click="closeManageModal" class="flex-1 py-3.5 rounded-xl bg-gray-100 text-text-muted font-bold hover:bg-gray-200 transition-colors" :disabled="isSubmitting">Cancel</button>
            <button type="submit" class="flex-1 py-3.5 rounded-xl bg-text-main text-white font-bold shadow-md hover:bg-black transition-all disabled:opacity-70" :disabled="isSubmitting || newPhase === selectedCampaign?.phase">
              {{ isSubmitting ? 'Updating...' : 'Save Phase' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Premium thin scrollbar for the asset legend */
.custom-scrollbar::-webkit-scrollbar {
  width: 4px;
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
