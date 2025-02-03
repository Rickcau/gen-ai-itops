import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"

interface DeleteSessionDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (data: { removeFromStorage: boolean }) => void;
}

export function DeleteSessionDialog({ 
  open, 
  onOpenChange,
  onSubmit 
}: DeleteSessionDialogProps) {
  const [removeFromStorage, setRemoveFromStorage] = useState(false)

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSubmit({ removeFromStorage })
    onOpenChange(false)
    setRemoveFromStorage(false) // Reset state
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Delete Chat Session</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4 mt-4">
          <div className="flex items-center space-x-2">
            <Checkbox 
              id="removeFromStorage"
              checked={removeFromStorage}
              onCheckedChange={(checked) => setRemoveFromStorage(checked as boolean)}
            />
            <Label htmlFor="removeFromStorage">Remove from storage</Label>
          </div>
          <div className="flex justify-end space-x-2">
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => {
                onOpenChange(false)
                setRemoveFromStorage(false) // Reset state on cancel
              }}
            >
              Cancel
            </Button>
            <Button type="submit" variant="destructive">
              Delete
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
}
