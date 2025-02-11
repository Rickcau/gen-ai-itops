import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(
  request: NextRequest,
  { params }: { params: { sessionId: string } }
) {
  console.log('Next.js API Route: POST /api/sessions/[sessionId]/messages called')
  try {
    const sessionId = params.sessionId
    console.log('Next.js API Route: Adding message to session:', sessionId)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const messageData = await request.json()
    console.log('Next.js API Route: Message data:', messageData)

    const url = `${API_BASE_URL}/sessions/${encodeURIComponent(sessionId)}/messages`
    console.log('Next.js API Route: Posting to:', url)

    const response = await fetchFromApi(url, {
      method: 'POST',
      body: JSON.stringify(messageData)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in add message API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to add message',
      500
    )
  }
}

export async function GET(
  request: NextRequest,
  { params }: { params: { sessionId: string } }
) {
  console.log('Next.js API Route: GET /api/sessions/[sessionId]/messages called')
  try {
    const sessionId = params.sessionId
    console.log('Next.js API Route: Fetching messages for session:', sessionId)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/sessions/${encodeURIComponent(sessionId)}/messages`
    console.log('Next.js API Route: Fetching from:', url)

    const response = await fetchFromApi(url)
    if (!response.ok) {
      throw new Error(`API request failed: ${response.statusText}`)
    }
    
    const data = await response.json()
    console.log('Next.js API Route: Messages data received:', data)
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in get session messages API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch session messages',
      500
    )
  }
}
