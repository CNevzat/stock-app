import api from './api';

export interface DashboardStats {
  totalCategories: number;
  totalProducts: number;
  totalProductAttributes: number;
  totalStockQuantity: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  categoryStats: CategoryStats[];
  productStockStatus: ProductStock[];
  stockDistribution: StockDistribution[];
  recentStockMovements?: RecentStockMovement[];
}

export interface RecentStockMovement {
  id: number;
  productName: string;
  type: number;
  quantity: number;
  createdAt: string;
}

export interface CategoryStats {
  categoryId: number;
  categoryName: string;
  productCount: number;
  totalStock: number;
}

export interface ProductStock {
  productId: number;
  productName: string;
  stockCode: string;
  stockQuantity: number;
  categoryName: string;
  status: string;
}

export interface StockDistribution {
  status: string;
  count: number;
  percentage: number;
}

export const dashboardService = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await api.api.dashboardStatsList();
    return response.data as DashboardStats;
  },
};

