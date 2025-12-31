import api, {type PaginationQuery} from "./api.ts";
import type {CreateProductAttributeCommand, UpdateProductAttributeCommand} from "../Api";
import { authService } from './authService';
import { getApiBaseUrl } from '../utils/apiConfig';
import { handleResponseError } from '../utils/errorHandler';

// Helper function to add authorization header
const getAuthHeaders = () => {
  const token = authService.getToken();
  const headers: HeadersInit = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
};

export const productAttributeService = {
  getAll: async (params: PaginationQuery & { searchKey?: string; productId?: number }) => {
    const { data } = await api.api.productAttributesList({
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        searchKey: params.searchKey,
        productId: params.productId
    });
    return data;
  },

  getById: async (id: number) => {
    const { data } = await api.api.productAttributesByIdList({
        id : id
    });
    return data;
  },

  create: async (dto: CreateProductAttributeCommand) => {
    const response = await api.api.productAttributesCreate(dto);
    return response.data.productAttributeId;
  },

  update: async (dto: UpdateProductAttributeCommand) => {
    const response = await api.api.productAttributesUpdate(dto);
    return response.data;
  },

  delete: async (id: number): Promise<void> => {
    await api.api.productAttributesDelete({
        id : id
    });
  },

  exportExcel: async () => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/product-attributes/export/excel`, {
      method: 'GET',
      headers: getAuthHeaders(),
    });
    
    if (!response.ok) {
      await handleResponseError(response, 'Excel export başarısız oldu');
    }
    
    // Blob'dan dosya oluştur ve indir
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    
    // Dosya adını header'dan al veya varsayılan ad kullan
    const contentDisposition = response.headers.get('content-disposition');
    let fileName = 'Urun_Oznitelikleri.xlsx';
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

