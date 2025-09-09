import { apiClient } from './client';
import type { AuthResult } from '@/types/AuthResult';
import type { LoginCredentials } from '@/types/LoginCredentials';
import type { RegisterCredentials } from '@/types/RegisterCredentials';

export const authApi = {
  login: async (credentials: LoginCredentials): Promise<AuthResult> => {
    const response = await apiClient.post<AuthResult>('/auth/login', credentials);
    return response.data;
  },

  register: async (credentials: RegisterCredentials): Promise<AuthResult> => {
    const response = await apiClient.post<AuthResult>('/auth/register', credentials);
    return response.data;
  },

  refresh: async (): Promise<AuthResult> => {
    // Server expects refresh token from HTTP cookie, not body
    const response = await apiClient.post<AuthResult>('/auth/refresh', {}, {
      withCredentials: true
    });
    return response.data;
  },

  logout: async (): Promise<void> => {
    // Server expects refresh token from HTTP cookie, not body
    await apiClient.post('/auth/logout', {}, {
      withCredentials: true
    });
  },
};