import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET() {
  console.log('Next.js API Route: GET /api/capabilities called')
  try {
    const url = `${API_BASE_URL}/capabilities`
    console.log('Next.js API Route: Fetching capabilities from:', url)

    // Ensure we have an API key
    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in capabilities API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch capabilities',
      500
    )
  }
} 