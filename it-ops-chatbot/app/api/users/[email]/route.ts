import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET(
  request: NextRequest,
  { params }: { params: { email: string } }
) {
  console.log('Next.js API Route: GET /api/users/[email] called')
  try {
    const email = params.email
    console.log('Next.js API Route: Fetching user:', email)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/users/${encodeURIComponent(email)}`
    console.log('Next.js API Route: Fetching from:', url)

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in get user API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch user',
      500
    )
  }
}

export async function PUT(
  request: NextRequest,
  { params }: { params: { email: string } }
) {
  console.log('Next.js API Route: PUT /api/users/[email] called')
  try {
    const email = params.email
    console.log('Next.js API Route: Updating user:', email)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const userData = await request.json()
    console.log('Next.js API Route: Update data:', userData)

    const url = `${API_BASE_URL}/users/${encodeURIComponent(email)}`
    console.log('Next.js API Route: Updating at:', url)

    const response = await fetchFromApi(url, {
      method: 'PUT',
      body: JSON.stringify(userData)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in update user API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to update user',
      500
    )
  }
} 