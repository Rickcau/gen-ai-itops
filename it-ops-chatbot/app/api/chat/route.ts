import { NextResponse } from 'next/server'
import type { ChatApiRequest, ChatApiResponse } from '@/types/api'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function POST(request: Request) {
  try {
    const payload: ChatApiRequest = await request.json()
    console.log('Received request at Next.js API route:', payload)
    
    // In development, configure Node to accept self-signed certificates
    if (process.env.NODE_ENV === 'development') {
      process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
    }

    const apiUrl = `${API_BASE_URL}/chat`
    console.log('Making backend request to:', apiUrl)

    const response = await fetch(apiUrl, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'api-key': process.env.API_KEY || ''
      },
      body: JSON.stringify(payload)
    })

    console.log('Backend response status:', response.status)

    if (!response.ok) {
      const errorText = await response.text()
      console.error('Error from backend:', {
        status: response.status,
        statusText: response.statusText,
        body: errorText
      })
      return NextResponse.json(
        { error: errorText || 'Backend API request failed' },
        { status: response.status }
      )
    }

    const data: ChatApiResponse = await response.json()
    console.log('Backend response data:', data)
    return NextResponse.json(data)
  } catch (error) {
    console.error('Error in chat API route:', {
      name: error instanceof Error ? error.name : 'Unknown',
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined
    })
    return NextResponse.json(
      { error: 'Failed to process request' },
      { status: 500 }
    )
  }
} 