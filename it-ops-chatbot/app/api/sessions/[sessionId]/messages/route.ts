import { NextResponse } from 'next/server'

const API_BASE_URL = process.env.API_BASE_URL || 'https://localhost:7049'

export async function GET(
  request: Request,
  { params }: { params: { sessionId: string } }
) {
  try {
    const sessionId = params.sessionId

    if (!sessionId) {
      return NextResponse.json({ error: 'sessionId is required' }, { status: 400 })
    }

    // In development, configure Node to accept self-signed certificates
    if (process.env.NODE_ENV === 'development') {
      process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0'
    }

    const apiUrl = `${API_BASE_URL}/sessions/${sessionId}/messages`
    console.log('Fetching messages from:', apiUrl)

    const response = await fetch(apiUrl, {
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        'api-key': process.env.API_KEY || ''
      }
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

    const data = await response.json()
    console.log('Received messages from backend:', {
      count: Array.isArray(data) ? data.length : 0,
      messages: data
    })

    // Ensure we're returning the full array of messages
    return NextResponse.json(data)
  } catch (error) {
    console.error('Error in messages API route:', {
      name: error instanceof Error ? error.name : 'Unknown',
      message: error instanceof Error ? error.message : String(error),
      stack: error instanceof Error ? error.stack : undefined
    })
    return NextResponse.json(
      { error: 'Internal server error' },
      { status: 500 }
    )
  }
}
