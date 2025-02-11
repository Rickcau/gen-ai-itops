export interface SearchParams {
  query: string
  k: number
  top: number
  filter: string | null
  textOnly: boolean
  hybrid: boolean
  semantic: boolean
  minRerankerScore: number
}

export interface SearchResult {
  id: string
  name: string
  description: string
  score: number
  capabilityType: string
  tags: string[]
  parameters: Array<{
    name: string
    type: string
    description: string
  }>
  executionMethod: {
    type: string
    details: string
  }
} 