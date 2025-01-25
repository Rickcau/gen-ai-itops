export const config = {
  apiBaseUrl: process.env.NEXT_PUBLIC_API_BASE_URL?.replace(/\/$/, '') || 'https://localhost:7049',
  apiConfigured: process.env.NEXT_PUBLIC_API_CONFIGURED === 'true',
  testUser: process.env.NEXT_PUBLIC_TEST_USER || 'test.user@example.com',
  apiKey: process.env.NEXT_PUBLIC_API_KEY || '',
  endpoints: {
    chat: 'chat'
  }
} as const 