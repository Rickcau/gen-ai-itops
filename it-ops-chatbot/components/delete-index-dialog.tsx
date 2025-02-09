import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"

interface DeleteIndexDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  onConfirm: (indexName: string) => void
  isDeleting: boolean
}

export function DeleteIndexDialog({ 
  open, 
  onOpenChange, 
  indexName,
  onConfirm,
  isDeleting
}: DeleteIndexDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Index</DialogTitle>
          <DialogDescription>
            Are you sure you want to delete the index &quot;{indexName}&quot;? This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end gap-2">
          <Button 
            variant="outline" 
            onClick={() => onOpenChange(false)}
            disabled={isDeleting}
          >
            Cancel
          </Button>
          <Button 
            variant="destructive" 
            onClick={() => onConfirm(indexName)}
            disabled={isDeleting}
          >
            {isDeleting ? 'Deleting...' : 'Delete Index'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 