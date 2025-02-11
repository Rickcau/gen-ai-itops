import type { Session, SessionMessage, SessionUpdate } from '@/types/session'
import type { Document } from '@/types/document'
import type { SearchResult } from '@/types/search'

const [documents, setDocuments] = useState<Document[]>([])
const [searchResults, setSearchResults] = useState<SearchResult[]>([])
const [selectedUserData, setSelectedUserData] = useState<UserData | null>(null)
const [selectedSession, setSelectedSession] = useState<Session | null>(null)

const handleCreateSession = async (data: { userId: string; name: string }) => {
  setIsCreatingSession(true)
  try {
    const response = await fetch('/api/sessions', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
    if (!response.ok) {
      throw new Error('Failed to create session')
    }
    toast({
      title: "Success",
      description: "Session created successfully"
    })
    handleListSessions() // Refresh the sessions list
  } catch (error) {
    console.error('Error creating session:', error)
    toast({
      variant: "destructive",
      title: "Error",
      description: error instanceof Error ? error.message : 'Failed to create session'
    })
  } finally {
    setIsCreatingSession(false)
    setCreateSessionDialogOpen(false)
  }
} 