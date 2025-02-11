'use client'

import { useState, useMemo } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ThemeToggle } from '@/components/theme-switcher'
import { ArrowLeft, Settings, Zap, User, Plus, Database, ChevronDown, ChevronRight, MessageSquare, Loader2 } from 'lucide-react'
import { CapabilityDialog } from '@/components/capability-dialog'
import { CreateIndexDialog } from '@/components/create-index-dialog'
import { DeleteIndexDialog } from '@/components/delete-index-dialog'
import type { Capability } from '@/types/capabilities'
import { nanoid } from 'nanoid'
import { useAuth } from '@/components/providers/auth-provider'
import { useToast } from "@/components/ui/use-toast"
import { ListDocumentsDialog } from "@/components/list-documents-dialog"
import { GenerateEmbeddingsDialog } from "@/components/generate-embeddings-dialog"
import { SearchIndexDialog } from "@/components/search-index-dialog"
import { DeleteCapabilityDialog } from '@/components/delete-capability-dialog'
import { CreateUserDialog } from '@/components/create-user-dialog'
import { ViewUserDialog } from '@/components/view-user-dialog'
import { DeleteUserDialog } from '@/components/delete-user-dialog'
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { ViewSessionDialog } from '@/components/view-session-dialog'
import { DeleteSessionDialog } from '@/components/delete-session-dialog'
import { CreateSessionDialog } from '@/components/create-session-dialog'
import { SystemWipeDialog } from '@/components/system-wipe-dialog'
import { ViewCapabilityDialog } from '@/components/view-capability-dialog'
import type { Session, SessionMessage, SessionUpdate } from '@/types/session'
import type { SearchResult, SearchParams } from '@/types/search'
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { TestDialog } from "@/components/test-dialog"

interface CollapsibleCardProps {
  title: string
  icon: React.ReactNode
  defaultExpanded?: boolean
  headerContent?: React.ReactNode
  children: React.ReactNode
}

function CollapsibleCard({ 
  title, 
  icon, 
  defaultExpanded = true, 
  headerContent, 
  children 
}: CollapsibleCardProps) {
  const [isExpanded, setIsExpanded] = useState(defaultExpanded)

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <button
          onClick={() => setIsExpanded(!isExpanded)}
          className="flex items-center gap-2 hover:opacity-80 transition-opacity"
        >
          {icon}
          <CardTitle className="flex items-center gap-2">
            {title}
            {isExpanded ? (
              <ChevronDown className="h-4 w-4 text-muted-foreground" />
            ) : (
              <ChevronRight className="h-4 w-4 text-muted-foreground" />
            )}
          </CardTitle>
        </button>
        {headerContent}
      </CardHeader>
      {isExpanded && <CardContent>{children}</CardContent>}
    </Card>
  )
}

interface UserData {
  userInfo: {
    email: string;
    firstName: string;
    lastName: string;
  };
  role: string;
  tier: string;
  mockMode: boolean;
  preferences: {
    theme: string;
  };
}

interface Document {
  id: string;
  content: string;
  metadata: Record<string, unknown>;
  embedding?: number[];
}

// Add a type guard function to check if userId exists and is a string
const hasUserId = (session: Session): session is Session & { userId: string } => {
  return typeof session.userId === 'string'
}

