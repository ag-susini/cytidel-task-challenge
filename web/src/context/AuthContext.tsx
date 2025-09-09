import React, { createContext, useContext, useState, useEffect, useCallback, type ReactNode } from 'react';
import type { User } from '@/types/User';
import type { AuthResult } from '@/types/AuthResult';
import type { LoginCredentials } from '@/types/LoginCredentials';
import type { RegisterCredentials } from '@/types/RegisterCredentials';
import { authApi } from '@/api/auth';
import { setAuthInterceptors } from '@/api/client';
import { jwtDecode } from 'jwt-decode';

interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (credentials: LoginCredentials) => Promise<AuthResult>;
  register: (credentials: RegisterCredentials) => Promise<AuthResult>;
  logout: () => Promise<void>;
  getAccessToken: () => string | null;
  refreshAccessToken: () => Promise<boolean>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface JwtPayload {
  sub: string;
  [key: string]: any; // Allow any claim type
  exp: number;
}

// Cookie helper functions
const setCookie = (name: string, value: string, hours: number = 24) => {
  const expires = new Date();
  expires.setTime(expires.getTime() + hours * 60 * 60 * 1000);
  const isSecure = window.location.protocol === 'https:';
  const cookieString = `${name}=${value};expires=${expires.toUTCString()};path=/;${isSecure ? 'secure;' : ''}samesite=strict`;
  document.cookie = cookieString;
};

const getCookie = (name: string): string | null => {
  const nameEQ = name + "=";
  const ca = document.cookie.split(';');
  for (let i = 0; i < ca.length; i++) {
    let c = ca[i];
    while (c.charAt(0) === ' ') c = c.substring(1, c.length);
    if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
  }
  return null;
};

const deleteCookie = (name: string) => {
  document.cookie = `${name}=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;`;
};

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isInitializing, setIsInitializing] = useState(true);

  const setTokens = (newAccessToken: string, newRefreshToken: string) => {
    // Store access token for 15 minutes (matches backend expiry)
    setCookie('accessToken', newAccessToken, 0.25);
    // Store refresh token for 14 days (matches backend expiry)
    setCookie('refreshToken', newRefreshToken, 336);

    try {
      const decoded = jwtDecode<JwtPayload>(newAccessToken);
      
      // Try to find email in different possible claim names
      const email = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
                    decoded.email || 
                    decoded.Email ||
                    decoded['unique_name'] ||
                    decoded['nameid'];
                    
        
      setUser({
        id: decoded.sub,
        email: email,
      });
      
      // Mark initialization as complete when tokens are set (after login)
      setIsInitializing(false);
    } catch (error) {
      console.error('Failed to decode token:', error);
    }
  };

  const clearAuth = () => {
    deleteCookie('accessToken');
    deleteCookie('refreshToken');
    setUser(null);
  };

  const refreshAccessToken = useCallback(async (): Promise<boolean> => {
    const currentRefreshToken = getCookie('refreshToken');
    if (!currentRefreshToken) {
      clearAuth();
      return false;
    }

    try {
      const result = await authApi.refresh();
      if (result.success && result.accessToken && result.refreshToken) {
        setTokens(result.accessToken, result.refreshToken);
        return true;
      }
      clearAuth();
      return false;
    } catch {
      clearAuth();
      return false;
    }
  }, []);

  const login = useCallback(async (credentials: LoginCredentials): Promise<AuthResult> => {
    try {
      setIsLoading(true);
      const result = await authApi.login(credentials);
      
      if (result.success && result.accessToken && result.refreshToken) {
        setTokens(result.accessToken, result.refreshToken);
      }
      
      return result;
    } catch (error: any) {
      return {
        success: false,
        error: error.response?.data?.error || 'Login failed',
      };
    } finally {
      setIsLoading(false);
    }
  }, []);

  const register = async (credentials: RegisterCredentials): Promise<AuthResult> => {
    try {
      setIsLoading(true);
      const result = await authApi.register(credentials);
      
      if (result.success && result.accessToken && result.refreshToken) {
        setTokens(result.accessToken, result.refreshToken);
      }
      
      return result;
    } catch (error: any) {
      return {
        success: false,
        error: error.response?.data?.error || 'Registration failed',
      };
    } finally {
      setIsLoading(false);
    }
  };

  const logout = useCallback(async () => {
    try {
      const currentRefreshToken = getCookie('refreshToken');
      if (currentRefreshToken) {
        await authApi.logout();
      }
    } catch {
      // Logout errors are not critical
    } finally {
      clearAuth();
    }
  }, []);

  const getAccessToken = () => getCookie('accessToken');

  // Initialize user from existing token on app start
  useEffect(() => {
    const initializeAuth = async () => {
      const token = getCookie('accessToken');
      
      if (token) {
        try {
          const decoded = jwtDecode<JwtPayload>(token);
          // Check if token is expired
          if (decoded.exp * 1000 > Date.now()) {
            // Try to find email in different possible claim names
            const email = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ||
                          decoded.email || 
                          decoded.Email ||
                          decoded['unique_name'] ||
                          decoded['nameid'];
              
            setUser({
              id: decoded.sub,
              email: email,
            });
          } else {
            // Token expired, try to refresh
            await refreshAccessToken();
          }
        } catch {
          clearAuth();
        }
      }
      
      setIsInitializing(false);
    };

    initializeAuth();
    setAuthInterceptors(getAccessToken, refreshAccessToken);
  }, []);

  return (
    <AuthContext.Provider 
      value={{
        user,
        isAuthenticated: !isInitializing && !!getCookie('accessToken') && !!user,
        isLoading: isLoading || isInitializing,
        login,
        register,
        logout,
        getAccessToken,
        refreshAccessToken,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

// eslint-disable-next-line react-refresh/only-export-components
export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};