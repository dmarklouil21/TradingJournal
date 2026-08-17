import apiClient from "@/utils/axios";

export async function submitPurchase(payload) {
  const response = await apiClient.post("investment/purchase", payload);
  return response;
}

export async function fetchCryptoPrice(symbol) {
  const response = await apiClient.get(`investment/price/${symbol}`);
  return response;
}
