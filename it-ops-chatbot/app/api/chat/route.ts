import { NextResponse } from 'next/server'
import type { ChatApiRequest, ChatApiResponse } from '@/types/api'

export async function POST(request: Request) {
  try {
    console.log('Received request at Next.js API route')
    
    const body: ChatApiRequest = await request.json()
    console.log('Parsed request body:', JSON.stringify(body, null, 2))

    const apiUrl = `${process.env.API_BASE_URL}/chat`
    console.log('Server-side API call to:', apiUrl)

    // In development, configure Node to accept self-signed certificates
    if (process.env.NODE_ENV === 'development') {
      process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
    }

    // Make the API call with the required headers
    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'api-key': process.env.API_KEY || ''
      },
      body: JSON.stringify(body)
    })

    console.log('Backend response status:', response.status)
    console.log('Request headers:', {
      'Content-Type': 'application/json',
      'Accept': 'application/json',
      'api-key': '[REDACTED]'  // Don't log the actual API key
    })

    if (!response.ok) {
      const errorText = await response.text()
      console.error('Error from backend:', errorText)
      return NextResponse.json(
        { error: `Backend API request failed: ${errorText}` },
        { status: response.status }
      )
    }

    const data: ChatApiResponse = await response.json()
    return NextResponse.json(data)
  } catch (error) {
    console.error('Server-side API error details:', {
      name: error instanceof Error ? error.name : 'Unknown',
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined
    })

    return NextResponse.json(
      { 
        error: 'Internal server error',
        details: error instanceof Error ? error.message : String(error)
      },
      { status: 500 }
    )
  }
} 