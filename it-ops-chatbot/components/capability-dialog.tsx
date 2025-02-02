'use client'

import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Plus, X } from 'lucide-react'
import type { Capability, Parameter } from '@/types/capabilities'

interface CapabilityDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (capability: Omit<Capability, 'id'>) => void
  initialData?: Capability
}

export function CapabilityDialog({ 
  open, 
  onOpenChange, 
  onSubmit,
  initialData 
}: CapabilityDialogProps) {
  const [name, setName] = useState(initialData?.name || '')
  const [description, setDescription] = useState(initialData?.description || '')
  const [tags, setTags] = useState<string[]>(initialData?.tags || [])
  const [parameters, setParameters] = useState<Parameter[]>(initialData?.parameters || [])
  const [currentTag, setCurrentTag] = useState('')

  const handleAddTag = () => {
    if (currentTag.trim() && !tags.includes(currentTag.trim())) {
      setTags([...tags, currentTag.trim()])
      setCurrentTag('')
    }
  }

  const handleRemoveTag = (tagToRemove: string) => {
    setTags(tags.filter(tag => tag !== tagToRemove))
  }

  const handleAddParameter = () => {
    setParameters([
      ...parameters,
      {
        name: '',
        type: 'string',
        description: '',
        required: false
      }
    ])
  }

  const handleRemoveParameter = (index: number) => {
    setParameters(parameters.filter((_, i) => i !== index))
  }

  const handleUpdateParameter = (index: number, updates: Partial<Parameter>) => {
    setParameters(parameters.map((param, i) => 
      i === index ? { ...param, ...updates } : param
    ))
  }

  const handleSubmit = () => {
    onSubmit({
      name,
      description,
      tags,
      parameters
    })
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>
            {initialData ? 'Edit Capability' : 'Add New Capability'}
          </DialogTitle>
        </DialogHeader>
        
        <div className="space-y-6 py-4">
          {/* Name and Description */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter capability name"
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Enter capability description"
              />
            </div>
          </div>

          {/* Tags */}
          <div className="space-y-2">
            <Label>Tags</Label>
            <div className="flex gap-2">
              <Input
                value={currentTag}
                onChange={(e) => setCurrentTag(e.target.value)}
                placeholder="Enter a tag"
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    e.preventDefault()
                    handleAddTag()
                  }
                }}
              />
              <Button 
                type="button" 
                variant="outline" 
                onClick={handleAddTag}
              >
                Add
              </Button>
            </div>
            <div className="flex flex-wrap gap-2 mt-2">
              {tags.map((tag) => (
                <span 
                  key={tag} 
                  className="inline-flex items-center gap-1 rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary-foreground"
                >
                  {tag}
                  <button
                    onClick={() => handleRemoveTag(tag)}
                    className="rounded-full p-0.5 hover:bg-primary/20"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </span>
              ))}
            </div>
          </div>

          {/* Parameters */}
          <div className="space-y-2">
            <div className="flex justify-between items-center">
              <Label>Parameters</Label>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleAddParameter}
                className="gap-1"
              >
                <Plus className="h-4 w-4" />
                Add Parameter
              </Button>
            </div>
            <div className="space-y-4">
              {parameters.map((param, index) => (
                <div key={index} className="grid grid-cols-[1fr_1fr_auto] gap-4 items-start">
                  <div className="space-y-2">
                    <Label>Name</Label>
                    <Input
                      value={param.name}
                      onChange={(e) => handleUpdateParameter(index, { name: e.target.value })}
                      placeholder="Parameter name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Description</Label>
                    <Input
                      value={param.description}
                      onChange={(e) => handleUpdateParameter(index, { description: e.target.value })}
                      placeholder="Parameter description"
                    />
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="icon"
                    className="mt-8"
                    onClick={() => handleRemoveParameter(index)}
                  >
                    <X className="h-4 w-4" />
                  </Button>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="flex justify-end gap-2">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button onClick={handleSubmit}>
            {initialData ? 'Save Changes' : 'Add Capability'}
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
} 