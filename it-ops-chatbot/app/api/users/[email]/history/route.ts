import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function DELETE(
  request: NextRequest,
  { params }: { params: { email: string } }
) {
  console.log('Next.js API Route: DELETE /api/users/[email]/history called')
  try {
    const email = params.email
    console.log('Next.js API Route: Deleting user history:', email)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/users/${encodeURIComponent(email)}/history`
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
    console.error('Error in delete user history API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to delete user history',
      500
    )
  }
} 