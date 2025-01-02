import { NextResponse } from 'next/server'

const API_ENDPOINT = 'https://your-api-endpoint.com/action'

export async function POST(req: Request) {
  const { action } = await req.json()

  try {
    const response = await fetch(API_ENDPOINT, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        // Add any additional headers your API requires
      },
      body: JSON.stringify({ action }),
    })

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`)
    }

    const data = await response.json()
    return NextResponse.json(data)
  } catch (error) {
    console.error('Error:', error)
    return NextResponse.json({ error: 'An error occurred while processing your request' }, { status: 500 })
  }
}

