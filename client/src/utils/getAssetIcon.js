export function getAssetIcon (symbol) {
  return `${import.meta.env.VITE_API_BASE_URL}/investment/logo/${symbol}`;
};
