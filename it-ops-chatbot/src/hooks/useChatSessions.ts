import { useState, useEffect, useCallback } from 'react'

interface ChatSession {
  sessionId: string
  chatName: string
  createdAt: string
}

interface ApiSession {
  id: string
  type: string
  sessionId: string
  userId: string
  tokens: null
  name: string
  timestamp: string
}

interface ChatSessionCache {
  sessions: ChatSession[]
  lastUpdated: number
}

const CACHE_KEY = 'chatSessionCache'
const CACHE_DURATION = 60 * 60 * 1000 // 1 hour in milliseconds

export function useChatSessions(userId: string) {
  const [sessions, setSessions] = useState<ChatSession[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // Function to transform API response format to our format
  const transformSessions = (apiSessions: ApiSession[]): ChatSession[] => {
    return apiSessions.map(session => ({
      sessionId: session.sessionId,
      chatName: session.name,
      createdAt: session.timestamp
    }))
  }

  // Function to fetch sessions from API
  const fetchSessions = useCallback(async () => {
    console.log('fetchSessions called')
    try {
      setError(null)
      const response = await fetch(`/api/sessions?userId=${encodeURIComponent(userId)}`)
      console.log('API response status:', response.status)
      
      if (!response.ok) {
        throw new Error(`Failed to fetch sessions: ${response.statusText}`)
      }

      const data = await response.json()
      console.log('Received sessions data:', data)
      
      // Transform the data from API format
      const transformedSessions = Array.isArray(data) ? transformSessions(data) : []
      console.log('Transformed sessions:', transformedSessions)
      
      // Update both state and cache
      setSessions(transformedSessions)
      const cacheData: ChatSessionCache = {
        sessions: transformedSessions,
        lastUpdated: Date.now()
      }
      localStorage.setItem(CACHE_KEY, JSON.stringify(cacheData))
      
      return transformedSessions
    } catch (err) {
      console.error('Error fetching sessions:', err)
      setError('Failed to load chat sessions')
      return null
    }
  }, [userId])

  // Function to check and update cache if needed
  const updateCacheIfNeeded = useCallback(async () => {
    console.log('updateCacheIfNeeded called')
    const cachedData = localStorage.getItem(CACHE_KEY)
    
    if (cachedData) {
      try {
        const { sessions: cachedSessions, lastUpdated }: ChatSessionCache = JSON.parse(cachedData)
        const now = Date.now()
        console.log('Found cached data:', { 
          sessionCount: cachedSessions.length,
          age: (now - lastUpdated) / 1000,
          'seconds': 'seconds'
        })
        
        // If cache is valid AND we have some sessions, use it
        if (now - lastUpdated < CACHE_DURATION && cachedSessions.length > 0) {
          console.log('Using cached data with', cachedSessions.length, 'sessions')
          setSessions(cachedSessions)
          setIsLoading(false)
          return
        }
      } catch (err) {
        console.log('Error parsing cache, will fetch new data:', err)
      }
    }
    
    // If we reach here, we need to fetch new data
    setIsLoading(true)
    await fetchSessions()
    setIsLoading(false)
  }, [fetchSessions])

  // Initialize cache and set up periodic updates
  useEffect(() => {
    updateCacheIfNeeded()

    // Set up periodic updates
    const interval = setInterval(() => {
      updateCacheIfNeeded()
    }, CACHE_DURATION)

    return () => clearInterval(interval)
  }, [updateCacheIfNeeded])

  // Function to add a new session to the cache
  const addSession = useCallback((newSession: ChatSession) => {
    setSessions(prev => {
      const updated = [newSession, ...prev]
      const cacheData: ChatSessionCache = {
        sessions: updated,
        lastUpdated: Date.now()
      }
      localStorage.setItem(CACHE_KEY, JSON.stringify(cacheData))
      return updated
    })
  }, [])

  // Function to remove a session from the cache
  const removeSession = useCallback((sessionId: string) => {
    setSessions(prev => {
      const updated = prev.filter(session => session.sessionId !== sessionId)
      const cacheData: ChatSessionCache = {
        sessions: updated,
        lastUpdated: Date.now()
      }
      localStorage.setItem(CACHE_KEY, JSON.stringify(cacheData))
      return updated
    })
  }, [])

  return {
    sessions,
    isLoading,
    error,
    addSession,
    removeSession,
    refreshSessions: fetchSessions
  }
}
