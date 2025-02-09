import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { useState } from "react"
import { Loader2 } from "lucide-react"

interface ListDocumentsDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  isLoading?: boolean
  documents?: any[]
  error?: string
  onFetch: (params: { 
    indexName: string
    suppressVectorFields: boolean
    maxResults: number
  }) => Promise<void>
}

export function ListDocumentsDialog({ 
  open, 
  onOpenChange, 
  indexName,
  isLoading,
  documents,
  error,
  onFetch 
}: ListDocumentsDialogProps) {
  const [suppressVectorFields, setSuppressVectorFields] = useState(true)
  const [maxResults, setMaxResults] = useState(1000)

  const handleFetch = async () => {
    await onFetch({
      indexName,
      suppressVectorFields,
      maxResults
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[80vh] flex flex-col">
        <DialogHeader>
          <DialogTitle>Documents for {indexName}</DialogTitle>
        </DialogHeader>
        
        {/* Configuration Options */}
        <div className="flex items-center gap-8 py-4 border-b">
          <div className="flex items-center gap-2">
            <Switch
              id="suppress-vectors"
              checked={suppressVectorFields}
              onCheckedChange={setSuppressVectorFields}
            />
            <Label htmlFor="suppress-vectors">Suppress Vector Fields</Label>
          </div>
          <div className="flex items-center gap-2">
            <Label htmlFor="max-results">Max Results:</Label>
            <Input
              id="max-results"
              type="number"
              min={1}
              max={10000}
              value={maxResults}
              onChange={(e) => setMaxResults(Number(e.target.value))}
              className="w-24"
            />
          </div>
          <Button 
            onClick={handleFetch}
            disabled={isLoading}
          >
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Refresh
          </Button>
        </div>

        {/* Content Area */}
        <div className="flex-1 overflow-y-auto">
          {error ? (
            <div className="p-4 text-red-500">
              {error}
            </div>
          ) : isLoading ? (
            <div className="flex items-center justify-center p-8">
              <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
            </div>
          ) : documents && documents.length > 0 ? (
            <div className="space-y-4 p-4">
              {documents.map((doc, index) => (
                <div 
                  key={doc.id || index} 
                  className="rounded-lg border p-4 hover:bg-muted/50"
                >
                  <pre className="whitespace-pre-wrap text-sm">
                    {JSON.stringify(doc, null, 2)}
                  </pre>
                </div>
              ))}
            </div>
          ) : (
            <div className="p-8 text-center text-muted-foreground">
              No documents found in this index.
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  )
} 