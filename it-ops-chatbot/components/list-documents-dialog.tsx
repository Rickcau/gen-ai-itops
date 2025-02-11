import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { useState } from "react"
import { Loader2, Upload, Copy, ClipboardCheck } from "lucide-react"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Textarea } from "@/components/ui/textarea"
import { useToast } from "@/components/ui/use-toast"
import type { Document } from "@/types/document"

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

const sampleDocument = `[ 
  {
    "id": "capability-0009",
    "capabilityType": "Runbook",
    "name": "Shutdown-VM",
    "description": "This runbook ...",
    "tags": [
        "VM",
        "PowerManagement",
        "Shutdown"
    ],
    "parameters": [
        {
            "name": "VMName",
            "type": "string",
            "description": "Name of the Virtual Machine"
        },
        {
            "name": "ResourceGroup",
            "type": "string",
            "description": "Name of the Resource Group"
        }
    ],
    "executionMethod": {
        "type": "Azure Automation",
        "details": "Shutdown a VM using an Azure Automation Runbook"
    }
  }
]`

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
  const [jsonInput, setJsonInput] = useState('')
  const [isUploading, setIsUploading] = useState(false)
  const [jsonError, setJsonError] = useState<string | null>(null)
  const [hasCopied, setHasCopied] = useState(false)
  const { toast } = useToast()

  const handleFetch = async () => {
    await onFetch({
      indexName,
      suppressVectorFields,
      maxResults
    })
  }

  const validateAndParseJSON = (input: string) => {
    try {
      const parsed = JSON.parse(input)
      if (!Array.isArray(parsed)) {
        throw new Error('Input must be an array of documents')
      }
      return parsed
    } catch (error: unknown) {
      throw new Error(error instanceof Error ? error.message : 'Invalid JSON format')
    }
  }

  const handleUpload = async () => {
    setJsonError(null)
    setIsUploading(true)

    try {
      const documents = validateAndParseJSON(jsonInput)
      
      const response = await fetch(`/api/indexes/capabilities?indexName=${encodeURIComponent(indexName)}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(documents)
      })

      if (!response.ok) {
        const error = await response.text()
        throw new Error(error || 'Failed to upload documents')
      }

      const result = await response.json()
      toast({
        title: "Success",
        description: result.message || 'Documents uploaded successfully'
      })

      // Clear the input
      setJsonInput('')
      
      // Refresh the documents list
      await handleFetch()
    } catch (error) {
      console.error('Error uploading documents:', error)
      setJsonError(error instanceof Error ? error.message : 'Failed to upload documents')
      toast({
        variant: "destructive",
        title: "Error",
        description: error instanceof Error ? error.message : 'Failed to upload documents'
      })
    } finally {
      setIsUploading(false)
    }
  }

  const handleCopySample = async () => {
    try {
      await navigator.clipboard.writeText(sampleDocument)
      setHasCopied(true)
      toast({
        title: "Success",
        description: "Sample document copied to clipboard"
      })
      setTimeout(() => setHasCopied(false), 2000)
    } catch (err) {
      toast({
        variant: "destructive",
        title: "Failed to copy",
        description: "Please try copying manually"
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl">
        <DialogHeader>
          <DialogTitle>Documents for {indexName}</DialogTitle>
        </DialogHeader>

        <Tabs defaultValue="view">
          <TabsList className="w-full">
            <TabsTrigger value="view" className="flex-1">View Documents</TabsTrigger>
            <TabsTrigger value="add" className="flex-1">Add Documents</TabsTrigger>
          </TabsList>

          <TabsContent value="view">
            <div className="flex items-center gap-8 py-4">
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

            <div className="relative border rounded-md h-[600px]">
              <div className="absolute inset-0 overflow-y-auto p-4">
                {error ? (
                  <div className="text-red-500">
                    {error}
                  </div>
                ) : isLoading ? (
                  <div className="flex items-center justify-center h-full">
                    <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
                  </div>
                ) : documents && documents.length > 0 ? (
                  <div className="space-y-4">
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
                  <div className="flex items-center justify-center h-full text-muted-foreground">
                    No documents found in this index
                  </div>
                )}
              </div>
            </div>
          </TabsContent>

          <TabsContent value="add">
            <div className="py-4">
              <div className="mb-4">
                <Label htmlFor="json-input">JSON Documents</Label>
                <p className="text-sm text-muted-foreground mt-1">
                  Paste a JSON array of documents to upload. Each document should follow the capability schema.
                </p>
              </div>

              <div className="relative border rounded-md h-[600px]">
                <Textarea
                  id="json-input"
                  placeholder={sampleDocument}
                  value={jsonInput}
                  onChange={(e) => setJsonInput(e.target.value)}
                  className="absolute inset-0 resize-none font-mono border-0 rounded-md"
                />
              </div>

              {jsonError && (
                <div className="mt-4 text-sm text-red-500 bg-red-50 dark:bg-red-900/20 p-3 rounded-md">
                  {jsonError}
                </div>
              )}

              <div className="mt-4 flex justify-end gap-2">
                <Button
                  onClick={handleCopySample}
                  variant="outline"
                  className="gap-2"
                >
                  {hasCopied ? (
                    <>
                      <ClipboardCheck className="h-4 w-4" />
                      Copied!
                    </>
                  ) : (
                    <>
                      <Copy className="h-4 w-4" />
                      Copy Sample
                    </>
                  )}
                </Button>
                <Button
                  onClick={handleUpload}
                  disabled={isUploading || !jsonInput.trim()}
                  className="gap-2"
                >
                  {isUploading ? (
                    <>
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Uploading...
                    </>
                  ) : (
                    <>
                      <Upload className="h-4 w-4" />
                      Upload Documents
                    </>
                  )}
                </Button>
              </div>
            </div>
          </TabsContent>
        </Tabs>
      </DialogContent>
    </Dialog>
  )
} 