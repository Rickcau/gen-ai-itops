'use client'

import { useState, useEffect } from 'react'
import { nanoid } from 'nanoid'
import { Eraser, Send, User } from 'lucide-react'
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { MessageBubble } from '@/components/message-bubble'
import { ActionButtons } from '@/components/action-buttons'
import { ThemeToggle } from '@/components/theme-switcher'
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { EndpointWarningDialog } from '@/components/endpoint-warning-dialog'
import type { Message, ChatState } from '@/types/chat'
import type { ChatApiRequest } from '@/types/api'
import { mockChatResponses, mockActionResponses } from '@/lib/mockData'
import { config } from '@/lib/config'

export default function ChatInterface() {
  const [chatState, setChatState] = useState<ChatState>({
    messages: [],
    isLoading: false
  })
  const [input, setInput] = useState('')
  const [mockMode, setMockMode] = useState(!config.apiConfigured)
  const [sessionId, setSessionId] = useState('')
  const [showEndpointWarning, setShowEndpointWarning] = useState(false)

  // Initialize session ID on component mount
  useEffect(() => {
    setSessionId(nanoid())
  }, [])

  const handleSend = async () => {
    if (!input.trim()) return

    const userMessage: Message = {
      id: nanoid(),
      content: input,
      role: 'user'
    }

    setChatState(prev => ({
      ...prev,
      messages: [...prev.messages, userMessage],
      isLoading: true
    }))
    setInput('')

    if (mockMode || !config.apiConfigured) {
      if (!mockMode && !config.apiConfigured) {
        setShowEndpointWarning(true)
      }
      await new Promise(resolve => setTimeout(resolve, 1000))
      const mockResponse = mockChatResponses[input.toLowerCase()] || mockChatResponses.default
      setChatState(prev => ({
        isLoading: false,
        messages: [...prev.messages, ...mockResponse]
      }))
    } else {
      try {
        const payload: ChatApiRequest = {
          sessionId,
          userId: config.testUser,
          prompt: input
        }

        const apiUrl = `${config.apiBaseUrl}/${config.endpoints.chat}`
        console.log('Calling API:', apiUrl)
        console.log('With payload:', payload)

        const response = await fetch(apiUrl, {
          method: 'POST',
          headers: { 
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'api-key': config.apiKey
          },
          body: JSON.stringify(payload)
        })

        console.log('Response status:', response.status)
        
        if (!response.ok) {
          const errorText = await response.text()
          console.error('API Error:', {
            status: response.status,
            statusText: response.statusText,
            body: errorText
          })
          throw new Error(`API error: ${response.status} - ${errorText}`)
        }

        const data = await response.json()
        console.log('API Response:', data)
        
        // Convert the API response format to our Message format
        const apiMessage: Message = {
          id: nanoid(),
          role: 'assistant',
          content: data.chatResponse
        }
        
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, apiMessage]
        }))
      } catch (error) {
        console.error('Error details:', error)
        setShowEndpointWarning(true)
        // Fallback to mock response on error
        const mockResponse = mockChatResponses[input.toLowerCase()] || mockChatResponses.default
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...mockResponse]
        }))
      }
    }
  }

  const handleAction = async (prompt: string) => {
    const userMessage: Message = {
      id: nanoid(),
      content: prompt,
      role: 'user'
    }

    setChatState(prev => ({
      ...prev,
      messages: [...prev.messages, userMessage],
      isLoading: true
    }))

    if (mockMode || !config.apiConfigured) {
      if (!mockMode && !config.apiConfigured) {
        setShowEndpointWarning(true)
      }
      await new Promise(resolve => setTimeout(resolve, 1000))
      const mockResponse = mockActionResponses[prompt] || mockActionResponses.default || []
      setChatState(prev => ({
        isLoading: false,
        messages: [...prev.messages, ...mockResponse]
      }))
    } else {
      try {
        const payload: ChatApiRequest = {
          sessionId,
          userId: config.testUser,
          prompt
        }

        const apiUrl = `${config.apiBaseUrl}/${config.endpoints.chat}`
        console.log('Calling API:', apiUrl)
        console.log('With payload:', payload)

        const response = await fetch(apiUrl, {
          method: 'POST',
          headers: { 
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'api-key': config.apiKey
          },
          body: JSON.stringify(payload)
        })

        console.log('Response status:', response.status)
        
        if (!response.ok) {
          const errorText = await response.text()
          console.error('API Error:', {
            status: response.status,
            statusText: response.statusText,
            body: errorText
          })
          throw new Error(`API error: ${response.status} - ${errorText}`)
        }

        const data = await response.json()
        console.log('API Response:', data)
        
        // Convert the API response format to our Message format
        const apiMessage: Message = {
          id: nanoid(),
          role: 'assistant',
          content: data.chatResponse
        }
        
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, apiMessage]
        }))
      } catch (error) {
        console.error('Error details:', error)
        setShowEndpointWarning(true)
        // Fallback to mock response on error
        const mockResponse = mockActionResponses.default || []
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...mockResponse]
        }))
      }
    }
  }

  const handleClear = () => {
    setChatState({ messages: [], isLoading: false })
    setSessionId(nanoid())
  }

  return (
    <>
      <div className="flex h-screen w-full overflow-hidden">
        <div className="container mx-auto max-w-4xl p-4 flex flex-col h-full">
          <Card className="flex-1 flex flex-col overflow-hidden">
            <CardHeader className="flex flex-col space-y-3 pb-4">
              <div className="flex justify-between items-center">
                <h1 className="text-2xl font-bold tracking-tight">IT Operations Assistant</h1>
                <div className="flex items-center gap-3">
                  <ThemeToggle />
                  <div className="h-4 w-px bg-border" />
                  <div className="flex items-center text-sm">
                    <User className="h-4 w-4 mr-1.5 text-muted-foreground" />
                    <span className="text-muted-foreground">{config.testUser}</span>
                  </div>
                </div>
              </div>
              <div className="flex items-center space-x-2">
                <Switch
                  id="mock-mode"
                  checked={mockMode}
                  onCheckedChange={setMockMode}
                />
                <Label htmlFor="mock-mode">Mock Mode</Label>
              </div>
            </CardHeader>
            <CardContent className="flex-1 flex flex-col min-h-0">
              <div className="flex-1 overflow-y-auto space-y-4 mb-4 px-2 mr-2">
                {chatState.messages.map((message) => (
                  <MessageBubble
                    key={message.id}
                    content={message.content}
                    role={message.role}
                  />
                ))}
                {chatState.isLoading && (
                  <div className="text-sm text-muted-foreground animate-pulse">
                    Processing...
                  </div>
                )}
              </div>
              
              <div className="mt-auto">
                <ActionButtons onAction={handleAction} />
                
                <div className="relative">
                  <Textarea
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder="Type your question here..."
                    className="pr-24 resize-none"
                    rows={3}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' && !e.shiftKey) {
                        e.preventDefault()
                        handleSend()
                      }
                    }}
                  />
                  <div className="absolute right-2 bottom-2 flex gap-2">
                    <Button
                      size="icon"
                      variant="ghost"
                      onClick={handleClear}
                      title="Clear chat"
                    >
                      <Eraser className="h-4 w-4" />
                    </Button>
                    <Button
                      size="icon"
                      onClick={handleSend}
                      disabled={!input.trim() || chatState.isLoading}
                    >
                      <Send className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
      <EndpointWarningDialog 
        open={showEndpointWarning} 
        onOpenChange={setShowEndpointWarning}
      />
    </>
  )
}

