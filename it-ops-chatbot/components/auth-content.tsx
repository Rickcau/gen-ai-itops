"use client";

import { useAuth } from '@/components/providers/auth-provider';
import { LoadingSpinner } from '@/components/ui/loading';

export function AuthContent({ children }: { children: React.ReactNode }) {
  const { authState } = useAuth();

  // Only show loading spinner during initial authentication and when we don't have a user
  if (authState.isLoading && !authState.user) {
    return <LoadingSpinner />;
  }

  // Show error state if authentication failed
  if (!authState.isAuthenticated) {
    return (
      <div className="flex items-center justify-center min-h-screen">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-red-500 mb-4">Authentication Required</h1>
          <p className="text-gray-700">{authState.error || 'Please login with az login first'}</p>
        </div>
      </div>
    );
  }

  // If we get here, we're authenticated and have user data
  return children;
} 
