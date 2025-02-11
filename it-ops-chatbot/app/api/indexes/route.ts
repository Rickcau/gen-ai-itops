import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET() {
  console.log('Next.js API Route: GET /api/indexes called')
  try {
    const url = `${API_BASE_URL}/indexes`
    console.log('Next.js API Route: Fetching indexes from:', url)

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
    console.error('Error in indexes API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch indexes',
      500
    )
  }
}

export async function DELETE(request: NextRequest) {
  console.log('Next.js API Route: DELETE /api/indexes called')
  try {
    // Get the indexName from query parameters
    const { searchParams } = new URL(request.url)
    const indexName = searchParams.get('indexName')
    console.log('Next.js API Route: Extracted indexName:', indexName)

    if (!indexName) {
      return createErrorResponse('Index name is required', 400)
    }

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/indexes?indexName=${encodeURIComponent(indexName)}`
    console.log('Next.js API Route: Deleting index from:', url)

    // Use fetchFromApi and await it without assigning to an unused variable
    await fetchFromApi(url, {
      method: 'DELETE'
    })
    
    return createApiResponse({ success: true, message: `Successfully deleted index: ${indexName}` })
  } catch (error) {
    console.error('Error in delete index API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to delete index',
      500
    )
  }
}

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/indexes called')
  try {
    // Get the indexName from the path
    const segments = request.url.split('/')
    const indexName = segments[segments.length - 1]

    if (!indexName) {
      return createErrorResponse('Index name is required', 400)
    }

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/indexes/${encodeURIComponent(indexName)}`
    console.log('Next.js API Route: Creating index at:', url)

    const response = await fetch(url, {
      method: 'POST',
      headers: {
        'accept': 'application/json',
        'api-key': apiKey
      }
    })

    if (!response.ok) {
      const errorText = await response.text()
      console.error('Create failed:', {
        status: response.status,
        statusText: response.statusText,
        body: errorText
      })
      return createErrorResponse(errorText || response.statusText, response.status)
    }
    
    return createApiResponse({ success: true, message: `Successfully created index: ${indexName}` })
  } catch (error) {
    console.error('Error in create index API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to create index',
      500
    )
  }
} 