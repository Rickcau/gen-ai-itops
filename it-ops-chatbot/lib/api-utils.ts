import { NextResponse } from 'next/server'

const NO_CACHE_HEADERS = {
  'Cache-Control': 'no-store, no-cache, must-revalidate, proxy-revalidate',
  'Pragma': 'no-cache',
  'Expires': '0'
} as const

export const NO_STORE_OPTIONS = {
  cache: 'no-store' as RequestCache,
  headers: {
    'Cache-Control': 'no-cache',
    'Pragma': 'no-cache'
  }
} as const

/**
 * Creates a NextResponse with consistent no-cache headers
 */
export function createApiResponse<T>(
  data: T,
  options: { status?: number; headers?: HeadersInit } = {}
) {
  return NextResponse.json(data, {
    status: options.status || 200,
    headers: {
      ...NO_CACHE_HEADERS,
      ...(options.headers || {})
    }
  })
}

/**
 * Creates an error response with consistent no-cache headers
 */
export function createErrorResponse(
  error: string,
  status: number = 500,
  additionalHeaders: HeadersInit = {}
) {
  return NextResponse.json(
    { error },
    {
      status,
      headers: {
        ...NO_CACHE_HEADERS,
        ...additionalHeaders
      }
    }
  )
}

/**
 * Fetches data from the backend API with no-cache settings
 */
export async function fetchFromApi(
  url: string,
  options: RequestInit = {}
) {
  const apiKey = process.env.API_KEY
  if (!apiKey) {
    throw new Error('API key is not configured')
  }

  // In development, configure Node to accept self-signed certificates
  if (process.env.NODE_ENV === 'development') {
    process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
  }

  const response = await fetch(url, {
    ...options,
    cache: 'no-store',
    headers: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      'api-key': apiKey,
      'Cache-Control': 'no-cache',
      ...(options.headers || {})
    }
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(errorText || `API request failed with status ${response.status}`)
  }

  return response
} 