"use client";

import { createContext, useContext, useEffect, useState } from 'react';
import { AuthContextType, AuthState } from '@/types/auth';

const initialState: AuthState = {
  user: null,
  isLoading: true,
  error: null,
  isAuthenticated: false
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [authState, setAuthState] = useState<AuthState>(initialState);

  const getCurrentUser = async () => {
    const response = await fetch('/api/user');
    if (!response.ok) {
      throw new Error('Please login using az login first');
    }
    return await response.json();
  };

  useEffect(() => {
    const authenticate = async () => {
      try {
        const user = await getCurrentUser();
        setAuthState({
          user,
          isLoading: false,
          error: null,
          isAuthenticated: true
        });
      } catch (error) {
        setAuthState({
          user: null,
          isLoading: false,
          error: error instanceof Error ? error.message : 'Authentication failed',
          isAuthenticated: false
        });
      }
    };

    authenticate();
  }, []);

  const login = async () => {
    if (authState.isAuthenticated) return;
    setAuthState(prev => ({ ...prev, isLoading: true }));
    
    try {
      const user = await getCurrentUser();
      setAuthState({
        user,
        isLoading: false,
        error: null,
        isAuthenticated: true
      });
    } catch (error) {
      setAuthState({
        user: null,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Authentication failed',
        isAuthenticated: false
      });
    }
  };

  const logout = async () => {
    try {
      // Add any async cleanup if needed in the future
      setAuthState({
        user: null,
        isLoading: false,
        error: null,
        isAuthenticated: false
      });
    } catch (error) {
      console.error('Logout error:', error);
      setAuthState({
        user: null,
        isLoading: false,
        error: error instanceof Error ? error.message : 'Logout failed',
        isAuthenticated: false
      });
    }
  };

  return (
    <AuthContext.Provider value={{ 
      authState,
      login,
      logout
    }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
} 
