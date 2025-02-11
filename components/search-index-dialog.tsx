interface SearchIndexDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  isSearching: boolean
  onSearch: (indexName: string, params: SearchParams) => Promise<void>
  results?: SearchResult[]
} 