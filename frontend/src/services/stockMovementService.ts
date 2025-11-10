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

export const stockMovementService = {
  getAll: async (params: {
    pageNumber?: number
    pageSize?: number
    productId?: number
    categoryId?: number
    type?: number
  }) => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(
      `${API_BASE_URL}/api/stock-movements?${new URLSearchParams({
        ...(params.pageNumber && { pageNumber: params.pageNumber.toString() }),
        ...(params.pageSize && { pageSize: params.pageSize.toString() }),
        ...(params.productId && { productId: params.productId.toString() }),
        ...(params.categoryId && { categoryId: params.categoryId.toString() }),
        ...(params.type && { type: params.type.toString() }),
      })}`
    )
    return response.json()
  },

  exportExcel: async () => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/stock-movements/export/excel`, {
      method: 'GET',
    });

    if (!response.ok) {
      throw new Error('Excel export başarısız oldu');
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


