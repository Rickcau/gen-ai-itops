import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function DELETE(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  console.log('Next.js API Route: DELETE /api/capabilities/[id] called')
  try {
    const id = params.id
    console.log('Next.js API Route: Deleting capability:', id)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const url = `${API_BASE_URL}/capabilities/${encodeURIComponent(id)}`
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
    console.error('Error in delete capability API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to delete capability',
      500
    )
  }
}

export async function PUT(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  console.log('Next.js API Route: PUT /api/capabilities/[id] called')
  try {
    const id = params.id
    console.log('Next.js API Route: Updating capability:', id)

    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const updatedCapability = await request.json()
    console.log('Next.js API Route: Update data:', updatedCapability)

    const url = `${API_BASE_URL}/capabilities/${encodeURIComponent(id)}`
    console.log('Next.js API Route: Updating at:', url)

    const response = await fetchFromApi(url, {
      method: 'PUT',
      body: JSON.stringify(updatedCapability)
    })

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in update capability API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to update capability',
      500
    )
  }
} 