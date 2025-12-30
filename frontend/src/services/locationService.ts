// API base URL helper - hem dev hem production'da 5134 portunu kullan
const getApiBaseUrl = () => {
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL;
  }
  if (import.meta.env.PROD) {
    return `http://${window.location.hostname}:5134`;
  }
  return 'http://localhost:5134';
};

const API_BASE_URL = getApiBaseUrl();

// Helper function to add authorization header
import { authService } from './authService';

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
    if (!response.ok) throw new Error('Failed to fetch locations');
    return response.json();
  },

  getById: async (id: number) => {
    const response = await fetch(`${API_BASE_URL}/api/locations/by-id?id=${id}`, {
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to fetch location');
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
    if (!response.ok) throw new Error('Failed to create location');
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
    if (!response.ok) throw new Error('Failed to update location');
    return response.json();
  },

  delete: async (id: number) => {
    const response = await fetch(`${API_BASE_URL}/api/locations?id=${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    });
    if (!response.ok) throw new Error('Failed to delete location');
  }
};
