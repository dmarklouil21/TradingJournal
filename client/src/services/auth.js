import apiClient from "@/utils/axios";

export async function login(credentials) {
  const response = await apiClient.post("auth/login", credentials);
  return response;
}

export async function register(form) {
  const response = await apiClient.post("auth/register", form);
  return response;
}