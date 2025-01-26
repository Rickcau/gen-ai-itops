'use client'

import { useState, forwardRef } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Settings, MessageSquarePlus, Search, Pencil, Trash2, Check, Shield } from 'lucide-react'
import { format } from 'date-fns'

interface ChatSidebarProps {
  onNewChat: () => void
  onSelectChat: (id: string) => void
}

interface ChatItem {
  id: string
  title: string
  date: Date
  isEditing?: boolean
}

export const ChatSidebar = forwardRef<
  { updateChatTitle: (id: string, title: string) => void },
  ChatSidebarProps
>(({ onNewChat, onSelectChat }, ref) => {
  const router = useRouter()
  const [isOpen, setIsOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')
  const [chatHistory, setChatHistory] = useState<ChatItem[]>([])
  const [editingTitle, setEditingTitle] = useState('')

  const handleNewChat = () => {
    // Stub: Would normally create a new chat and save to storage/DB
    const newChat = {
      id: Math.random().toString(),
      title: 'New Chat',
      date: new Date(),
    }
    setChatHistory([newChat, ...chatHistory])
    onNewChat()
  }

  const startEditing = (chat: ChatItem) => {
    setChatHistory(prev =>
      prev.map(c =>
        c.id === chat.id ? { ...c, isEditing: true } : { ...c, isEditing: false }
      )
    )
    setEditingTitle(chat.title)
  }

  const handleEditSave = (chat: ChatItem) => {
    if (editingTitle.trim()) {
      setChatHistory(prev =>
        prev.map(c =>
          c.id === chat.id ? { ...c, title: editingTitle.trim(), isEditing: false } : c
        )
      )
    }
  }

  const handleDelete = (chatId: string) => {
    if (confirm('Are you sure you want to delete this chat?')) {
      setChatHistory(prev => prev.filter(chat => chat.id !== chatId))
    }
  }

  const handleKeyDown = (e: React.KeyboardEvent, chat: ChatItem) => {
    if (e.key === 'Enter') {
      handleEditSave(chat)
    } else if (e.key === 'Escape') {
      setChatHistory(prev =>
        prev.map(c =>
          c.id === chat.id ? { ...c, isEditing: false } : c
        )
      )
    }
  }

  // Filter chats based on search query
  const filteredChats = chatHistory
    .filter((chat) => chat.title.toLowerCase().includes(searchQuery.toLowerCase()))

  // Group chats by date
  const groupedChats = filteredChats.reduce((groups, chat) => {
    const date = format(chat.date, 'EEEE')
    if (!groups[date]) {
      groups[date] = []
    }
    groups[date].push(chat)
    return groups
  }, {} as Record<string, typeof chatHistory>)

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
              <h2 className="text-sm font-semibold mb-2 px-2">Your Chats</h2>
              {Object.entries(groupedChats).map(([date, chats]) => (
                <div key={date} className="mb-4">
                  <div className="text-xs text-muted-foreground mb-2 px-2">
                    {date}
                  </div>
                  {chats.map((chat) => (
                    <div key={chat.id} className="group relative flex items-center">
                      {chat.isEditing ? (
                        <div className="flex items-center w-full px-2">
                          <Input
                            value={editingTitle}
                            onChange={(e) => setEditingTitle(e.target.value)}
                            onKeyDown={(e) => handleKeyDown(e, chat)}
                            onBlur={() => {
                              setChatHistory(prev =>
                                prev.map(c =>
                                  c.id === chat.id ? { ...c, isEditing: false } : c
                                )
                              )
                            }}
                            className="h-9 border-2"
                            autoFocus
                          />
                          <Button
                            variant="ghost"
                            size="icon"
                            className="h-9 w-9 ml-1"
                            onClick={() => handleEditSave(chat)}
                          >
                            <Check className="h-4 w-4" />
                          </Button>
                        </div>
                      ) : (
                        <Button
                          variant="ghost"
                          className="w-full justify-start font-normal"
                          onClick={() => onSelectChat(chat.id)}
                        >
                          <MessageSquarePlus className="mr-2 h-4 w-4" />
                          {chat.title}
                        </Button>
                      )}
                      <div className="absolute right-2 hidden group-hover:flex items-center gap-1">
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => startEditing(chat)}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="h-8 w-8"
                          onClick={() => handleDelete(chat.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              ))}
            </div>
          </ScrollArea>

          {/* Settings and Administration */}
          <div className="p-4 space-y-2">
            <div className="h-px bg-border mb-4" />
            <Button
              variant="ghost"
              className="w-full justify-start text-sm font-normal"
              onClick={() => {}}
            >
              <Settings className="mr-2 h-4 w-4" />
              Settings
            </Button>
            <Button
              variant="ghost"
              className="w-full justify-start text-sm font-normal"
              onClick={() => router.push('/admin')}
            >
              <Shield className="mr-2 h-4 w-4" />
              Administration
            </Button>
          </div>
        </div>
      </div>
    </>
  )
})

ChatSidebar.displayName = 'ChatSidebar' 