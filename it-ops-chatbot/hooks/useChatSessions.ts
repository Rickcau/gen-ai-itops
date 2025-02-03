import { useState, useEffect, useCallback } from 'react'

interface ChatSession {
  sessionId: string
  chatName: string
  createdAt: string
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

  // Function to fetch sessions from API
  const fetchSessions = useCallback(async () => {
    try {
      setError(null)
      const response = await fetch(`/api/sessions?userId=${encodeURIComponent(userId)}`)
      
      if (!response.ok) {
        throw new Error(`Failed to fetch sessions: ${response.statusText}`)
      }

      const data = await response.json()
      
      // Update both state and cache
      setSessions(data)
      const cacheData: ChatSessionCache = {
        sessions: data,
        lastUpdated: Date.now()
      }
      localStorage.setItem(CACHE_KEY, JSON.stringify(cacheData))
      
      return data
    } catch (err) {
      console.error('Error fetching sessions:', err)
      setError('Failed to load chat sessions')
      return null
    }
  }, [userId])

  // Function to check and update cache if needed
  const updateCacheIfNeeded = useCallback(async () => {
    const cachedData = localStorage.getItem(CACHE_KEY)
    
    if (cachedData) {
      const { sessions: cachedSessions, lastUpdated }: ChatSessionCache = JSON.parse(cachedData)
      const now = Date.now()
      
      // If cache is still valid, use it
      if (now - lastUpdated < CACHE_DURATION) {
        setSessions(cachedSessions)
        setIsLoading(false)
        return
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
