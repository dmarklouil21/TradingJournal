import apiClient from "@/utils/axios";

export async function submitPurchase(payload) {
  const response = await apiClient.post("investment/purchase", payload);
  return response;
}