import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Button } from "@/components/ui/button"
import type { Capability } from "@/types/capabilities"
import { useState, useEffect } from "react"
import { Loader2, Pencil, Plus, X } from "lucide-react"

interface ViewCapabilityDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  capability: Capability | null
  onSubmit?: (data: Capability) => Promise<void>
}

export function ViewCapabilityDialog({
  open,
  onOpenChange,
  capability,
  onSubmit
}: ViewCapabilityDialogProps) {
  const [isEditMode, setIsEditMode] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formData, setFormData] = useState<Capability | null>(capability)
  const [newTag, setNewTag] = useState("")
  const [newParamName, setNewParamName] = useState("")
  const [newParamType, setNewParamType] = useState("")
  const [newParamDescription, setNewParamDescription] = useState("")

  useEffect(() => {
    setFormData(capability)
    setIsEditMode(false)
  }, [capability])

  if (!formData) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!onSubmit) return

    setIsSubmitting(true)
    try {
      await onSubmit(formData)
      setIsEditMode(false)
    } catch (error) {
      console.error('Error updating capability:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setFormData(capability)
    setIsEditMode(false)
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
          <DialogTitle>
            {isEditMode ? 'Edit Capability' : 'View Capability'}
            {!isEditMode && onSubmit && (
              <Button
                variant="outline"
                size="sm"
                className="absolute right-12 top-6"
                onClick={() => setIsEditMode(true)}
              >
                <Pencil className="h-4 w-4 mr-2" />
                Edit
              </Button>
            )}
          </DialogTitle>
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
            {isEditMode ? (
              <Input
                id="name"
                value={formData.name}
                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                required
              />
            ) : (
              <div className="p-2 rounded-md bg-muted/50">
                {formData.name}
              </div>
            )}
          </div>

          {/* Description Section */}
          <div className="grid gap-2">
            <Label htmlFor="description">Description</Label>
            {isEditMode ? (
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                required
                className="min-h-[100px]"
              />
            ) : (
              <div className="p-2 rounded-md bg-muted/50 whitespace-pre-wrap">
                {formData.description}
              </div>
            )}
          </div>

          {/* Type Section */}
          <div className="grid gap-2">
            <Label htmlFor="type">Type</Label>
            {isEditMode ? (
              <Input
                id="type"
                value={formData.capabilityType}
                onChange={(e) => setFormData({ ...formData, capabilityType: e.target.value })}
                required
              />
            ) : (
              <div className="p-2 rounded-md bg-muted/50">
                {formData.capabilityType}
              </div>
            )}
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
                    {isEditMode && (
                      <button
                        type="button"
                        onClick={() => removeTag(tag)}
                        className="ml-1 hover:text-destructive"
                      >
                        <X className="h-3 w-3" />
                      </button>
                    )}
                  </span>
                );
              })}
            </div>
            {isEditMode && (
              <div className="flex gap-2">
                <Input
                  placeholder="Add new tag"
                  value={newTag}
                  onChange={(e) => setNewTag(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      addTag()
                    }
                  }}
                />
                <Button type="button" variant="outline" onClick={addTag}>
                  Add
                </Button>
              </div>
            )}
          </div>

          {/* Parameters Section */}
          <div className="grid gap-2">
            <div className="flex justify-between items-center">
              <Label>Parameters</Label>
              {isEditMode && (
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={addParameter}
                  className="gap-1"
                >
                  <Plus className="h-4 w-4" />
                  Add Parameter
                </Button>
              )}
            </div>
            <div className="space-y-2">
              {formData.parameters.map((param, index) => (
                <div
                  key={index}
                  className="p-2 rounded-md bg-muted/50 grid grid-cols-[1fr_1fr_2fr_auto] gap-2 items-center"
                >
                  {isEditMode ? (
                    <>
                      <Input
                        value={param.name}
                        onChange={(e) => {
                          const updatedParams = [...formData.parameters]
                          updatedParams[index] = { ...param, name: e.target.value }
                          setFormData({ ...formData, parameters: updatedParams })
                        }}
                        placeholder="Parameter name"
                      />
                      <Input
                        value={param.type}
                        onChange={(e) => {
                          const updatedParams = [...formData.parameters]
                          updatedParams[index] = { ...param, type: e.target.value }
                          setFormData({ ...formData, parameters: updatedParams })
                        }}
                        placeholder="Parameter type"
                      />
                      <Input
                        value={param.description}
                        onChange={(e) => {
                          const updatedParams = [...formData.parameters]
                          updatedParams[index] = { ...param, description: e.target.value }
                          setFormData({ ...formData, parameters: updatedParams })
                        }}
                        placeholder="Parameter description"
                      />
                      <Button
                        type="button"
                        variant="ghost"
                        size="sm"
                        className="text-destructive"
                        onClick={() => removeParameter(index)}
                      >
                        <X className="h-4 w-4" />
                      </Button>
                    </>
                  ) : (
                    <>
                      <span className="text-sm font-medium">{param.name}</span>
                      <span className="text-sm text-muted-foreground">
                        Type: {param.type}
                      </span>
                      <span className="text-sm text-muted-foreground">
                        {param.description}
                      </span>
                    </>
                  )}
                </div>
              ))}
              {isEditMode && formData.parameters.length === 0 && (
                <div className="text-center text-muted-foreground py-4">
                  No parameters defined. Click &quot;Add Parameter&quot; to add one.
                </div>
              )}
            </div>
          </div>

          {/* Execution Method Section */}
          <div className="grid gap-2">
            <Label>Execution Method</Label>
            {isEditMode ? (
              <div className="space-y-2">
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
            ) : (
              <div className="space-y-2">
                <div className="p-2 rounded-md bg-muted/50">
                  Type: {formData.executionMethod.type}
                </div>
                <div className="p-2 rounded-md bg-muted/50">
                  Details: {formData.executionMethod.details}
                </div>
              </div>
            )}
          </div>

          {/* Action Buttons */}
          <div className="flex justify-end gap-2">
            {isEditMode ? (
              <>
                <Button
                  type="button"
                  variant="outline"
                  onClick={handleCancel}
                  disabled={isSubmitting}
                >
                  Cancel
                </Button>
                <Button
                  type="submit"
                  disabled={isSubmitting}
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      Saving...
                    </>
                  ) : (
                    'Save Changes'
                  )}
                </Button>
              </>
            ) : (
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Close
              </Button>
            )}
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
} 