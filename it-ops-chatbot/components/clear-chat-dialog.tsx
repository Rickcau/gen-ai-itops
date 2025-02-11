import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { AlertCircle } from "lucide-react"
import { Button } from "@/components/ui/button"

interface ClearChatDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
}

export function ClearChatDialog({
  open,
  onOpenChange,
  onConfirm,
}: ClearChatDialogProps) {
  const descriptionId = "clear-chat-dialog-description"

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent aria-describedby={descriptionId}>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-amber-500">
            <AlertCircle className="h-5 w-5" />
            Clear Chat History?
          </DialogTitle>
          <DialogDescription id={descriptionId}>
            Are you sure you want to clear all messages from this chat?
            This action cannot be undone.
          </DialogDescription>
        </DialogHeader>
        <div className="flex justify-end gap-2">
          <Button 
            variant="outline" 
            onClick={() => onOpenChange(false)}
          >
            Cancel
          </Button>
          <Button 
            variant="destructive" 
            onClick={() => {
              onConfirm()
              onOpenChange(false)
            }}
          >
            Clear Chat
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
