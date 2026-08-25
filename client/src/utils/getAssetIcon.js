export function getAssetIcon (symbol) {
  return `${import.meta.env.API_BASE_URL}/api/investment/logo/${symbol}`;
};
