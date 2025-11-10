import { useEffect, useRef, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { signalRService } from '../services/signalRService';
import type { DashboardStats } from '../services/dashboardService';

export const useSignalR = () => {
  const queryClient = useQueryClient();
  const callbackRef = useRef<((stats: DashboardStats) => void) | null>(null);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    // SignalR bağlantısını başlat
    signalRService.startConnection().then(() => {
      setIsConnected(signalRService.isConnected());
    }).catch((error) => {
      console.error('SignalR bağlantı başlatma hatası:', error);
      setIsConnected(false);
    });

    // Bağlantı durumu değişikliklerini kontrol et
    const checkConnection = () => {
      setIsConnected(signalRService.isConnected());
    };

    // Her saniye bağlantı durumunu kontrol et (basit yaklaşım)
    const interval = setInterval(checkConnection, 1000);

    // Dashboard stats güncellemesi için callback
    const handleDashboardStatsUpdate = (stats: DashboardStats) => {
      console.log('Dashboard istatistikleri güncellendi:', stats);
      
      // React Query cache'ini güncelle
      queryClient.setQueryData(['dashboard-stats'], stats);
    };

    callbackRef.current = handleDashboardStatsUpdate;
    signalRService.onDashboardStatsUpdated(handleDashboardStatsUpdate);

    // Cleanup
    return () => {
      clearInterval(interval);
      if (callbackRef.current) {
        signalRService.offDashboardStatsUpdated(callbackRef.current);
      }
      // Bağlantıyı kapatmayın, diğer sayfalarda da kullanılabilir
      // signalRService.stopConnection();
    };
  }, [queryClient]);

  return {
    isConnected,
    connectionState: signalRService.getConnectionState(),
  };
};
