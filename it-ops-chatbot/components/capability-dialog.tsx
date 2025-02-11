'use client'

import { useState } from 'react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Plus, X } from 'lucide-react'
import type { Capability, Parameter } from '@/types/capabilities'
import { nanoid } from 'nanoid'

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
  // Generate a unique ID for new capabilities
  const generatedId = `capability-${nanoid(4)}`
  
  const [name, setName] = useState(initialData?.name || '')
  const [description, setDescription] = useState(initialData?.description || '')
  const [capabilityType, setCapabilityType] = useState(initialData?.capabilityType || 'Runbook')
  const [tags, setTags] = useState<string[]>(initialData?.tags || [])
  const [parameters, setParameters] = useState<Parameter[]>(initialData?.parameters || [])
  const [executionMethod, setExecutionMethod] = useState(initialData?.executionMethod || {
    type: 'Azure Automation',
    details: 'Execute via Azure Automation Runbook'
  })
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
        description: ''
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
      capabilityType,
      tags,
      parameters,
      executionMethod
    })
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {initialData ? 'Edit Capability' : 'Add New Capability'}
          </DialogTitle>
        </DialogHeader>
        
        <div className="space-y-6 py-4">
          {/* ID Section (Read-only) */}
          <div className="grid gap-2">
            <Label className="text-muted-foreground">ID (read-only)</Label>
            <div className="p-2 rounded-md bg-muted/50 font-mono text-sm">
              {initialData?.id || generatedId}
            </div>
          </div>

          {/* Capability Type */}
          <div className="space-y-2">
            <Label htmlFor="capabilityType">Type</Label>
            <Input
              id="capabilityType"
              value={capabilityType}
              onChange={(e) => setCapabilityType(e.target.value)}
              placeholder="Enter capability type"
              required
            />
          </div>
          
          {/* Name and Description */}
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="name">Name</Label>
              <Input
                id="name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="Enter capability name"
                required
              />
            </div>
            
            <div className="space-y-2">
              <Label htmlFor="description">Description</Label>
              <Textarea
                id="description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Enter capability description"
                required
                className="min-h-[100px]"
              />
            </div>
          </div>

          {/* Tags */}
          <div className="space-y-2">
            <Label>Tags</Label>
            <div className="flex flex-wrap gap-2 mb-2">
              {tags.map((tag, index) => {
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
                      onClick={() => handleRemoveTag(tag)}
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
                <div key={index} className="grid grid-cols-[1fr_1fr_2fr_auto] gap-4 items-start">
                  <div className="space-y-2">
                    <Label>Name</Label>
                    <Input
                      value={param.name}
                      onChange={(e) => handleUpdateParameter(index, { name: e.target.value })}
                      placeholder="Parameter name"
                    />
                  </div>
                  <div className="space-y-2">
                    <Label>Type</Label>
                    <Input
                      value={param.type}
                      onChange={(e) => handleUpdateParameter(index, { type: e.target.value })}
                      placeholder="Parameter type"
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

          {/* Execution Method */}
          <div className="space-y-2">
            <Label>Execution Method</Label>
            <div className="space-y-2">
              <Input
                placeholder="Type"
                value={executionMethod.type}
                onChange={(e) => setExecutionMethod({
                  ...executionMethod,
                  type: e.target.value
                })}
                required
              />
              <Input
                placeholder="Details"
                value={executionMethod.details}
                onChange={(e) => setExecutionMethod({
                  ...executionMethod,
                  details: e.target.value
                })}
                required
              />
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