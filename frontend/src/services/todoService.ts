import api from './api'

export const todoService = {
  getAll: async (params: {
    pageNumber?: number
    pageSize?: number
    status?: number
    priority?: number
  }) => {
    const searchParams = new URLSearchParams()
    if (params.pageNumber) searchParams.append('pageNumber', params.pageNumber.toString())
    if (params.pageSize) searchParams.append('pageSize', params.pageSize.toString())
    if (params.status !== undefined) searchParams.append('status', params.status.toString())
    if (params.priority !== undefined) searchParams.append('priority', params.priority.toString())
    
    const response = await fetch(`http://localhost:5134/api/todos?${searchParams}`)
    return response.json()
  },

  create: async (data: {
    title: string
    description?: string
    status?: number
    priority?: number
  }) => {
    const response = await fetch('http://localhost:5134/api/todos', {
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
    const response = await fetch(`http://localhost:5134/api/todos/${id}`, {
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
    const response = await fetch(`http://localhost:5134/api/todos/${id}`, {
      method: 'DELETE',
    })
    if (!response.ok) {
      const error = await response.json()
      throw new Error(error.message || 'Bir hata oluştu')
    }
    return response.json()
  },
}

