import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"
import type { Capability } from "@/types/capabilities"

interface DeleteCapabilityDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  capability: Capability | null
  onConfirm: (id: string) => Promise<void>
  isDeleting: boolean
}

export function DeleteCapabilityDialog({
  open,
  onOpenChange,
  capability,
  onConfirm,
  isDeleting
}: DeleteCapabilityDialogProps) {
  if (!capability) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Capability</DialogTitle>
        </DialogHeader>

        <div className="pt-4">
          <p>Are you sure you want to delete capability <span className="font-medium">{capability.name}</span>?</p>
          <div className="mt-2 p-4 bg-red-50 dark:bg-red-900/20 rounded-md text-red-800 dark:text-red-200">
            This action cannot be undone. This will permanently delete the capability
            and remove all associated data.
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
            onClick={() => onConfirm(capability.id)}
            disabled={isDeleting}
          >
            {isDeleting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Deleting...
              </>
            ) : (
              'Delete'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 