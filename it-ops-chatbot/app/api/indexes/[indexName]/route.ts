import { createApiResponse, createErrorResponse } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(
  request: NextRequest,
  { params }: { params: { indexName: string } }
) {
  console.log('Next.js API Route: POST /api/indexes/[indexName] called')
  try {
    const { indexName } = params

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