import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"

interface DeleteSessionDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: (sessionId: string) => Promise<void>
  isDeleting: boolean
  sessionId: string | null
}

export function DeleteSessionDialog({
  open,
  onOpenChange,
  onConfirm,
  isDeleting,
  sessionId
}: DeleteSessionDialogProps) {
  if (!sessionId) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Session</DialogTitle>
        </DialogHeader>

        <div className="pt-4">
          <p>Are you sure you want to delete this session?</p>
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
            onClick={() => onConfirm(sessionId)}
            disabled={isDeleting}
          >
            {isDeleting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Deleting...
              </>
            ) : (
              'Delete Session'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
