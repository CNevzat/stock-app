import { Capacitor } from '@capacitor/core';

/**
 * Mobil ve web için API base URL'ini döndürür
 * Mobil cihazlarda IP adresi kullanılmalı (localhost çalışmaz)
 */
export const getApiBaseUrl = (): string => {
  // Environment variable varsa onu kullan
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL;
  }

  // Capacitor ile mobil cihazda mıyız?
  const isNative = Capacitor.isNativePlatform();

  if (isNative) {
    // Mobil cihazda - IP adresi kullanılmalı
    // Varsayılan olarak 10.0.2.2 (Android Emulator için) veya gerçek IP
    // Production'da bu değer environment variable'dan gelecek veya kullanıcı ayarlarından
    const mobileApiUrl = import.meta.env.VITE_MOBILE_API_URL || 'http://10.10.3.80:5134';
    return mobileApiUrl;
  }

  // Web'de
  if (import.meta.env.PROD) {
    // Production'da window.location.hostname kullan
    return `http://${window.location.hostname}:5134`;
  }

  // Development'ta localhost
  return 'http://localhost:5134';
};

/**
 * SignalR Hub URL'ini döndürür
 */
export const getHubUrl = (): string => {
  const apiBaseUrl = getApiBaseUrl();
  return `${apiBaseUrl}/hubs/stock`;
};