export default function AdminPage() {
  const router = useRouter()
  const { authState } = useAuth()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [createIndexDialogOpen, setCreateIndexDialogOpen] = useState(false)
  const [editingCapability, setEditingCapability] = useState<Capability | undefined>()
  const [indexes, setIndexes] = useState<string[]>([])
  const [isLoadingIndexes, setIsLoadingIndexes] = useState(false)
  const [indexError, setIndexError] = useState<string | null>(null)
  const [capabilities, setCapabilities] = useState<Capability[]>([])
  const [isLoadingCapabilities, setIsLoadingCapabilities] = useState(false)
  const [capabilitiesError, setCapabilitiesError] = useState<string | undefined>(undefined)
  const [indexToDelete, setIndexToDelete] = useState<string | null>(null)
  const [isDeletingIndex, setIsDeletingIndex] = useState(false)
  const { toast } = useToast()
  const [documentsDialogOpen, setDocumentsDialogOpen] = useState(false)
  const [selectedIndexForDocuments, setSelectedIndexForDocuments] = useState<string>('')
  const [documents, setDocuments] = useState<Document[]>([])
  const [isLoadingDocuments, setIsLoadingDocuments] = useState(false)
  const [documentsError, setDocumentsError] = useState<string | undefined>(undefined)
  const [generateDialogOpen, setGenerateDialogOpen] = useState(false)
  const [selectedIndexForGenerate, setSelectedIndexForGenerate] = useState<string>('')
  const [isGeneratingEmbeddings, setIsGeneratingEmbeddings] = useState(false)
  const [searchDialogOpen, setSearchDialogOpen] = useState(false)
  const [selectedIndexForSearch, setSelectedIndexForSearch] = useState<string>('')
  const [isSearching, setIsSearching] = useState(false)
  const [searchResults, setSearchResults] = useState<SearchResult[]>([])
  const [selectedCapability, setSelectedCapability] = useState<Capability | null>(null)
  const [viewCapabilityDialogOpen, setViewCapabilityDialogOpen] = useState(false)
  const [capabilityToDelete, setCapabilityToDelete] = useState<Capability | null>(null)
  const [isDeletingCapability, setIsDeletingCapability] = useState(false)
  const [sessions, setSessions] = useState<Session[]>([])
  const [isLoadingSessions, setIsLoadingSessions] = useState(false)
  const [sessionsError, setSessionsError] = useState<string | undefined>(undefined)
  const [users, setUsers] = useState<UserData[]>([])
  const [isLoadingUsers, setIsLoadingUsers] = useState(false)
  const [usersError, setUsersError] = useState<string | undefined>(undefined)
  const [createUserDialogOpen, setCreateUserDialogOpen] = useState(false)
  const [viewUserDialogOpen, setViewUserDialogOpen] = useState(false)
  const [selectedUserData, setSelectedUserData] = useState<UserData | null>(null)
  const [userToDelete, setUserToDelete] = useState<string | null>(null)
  const [isDeletingUser, setIsDeletingUser] = useState(false)
  const [sessionFilter, setSessionFilter] = useState('')
  const [userFilter, setUserFilter] = useState('')
  const [selectedSession, setSelectedSession] = useState<Session | null>(null)
  const [viewSessionDialogOpen, setViewSessionDialogOpen] = useState(false)
  const [sessionToDelete, setSessionToDelete] = useState<string | null>(null)
  const [deleteSessionDialogOpen, setDeleteSessionDialogOpen] = useState(false)
  const [systemWipeDialogOpen, setSystemWipeDialogOpen] = useState(false)
  const [isDeletingSession, setIsDeletingSession] = useState(false)
  const [createSessionDialogOpen, setCreateSessionDialogOpen] = useState(false)
  const [isCreatingSession, setIsCreatingSession] = useState(false)
  const [testDialogOpen, setTestDialogOpen] = useState(false)

  // Update the filteredSessions to use the type guard
  const filteredSessions = useMemo(() => {
    return sessions.filter(session => {
      const matchesSessionId = session.sessionId.toLowerCase().includes(sessionFilter.toLowerCase())
      const matchesUserId = !userFilter || (hasUserId(session) && session.userId.toLowerCase().includes(userFilter.toLowerCase()))
      return matchesSessionId && matchesUserId
    })
  }, [sessions, sessionFilter, userFilter])

  const handleAddCapability = async (data: Omit<Capability, 'id'>) => {
    try {
      const newCapability = {
        id: `capability-${nanoid(4)}`,
        ...data
      }

      console.log('Frontend: Starting request to add capability:', newCapability)
      const response = await fetch('/api/capabilities', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        },
        body: JSON.stringify(newCapability)
      })

      console.log('Frontend: Add response received:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText
      })

      if (!response.ok) {
        const errorData = await response.text()
        console.error('Frontend: Add failed with error:', errorData)
        throw new Error(errorData || 'Failed to add capability')
      }

      // Just log the response directly without assigning to a variable
      console.log('Frontend: Add successful:', await response.json())
      
      toast({
        title: "Success",
        description: `Successfully added capability: ${newCapability.name}`
      })

      // Refresh the capabilities list
      await handleListCapabilities()
    } catch (error) {
      console.error('Error adding capability:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to add capability'
      })
    }
  }

  const handleEditCapability = (data: Omit<Capability, 'id'>) => {
    if (!editingCapability) return
    setCapabilities(capabilities.map(cap => 
      cap.id === editingCapability.id 
        ? { ...data, id: cap.id }
        : cap
    ))
    setEditingCapability(undefined)
  }

  const handleDeleteCapability = async (id: string) => {
    setIsDeletingCapability(true)
    try {
      console.log('Frontend: Starting request to delete capability:', id)
      const response = await fetch(`/api/capabilities/${encodeURIComponent(id)}`, {
        method: 'DELETE',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })

      console.log('Frontend: Delete response received:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText
      })

      if (!response.ok) {
        const errorData = await response.text()
        console.error('Frontend: Delete failed with error:', errorData)
        throw new Error(errorData || 'Failed to delete capability')
      }

      // For 204 No Content, we don't try to parse the response
      if (response.status === 204) {
        toast({
          title: "Success",
          description: `Successfully deleted capability: ${capabilityToDelete?.name}`
        })
      }
      
      // Refresh the capabilities list
      await handleListCapabilities()
    } catch (error) {
      console.error('Error deleting capability:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to delete capability'
      })
    } finally {
      setIsDeletingCapability(false)
      setCapabilityToDelete(null)
    }
  }

  const handleUpdate = (capability: Capability) => {
    setSelectedCapability(capability)
    setViewCapabilityDialogOpen(true)
  }

  const handleListIndexes = async () => {
    setIsLoadingIndexes(true)
    setIndexError(null)
    
    try {
      console.log('Frontend: Starting request to list indexes')
      console.log('Frontend: About to fetch from:', '/api/indexes')
      
      const response = await fetch('/api/indexes', {
        method: 'GET',
        cache: 'no-store',
        headers: {
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })
      
      console.log('Frontend: Received response:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText
      })
      
      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`)
      }
      
      const rawText = await response.text()
      console.log('Raw response text:', rawText)
      
      const data = JSON.parse(rawText)
      console.log('Parsed indexes data:', data)
      setIndexes(data)
    } catch (error) {
      console.error('Error listing indexes:', error)
      toast({
        title: 'Error',
        description: error instanceof Error ? error.message : 'Failed to list indexes',
        variant: 'destructive'
      })
    } finally {
      setIsLoadingIndexes(false)
    }
  }

  const handleCreateIndex = async (indexName: string) => {
    setIsLoadingIndexes(true);
    try {
      const response = await fetch(`/api/indexes/${encodeURIComponent(indexName)}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || 'Failed to create index');
      }

      await response.json(); // Just consume the response
      toast({
        title: "Success",
        description: `Successfully created index: ${indexName}`
      });
      await handleListIndexes();
    } catch (error) {
      console.error('Error creating index:', error);
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to create index'
      });
    } finally {
      setIsLoadingIndexes(false);
    }
  };

  const handleDeleteIndex = async (indexName: string) => {
    setIsDeletingIndex(true);
    try {
      const url = `/api/indexes?indexName=${encodeURIComponent(indexName)}`;
      console.log('Frontend: Attempting to delete index with URL:', url);
      
      const response = await fetch(url, {
        method: 'DELETE',
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json',
          'Cache-Control': 'no-cache'
        }
      });

      console.log('Frontend: Delete response received:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText,
        url: response.url
      });

      if (!response.ok) {
        const errorData = await response.text();
        console.error('Frontend: Delete failed with error:', errorData);
        throw new Error(errorData || 'Failed to delete index');
      }

      const data = await response.json();
      console.log('Frontend: Delete successful:', data);
      toast({
        title: "Success",
        description: `Successfully deleted index: ${indexName}`
      });
      await handleListIndexes();
    } catch (error) {
      console.error('Error deleting index:', error);
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to delete index'
      });
    } finally {
      setIsDeletingIndex(false);
      setIndexToDelete(null);
    }
  };

  const handleListDocuments = async ({ 
    indexName, 
    suppressVectorFields, 
    maxResults 
  }: { 
    indexName: string; 
    suppressVectorFields: boolean; 
    maxResults: number; 
  }) => {
    setIsLoadingDocuments(true);
    setDocumentsError(undefined);
    try {
      console.log('Frontend: Starting request to list documents for index:', indexName);
      const url = `/api/indexes/documents?indexName=${encodeURIComponent(indexName)}&suppressVectorFields=${suppressVectorFields}&maxResults=${maxResults}`;
      
      const response = await fetch(url, {
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache'
        }
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || 'Failed to list documents');
      }

      const data = await response.json();
      console.log('Documents retrieved:', data);
      setDocuments(data);
    } catch (error) {
      console.error('Error listing documents:', error);
      setDocumentsError(error instanceof Error ? error.message : 'Failed to list documents');
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to list documents'
      });
    } finally {
      setIsLoadingDocuments(false);
    }
  };

  const handleGenerateEmbeddings = async (indexName: string) => {
    setIsGeneratingEmbeddings(true);
    try {
      console.log('Frontend: Starting request to generate embeddings for index:', indexName);
      const url = `/api/indexes/embeddings?indexName=${encodeURIComponent(indexName)}`;
      
      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache'
        }
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || 'Failed to generate embeddings');
      }

      console.log('Embeddings generation response:', await response.json());
      toast({
        title: "Success",
        description: `Successfully generated embeddings for index: ${indexName}`
      });
    } catch (error) {
      console.error('Error generating embeddings:', error);
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to generate embeddings'
      });
    } finally {
      setIsGeneratingEmbeddings(false);
      setGenerateDialogOpen(false);
    }
  };

  const handleSearch = async (indexName: string, params: SearchParams) => {
    setIsSearching(true);
    try {
      console.log('Frontend: Starting request to search index:', indexName);
      const url = `/api/indexes/capabilities/search?indexName=${encodeURIComponent(indexName)}`;
      
      const adjustedSearchParams = { 
        ...params, 
        filter: params.filter ?? undefined 
      };

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          'Cache-Control': 'no-cache'
        },
        body: JSON.stringify(adjustedSearchParams)
      });

      if (!response.ok) {
        const errorData = await response.text();
        throw new Error(errorData || 'Failed to search index');
      }

      const searchResults = await response.json();
      console.log('Search results:', searchResults);
      setSearchResults(searchResults);
      toast({
        title: "Success",
        description: `Found ${searchResults.length} results`
      });
    } catch (error) {
      console.error('Error searching index:', error);
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to search index'
      });
    } finally {
      setIsSearching(false);
    }
  };

  const handleListCapabilities = async () => {
    setIsLoadingCapabilities(true)
    setCapabilitiesError(undefined)
    
    try {
      console.log('Frontend: Starting request to list capabilities')
      console.log('Frontend: About to fetch from:', '/api/capabilities')
      
      const response = await fetch('/api/capabilities', {
        method: 'GET',
        cache: 'no-store',
        headers: {
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })
      
      console.log('Frontend: Received response:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText
      })
      
      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`)
      }
      
      const data = await response.json()
      console.log('Parsed capabilities data:', data)
      setCapabilities(data)
    } catch (error) {
      console.error('Error listing capabilities:', error)
      setCapabilitiesError(error instanceof Error ? error.message : 'Failed to list capabilities')
      toast({
        title: 'Error',
        description: error instanceof Error ? error.message : 'Failed to list capabilities',
        variant: 'destructive'
      })
    } finally {
      setIsLoadingCapabilities(false)
    }
  }

  const handleUpdateCapability = async (updatedCapability: Capability) => {
    try {
      console.log('Frontend: Starting request to update capability:', updatedCapability.id)
      const response = await fetch(`/api/capabilities/${encodeURIComponent(updatedCapability.id)}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        },
        body: JSON.stringify(updatedCapability)
      })

      console.log('Frontend: Update response received:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText,
        url: response.url
      })

      if (!response.ok) {
        const errorData = await response.text()
        console.error('Frontend: Update failed with error:', errorData)
        throw new Error(errorData || 'Failed to update capability')
      }

      console.log('Frontend: Update successful:', await response.json())
      toast({
        title: "Success",
        description: `Successfully updated capability: ${updatedCapability.name}`
      })
      
      // Refresh the capabilities list
      await handleListCapabilities()
      setViewCapabilityDialogOpen(false)
    } catch (error) {
      console.error('Error updating capability:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to update capability'
      })
    }
  }

  const handleListSessions = async () => {
    setIsLoadingSessions(true)
    setSessionsError(undefined)
    
    try {
      console.log('Frontend: Starting request to list sessions')
      const response = await fetch('/api/sessions', {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })
      
      console.log('Frontend: Received response:', {
        ok: response.ok,
        status: response.status,
        statusText: response.statusText
      })

      if (!response.ok) {
        const errorData = await response.text()
        console.error('Frontend: List sessions failed with error:', errorData)
        throw new Error(errorData || 'Failed to list sessions')
      }
      
      const data = await response.json()
      console.log('Frontend: Sessions data retrieved:', data)
      setSessions(data)
    } catch (error) {
      console.error('Error listing sessions:', error)
      setSessionsError(error instanceof Error ? error.message : 'Failed to list sessions')
      toast({
        title: 'Error',
        description: error instanceof Error ? error.message : 'Failed to list sessions',
        variant: 'destructive'
      })
    } finally {
      setIsLoadingSessions(false)
    }
  }

  const handleListUsers = async () => {
    setIsLoadingUsers(true)
    setUsersError(undefined)
    
    try {
      console.log('Frontend: Starting request to list users')
      const response = await fetch('/api/users', {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })
      
      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`)
      }
      
      const data = await response.json()
      console.log('Parsed users data:', data)
      setUsers(data)
    } catch (error) {
      console.error('Error listing users:', error)
      setUsersError(error instanceof Error ? error.message : 'Failed to list users')
      toast({
        title: 'Error',
        description: error instanceof Error ? error.message : 'Failed to list users',
        variant: 'destructive'
      })
    } finally {
      setIsLoadingUsers(false)
    }
  }

  const handleCreateUser = async (userData: UserData) => {
    try {
      console.log('Frontend: Starting request to create user:', userData)
      const response = await fetch('/api/users', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        },
        body: JSON.stringify(userData)
      })

      if (!response.ok) {
        const errorData = await response.text()
        console.error('Frontend: Create user failed with error:', errorData)
        throw new Error(errorData || 'Failed to create user')
      }

      const data = await response.json()
      console.log('Frontend: Create user successful:', data)
      
      toast({
        title: "Success",
        description: `Successfully created user: ${userData.userInfo.email}`
      })

      // Refresh the users list
      await handleListUsers()
      setCreateUserDialogOpen(false)
    } catch (error) {
      console.error('Error creating user:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to create user'
      })
    }
  }

  const handleUpdateUser = async (userData: UserData) => {
    try {
      console.log('Frontend: Starting request to update user:', userData)
      const response = await fetch(`/api/users/${encodeURIComponent(userData.userInfo.email)}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        },
        body: JSON.stringify(userData)
      })

      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`)
      }

      const data = await response.json()
      console.log('Frontend: User update successful:', data)
      toast({
        title: "Success",
        description: `Successfully updated user: ${userData.userInfo.email}`
      })

      // Refresh the users list and close the dialog
      await handleListUsers()
      setViewUserDialogOpen(false)
    } catch (error) {
      console.error('Error updating user:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to update user'
      })
    }
  }

  const handleDeleteUser = async (email: string) => {
    setIsDeletingUser(true)
    try {
      console.log('Frontend: Starting request to delete user:', email)
      const response = await fetch(`/api/users/${encodeURIComponent(email)}/history`, {
        method: 'DELETE',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })

      if (!response.ok) {
        throw new Error(`API request failed: ${response.statusText}`)
      }

      toast({
        title: "Success",
        description: `Successfully deleted user: ${email}`
      })

      // Refresh the users list
      await handleListUsers()
    } catch (error) {
      console.error('Error deleting user:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to delete user'
      })
    } finally {
      setIsDeletingUser(false)
      setUserToDelete(null)
    }
  }

  const abbreviateId = (id: string, maxLength: number = 8) => {
    if (id.length <= maxLength) return id;
    return `${id.slice(0, maxLength)}...`;
  }

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
      const responseData = await response.json()
      if (!response.ok) {
        throw new Error(responseData.message || 'Failed to create session')
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

  const handleUpdateSession = async (sessionId: string, updates: SessionUpdate) => {
    try {
      const response = await fetch(`/api/sessions/${encodeURIComponent(sessionId)}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updates)
      })

      if (!response.ok) {
        throw new Error('Failed to update session')
      }

      const updatedSession = await response.json()
      setSessions(prevSessions => 
        prevSessions.map(session => 
          session.sessionId === sessionId ? updatedSession : session
        )
      )

      toast({
        title: "Success",
        description: "Session updated successfully"
      })
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to update session'
      })
    }
  }

  const handleDeleteSession = async (sessionId: string) => {
    setIsDeletingSession(true)
    try {
      const response = await fetch(`/api/sessions/${encodeURIComponent(sessionId)}`, {
        method: 'DELETE'
      })

      if (!response.ok) {
        throw new Error('Failed to delete session')
      }

      setSessions(prevSessions => prevSessions.filter(session => session.sessionId !== sessionId))
      setDeleteSessionDialogOpen(false)
      setSessionToDelete(null)

      toast({
        title: "Success",
        description: "Session deleted successfully"
      })
    } catch (error) {
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to delete session'
      })
    } finally {
      setIsDeletingSession(false)
    }
  }

  const handleSystemWipe = async (systemWipeKey: string) => {
    try {
      console.log('Frontend: Starting system wipe request')
      const response = await fetch('/api/sessions/system-wipe', {
        method: 'DELETE',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache',
          'X-System-Wipe-Key': systemWipeKey
        }
      })

      if (!response.ok) {
        const errorData = await response.text()
        throw new Error(errorData || 'Failed to perform system wipe')
      }

      toast({
        title: "Success",
        description: "System wipe completed successfully"
      })

      // Refresh the sessions list
      await handleListSessions()
      setSystemWipeDialogOpen(false)
    } catch (error) {
      console.error('Error during system wipe:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to perform system wipe'
      })
    }
  }

  const handleViewSession = (session: Session) => {
    setSelectedSession(session)
    setViewSessionDialogOpen(true)
  }

  const handleLoadSessionMessages = async (sessionId: string) => {
    try {
      console.log('Frontend: Starting request to load session messages:', sessionId)
      const response = await fetch(`/api/sessions/${encodeURIComponent(sessionId)}/messages`, {
        method: 'GET',
        headers: {
          'Accept': 'application/json',
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache'
        }
      })

      if (!response.ok) {
        const errorText = await response.text()
        throw new Error(errorText || 'Failed to load session messages')
      }

      const messages: SessionMessage[] = await response.json()
      console.log('Frontend: Messages loaded:', messages)
      
      setSelectedSession(prev => {
        if (!prev) return prev
        return {
          ...prev,
          messages
        }
      })
    } catch (error) {
      console.error('Error loading session messages:', error)
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to load session messages'
      })
    }
  }

  return (
    <div className="container mx-auto max-w-4xl p-4 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="icon"
            onClick={() => router.push('/')}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1 className="text-2xl font-bold tracking-tight">Administration</h1>
        </div>
        <div className="flex items-center gap-3">
          <ThemeToggle />
          <div className="h-4 w-px bg-border" />
          <div className="flex items-center text-sm">
            <User className="h-4 w-4 mr-1.5 text-muted-foreground" />
            <span className="text-muted-foreground">{authState.user?.email}</span>
          </div>
        </div>
      </div>

      {/* Capabilities Section */}
      <CollapsibleCard
        title="Capabilities"
        icon={<Zap className="h-5 w-5" />}
        headerContent={
          <div className="flex gap-2">
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={() => {
                setEditingCapability(undefined)
                setDialogOpen(true)
              }}
            >
              <Plus className="h-4 w-4" />
              Create Capability
            </Button>
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={handleListCapabilities}
              disabled={isLoadingCapabilities}
            >
              {isLoadingCapabilities ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Loading...
                </>
              ) : (
                'Refresh'
              )}
            </Button>
            <Button
              onClick={() => setTestDialogOpen(true)}
              variant="outline"
              size="sm"
            >
              Test Dialog
            </Button>
          </div>
        }
      >
        <div className="rounded-md border">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="text-left p-4 font-medium w-[200px]">Name</th>
                <th className="text-left p-4 font-medium w-[200px]">Description</th>
                <th className="text-left p-4 font-medium w-[120px]">Type</th>
                <th className="text-left p-4 font-medium w-[200px]">Tags</th>
                <th className="text-right p-4 font-medium w-[180px]">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {capabilitiesError ? (
                <tr>
                  <td colSpan={5} className="p-4 text-center text-red-500">
                    {capabilitiesError}
                  </td>
                </tr>
              ) : capabilities.length === 0 ? (
                <tr>
                  <td colSpan={5} className="p-4 text-center text-muted-foreground">
                    {isLoadingCapabilities ? 'Loading capabilities...' : 'No capabilities found'}
                  </td>
                </tr>
              ) : (
                capabilities.map((capability) => (
                  <tr key={capability.id}>
                    <td className="p-4 font-medium">{capability.name}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      <span 
                        title={capability.description}
                        className="cursor-help inline-block max-w-[180px] truncate"
                      >
                        {capability.description}
                      </span>
                    </td>
                    <td className="p-4 text-sm">{capability.capabilityType}</td>
                    <td className="p-4">
                <div className="flex flex-wrap gap-1">
                        {capability.tags.map((tag, index) => {
                          const colorClasses = [
                            "bg-blue-500/10 text-blue-700 dark:text-blue-300",
                            "bg-purple-500/10 text-purple-700 dark:text-purple-300",
                            "bg-green-500/10 text-green-700 dark:text-green-300",
                            "bg-orange-500/10 text-orange-700 dark:text-orange-300",
                            "bg-pink-500/10 text-pink-700 dark:text-pink-300"
                          ];
                          const colorClass = colorClasses[index % colorClasses.length];
                          
                          return (
                            <span 
                              key={tag} 
                              className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${colorClass}`}
                            >
                      {tag}
                    </span>
                          );
                        })}
                </div>
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-end gap-2">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="text-muted-foreground hover:text-foreground w-[110px]"
                          onClick={() => handleUpdate(capability)}
                        >
                          View & Edit
                        </Button>
                        <Button 
                          variant="destructive"
                          size="sm"
                          onClick={() => setCapabilityToDelete(capability)}
                        >
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
              </div>
      </CollapsibleCard>

      {/* Indexes Section */}
      <CollapsibleCard
        title="Indexes"
        icon={<Database className="h-5 w-5" />}
        headerContent={
          <div className="flex gap-2">
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={() => setCreateIndexDialogOpen(true)}
            >
              <Plus className="h-4 w-4" />
              Create Index
            </Button>
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={handleListIndexes}
              disabled={isLoadingIndexes}
            >
              {isLoadingIndexes ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Loading...
                </>
              ) : (
                'Refresh'
              )}
            </Button>
          </div>
        }
      >
          <div className="rounded-md border">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="text-left p-4 font-medium">Name</th>
                <th className="text-left p-4 font-medium w-[100px]">Status</th>
                <th className="text-right p-4 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
            {indexError ? (
                <tr>
                  <td colSpan={3} className="p-4 text-center text-red-500">
                {indexError}
                  </td>
                </tr>
            ) : indexes.length === 0 ? (
                <tr>
                  <td colSpan={3} className="p-4 text-center text-muted-foreground">
                {isLoadingIndexes ? 'Loading indexes...' : 'No indexes created yet'}
                  </td>
                </tr>
              ) : (
                indexes.map((indexName) => (
                  <tr key={indexName}>
                    <td className="p-4 font-medium">{indexName}</td>
                    <td className="p-4">
                      <span className="inline-flex items-center rounded-full bg-emerald-500/15 px-2 py-1 text-xs font-medium text-emerald-700 dark:bg-emerald-500/10 dark:text-emerald-400">
                        Active
                      </span>
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-end space-x-2">
                      <Button 
                        variant="ghost" 
                        size="sm"
                        className="text-muted-foreground hover:text-foreground"
                        onClick={() => {
                            setSelectedIndexForSearch(indexName);
                            setSearchDialogOpen(true);
                        }}
                      >
                          Search
                      </Button>
                      <Button 
                        variant="ghost" 
                        size="sm"
                        className="text-muted-foreground hover:text-foreground"
                        onClick={() => {
                            setSelectedIndexForDocuments(indexName);
                            setDocumentsDialogOpen(true);
                        }}
                      >
                        Documents
                      </Button>
                        <div className="h-4 w-px bg-border" />
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="text-muted-foreground hover:text-foreground"
                          onClick={() => {
                            setSelectedIndexForGenerate(indexName);
                            setGenerateDialogOpen(true);
                          }}
                        >
                          Generate
                        </Button>
                        <div className="h-4 w-px bg-border" />
                        <Button 
                          variant="destructive"
                          size="sm"
                          onClick={() => setIndexToDelete(indexName)}
                          disabled={isLoadingIndexes}
                        >
                          Delete
                        </Button>
                    </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
                  </div>
      </CollapsibleCard>

      {/* Users Section */}
      <CollapsibleCard
        title="Users"
        icon={<User className="h-5 w-5" />}
        headerContent={
          <div className="flex gap-2">
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={() => setCreateUserDialogOpen(true)}
            >
              <Plus className="h-4 w-4" />
              Create User
            </Button>
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={handleListUsers}
              disabled={isLoadingUsers}
            >
              {isLoadingUsers ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Loading...
                </>
              ) : (
                'Refresh'
              )}
            </Button>
          </div>
        }
      >
        <div className="rounded-md border">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="text-left p-4 font-medium">Email</th>
                <th className="text-left p-4 font-medium">Role</th>
                <th className="text-left p-4 font-medium">Tier</th>
                <th className="text-right p-4 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {usersError ? (
                <tr>
                  <td colSpan={4} className="p-4 text-center text-red-500">
                    {usersError}
                  </td>
                </tr>
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan={4} className="p-4 text-center text-muted-foreground">
                    {isLoadingUsers ? 'Loading users...' : 'No users found'}
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr key={user.userInfo.email}>
                    <td className="p-4 font-medium">{user.userInfo.email}</td>
                    <td className="p-4">
                      <span className="inline-flex items-center rounded-full bg-blue-500/10 px-2 py-1 text-xs font-medium text-blue-700 dark:text-blue-300">
                        {user.role}
                      </span>
                    </td>
                    <td className="p-4">
                      <span className="inline-flex items-center rounded-full bg-purple-500/10 px-2 py-1 text-xs font-medium text-purple-700 dark:text-purple-300">
                        {user.tier}
                      </span>
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-end gap-2">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="text-muted-foreground hover:text-foreground"
                          onClick={() => {
                            setSelectedUserData(user)
                            setViewUserDialogOpen(true)
                          }}
                        >
                          View & Edit
                        </Button>
                        <Button 
                          variant="destructive"
                          size="sm"
                          onClick={() => setUserToDelete(user.userInfo.email)}
                        >
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
          </div>
      </CollapsibleCard>

      {/* Sessions Section */}
      <CollapsibleCard
        title="Sessions"
        icon={<MessageSquare className="h-5 w-5" />}
        headerContent={
          <div className="flex gap-2">
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={() => {
                setCreateSessionDialogOpen(true)
              }}
            >
              <Plus className="h-4 w-4" />
              Create Session
            </Button>
            <Button 
              variant="outline" 
              className="gap-2"
              onClick={handleListSessions}
              disabled={isLoadingSessions}
            >
              {isLoadingSessions ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Loading...
                </>
              ) : (
                'Refresh'
              )}
            </Button>
          </div>
        }
      >
        {/* Filter Section */}
        <div className="mb-4 flex gap-4">
          <div className="flex-1">
            <Label htmlFor="sessionFilter">Session ID</Label>
            <Input
              id="sessionFilter"
              placeholder="Filter by Session ID..."
              value={sessionFilter}
              onChange={(e) => setSessionFilter(e.target.value)}
              className="mt-1"
            />
          </div>
          <div className="flex-1">
            <Label htmlFor="userFilter">User ID</Label>
            <Input
              id="userFilter"
              placeholder="Filter by User ID..."
              value={userFilter}
              onChange={(e) => setUserFilter(e.target.value)}
              className="mt-1"
            />
          </div>
          <div className="flex items-end">
            <Button 
              variant="ghost" 
              onClick={() => {
                setSessionFilter('')
                setUserFilter('')
              }}
            >
              Clear Filters
            </Button>
          </div>
        </div>

        <div className="rounded-md border">
          <table className="w-full">
            <thead className="bg-muted/50">
              <tr>
                <th className="text-left p-4 font-medium">Session ID</th>
                <th className="text-left p-4 font-medium">User ID</th>
                <th className="text-left p-4 font-medium">Name</th>
                <th className="text-left p-4 font-medium">Created</th>
                <th className="text-right p-4 font-medium">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {sessionsError ? (
                <tr>
                  <td colSpan={5} className="p-4 text-center text-red-500">
                    {sessionsError}
                  </td>
                </tr>
              ) : sessions.length === 0 ? (
                <tr>
                  <td colSpan={5} className="p-4 text-center text-muted-foreground">
                    {isLoadingSessions ? 'Loading sessions...' : 'No active sessions found'}
                  </td>
                </tr>
              ) : (
                filteredSessions.map((session) => (
                  <tr key={session.id}>
                    <td className="p-4 font-mono text-sm">
                      <span title={session.sessionId}>
                        {abbreviateId(session.sessionId)}
                      </span>
                    </td>
                    <td className="p-4">
                      <span title={session.userId || ''} className="cursor-help">
                        {abbreviateId(session.userId || '', 12)}
                      </span>
                    </td>
                    <td className="p-4">{session.name || 'Chat'}</td>
                    <td className="p-4 text-sm text-muted-foreground">
                      {new Date(session.timestamp).toLocaleString()}
                    </td>
                    <td className="p-4">
                      <div className="flex items-center justify-end gap-2">
                        <Button 
                          variant="ghost" 
                          size="sm"
                          className="text-muted-foreground hover:text-foreground"
                          onClick={() => handleViewSession(session)}
                        >
                          View
                        </Button>
                        <Button 
                          variant="destructive"
                          size="sm"
                          onClick={() => {
                            setSessionToDelete(session.sessionId)
                            setDeleteSessionDialogOpen(true)
                          }}
                        >
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* System Operations Section */}
        <div className="mt-6 border-t pt-6">
          <h4 className="text-sm font-medium mb-4">System Operations</h4>
          <Button 
            variant="destructive" 
            size="sm"
            onClick={() => setSystemWipeDialogOpen(true)}
          >
            System Wipe
          </Button>
        </div>
      </CollapsibleCard>

      {/* Settings Section */}
      <CollapsibleCard
        title="Settings"
        icon={<Settings className="h-5 w-5" />}
        defaultExpanded={false}
      >
          <div className="grid grid-cols-2 gap-4">
            <Button variant="outline" className="justify-start">
              Configure API Endpoints
            </Button>
            <Button variant="outline" className="justify-start">
              Manage User Preferences
            </Button>
            <Button variant="outline" className="justify-start">
              Update Notification Settings
            </Button>
            <Button variant="outline" className="justify-start">
              System Configuration
            </Button>
          </div>
      </CollapsibleCard>

      <CapabilityDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSubmit={editingCapability ? handleEditCapability : handleAddCapability}
        initialData={editingCapability}
      />

      <CreateIndexDialog
        open={createIndexDialogOpen}
        onOpenChange={setCreateIndexDialogOpen}
        onSubmit={handleCreateIndex}
      />

      <DeleteIndexDialog
        open={indexToDelete !== null}
        onOpenChange={(open) => !open && setIndexToDelete(null)}
        indexName={indexToDelete || ''}
        onConfirm={handleDeleteIndex}
        isDeleting={isDeletingIndex}
      />

      <ListDocumentsDialog
        open={documentsDialogOpen}
        onOpenChange={(open) => {
          setDocumentsDialogOpen(open);
          if (!open) {
            setSelectedIndexForDocuments('');
            setDocuments([]);
            setDocumentsError(undefined);
          }
        }}
        indexName={selectedIndexForDocuments}
        documents={documents}
        isLoading={isLoadingDocuments}
        error={documentsError}
        onFetch={handleListDocuments}
      />

      <GenerateEmbeddingsDialog
        open={generateDialogOpen}
        onOpenChange={(open) => {
          setGenerateDialogOpen(open);
          if (!open) {
            setSelectedIndexForGenerate('');
          }
        }}
        indexName={selectedIndexForGenerate}
        isGenerating={isGeneratingEmbeddings}
        onConfirm={handleGenerateEmbeddings}
      />

      <SearchIndexDialog
        open={searchDialogOpen}
        onOpenChange={(open) => {
          setSearchDialogOpen(open);
          if (!open) {
            setSelectedIndexForSearch('');
            setSearchResults([]);
          }
        }}
        indexName={selectedIndexForSearch}
        isSearching={isSearching}
        onSearch={handleSearch}
        results={searchResults}
      />

      <ViewCapabilityDialog
        open={viewCapabilityDialogOpen}
        onOpenChange={(open) => {
          setViewCapabilityDialogOpen(open)
          if (!open) {
            setSelectedCapability(null)
          }
        }}
        capability={selectedCapability}
        onSubmit={handleUpdateCapability}
      />

      <DeleteCapabilityDialog
        open={capabilityToDelete !== null}
        onOpenChange={(open) => !open && setCapabilityToDelete(null)}
        capability={capabilityToDelete}
        onConfirm={handleDeleteCapability}
        isDeleting={isDeletingCapability}
      />

      <CreateUserDialog
        open={createUserDialogOpen}
        onOpenChange={setCreateUserDialogOpen}
        onSubmit={handleCreateUser}
      />

      <ViewUserDialog
        open={viewUserDialogOpen}
        onOpenChange={setViewUserDialogOpen}
        userData={selectedUserData}
        onSubmit={handleUpdateUser}
      />

      <DeleteUserDialog
        open={userToDelete !== null}
        onOpenChange={(open) => !open && setUserToDelete(null)}
        userEmail={userToDelete}
        onConfirm={handleDeleteUser}
        isDeleting={isDeletingUser}
      />

      <ViewSessionDialog
        open={viewSessionDialogOpen}
        onOpenChange={setViewSessionDialogOpen}
        session={selectedSession}
        onUpdate={handleUpdateSession}
        onLoadMessages={handleLoadSessionMessages}
      />

      <CreateSessionDialog
        open={createSessionDialogOpen}
        onOpenChange={setCreateSessionDialogOpen}
        onSubmit={handleCreateSession}
        isCreating={isCreatingSession}
      />

      <DeleteSessionDialog
        open={deleteSessionDialogOpen}
        onOpenChange={setDeleteSessionDialogOpen}
        sessionId={sessionToDelete}
        onConfirm={handleDeleteSession}
        isDeleting={isDeletingSession}
      />

      <SystemWipeDialog
        open={systemWipeDialogOpen}
        onOpenChange={setSystemWipeDialogOpen}
        onConfirm={handleSystemWipe}
        isWiping={false}
      />

      <TestDialog
        open={testDialogOpen}
        onOpenChange={setTestDialogOpen}
      />

    </div>
  )
} 