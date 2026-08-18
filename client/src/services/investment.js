import apiClient from "@/utils/axios";

export async function submitPurchase(payload) {
  const response = await apiClient.post("investment/purchase", payload);
  return response;
}

export async function fetchCryptoPrice(symbol) {
  const response = await apiClient.get(`investment/price/${symbol}`);
  return response;
}

export async function fetchCampaigns() {
  const response = await apiClient.get("investment/campaigns");
  return response;
}

export async function submitSale(payload) {
  const response = await apiClient.post('investment/sale', payload);
  return response;
}

export async function submitPhaseUpdate(payload) {
  const response = await apiClient.post('investment/phase', payload);
  return response;
}
