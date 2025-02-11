import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog"
import { AlertCircle } from "lucide-react"

interface EndpointWarningDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export function EndpointWarningDialog({
  open,
  onOpenChange,
}: EndpointWarningDialogProps) {
  const descriptionId = "endpoint-warning-dialog-description"

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent aria-describedby={descriptionId}>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-amber-500">
            <AlertCircle className="h-5 w-5" />
            Warning: Missing API Endpoint
          </DialogTitle>
          <DialogDescription id={descriptionId}>
            The API endpoint is not configured. Please check your environment variables and ensure the API is running.
          </DialogDescription>
        </DialogHeader>
      </DialogContent>
    </Dialog>
  )
} 