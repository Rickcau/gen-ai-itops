import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
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
        </DialogHeader>

        <div className="pt-4">
          <p className="text-muted-foreground">
            This action will permanently delete all data from the system. This includes all users, sessions, capabilities, and indexes.
            To confirm, please type &quot;WIPE&quot; in the input field below.
          </p>
        </div>

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