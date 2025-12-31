// Helper function to add authorization header
import { authService } from './authService';
import { getApiBaseUrl } from '../utils/apiConfig';

const API_BASE_URL = getApiBaseUrl();
import { handleResponseError } from '../utils/errorHandler';

const getAuthHeaders = () => {
  const token = authService.getToken();
  const headers: HeadersInit = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
};

export interface LocationDto {
  id: number;
  name: string;
  description?: string | null;
  productCount: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface LocationDetailDto {
  id: number;
  name: string;
  description?: string | null;
  products: LocationProductDto[];
}

export interface LocationProductDto {
  id: number;
  name: string;
  stockCode: string;
  stockQuantity: number;
}

export interface CreateLocationCommand {
  name: string;
  description?: string | null;
}

export interface UpdateLocationCommand {
  locationId: number;
  name?: string | null;
  description?: string | null;
}

export const locationService = {
  getAll: async (params: { pageNumber?: number; pageSize?: number; searchTerm?: string }) => {
    const response = await fetch(
      `${API_BASE_URL}/api/locations?pageNumber=${params.pageNumber || 1}&pageSize=${params.pageSize || 10}${params.searchTerm ? `&searchTerm=${encodeURIComponent(params.searchTerm)}` : ''}`,
      {
        headers: getAuthHeaders(),
      }
    );
    if (!response.ok) {
      await handleResponseError(response, 'Lokasyonlar yüklenirken bir hata oluştu');
    }
    return response.json();
  },

  getById: async (id: number) => {
    const response = await fetch(`${API_BASE_URL}/api/locations/by-id?id=${id}`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) {
      await handleResponseError(response, 'Lokasyon bilgisi yüklenirken bir hata oluştu');
    }
    return response.json();
  },

  create: async (dto: CreateLocationCommand) => {
    const response = await fetch(`${API_BASE_URL}/api/locations`, {
      method: 'POST',
      headers: { 
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(dto)
    });
    if (!response.ok) {
      await handleResponseError(response, 'Lokasyon oluşturulamadı');
    }
    const data = await response.json();
    return data.locationId;
  },

  update: async (id: number, dto: UpdateLocationCommand) => {
    const response = await fetch(`${API_BASE_URL}/api/locations`, {
      method: 'PUT',
      headers: { 
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify({ ...dto, locationId: id })
    });
    if (!response.ok) {
      await handleResponseError(response, 'Lokasyon güncellenemedi');
    }
    return response.json();
  },

  delete: async (id: number) => {
    const response = await fetch(`${API_BASE_URL}/api/locations?id=${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) {
      await handleResponseError(response, 'Lokasyon silinemedi');
    }
  }
};
