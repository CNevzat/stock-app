import api from './api';

export interface DashboardStats {
  totalCategories: number;
  totalProducts: number;
  totalProductAttributes: number;
  totalStockQuantity: number;
  lowStockProducts: number;
  outOfStockProducts: number;
  totalInventoryCost: number;
  totalInventoryPotentialRevenue: number;
  totalExpectedSalesRevenue: number;
  totalPurchaseSpent: number;
  totalPotentialProfit: number;
  averageMarginPercentage: number;
  categoryStats: CategoryStats[];
  productStockStatus: ProductStock[];
  stockDistribution: StockDistribution[];
  categoryValueDistribution?: CategoryValue[];
  topValuableProducts?: ProductValue[];
  recentStockMovements?: RecentStockMovement[];
  stockMovementTrend?: StockMovementTrend[];
  lastYearStockMovementTrend?: StockMovementTrend[];
}

export interface StockMovementTrend {
  dateLabel: string;
  stockIn: number;
  stockOut: number;
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

export interface CategoryValue {
  categoryId: number;
  categoryName: string;
  totalCost: number;
  totalPotentialRevenue: number;
  totalPotentialProfit: number;
}

export interface ProductValue {
  productId: number;
  productName: string;
  stockCode: string;
  inventoryCost: number;
  inventoryPotentialRevenue: number;
  potentialProfit: number;
}

export const dashboardService = {
  getStats: async (): Promise<DashboardStats> => {
    const response = await api.api.dashboardStatsList();
    return response.data as DashboardStats;
  },
};

