import apiClient from "@/utils/axios";

export async function fetchDashboardSummary() {
  const response = await apiClient.get("dashboard/summary");
  return response;
}
