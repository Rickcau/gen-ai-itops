import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"
import { Loader2 } from "lucide-react"

interface SystemWipeDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onConfirm: (systemWipeKey: string) => Promise<void>
  isWiping: boolean
}

export function SystemWipeDialog({
  open,
  onOpenChange,
  onConfirm,
  isWiping
}: SystemWipeDialogProps) {
  const [systemWipeKey, setSystemWipeKey] = useState("")

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>System Wipe</DialogTitle>
          <DialogDescription className="pt-4">
            You are about to delete ALL sessions and their messages from the system.
            <div className="mt-2 p-4 bg-red-50 dark:bg-red-900/20 rounded-md text-red-800 dark:text-red-200">
              This is a destructive operation that cannot be undone.
              Please make sure you have the correct system wipe key before proceeding.
            </div>
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="systemWipeKey">System Wipe Key</Label>
            <Input
              id="systemWipeKey"
              type="password"
              value={systemWipeKey}
              onChange={(e) => setSystemWipeKey(e.target.value)}
              placeholder="Enter system wipe key"
              className="font-mono"
            />
          </div>
        </div>

        <div className="flex justify-end gap-2">
          <Button
            variant="outline"
            onClick={() => {
              onOpenChange(false)
              setSystemWipeKey("")
            }}
            disabled={isWiping}
          >
            Cancel
          </Button>
          <Button
            variant="destructive"
            onClick={() => onConfirm(systemWipeKey)}
            disabled={isWiping || !systemWipeKey.trim()}
          >
            {isWiping ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Wiping System...
              </>
            ) : (
              'Confirm Wipe'
            )}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 