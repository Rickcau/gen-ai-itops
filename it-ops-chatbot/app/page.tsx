'use client'

import { useState, useEffect, useCallback } from 'react'
import { nanoid } from 'nanoid'
import { Eraser, Send, Trash2 } from 'lucide-react'
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader } from '@/components/ui/card'
import { Textarea } from '@/components/ui/textarea'
import { MessageBubble } from '@/components/message-bubble'
import { ActionButtons } from '@/components/action-buttons'
import { mockActionResponses } from '@/lib/mockData'
import { ChatSidebar } from '@/components/chat-sidebar'
import { useAuth } from '@/components/providers/auth-provider'
import { useChatSessions } from '@/src/hooks/useChatSessions'
import { ClearChatDialog } from '@/components/clear-chat-dialog'
import { EndpointWarningDialog } from '@/components/endpoint-warning-dialog'
import { UserHeader } from '@/components/user-header'
import type { ChatState } from '@/types/chat'
import type { ChatApiRequest, ChatApiResponse } from '@/types/api'
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { RunbookSidebar } from '@/components/runbook-sidebar'

type MessageRole = 'user' | 'assistant' | 'specialist' | 'weather'

interface Message {
  id: string
  content: string
  role: MessageRole
}

export default function ChatInterface() {
  const { authState } = useAuth();
  const { 
    addSession
  } = useChatSessions(authState.user?.email || '')
  const [chatState, setChatState] = useState<ChatState>({
    messages: [],
    isLoading: false
  })
  const [input, setInput] = useState('')
  const [mockMode, setMockMode] = useState(false)
  const [sessionId, setSessionId] = useState('')
  const [currentChatName, setCurrentChatName] = useState('')
  const [showClearDialog, setShowClearDialog] = useState(false)
  const [showEndpointWarning, setShowEndpointWarning] = useState(false)
  const [runbookExecutions, setRunbookExecutions] = useState<Array<{
    jobId: string
    runbookName: string
    status: 'running' | 'completed' | 'failed'
    timestamp: string
  }>>([])

  // Load messages for the current session
  const loadMessages = useCallback(async (sid: string) => {
    if (!sid || !authState.user) return;
    
    try {
      const response = await fetch(`/api/sessions/${sid}/messages`)
      if (!response.ok) {
        throw new Error(`Failed to fetch messages: ${response.statusText}`)
      }
      
      const messages = await response.json()
      setChatState(prev => ({
        ...prev,
        messages: messages.map((msg: { id: string; prompt: string; sender: string }) => ({
          id: msg.id,
          content: msg.prompt,
          role: msg.sender.toLowerCase() as MessageRole
        }))
      }))
    } catch (error) {
      console.error('Error loading messages:', error)
    }
  }, [authState.user]) // Only depends on authState.user

  // Load messages when sessionId changes
  useEffect(() => {
    if (sessionId) {
      loadMessages(sessionId)
    }
  }, [sessionId, loadMessages])

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

    try {
      // Use existing session or create a new one if none exists
      const currentSessionId = sessionId || nanoid()
      const currentChatTitle = currentChatName || `Chat ${new Date().toLocaleString()}`
      
      // Only set new session if we don't have one
      if (!sessionId) {
        setSessionId(currentSessionId)
        setCurrentChatName(currentChatTitle)
      }

      const payload: ChatApiRequest = {
        sessionId: currentSessionId,
        userId: authState.user.email,
        prompt,
        chatName: currentChatTitle
      }

      console.log('Sending request to API:', {
        url: '/api/chat',
        payload
      })

      const response = await fetch('/api/chat', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(payload)
      })

      if (!response.ok) {
        const errorText = await response.text()
        console.error('API Response Error:', {
          status: response.status,
          statusText: response.statusText,
          body: errorText
        })
        throw new Error(`API error: ${response.status}`)
      }

      const data: ChatApiResponse = await response.json()
      console.log('API Response:', data)

      // Create messages for each response type
      const newMessages: Message[] = []

      if (data.assistantResponse) {
        newMessages.push({
          id: nanoid(),
          content: data.assistantResponse,
          role: 'assistant'
        })
      }

      if (data.specialistResponse) {
        console.log('Specialist Response:', data.specialistResponse);
        
        // Try all possible patterns
        const patterns = [
          // Pattern 1: List VMs format
          /The runbook "(.*?)" has been started\. You can track its progress with Job ID: `(.*?)`/,
          // Pattern 2: Successfully executed format
          /runbook "(.*?)" has been successfully executed.*Job ID: `(.*?)`/,
          // Pattern 3: Shutdown VM format
          /The runbook "(.*?)" has been started with parameters.*Job ID: `(.*?)`/,
          // Pattern 4: Generic format (fallback)
          /runbook "([^"]+)".*Job ID: `([^`]+)`/
        ];

        let matched = false;
        for (const pattern of patterns) {
          const match = data.specialistResponse.match(pattern);
          console.log(`Trying pattern ${pattern}:`, match);
          
          if (match) {
            const [, runbookName, jobId] = match;
            console.log('Extracted Runbook Info:', { runbookName, jobId });
            handleRunbookExecution(runbookName, jobId);
            matched = true;
            break;
          }
        }

        if (!matched) {
          console.log('No patterns matched the specialist response');
        }

        newMessages.push({
          id: nanoid(),
          content: data.specialistResponse,
          role: 'specialist'
        });
      }

      if (data.weatherResponse) {
        newMessages.push({
          id: nanoid(),
          content: data.weatherResponse,
          role: 'weather'
        })
      }

      // Update chat state with all new messages
      setChatState(prev => ({
        ...prev,
        messages: [...prev.messages, ...newMessages],
        isLoading: false
      }))

      // Add this to your existing handleAction function where runbooks are executed
      if (data.runbookName && data.jobId) {
        handleRunbookExecution(data.runbookName, data.jobId)
      }
    } catch (error) {
      console.error('Error:', error)
      if (mockMode) {
        const mockResponse = mockActionResponses[prompt] || mockActionResponses.default
        setChatState(prev => ({
          isLoading: false,
          messages: [...prev.messages, ...mockResponse]
        }))
      } else {
        setShowEndpointWarning(true)
        setChatState(prev => ({
          ...prev,
          isLoading: false
        }))
      }
    }
  }

  const handleRunbookExecution = (runbookName: string, jobId: string) => {
    setRunbookExecutions(prev => [
      ...prev,
      {
        jobId,
        runbookName,
        status: 'running',
        timestamp: new Date().toISOString()
      }
    ])
  }

  const handleSend = async () => {
    if (!input.trim() || !authState.user) return
    const currentInput = input
    setInput('') // Clear input immediately
    await handleAction(currentInput)
  }

  const handleClear = () => {
    setShowClearDialog(true)
  }

  const confirmClear = () => {
    setChatState({
      messages: [],
      isLoading: false
    })
    setInput('')
  }

  const handleNewChat = () => {
    // Clear existing chat state
    setChatState({
      messages: [], // Clear messages
      isLoading: false
    })
    setInput('') // Clear input
    
    // Clear runbook executions for the new session
    setRunbookExecutions([])
    
    // Create new session
    const newSessionId = nanoid()
    const newChatName = `Chat ${new Date().toLocaleString()}`
    
    // Update local state
    setSessionId(newSessionId)
    setCurrentChatName(newChatName)

    // Add to sessions list
    addSession({
      sessionId: newSessionId,
      chatName: newChatName,
      createdAt: new Date().toISOString()
    })
  }

  const handleSelectChat = async (sid: string, chatName: string) => {
    console.log('Selecting chat session:', sid)
    // Clear current messages before loading new ones
    setChatState({
      messages: [],
      isLoading: true
    })
    setInput('')
    
    setSessionId(sid)
    setCurrentChatName(chatName)

    try {
      // Fetch messages for this session
      console.log('Fetching messages for session:', sid)
      const response = await fetch(`/api/sessions/${sid}/messages`)
      
      if (response.ok) {
        const data = await response.json()
        console.log('Raw response data:', data)
        
        if (!Array.isArray(data)) {
          console.error('Expected array of messages but got:', typeof data)
          throw new Error('Invalid response format')
        }
        
        // Map the API response messages to our Message type
        const messages = data.map((msg: { id: string; prompt: string; sender: string }) => {
          console.log('Processing message:', {
            id: msg.id,
            content: msg.prompt || '',
            role: msg.sender?.toLowerCase() || 'user'
          })
          return {
            id: msg.id,
            content: msg.prompt || '',
            role: (msg.sender || 'user').toLowerCase() as MessageRole
          }
        })
        
        console.log(`Processed ${messages.length} messages:`, messages)
        
        setChatState(prevState => {
          console.log('Previous state:', prevState)
          const newState = {
            ...prevState,
            messages,
            isLoading: false
          }
          console.log('New state:', newState)
          return newState
        })
      } else {
        console.error('Failed to fetch messages:', {
          status: response.status,
          statusText: response.statusText
        })
        setChatState({
          messages: [],
          isLoading: false
        })
      }
    } catch (error) {
      console.error('Error fetching messages:', error)
      setChatState({
        messages: [],
        isLoading: false
      })
    }
  }

  const handleRunbookExecutionClick = (runbookName: string, jobId: string) => {
    const statusCheckMessage = `Check the status of runbook "${runbookName}" with Job ID: ${jobId}`
    handleAction(statusCheckMessage)
  }

  return (
    <div className="flex h-screen w-full overflow-hidden">
      {authState.user && (
        <ChatSidebar
          onNewChat={handleNewChat}
          onSelectChat={handleSelectChat}
          userId={authState.user.email || ''}
        />
      )}
      <div className="container mx-auto max-w-5xl p-4 flex flex-col h-full">
        <Card className="flex-1 flex flex-col overflow-hidden">
          <CardHeader className="flex flex-row justify-between items-center pb-4">
            <div className="flex items-center gap-4">
              <h1 className="text-2xl font-bold tracking-tight">Multi-Agent IT Operations Platform</h1>
              <div className="flex items-center gap-2 ml-4">
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
            <div className="flex flex-col flex-1 h-full">
              {/* Header with session info */}
              {sessionId && (
                <div className="flex items-center justify-between px-6 py-3 mb-6 border-b border-border bg-muted/50">
                <div className="flex items-center space-x-3">
                  <span className="text-sm font-medium">{currentChatName}</span>
                  <span className="text-xs text-muted-foreground px-2 py-1 rounded-md bg-muted">Session: {sessionId.slice(0, 8)}</span>
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  onClick={handleClear}
                  className="text-muted-foreground hover:text-destructive"
                >
                  <Trash2 className="h-4 w-4" />
                </Button>
              </div>
              )}

              {/* Chat messages */}
              <div className="flex-1 overflow-y-auto space-y-6 px-6">
                <div className="space-y-6">
                  {chatState.messages.length > 0 ? (
                    chatState.messages.map((message) => (
                      <MessageBubble
                        key={message.id}
                        content={message.content}
                        role={message.role}
                      />
                    ))
                  ) : (
                    <div className="text-center text-muted-foreground py-4">
                      No messages in this chat yet
                    </div>
                  )}
                </div>
                {chatState.isLoading && (
                  <div className="text-sm text-muted-foreground animate-pulse">
                    Processing...
                  </div>
                )}
              </div>

              <div className="mt-auto space-y-4 px-6 py-4">
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
            </div>
          </CardContent>
        </Card>

        <ClearChatDialog
          open={showClearDialog}
          onOpenChange={setShowClearDialog}
          onConfirm={confirmClear}
        />

        <EndpointWarningDialog
          open={showEndpointWarning}
          onOpenChange={setShowEndpointWarning}
        />
      </div>
      {sessionId && (
        <RunbookSidebar
          sessionId={sessionId}
          executions={runbookExecutions}
          onExecutionClick={handleRunbookExecutionClick}
        />
      )}
    </div>
  )
}