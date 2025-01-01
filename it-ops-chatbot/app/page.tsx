'use client'

import { useState } from 'react'
import { nanoid } from 'nanoid'
import { Eraser, Send } from 'lucide-react'
import { Button } from "@/components/ui/button"
import { Textarea } from "@/components/ui/textarea"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { MessageBubble } from '@/components/message-bubble'
import { ActionButtons } from '@/components/action-buttons'
import { ThemeToggle } from '@/components/theme-switcher'
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import type { Message, ChatState } from '@/types/chat'
import { mockChatResponses, mockActionResponses } from '@/lib/mockData'

export default function ChatInterface() {
  const [chatState, setChatState] = useState<ChatState>({
    messages: [],
    isLoading: false
  })
  const [input, setInput] = useState('')
  const [mockMode, setMockMode] = useState(true)

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

    if (mockMode) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      const mockResponse = mockChatResponses[input.toLowerCase()] || mockChatResponses.default
      setChatState(prev => ({
        isLoading: false,
        messages: [...prev.messages, ...mockResponse]
      }))
    } else {
      try {
        const response = await fetch('/api/chat', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ message: input })
        })

        const data = await response.json()
        
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...data.messages]
        }))
      } catch (error) {
        console.error('Error:', error)
        setChatState(prev => ({ ...prev, isLoading: false }))
      }
    }
  }

  const handleAction = async (action: string) => {
    const userMessage: Message = {
      id: nanoid(),
      content: `Can you ${action} VMs?`,
      role: 'user'
    }

    setChatState(prev => ({
      ...prev,
      messages: [...prev.messages, userMessage],
      isLoading: true
    }))

    if (mockMode) {
      await new Promise(resolve => setTimeout(resolve, 1000))
      const mockResponse = mockActionResponses[action] || []
      setChatState(prev => ({
        isLoading: false,
        messages: [...prev.messages, ...mockResponse]
      }))
    } else {
      try {
        const response = await fetch('/api/action', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ action })
        })

        const data = await response.json()
        
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...data.messages]
        }))
      } catch (error) {
        console.error('Error:', error)
        setChatState(prev => ({ ...prev, isLoading: false }))
      }
    }
  }

  const handleClear = () => {
    setChatState({ messages: [], isLoading: false })
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      <div className="container mx-auto max-w-4xl p-4 flex flex-col h-full">
        <Card className="flex-1 flex flex-col overflow-hidden">
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
            <h1 className="text-2xl font-bold tracking-tight">IT Operations Assistant</h1>
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <Switch
                  id="mock-mode"
                  checked={mockMode}
                  onCheckedChange={setMockMode}
                />
                <Label htmlFor="mock-mode">Mock Mode</Label>
              </div>
              <ThemeToggle />
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
  )
}

