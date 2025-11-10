import * as signalR from '@microsoft/signalr';
import type { DashboardStats } from './dashboardService';
import { getHubUrl } from '../utils/apiConfig';

const HUB_URL = getHubUrl();

// Event callback type'ları
export type ProductCreatedCallback = (product: any) => void;
export type ProductUpdatedCallback = (product: any) => void;
export type ProductDeletedCallback = (productId: number) => void;

export type CategoryCreatedCallback = (category: any) => void;
export type CategoryUpdatedCallback = (category: any) => void;
export type CategoryDeletedCallback = (categoryId: number) => void;

export type ProductAttributeCreatedCallback = (attribute: any) => void;
export type ProductAttributeUpdatedCallback = (attribute: any) => void;
export type ProductAttributeDeletedCallback = (attributeId: number) => void;

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private reconnectAttempts = 0;
  private maxReconnectAttempts = 5;

  async startConnection(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      console.log('SignalR zaten bağlı');
      return;
    }

    if (this.connection?.state === signalR.HubConnectionState.Connecting) {
      console.log('SignalR bağlanıyor...');
      return;
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount < this.maxReconnectAttempts) {
            return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
          }
          return null;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Bağlantı durumu değişikliklerini dinle
    this.connection.onclose((error) => {
      console.log('SignalR bağlantısı kapandı', error);
    });

    this.connection.onreconnecting((error) => {
      console.log('SignalR yeniden bağlanıyor...', error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log('SignalR yeniden bağlandı:', connectionId);
      this.reconnectAttempts = 0;
    });

    try {
      await this.connection.start();
      console.log('SignalR bağlantısı başlatıldı');
      
      // Dashboard grubuna katıl (isteğe bağlı)
      await this.connection.invoke('JoinDashboardGroup');
    } catch (error) {
      console.error('SignalR bağlantı hatası:', error);
      this.reconnectAttempts++;
    }
  }

  async stopConnection(): Promise<void> {
    if (this.connection) {
      try {
        await this.connection.invoke('LeaveDashboardGroup');
      } catch (error) {
        console.error('Dashboard grubundan çıkma hatası:', error);
      }
      
      await this.connection.stop();
      this.connection = null;
      console.log('SignalR bağlantısı durduruldu');
    }
  }

  // Dashboard Stats
  onDashboardStatsUpdated(callback: (stats: DashboardStats) => void): void {
    if (this.connection) {
      this.connection.on('DashboardStatsUpdated', callback);
    }
  }

  offDashboardStatsUpdated(callback: (stats: DashboardStats) => void): void {
    if (this.connection) {
      this.connection.off('DashboardStatsUpdated', callback);
    }
  }

  // Product Events
  onProductCreated(callback: ProductCreatedCallback): void {
    if (this.connection) {
      this.connection.on('ProductCreated', callback);
    }
  }

  offProductCreated(callback: ProductCreatedCallback): void {
    if (this.connection) {
      this.connection.off('ProductCreated', callback);
    }
  }

  onProductUpdated(callback: ProductUpdatedCallback): void {
    if (this.connection) {
      this.connection.on('ProductUpdated', callback);
    }
  }

  offProductUpdated(callback: ProductUpdatedCallback): void {
    if (this.connection) {
      this.connection.off('ProductUpdated', callback);
    }
  }

  onProductDeleted(callback: ProductDeletedCallback): void {
    if (this.connection) {
      this.connection.on('ProductDeleted', callback);
    }
  }

  offProductDeleted(callback: ProductDeletedCallback): void {
    if (this.connection) {
      this.connection.off('ProductDeleted', callback);
    }
  }

  // Category Events
  onCategoryCreated(callback: CategoryCreatedCallback): void {
    if (this.connection) {
      this.connection.on('CategoryCreated', callback);
    }
  }

  offCategoryCreated(callback: CategoryCreatedCallback): void {
    if (this.connection) {
      this.connection.off('CategoryCreated', callback);
    }
  }

  onCategoryUpdated(callback: CategoryUpdatedCallback): void {
    if (this.connection) {
      this.connection.on('CategoryUpdated', callback);
    }
  }

  offCategoryUpdated(callback: CategoryUpdatedCallback): void {
    if (this.connection) {
      this.connection.off('CategoryUpdated', callback);
    }
  }

  onCategoryDeleted(callback: CategoryDeletedCallback): void {
    if (this.connection) {
      this.connection.on('CategoryDeleted', callback);
    }
  }

  offCategoryDeleted(callback: CategoryDeletedCallback): void {
    if (this.connection) {
      this.connection.off('CategoryDeleted', callback);
    }
  }

  // ProductAttribute Events
  onProductAttributeCreated(callback: ProductAttributeCreatedCallback): void {
    if (this.connection) {
      this.connection.on('ProductAttributeCreated', callback);
    }
  }

  offProductAttributeCreated(callback: ProductAttributeCreatedCallback): void {
    if (this.connection) {
      this.connection.off('ProductAttributeCreated', callback);
    }
  }

  onProductAttributeUpdated(callback: ProductAttributeUpdatedCallback): void {
    if (this.connection) {
      this.connection.on('ProductAttributeUpdated', callback);
    }
  }

  offProductAttributeUpdated(callback: ProductAttributeUpdatedCallback): void {
    if (this.connection) {
      this.connection.off('ProductAttributeUpdated', callback);
    }
  }

  onProductAttributeDeleted(callback: ProductAttributeDeletedCallback): void {
    if (this.connection) {
      this.connection.on('ProductAttributeDeleted', callback);
    }
  }

  offProductAttributeDeleted(callback: ProductAttributeDeletedCallback): void {
    if (this.connection) {
      this.connection.off('ProductAttributeDeleted', callback);
    }
  }

  getConnectionState(): signalR.HubConnectionState {
    return this.connection?.state ?? signalR.HubConnectionState.Disconnected;
  }

  isConnected(): boolean {
    return this.connection?.state === signalR.HubConnectionState.Connected;
  }
}

export const signalRService = new SignalRService();
