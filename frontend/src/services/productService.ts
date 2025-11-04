import api, {type PaginationQuery} from "./api.ts";
import type {CreateProductCommand, UpdateProductCommand} from "../Api";

export const productService = {
  getAll: async (params: PaginationQuery & { searchTerm?: string; categoryId?: number; locationId?: number }) => {
    const API_BASE_URL = 'http://localhost:5134/api';
    const queryParams = new URLSearchParams({
      pageNumber: params.pageNumber.toString(),
      pageSize: params.pageSize.toString(),
    });
    if (params.searchTerm) queryParams.append('searchTerm', params.searchTerm);
    if (params.categoryId) queryParams.append('categoryId', params.categoryId.toString());
    if (params.locationId) queryParams.append('locationId', params.locationId.toString());
    
    const response = await fetch(`${API_BASE_URL}/products?${queryParams}`);
    if (!response.ok) throw new Error('Failed to fetch products');
    return response.json();
  },

  getById: async (id: number) => {
    const { data } = await api.api.productsByIdList({
        id : id
    });
    return data;
  },

  create: async (dto: CreateProductCommand & { image?: File }) => {
    const API_BASE_URL = 'http://localhost:5134/';
    const formData = new FormData();
    
    formData.append('name', dto.name);
    formData.append('description', dto.description || '');
    formData.append('stockQuantity', dto.stockQuantity.toString());
    formData.append('lowStockThreshold', dto.lowStockThreshold.toString());
    formData.append('categoryId', dto.categoryId.toString());
    
    if ((dto as any).locationId) {
      formData.append('locationId', (dto as any).locationId.toString());
    }
    
    if (dto.image) {
      formData.append('image', dto.image);
    }
    
    const response = await fetch(`${API_BASE_URL}api/products`, {
      method: 'POST',
      body: formData,
    });
    
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Bir hata oluştu' }));
      throw new Error(error.message || 'Ürün oluşturulamadı');
    }
    
    const data = await response.json();
    return data.productId;
  },

  update: async (dto: UpdateProductCommand & { image?: File }) => {
    const API_BASE_URL = 'http://localhost:5134/';
    const formData = new FormData();
    
    formData.append('id', dto.id.toString());
    
    if (dto.name !== undefined && dto.name !== null) {
      formData.append('name', dto.name);
    }
    if (dto.description !== undefined) {
      formData.append('description', dto.description || '');
    }
    if (dto.stockQuantity !== undefined && dto.stockQuantity !== null) {
      formData.append('stockQuantity', dto.stockQuantity.toString());
    }
    if (dto.lowStockThreshold !== undefined && dto.lowStockThreshold !== null) {
      formData.append('lowStockThreshold', dto.lowStockThreshold.toString());
    }
    
    if ((dto as any).locationId !== undefined) {
      if ((dto as any).locationId === null || (dto as any).locationId === '') {
        formData.append('locationId', '');
      } else {
        formData.append('locationId', (dto as any).locationId.toString());
      }
    }
    
    if (dto.image) {
      formData.append('image', dto.image);
    }
    
    const response = await fetch(`${API_BASE_URL}api/products`, {
      method: 'PUT',
      body: formData,
    });
    
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Bir hata oluştu' }));
      throw new Error(error.message || 'Ürün güncellenemedi');
    }
    
    const data = await response.json();
    return data;
  },

  delete: async (id: number) => {
    await api.api.productsDelete({
        id : id
    });
  },

  exportExcel: async () => {
    const API_BASE_URL = 'http://localhost:5134/';
    const response = await fetch(`${API_BASE_URL}api/products/export/excel`, {
      method: 'GET',
    });
    
    if (!response.ok) {
      throw new Error('Excel export başarısız oldu');
    }
    
    // Blob'dan dosya oluştur ve indir
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    
    // Dosya adını header'dan al veya varsayılan ad kullan
    const contentDisposition = response.headers.get('content-disposition');
    let fileName = 'Urunler.xlsx';
    if (contentDisposition) {
      const fileNameMatch = contentDisposition.match(/filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/);
      if (fileNameMatch && fileNameMatch[1]) {
        fileName = fileNameMatch[1].replace(/['"]/g, '');
        // UTF-8 encoded filename için decode
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
};

