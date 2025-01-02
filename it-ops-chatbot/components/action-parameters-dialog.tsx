import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Action, ActionData } from '@/types/actions'

interface ActionParametersDialogProps {
  action: Action;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (action: Action, data: ActionData) => void;
}

export function ActionParametersDialog({ 
  action, 
  open, 
  onOpenChange,
  onSubmit 
}: ActionParametersDialogProps) {
  const [formData, setFormData] = useState<ActionData>({})

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSubmit(action, formData)
    onOpenChange(false)
    setFormData({}) // Reset form
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{action.label} Parameters</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4 mt-4">
          {action.parameters?.map((param) => (
            <div key={param.name} className="space-y-2">
              <Label htmlFor={param.name}>{param.label}</Label>
              <Input
                id={param.name}
                placeholder={param.placeholder}
                value={formData[param.name] || ''}
                onChange={(e) => 
                  setFormData(prev => ({
                    ...prev,
                    [param.name]: e.target.value
                  }))
                }
                required={param.required}
              />
            </div>
          ))}
          <div className="flex justify-end gap-2">
            <Button 
              type="button" 
              variant="outline" 
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit">Submit</Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
} 