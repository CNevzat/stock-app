import api from './api'

export const stockMovementService = {
  getAll: async (params: {
    pageNumber?: number
    pageSize?: number
    productId?: number
    categoryId?: number
    type?: number
  }) => {
    const response = await fetch(
      `http://localhost:5134/api/stock-movements?${new URLSearchParams({
        ...(params.pageNumber && { pageNumber: params.pageNumber.toString() }),
        ...(params.pageSize && { pageSize: params.pageSize.toString() }),
        ...(params.productId && { productId: params.productId.toString() }),
        ...(params.categoryId && { categoryId: params.categoryId.toString() }),
        ...(params.type && { type: params.type.toString() }),
      })}`
    )
    return response.json()
  },

  create: async (data: {
    productId: number
    type: number
    quantity: number
    description?: string
  }) => {
    const response = await fetch('http://localhost:5134/api/stock-movements', {
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
}


