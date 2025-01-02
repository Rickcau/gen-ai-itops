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

export function EndpointWarningDialog({ open, onOpenChange }: EndpointWarningDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-amber-500">
            <AlertCircle className="h-5 w-5" />
            API Endpoint Not Configured
          </DialogTitle>
          <DialogDescription>
            The REST API endpoint has not been set up yet. The application will automatically
            switch to Mock Mode to demonstrate functionality. Your request will be processed
            using mock data.
          </DialogDescription>
        </DialogHeader>
      </DialogContent>
    </Dialog>
  )
} 