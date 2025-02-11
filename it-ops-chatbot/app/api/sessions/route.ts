import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET() {
  console.log('Next.js API Route: GET /api/sessions called')
  try {
    const url = `${API_BASE_URL}/sessions`
    console.log('Next.js API Route: Fetching sessions from:', url)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in sessions API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch sessions',
      500
    )
  }
}

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/sessions called')
  try {
    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const sessionData = await request.json()
    console.log('Next.js API Route: Creating session:', sessionData)

    const url = `${API_BASE_URL}/sessions`
    console.log('Next.js API Route: Creating session at:', url)

    const response = await fetchFromApi(url, {
      method: 'POST',
      body: JSON.stringify(sessionData)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in create session API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to create session',
      500
    )
  }
}
