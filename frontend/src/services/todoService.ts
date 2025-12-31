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

export const todoService = {
  getAll: async (params: {
    pageNumber?: number
    pageSize?: number
    status?: number
    priority?: number
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const searchParams = new URLSearchParams()
    if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString())
    if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString())
    if (params.status !== undefined) searchParams.append('status', params.status.toString())
    if (params.priority !== undefined) searchParams.append('priority', params.priority.toString())
    
    const response = await fetch(`${API_BASE_URL}/api/todos?${searchParams}`, {
      headers: getAuthHeaders(),
    })
    return response.json()
  },

  create: async (data: {
    title: string
    description?: string
    status?: number
    priority?: number
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/todos`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      await handleResponseError(response, 'Yapılacak oluşturulamadı');
    }
    return response.json()
  },

  update: async (id: number, data: {
    title?: string
    description?: string
    status?: number
    priority?: number
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/todos/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      await handleResponseError(response, 'Yapılacak oluşturulamadı');
    }
    return response.json()
  },

  delete: async (id: number) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/todos/${id}`, {
      method: 'DELETE',
      headers: getAuthHeaders(),
    })
    if (!response.ok) {
      await handleResponseError(response, 'Yapılacak oluşturulamadı');
    }
    return response.json()
  },
}

