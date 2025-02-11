import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"

interface ViewSessionDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  session: {
    id: string
    sessionId: string
    userId?: string
    name?: string
    timestamp: string
    tokens?: number
    messages?: Array<{
      id: string
      timeStamp: string
      prompt: string
      sender: string
      promptTokens: number
      completion: string
      completionTokens: number
    }>
  } | null
  onUpdate?: (sessionId: string, updates: any) => Promise<void>
  onAddMessage?: (sessionId: string, message: any) => Promise<void>
  onLoadMessages?: (sessionId: string) => Promise<void>
}

export function ViewSessionDialog({
  open,
  onOpenChange,
  session,
  onUpdate,
  onAddMessage,
  onLoadMessages
}: ViewSessionDialogProps) {
  const [activeTab, setActiveTab] = useState("details")
  const [isUpdating, setIsUpdating] = useState(false)
  const [editedName, setEditedName] = useState(session?.name || "")
  const [newMessage, setNewMessage] = useState("")
  const [isAddingMessage, setIsAddingMessage] = useState(false)
  const [isLoadingMessages, setIsLoadingMessages] = useState(false)
  const [hasLoadedMessages, setHasLoadedMessages] = useState(false)

  // Update editedName when session changes
  useEffect(() => {
    if (session) {
      setEditedName(session.name || "")
      setHasLoadedMessages(false) // Reset the flag when session changes
    }
  }, [session])

  // Load messages when switching to messages tab
  useEffect(() => {
    const loadMessages = async () => {
      if (activeTab === "messages" && session && onLoadMessages && !hasLoadedMessages && !session.messages) {
        setIsLoadingMessages(true)
        try {
          await onLoadMessages(session.sessionId)
          setHasLoadedMessages(true)
        } catch (error) {
          console.error('Error loading messages:', error)
        } finally {
          setIsLoadingMessages(false)
        }
      }
    }
    loadMessages()
  }, [activeTab, session, onLoadMessages, hasLoadedMessages])

  if (!session) return null

  const handleUpdate = async () => {
    if (!onUpdate) return
    setIsUpdating(true)
    try {
      await onUpdate(session.sessionId, { name: editedName })
      // Success toast handled by parent
    } catch (error) {
      console.error('Error updating session:', error)
    } finally {
      setIsUpdating(false)
    }
  }

  const handleAddMessage = async () => {
    if (!onAddMessage || !newMessage.trim()) return
    setIsAddingMessage(true)
    try {
      await onAddMessage(session.sessionId, {
        prompt: newMessage,
        sender: "user"
      })
      setNewMessage("")
      // Success toast handled by parent
    } catch (error) {
      console.error('Error adding message:', error)
    } finally {
      setIsAddingMessage(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Session Details</DialogTitle>
        </DialogHeader>

        <Tabs
          defaultValue="details"
          value={activeTab}
          onValueChange={setActiveTab}
          className="flex-1 flex flex-col"
        >
          <TabsList>
            <TabsTrigger value="details">Details</TabsTrigger>
            <TabsTrigger value="messages">
              Messages
              {session.messages && (
                <span className="ml-2 text-xs bg-primary/10 px-2 py-0.5 rounded-full">
                  {session.messages.length}
                </span>
              )}
            </TabsTrigger>
          </TabsList>

          <TabsContent 
            value="details" 
            className="flex-1 mt-4"
          >
            <div className="grid gap-4">
              <div className="grid gap-2">
                <Label>Session ID</Label>
                <div className="p-2 rounded-md bg-muted/50 font-mono text-sm">
                  {session.sessionId}
                </div>
              </div>

              <div className="grid gap-2">
                <Label>User ID</Label>
                <div className="p-2 rounded-md bg-muted/50 font-mono text-sm">
                  {session.userId || 'N/A'}
                </div>
              </div>

              <div className="grid gap-2">
                <Label htmlFor="name">Name</Label>
                <div className="flex gap-2">
                  <Input
                    id="name"
                    value={editedName}
                    onChange={(e) => setEditedName(e.target.value)}
                    placeholder="Session name"
                  />
                  <Button 
                    onClick={handleUpdate}
                    disabled={isUpdating || editedName === session.name}
                  >
                    {isUpdating ? (
                      <>
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Updating...
                      </>
                    ) : (
                      'Update'
                    )}
                  </Button>
                </div>
              </div>

              <div className="grid gap-2">
                <Label>Created</Label>
                <div className="p-2 rounded-md bg-muted/50">
                  {new Date(session.timestamp).toLocaleString()}
                </div>
              </div>

              <div className="grid gap-2">
                <Label>Token Count</Label>
                <div className="p-2 rounded-md bg-muted/50">
                  {session.tokens || 0}
                </div>
              </div>
            </div>
          </TabsContent>

          <TabsContent 
            value="messages" 
            className="flex-1 mt-4"
          >
            <div className="flex flex-col h-[calc(65vh-180px)]">
              <div className="flex-1 overflow-y-auto">
                <div className="space-y-4 px-4">
                  {isLoadingMessages ? (
                    <div className="flex items-center justify-center py-8">
                      <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                    </div>
                  ) : session.messages && session.messages.length > 0 ? (
                    session.messages.map((message) => (
                      <div 
                        key={message.id}
                        className={`p-4 rounded-lg ${
                          message.sender === 'user' 
                            ? 'bg-primary/10 ml-auto max-w-[80%]' 
                            : 'bg-muted mr-auto max-w-[80%]'
                        }`}
                      >
                        <div className="flex items-center gap-2 mb-1">
                          <span className="text-sm font-medium">
                            {message.sender}
                          </span>
                          <span className="text-xs text-muted-foreground">
                            {new Date(message.timeStamp).toLocaleString()}
                          </span>
                        </div>
                        <div className="text-sm whitespace-pre-wrap">
                          {message.prompt}
                        </div>
                        {message.completion && (
                          <div className="mt-2 text-sm text-muted-foreground whitespace-pre-wrap">
                            {message.completion}
                          </div>
                        )}
                        <div className="mt-1 text-xs text-muted-foreground">
                          Tokens: {message.promptTokens + (message.completionTokens || 0)}
                        </div>
                      </div>
                    ))
                  ) : (
                    <div className="text-center text-muted-foreground py-8">
                      No messages in this session
                    </div>
                  )}
                </div>
              </div>
            </div>
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  )
} 