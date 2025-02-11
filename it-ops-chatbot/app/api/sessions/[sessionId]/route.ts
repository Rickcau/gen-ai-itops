import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET(
  request: NextRequest,
  { params }: { params: { sessionId: string } }
) {
  console.log('Next.js API Route: GET /api/sessions/[sessionId] called')
  try {
    const sessionId = params.sessionId
    console.log('Next.js API Route: Fetching session:', sessionId)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/sessions/${encodeURIComponent(sessionId)}`
    console.log('Next.js API Route: Fetching from:', url)

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in get session API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch session',
      500
    )
  }
}

export async function PUT(
  request: NextRequest,
  { params }: { params: { sessionId: string } }
) {
  console.log('Next.js API Route: PUT /api/sessions/[sessionId] called')
  try {
    const sessionId = params.sessionId
    console.log('Next.js API Route: Updating session:', sessionId)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const sessionData = await request.json()
    console.log('Next.js API Route: Update data:', sessionData)

    const url = `${API_BASE_URL}/sessions/${encodeURIComponent(sessionId)}`
    console.log('Next.js API Route: Updating at:', url)

    const response = await fetchFromApi(url, {
      method: 'PUT',
      body: JSON.stringify(sessionData)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in update session API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to update session',
      500
    )
  }
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: { sessionId: string } }
) {
  console.log('Next.js API Route: DELETE /api/sessions/[sessionId] called')
  try {
    const sessionId = params.sessionId
    console.log('Next.js API Route: Deleting session:', sessionId)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/sessions/${encodeURIComponent(sessionId)}`
    console.log('Next.js API Route: Deleting at:', url)

    const response = await fetchFromApi(url, {
      method: 'DELETE'
    })

    if (response.status === 204) {
      return createApiResponse({ success: true })
    }

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in delete session API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to delete session',
      500
    )
  }
}
