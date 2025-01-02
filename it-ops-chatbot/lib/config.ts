export const config = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, '') || 'http://localhost:5000',
  apiConfigured: process.env.NEXT_PUBLIC_API_CONFIGURED === 'true',
  testUser: process.env.NEXT_PUBLIC_TEST_USER || 'testuser@myapp.com',
  apiKey: process.env.NEXT_PUBLIC_API_KEY || '1234',
  endpoints: {
    chat: 'chat'
  }
} as const 