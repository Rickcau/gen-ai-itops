/**
 * Client-side configuration
 * IMPORTANT: Do not store any sensitive information here
 */
export const config = {
  isLocalDevelopment: typeof window !== 'undefined' && window.location.hostname === 'localhost',
  apiConfigured: true // Controls mock mode
} as const 