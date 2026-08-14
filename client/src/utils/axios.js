import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:5234/api/"
});

export default apiClient;