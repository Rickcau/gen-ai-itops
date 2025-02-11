import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET(request: NextRequest) {
  console.log('Next.js API Route: GET /api/indexes/documents called')
  try {
    const { searchParams } = new URL(request.url)
    const indexName = searchParams.get('indexName')
    console.log('Next.js API Route: Listing documents for index:', indexName)

    if (!indexName) {
      return createErrorResponse('Index name is required', 400)
    }

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    // Determine version based on index name
    const version = indexName.toLowerCase().includes('v2') ? 'v2' : 'v1'
    console.log('Next.js API Route: Using API version:', version, 'for index:', indexName)

    // Use the same parameters as the backend API with dynamic version
    const url = `${API_BASE_URL}/indexes/${version}/documents?indexName=${encodeURIComponent(indexName)}&suppressVectorFields=${searchParams.get('suppressVectorFields')}&maxResults=${searchParams.get('maxResults')}`
    console.log('Next.js API Route: Fetching documents from:', url)

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in list documents API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to list documents',
      500
    )
  }
} 