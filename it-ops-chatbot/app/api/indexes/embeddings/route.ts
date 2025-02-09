import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/indexes/embeddings called')
  try {
    const { searchParams } = new URL(request.url)
    const indexName = searchParams.get('indexName')
    console.log('Next.js API Route: Generating embeddings for index:', indexName)

    if (!indexName) {
      return createErrorResponse('Index name is required', 400)
    }

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/indexes/embeddings?indexName=${encodeURIComponent(indexName)}`
    console.log('Next.js API Route: Generating embeddings at:', url)

    const response = await fetchFromApi(url, {
      method: 'POST'
    })
    
    return createApiResponse({ success: true, message: `Successfully generated embeddings for index: ${indexName}` })
  } catch (error) {
    console.error('Error in generate embeddings API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to generate embeddings',
      500
    )
  }
} 