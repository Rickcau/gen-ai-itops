import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import type { Capability } from "@/types/capabilities"
import { useState } from "react"
import { X } from "lucide-react"

interface UpdateCapabilityDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  capability: Capability | undefined
  onSubmit: (data: Capability) => void
}

export function UpdateCapabilityDialog({
  open,
  onOpenChange,
  capability,
  onSubmit
}: UpdateCapabilityDialogProps) {
  const [formData, setFormData] = useState<Capability | null>(capability)
  const [newTag, setNewTag] = useState("")
  const [newParamName, setNewParamName] = useState("")
  const [newParamType, setNewParamType] = useState("")
  const [newParamDescription, setNewParamDescription] = useState("")

  if (!capability) return null

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSubmit(formData)
  }

  const addTag = () => {
    if (newTag.trim() && !formData.tags.includes(newTag.trim())) {
      setFormData({
        ...formData,
        tags: [...formData.tags, newTag.trim()]
      })
      setNewTag("")
    }
  }

  const removeTag = (tagToRemove: string) => {
    setFormData({
      ...formData,
      tags: formData.tags.filter(tag => tag !== tagToRemove)
    })
  }

  const addParameter = () => {
    if (newParamName.trim() && newParamType.trim()) {
      setFormData({
        ...formData,
        parameters: [...formData.parameters, {
          name: newParamName.trim(),
          type: newParamType.trim(),
          description: newParamDescription.trim()
        }]
      })
      setNewParamName("")
      setNewParamType("")
      setNewParamDescription("")
    }
  }

  const removeParameter = (index: number) => {
    setFormData({
      ...formData,
      parameters: formData.parameters.filter((_, i) => i !== index)
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Update Capability</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="grid gap-6 py-4">
          {/* ID Section (Read-only) */}
          <div className="grid gap-2">
            <Label className="text-muted-foreground">ID (read-only)</Label>
            <div className="p-2 rounded-md bg-muted/50 font-mono text-sm">
              {formData.id}
            </div>
          </div>

          {/* Name Section */}
          <div className="grid gap-2">
            <Label htmlFor="name">Name</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              required
            />
          </div>

          {/* Description Section */}
          <div className="grid gap-2">
            <Label htmlFor="description">Description</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => setFormData({ ...formData, description: e.target.value })}
              required
              className="min-h-[100px]"
            />
          </div>

          {/* Type Section */}
          <div className="grid gap-2">
            <Label htmlFor="type">Type</Label>
            <Input
              id="type"
              value={formData.capabilityType}
              onChange={(e) => setFormData({ ...formData, capabilityType: e.target.value })}
              required
            />
          </div>

          {/* Tags Section */}
          <div className="grid gap-2">
            <Label>Tags</Label>
            <div className="flex flex-wrap gap-2 mb-2">
              {formData.tags.map((tag, index) => {
                const colorClasses = [
                  "bg-blue-500/10 text-blue-700 dark:text-blue-300",
                  "bg-purple-500/10 text-purple-700 dark:text-purple-300",
                  "bg-green-500/10 text-green-700 dark:text-green-300",
                  "bg-orange-500/10 text-orange-700 dark:text-orange-300",
                  "bg-pink-500/10 text-pink-700 dark:text-pink-300"
                ];
                const colorClass = colorClasses[index % colorClasses.length];
                
                return (
                  <span
                    key={tag}
                    className={`inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium ${colorClass}`}
                  >
                    {tag}
                    <button
                      type="button"
                      onClick={() => removeTag(tag)}
                      className="ml-1 hover:text-destructive"
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </span>
                );
              })}
            </div>
            <div className="flex gap-2">
              <Input
                placeholder="Add new tag"
                value={newTag}
                onChange={(e) => setNewTag(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && (e.preventDefault(), addTag())}
              />
              <Button type="button" variant="outline" onClick={addTag}>
                Add
              </Button>
            </div>
          </div>

          {/* Parameters Section */}
          <div className="grid gap-2">
            <Label>Parameters</Label>
            <div className="space-y-2 mb-2">
              {formData.parameters.map((param, index) => (
                <div
                  key={index}
                  className="p-2 rounded-md bg-muted/50 grid grid-cols-[1fr_1fr_2fr_auto] gap-2 items-center"
                >
                  <span className="text-sm font-medium">{param.name}</span>
                  <span className="text-sm text-muted-foreground">
                    Type: {param.type}
                  </span>
                  <span className="text-sm text-muted-foreground">
                    {param.description}
                  </span>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="text-destructive"
                    onClick={() => removeParameter(index)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
            <div className="grid grid-cols-[1fr_1fr_2fr_auto] gap-2">
              <Input
                placeholder="Parameter name"
                value={newParamName}
                onChange={(e) => setNewParamName(e.target.value)}
              />
              <Input
                placeholder="Parameter type"
                value={newParamType}
                onChange={(e) => setNewParamType(e.target.value)}
              />
              <Input
                placeholder="Parameter description"
                value={newParamDescription}
                onChange={(e) => setNewParamDescription(e.target.value)}
              />
              <Button type="button" variant="outline" onClick={addParameter}>
                Add
              </Button>
            </div>
          </div>

          {/* Execution Method Section */}
          <div className="grid gap-2">
            <Label>Execution Method</Label>
            <div className="grid gap-2">
              <Input
                placeholder="Type"
                value={formData.executionMethod.type}
                onChange={(e) => setFormData({
                  ...formData,
                  executionMethod: {
                    ...formData.executionMethod,
                    type: e.target.value
                  }
                })}
                required
              />
              <Input
                placeholder="Details"
                value={formData.executionMethod.details}
                onChange={(e) => setFormData({
                  ...formData,
                  executionMethod: {
                    ...formData.executionMethod,
                    details: e.target.value
                  }
                })}
                required
              />
            </div>
          </div>

          {/* Submit Button */}
          <div className="flex justify-end gap-2">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Cancel
            </Button>
            <Button type="submit">
              Update Capability
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
} 