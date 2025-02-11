interface ListDocumentsDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  isLoading?: boolean
  documents?: Document[]
  error?: string
  onFetch: (params: { 
    indexName: string
    suppressVectorFields: boolean
    maxResults: number
  }) => Promise<void>
} 