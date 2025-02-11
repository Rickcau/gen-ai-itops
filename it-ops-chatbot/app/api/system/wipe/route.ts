import { createApiResponse, createErrorResponse, fetchFromApi } from '@/lib/api-utils'
import { NextRequest } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(request: NextRequest) {
  console.log('Next.js API Route: POST /api/system/wipe called')
  try {
    const apiKey = process.env.API_KEY
    if (!apiKey) {
      console.error('API key is missing')
      return createErrorResponse('API key is not configured', 500)
    }

    const { systemWipeKey } = await request.json()
    if (!systemWipeKey) {
      return createErrorResponse('System wipe key is required', 400)
    }

    console.log('Next.js API Route: Initiating system wipe')
    const url = `${API_BASE_URL}/system/wipe`

    const response = await fetchFromApi(url, {
      method: 'POST',
      body: JSON.stringify({ systemWipeKey })
    })

    if (response.status === 204) {
      return createApiResponse({ success: true })
    }

    const data = await response.json()
    return createApiResponse(data)
  } catch (error) {
    console.error('Error in system wipe API route:', error)
    return createErrorResponse(
      error instanceof Error ? error.message : 'Failed to perform system wipe',
      500
    )
  }
} 