// Helper function to add authorization header
import { authService } from './authService';
import { getApiBaseUrl } from '../utils/apiConfig';
import { handleResponseError } from '../utils/errorHandler';

const getAuthHeaders = () => {
  const token = authService.getToken();
  const headers: HeadersInit = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
};

export const stockMovementService = {
  getAll: async (params: {
    pageNumber?: number
    pageSize?: number
    productId?: number
    categoryId?: number
    type?: number
    searchTerm?: string
    startDate?: string
    endDate?: string
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const searchParams = new URLSearchParams();
    if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString());
    if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString());
    if (params.productId) searchParams.append('productId', params.productId.toString());
    if (params.categoryId) searchParams.append('categoryId', params.categoryId.toString());
    if (params.type) searchParams.append('type', params.type.toString());
    if (params.searchTerm) searchParams.append('searchTerm', params.searchTerm);
    if (params.startDate) searchParams.append('startDate', params.startDate);
    if (params.endDate) searchParams.append('endDate', params.endDate);
    
    const response = await fetch(
      `${API_BASE_URL}/api/stock-movements?${searchParams.toString()}`,
      {
        headers: getAuthHeaders(),
      }
    )
    if (!response.ok) {
      await handleResponseError(response, 'Stok hareketleri yüklenirken bir hata oluştu');
    }
    return response.json()
  },

  exportExcel: async () => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/stock-movements/export/excel`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });

    if (!response.ok) {
      await handleResponseError(response, 'Excel export başarısız oldu');
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;

    const contentDisposition = response.headers.get('content-disposition');
    let fileName = 'StokHareketleri.xlsx';
    if (contentDisposition) {
      const match = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (match && match[1]) {
        fileName = match[1].replace(/['"]/g, '');
        if (fileName.startsWith("UTF-8''")) {
          fileName = decodeURIComponent(fileName.replace("UTF-8''", ""));
        }
      }
    }

    link.setAttribute('download', fileName);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  },

  create: async (data: {
    productId: number
    type: number
    quantity: number
    unitPrice: number
    description?: string
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/stock-movements`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      await handleResponseError(response, 'Stok hareketi oluşturulamadı');
    }
    return response.json()
  },
}


