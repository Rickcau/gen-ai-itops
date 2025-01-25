export type MessageRole = 'user' | 'assistant' | 'specialist' | 'weather'

export interface Message {
  id: string
  content: string
  role: MessageRole
}

export interface ChatState {
  messages: Message[]
  isLoading: boolean
}

