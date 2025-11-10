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
    
    const response = await fetch(`${API_BASE_URL}/api/todos?${searchParams}`)
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
      },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Bir hata oluştu')
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
      },
      body: JSON.stringify(data),
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Bir hata oluştu')
    }
    return response.json()
  },

  delete: async (id: number) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/todos/${id}`, {
      method: 'DELETE',
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Bir hata oluştu')
    }
    return response.json()
  },
}

