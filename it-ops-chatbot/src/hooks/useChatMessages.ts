import { useState, useCallback } from 'react'

export interface ChatMessage {
  id: string
  type: string
  sessionId: string
  timeStamp: string
  prompt: string
  sender: 'user' | 'Assistant' | 'Specialist' | 'Weather'
  promptTokens: number
  completion: string | null
  completionTokens: number
}

export function useChatMessages() {
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const fetchMessages = useCallback(async (sessionId: string) => {
    console.log('Fetching messages for session:', sessionId)
    setIsLoading(true)
    setError(null)

    try {
      const response = await fetch(`/api/sessions/${sessionId}/messages`)
      if (!response.ok) {
        throw new Error(`Failed to fetch messages: ${response.statusText}`)
      }

      const data = await response.json()
      console.log('Received messages:', data)
      setMessages(data)
    } catch (err) {
      console.error('Error fetching messages:', err)
      setError('Failed to load messages')
    } finally {
      setIsLoading(false)
    }
  }, [])

  const clearMessages = useCallback(() => {
    setMessages([])
    setError(null)
  }, [])

  return {
    messages,
    isLoading,
    error,
    fetchMessages,
    clearMessages
  }
}
