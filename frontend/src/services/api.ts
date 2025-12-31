// API base URL - mobil ve web için
import { getApiBaseUrl } from '../utils/apiConfig';
import { authService } from './authService';
import { showToast } from '../components/Toast';

const API_BASE_URL = getApiBaseUrl();

import {Api} from "../Api";

export interface PaginationQuery{
    pageNumber: number;
    pageSize: number;
}

const api = new Api({
    baseURL: API_BASE_URL,
    secure: true,
    timeout: 20000,
});

// Add token to requests
api.instance.interceptors.request.use(
  (config) => {
    const token = authService.getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Handle 401 errors (unauthorized) and 403 errors (forbidden)
api.instance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // Handle 403 Forbidden - show permission error message
    if (error.response?.status === 403) {
      showToast('Bu işlemi yapmanız için yetkiniz yoktur.', 'error');
      return Promise.reject(error);
    }

    // If 401 and we haven't tried to refresh yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken = authService.getRefreshToken();
        if (refreshToken) {
          const response = await authService.refreshToken(refreshToken);
          authService.setTokens(response.accessToken, response.refreshToken);
          authService.setUser(response.user);
          
          // Retry original request with new token
          originalRequest.headers.Authorization = `Bearer ${response.accessToken}`;
          return api.instance(originalRequest);
        }
      } catch (refreshError) {
        // Refresh failed, logout user
        authService.clearTokens();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default api
