import apiClient from "@/utils/axios";

export async function submitStrategy(payload) {
  const response = await apiClient.post("settings/strategy", payload);
  return response;
}

export async function fetchStrategies() {
  const response = await apiClient.get("settings/strategies");
  return response;
}