import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET() {
  console.log('Next.js API Route: GET /api/users called')
  try {
    const url = `${API_BASE_URL}/users`
    console.log('Next.js API Route: Fetching users from:', url)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const response = await fetchFromApi(url)
    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in users API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to fetch users',
      500
    )
  }
}

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/users called')
  try {
    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const userData = await request.json()
    console.log('Next.js API Route: Creating user:', userData)

    const url = `${API_BASE_URL}/users`
    console.log('Next.js API Route: Creating user at:', url)

    const response = await fetchFromApi(url, {
      method: 'POST',
      body: JSON.stringify(userData)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in create user API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to create user',
      500
    )
  }
} 