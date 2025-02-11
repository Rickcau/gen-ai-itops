import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"

interface DeleteIndexDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  indexName: string
  onConfirm: (indexName: string) => Promise<void>
  isDeleting: boolean
}

export function DeleteIndexDialog({ 
  open, 
  onOpenChange, 
  indexName,
  onConfirm,
  isDeleting
}: DeleteIndexDialogProps) {
  if (!indexName) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Index</DialogTitle>
        </DialogHeader>

        <div className="pt-4">
          <p>Are you sure you want to delete the index <span className="font-medium">{indexName}</span>?</p>
          <div className="mt-2 p-4 bg-red-50 dark:bg-red-900/20 rounded-md text-red-800 dark:text-red-200">
            This action cannot be undone.
          </div>
        </div>

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
            {isDeleting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Deleting...
              </>
            ) : (
              'Delete Index'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 