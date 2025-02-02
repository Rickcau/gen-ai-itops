'use client'

import { useState } from 'react'
import { nanoid } from 'nanoid'
import { Eraser, Send } from 'lucide-react'
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { MessageBubble } from '@/components/message-bubble'
import { ActionButtons } from '@/components/action-buttons'
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { EndpointWarningDialog } from '@/components/endpoint-warning-dialog'
import { UserHeader } from '@/components/user-header'
import type { Message, ChatState, MessageRole } from '@/types/chat'
import type { ChatApiRequest, ChatApiResponse } from '@/types/api'
import { mockChatResponses, mockActionResponses } from '@/lib/mockData'
import { config } from '@/lib/config'
import { ChatSidebar } from '@/components/chat-sidebar'
import { useAuth } from '@/components/providers/auth-provider'

export default function ChatInterface() {
  const { authState } = useAuth();
  const [chatState, setChatState] = useState<ChatState>({
    messages: [],
    isLoading: false
  })
  const [input, setInput] = useState('')
  const [mockMode, setMockMode] = useState(!config.apiConfigured)
  const [sessionId, setSessionId] = useState(nanoid())
  const [showEndpointWarning, setShowEndpointWarning] = useState(false)

  const handleAction = async (prompt: string) => {
    if (!authState.user) return

    const userMessage: Message = {
      id: nanoid(),
      content: prompt,
      role: 'user' as MessageRole
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
      const mockResponse = mockActionResponses[prompt] || mockActionResponses.default
      setChatState(prev => ({
        isLoading: false,
        messages: [...prev.messages, ...mockResponse]
      }))
    } else {
      try {
        const payload: ChatApiRequest = {
          sessionId,
          userId: authState.user.email,
          prompt
        }

        const response = await fetch(`${config.apiBaseUrl}/${config.endpoints.chat}`, {
          method: 'POST',
          credentials: 'include',
          headers: { 
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
          body: JSON.stringify(payload)
        })

        if (!response.ok) {
          throw new Error(`API error: ${response.status}`)
        }

        const data: ChatApiResponse = await response.json()
        setChatState(prev => ({
          isLoading: false,
          messages: [
            ...prev.messages,
            ...(data.assistantResponse ? [{
              id: nanoid(),
              role: 'assistant' as MessageRole,
              content: data.assistantResponse
            }] : []),
            ...(data.weatherResponse ? [{
              id: nanoid(),
              role: 'weather' as MessageRole,
              content: data.weatherResponse
            }] : []),
            ...(data.specialistResponse ? [{
              id: nanoid(),
              role: 'specialist' as MessageRole,
              content: data.specialistResponse
            }] : [])
          ]
        }))
      } catch (error) {
        console.error('Error:', error)
        setShowEndpointWarning(true)
        const mockResponse = mockActionResponses[prompt] || mockActionResponses.default
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...mockResponse]
        }))
      }
    }
  }

  const handleSend = async () => {
    if (!input.trim() || !authState.user) return
    await handleAction(input)
    setInput('')
  }

  const handleClear = () => {
    setChatState({ messages: [], isLoading: false })
    setSessionId(nanoid())
  }

  const handleNewChat = () => {
    handleClear()
    return nanoid()
  }

  const handleSelectChat = (id: string) => {
    console.log('Selected chat:', id)
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      <ChatSidebar 
        onNewChat={handleNewChat}
        onSelectChat={handleSelectChat}
      />
      <div className="container mx-auto max-w-4xl p-4 flex flex-col h-full">
        <Card className="flex-1 flex flex-col overflow-hidden">
          <CardHeader className="flex flex-row justify-between items-center pb-4">
            <div className="space-y-2">
              <h1 className="text-2xl font-bold tracking-tight">Multi-Agent IT Operations Platform</h1>
              <div className="flex items-center gap-2">
                <Switch
                  id="mock-mode"
                  checked={mockMode}
                  onCheckedChange={setMockMode}
                />
                <Label htmlFor="mock-mode">Mock Mode</Label>
              </div>
            </div>
            <UserHeader />
          </CardHeader>

          <CardContent className="flex-1 flex flex-col min-h-0">
            <ActionButtons onAction={handleAction} />
            
            <div className="flex-1 overflow-y-auto space-y-4 mb-4 px-2 mr-2 mt-4">
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
          </CardContent>
        </Card>

        <EndpointWarningDialog
          open={showEndpointWarning}
          onOpenChange={setShowEndpointWarning}
        />
      </div>
    </div>
  )
}