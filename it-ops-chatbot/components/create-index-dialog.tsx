import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useState } from "react"

interface CreateIndexDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (indexName: string) => void
}

export function CreateIndexDialog({ 
  open, 
  onOpenChange, 
  onSubmit 
}: CreateIndexDialogProps) {
  const [indexName, setIndexName] = useState('')

  const handleSubmit = () => {
    if (indexName.trim()) {
      onSubmit(indexName.trim())
      setIndexName('')
      onOpenChange(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Create New Index</DialogTitle>
        </DialogHeader>
        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="name">Name</Label>
            <Input
              id="name"
              value={indexName}
              onChange={(e) => setIndexName(e.target.value)}
              placeholder="Enter index name"
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault()
                  handleSubmit()
                }
              }}
            />
          </div>
        </div>
        <div className="flex justify-end gap-2">
          <Button 
            variant="outline" 
            onClick={() => {
              setIndexName('')
              onOpenChange(false)
            }}
          >
            Cancel
          </Button>
          <Button onClick={handleSubmit} disabled={!indexName.trim()}>
            Add Index
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 