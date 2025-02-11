export interface Document {
  id: string
  content: string
  metadata?: Record<string, unknown>
  embedding?: number[]
}

export interface DocumentListResponse {
  documents: Document[]
  totalCount: number
} 