import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/indexes/capabilities/search called')
  try {
    const { searchParams } = new URL(request.url)
    const indexName = searchParams.get('indexName')
    console.log('Next.js API Route: Searching index:', indexName)

    if (!indexName) {
      return createErrorResponse('Index name is required', 400)
    }

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    // Get the search parameters from the request body
    const searchBody = await request.json()
    console.log('Next.js API Route: Search parameters:', searchBody)

    const url = `${API_BASE_URL}/indexes/capabilities/search?indexName=${encodeURIComponent(indexName)}`
    console.log('Next.js API Route: Searching at:', url)

    const response = await fetchFromApi(url, {
      method: 'POST',
      body: JSON.stringify(searchBody)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in search API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to search index',
      500
    )
  }
} 