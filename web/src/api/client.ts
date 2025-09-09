import axios, { AxiosError } from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5080';

export const apiClient = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
  withCredentials: true, // Always send cookies with requests
});

// Auth functions that will be set by the auth provider
let getAccessToken: (() => string | null) | null = null;
let refreshAccessToken: (() => Promise<boolean>) | null = null;

export const setAuthInterceptors = (
  getToken: () => string | null,
  refreshToken: () => Promise<boolean>
) => {
  getAccessToken = getToken;
  refreshAccessToken = refreshToken;
};

// Request interceptor to add the auth token
apiClient.interceptors.request.use(
  (config) => {
    if (getAccessToken) {
      const token = getAccessToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor to handle 401s
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as any;

    // Don't attempt to refresh for auth endpoints (login, register, refresh)
    const isAuthEndpoint = originalRequest.url?.includes('/auth/');
    
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        }).then(() => {
          return apiClient(originalRequest);
        }).catch(err => {
          return Promise.reject(err);
        });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        if (refreshAccessToken) {
          const success = await refreshAccessToken();
          if (success) {
            processQueue(null);
            return apiClient(originalRequest);
          }
        }
        
        processQueue(error, null);
        // Redirect to login if refresh failed
        window.location.href = '/login';
        return Promise.reject(error);
      } catch (refreshError) {
        processQueue(refreshError, null);
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);