import apiClient from "@/utils/axios";

export async function submitNewTrade(payload) {
  const response = await apiClient.post("trading-journal/new-trade", payload);
  return response;
}

export async function fetchTrades() {
  const response = await apiClient.get("trading-journal/trades");
  return response;
}