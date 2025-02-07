'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ScrollArea } from '@/components/ui/scroll-area'
import { MessageSquarePlus, Search, Trash2, Settings, Shield } from 'lucide-react'
import { format } from 'date-fns'
import { nanoid } from 'nanoid'
import { useChatSessions } from '@/src/hooks/useChatSessions'
import { DeleteSessionDialog } from './delete-session-dialog'

interface ChatSidebarProps {
  onSelectChat: (sessionId: string, chatName: string) => void
  userId: string
}

export function ChatSidebar({ onSelectChat, userId }: ChatSidebarProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const router = useRouter()

  const {
    sessions,
    isLoading,
    error,
    addSession,
    removeSession
  } = useChatSessions(userId)

  const [showDeleteDialog, setShowDeleteDialog] = useState(false)
  const [sessionToDelete, setSessionToDelete] = useState<string | null>(null)

  const handleNewChat = () => {
    const newSession = {
      sessionId: nanoid(),
      chatName: `Chat ${new Date().toLocaleString()}`,
      createdAt: new Date().toISOString()
    }
    addSession(newSession)
    onSelectChat(newSession.sessionId, newSession.chatName)
  }

  const handleDeleteClick = (sessionId: string) => {
    setSessionToDelete(sessionId)
    setShowDeleteDialog(true)
  }

  const handleDeleteSubmit = async (data: { removeFromStorage: boolean }) => {
    if (sessionToDelete) {
      if (data.removeFromStorage) {
        try {
          // Call our Next.js API route instead of the backend directly
          const response = await fetch(`/api/sessions/${sessionToDelete}`, {
            method: 'DELETE'
          })
          
          if (!response.ok) {
            console.error('Error deleting session:', response.statusText)
            return // Don't remove from local if server delete failed
          }
        } catch (err) {
          console.error('Error deleting session:', err)
          return // Don't remove from local if server delete failed
        }
      }
      
      // Only remove from local list/cache if server delete succeeded or wasn't requested
      removeSession(sessionToDelete)
    }
    setSessionToDelete(null)
  }

  // If no userId is provided, show a message
  if (!userId) {
    return (
      <div className="text-center py-4 text-muted-foreground">
        Please log in to view chat sessions
      </div>
    )
  }

  // Filter sessions based on search query
  const filteredSessions = sessions
    .filter((session) => session.chatName?.toLowerCase().includes(searchQuery.toLowerCase()) ?? false)
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())

  return (
    <>
      {/* Hover trigger area */}
      <div
        className="fixed left-0 top-0 w-24 h-full z-40 bg-gradient-to-r from-transparent to-transparent"
        onMouseEnter={() => setIsOpen(true)}
      />

      {/* Sidebar */}
      <div
        className={`fixed left-0 top-4 bottom-4 bg-background border border-border rounded-r-xl w-80 transform transition-transform duration-300 ease-in-out z-50 ${
          isOpen ? 'translate-x-0' : '-translate-x-full'
        }`}
        onMouseLeave={() => setIsOpen(false)}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="p-4">
            <div className="flex items-center text-sm text-muted-foreground">
              <div className="h-4 w-4 mr-2" />
              {format(new Date(), 'M/d/yyyy h:mm a')}
            </div>
            <div className="h-px bg-border my-4" />
          </div>

          {/* Search and New Chat */}
          <div className="px-4 space-y-2">
            <Button
              variant="ghost"
              className="w-full justify-start text-purple-400 hover:text-purple-300 hover:bg-purple-500/10"
              onClick={handleNewChat}
            >
              <MessageSquarePlus className="mr-2 h-4 w-4" />
              Start new chat
            </Button>

            <div className="relative mt-2">
              <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search chats..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 bg-muted/50"
              />
            </div>
          </div>

          {/* Chat List */}
          <ScrollArea className="flex-1 px-2">
            <div className="p-2">
              {isLoading ? (
                <div className="text-center py-4 text-muted-foreground">
                  Loading sessions...
                </div>
              ) : error ? (
                <div className="text-center py-4 text-red-500">
                  {error}
                </div>
              ) : filteredSessions.length === 0 ? (
                <div className="text-center py-4 text-muted-foreground">
                  No chat sessions found
                </div>
              ) : (
                <div className="space-y-2">
                  {filteredSessions.map((session) => (
                    <div key={session.sessionId} className="group relative flex items-center">
                      <Button
                        variant="ghost"
                        className="w-full justify-start font-normal"
                        onClick={() => onSelectChat(session.sessionId, session.chatName)}
                      >
                        <MessageSquarePlus className="mr-2 h-4 w-4" />
                        {session.chatName}
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="absolute right-2 hidden h-8 w-8 group-hover:flex"
                        onClick={() => handleDeleteClick(session.sessionId)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </ScrollArea>

          {/* Footer with Settings and Administration */}
          <div className="p-4 border-t border-border mt-auto">
            <div className="space-y-2">
              <Button
                variant="ghost"
                className="w-full justify-start"
                onClick={() => router.push('/settings')}
              >
                <Settings className="mr-2 h-4 w-4" />
                Settings
              </Button>
              <Button
                variant="ghost"
                className="w-full justify-start"
                onClick={() => router.push('/admin')}
              >
                <Shield className="mr-2 h-4 w-4" />
                Administration
              </Button>
            </div>
          </div>
        </div>
      </div>

      <DeleteSessionDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        onSubmit={handleDeleteSubmit}
      />
    </>
  )
}