import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"

interface GenerateEmbeddingsDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  isGenerating: boolean
  onConfirm: (indexName: string) => Promise<void>
}

export function GenerateEmbeddingsDialog({ 
  open, 
  onOpenChange, 
  indexName,
  isGenerating,
  onConfirm 
}: GenerateEmbeddingsDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Generate Embeddings</DialogTitle>
          <DialogDescription className="pt-4">
            You are about to generate embeddings for index <span className="font-medium">{indexName}</span>.
            <div className="mt-2 p-4 bg-yellow-50 dark:bg-yellow-900/20 rounded-md text-yellow-800 dark:text-yellow-200">
              This will generate embeddings for all Capabilities and add documents to your vector store. 
              This process may take a few minutes depending on the amount of data.
            </div>
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end gap-2">
          <Button 
            variant="outline" 
            onClick={() => onOpenChange(false)}
            disabled={isGenerating}
          >
            Cancel
          </Button>
          <Button 
            variant="default"
            onClick={() => onConfirm(indexName)}
            disabled={isGenerating}
          >
            {isGenerating ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Generating...
              </>
            ) : (
              'Generate'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 