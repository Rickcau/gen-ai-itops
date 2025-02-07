export const config = {
  apiBaseUrl: process.env.API_BASE_URL || 'http://localhost:5000',
  apiKey: process.env.API_KEY || '',
  isLocalDevelopment: typeof window !== 'undefined' && window.location.hostname === 'localhost',
  apiConfigured: true // Controls mock mode
} as const

export const getApiBaseUrl = () => {
  return process.env.API_BASE_URL || 'https://localhost:7049'
}

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
      const response = await fetch('/.auth/me');
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