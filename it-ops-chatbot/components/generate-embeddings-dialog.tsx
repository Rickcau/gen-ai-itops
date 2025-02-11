import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
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
        </DialogHeader>

        <div className="pt-4">
          <p>Are you sure you want to generate embeddings for index <span className="font-medium">{indexName}</span>?</p>
        </div>

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