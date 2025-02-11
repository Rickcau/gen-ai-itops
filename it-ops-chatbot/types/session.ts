export interface SessionMessage {
  id: string
  timeStamp: string
  prompt: string
  sender: string
  promptTokens: number
  completion: string | null
  completionTokens: number
}

export interface Session {
  id: string
  sessionId: string
  userId?: string
  name?: string
  timestamp: string
  tokens?: number
  messages?: SessionMessage[]
}

export interface SessionUpdate {
  name?: string
  userId?: string
  tokens?: number
} 