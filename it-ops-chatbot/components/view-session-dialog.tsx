import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState, useEffect } from "react"
import { Loader2 } from "lucide-react"
import type { Session, SessionUpdate } from "@/types/session"

interface ViewSessionDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  session: Session | null
  onUpdate: (sessionId: string, updates: SessionUpdate) => Promise<void>
  onLoadMessages: (sessionId: string) => Promise<void>
}

export function ViewSessionDialog({
  open,
  onOpenChange,
  session,
  onUpdate,
  onLoadMessages
}: ViewSessionDialogProps) {
  const [activeTab, setActiveTab] = useState("details")
  const [isEditMode, setIsEditMode] = useState(false)
  const [isUpdating, setIsUpdating] = useState(false)
  const [editedName, setEditedName] = useState(session?.name || "")
  const [isLoadingMessages, setIsLoadingMessages] = useState(false)
  const [hasLoadedMessages, setHasLoadedMessages] = useState(false)

  useEffect(() => {
    if (session) {
      setEditedName(session.name || "")
      setHasLoadedMessages(false)
      setIsEditMode(false)
    }
  }, [session])

  useEffect(() => {
    const loadMessages = async () => {
      if (activeTab === "messages" && session && !hasLoadedMessages && !session.messages) {
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
      setIsEditMode(false)
    } catch (error) {
      console.error('Error updating session:', error)
    } finally {
      setIsUpdating(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl">
        <DialogHeader>
          <DialogTitle>View Session</DialogTitle>
        </DialogHeader>

        <Tabs defaultValue="details" value={activeTab} onValueChange={setActiveTab}>
          <TabsList className="grid w-full grid-cols-2">
            <TabsTrigger value="details">Details</TabsTrigger>
            <TabsTrigger value="messages">Messages</TabsTrigger>
          </TabsList>

          <TabsContent value="details">
            <div className="space-y-4">
              <div className="grid gap-4">
                <div className="space-y-2">
                  <Label htmlFor="userId">User ID</Label>
                  <div id="userId" className="p-2 bg-muted/50 rounded-md">
                    {session.userId || 'Anonymous'}
                  </div>
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor={isEditMode ? "sessionName" : "sessionNameDisplay"}>Name</Label>
                  {isEditMode ? (
                    <Input
                      id="sessionName"
                      value={editedName}
                      onChange={(e) => setEditedName(e.target.value)}
                      placeholder="Enter session name"
                    />
                  ) : (
                    <div id="sessionNameDisplay" className="p-2 bg-muted/50 rounded-md">
                      {session.name || 'Chat'}
                    </div>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="created">Created</Label>
                  <div id="created" className="p-2 bg-muted/50 rounded-md">
                    {new Date(session.timestamp).toLocaleString()}
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="totalTokens">Total Tokens</Label>
                  <div id="totalTokens" className="p-2 bg-muted/50 rounded-md">
                    {session.tokens?.toLocaleString() || '0'}
                  </div>
                </div>

                <div className="flex justify-end">
                  {isEditMode ? (
                    <div className="flex gap-2">
                      <Button
                        variant="outline"
                        onClick={() => {
                          setIsEditMode(false)
                          setEditedName(session.name || "")
                        }}
                        disabled={isUpdating}
                      >
                        Cancel
                      </Button>
                      <Button
                        onClick={handleUpdate}
                        disabled={isUpdating}
                      >
                        {isUpdating ? (
                          <>
                            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                            Saving...
                          </>
                        ) : (
                          'Save Changes'
                        )}
                      </Button>
                    </div>
                  ) : (
                    <Button
                      variant="outline"
                      onClick={() => setIsEditMode(true)}
                    >
                      Edit Details
                    </Button>
                  )}
                </div>
              </div>
            </div>
          </TabsContent>

          <TabsContent value="messages" className="h-[500px] overflow-auto border rounded-md">
            {isLoadingMessages ? (
              <div className="flex items-center justify-center p-8">
                <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
              </div>
            ) : session.messages && session.messages.length > 0 ? (
              <div className="divide-y">
                {session.messages.map((message, index) => (
                  <div key={index} className="p-4">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <span className="font-medium capitalize">{message.sender}</span>
                        <span className="text-xs text-muted-foreground">
                          {message.promptTokens + (message.completionTokens || 0)} tokens
                        </span>
                      </div>
                      <span className="text-xs text-muted-foreground">
                        {new Date(message.timeStamp).toLocaleString()}
                      </span>
                    </div>
                    <div className="text-sm whitespace-pre-wrap">{message.prompt}</div>
                    {message.completion && (
                      <div className="mt-2 pl-4 border-l-2 border-primary/20">
                        <p className="text-sm whitespace-pre-wrap text-muted-foreground">
                          {message.completion}
                        </p>
                        <div className="mt-1 text-xs text-muted-foreground">
                          {message.completionTokens} completion tokens
                        </div>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className="text-center p-8">
                <p className="text-muted-foreground mb-4">No messages loaded</p>
                <Button
                  variant="outline"
                  onClick={() => onLoadMessages(session.sessionId)}
                  disabled={isLoadingMessages}
                >
                  Load Messages
                </Button>
              </div>
            )}
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  )
} 