import { InteractiveBrowserCredential } from '@azure/identity';

export const config = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, '') || 'https://localhost:7049',
  apiConfigured: process.env.NEXT_PUBLIC_API_CONFIGURED === 'true',
  isLocalDevelopment: typeof window !== 'undefined' && window.location.hostname === 'localhost',
  endpoints: {
    chat: 'chat',
    user: 'users/me'
  }
} as const

export const getCurrentUser = async () => {
  try {
    // If running locally, use az CLI through our API route
    if (config.isLocalDevelopment) {
      const response = await fetch('/api/user');
      if (!response.ok) {
        throw new Error('Please login using az login first');
      }
      return await response.json();
    } 
    // If running in Azure App Service, use Easy Auth
    else {
      // Use window.location to get the base URL for Easy Auth
      const baseUrl = window.location.origin;
      const response = await fetch(`${baseUrl}/.auth/me`);
      if (!response.ok) {
        throw new Error('Authentication required');
      }
      const authData = await response.json();
      const userDetails = authData?.clientPrincipal;
      if (!userDetails?.userDetails) {
        throw new Error('No user details found');
      }
      return { email: userDetails.userDetails };
    }
  } catch (error) {
    console.error('Error getting user info:', error);
    throw error;
  }
}