'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ThemeToggle } from '@/components/theme-switcher'
import { ArrowLeft, Settings, Shield, Zap, User, Plus } from 'lucide-react'
import { config } from '@/lib/config'
import { CapabilityDialog } from '@/components/capability-dialog'
import type { Capability } from '@/types/capabilities'
import { nanoid } from 'nanoid'

export default function AdminPage() {
  const router = useRouter()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingCapability, setEditingCapability] = useState<Capability | undefined>()
  const [capabilities, setCapabilities] = useState<Capability[]>([
    {
      id: '1',
      name: 'Start VM',
      description: 'Start a virtual machine in Azure',
      tags: ['azure', 'vm', 'start'],
      parameters: [
        {
          name: 'vmName',
          type: 'string',
          description: 'Name of the virtual machine',
          required: true
        },
        {
          name: 'resourceGroup',
          type: 'string',
          description: 'Resource group containing the VM',
          required: true
        }
      ]
    },
    {
      id: '2',
      name: 'Stop VM',
      description: 'Stop a virtual machine in Azure',
      tags: ['azure', 'vm', 'stop'],
      parameters: [
        {
          name: 'vmName',
          type: 'string',
          description: 'Name of the virtual machine',
          required: true
        },
        {
          name: 'resourceGroup',
          type: 'string',
          description: 'Resource group containing the VM',
          required: true
        }
      ]
    }
  ])

  const handleAddCapability = (data: Omit<Capability, 'id'>) => {
    const newCapability = {
      ...data,
      id: nanoid()
    }
    setCapabilities([...capabilities, newCapability])
  }

  const handleEditCapability = (data: Omit<Capability, 'id'>) => {
    if (!editingCapability) return
    setCapabilities(capabilities.map(cap => 
      cap.id === editingCapability.id 
        ? { ...data, id: cap.id }
        : cap
    ))
    setEditingCapability(undefined)
  }

  const handleDeleteCapability = (id: string) => {
    if (confirm('Are you sure you want to delete this capability?')) {
      setCapabilities(capabilities.filter(cap => cap.id !== id))
    }
  }

  const handleEdit = (capability: Capability) => {
    setEditingCapability(capability)
    setDialogOpen(true)
  }

  return (
    <div className="container mx-auto max-w-4xl p-4 space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="icon"
            onClick={() => router.push('/')}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <h1 className="text-2xl font-bold tracking-tight">Administration</h1>
        </div>
        <div className="flex items-center gap-3">
          <ThemeToggle />
          <div className="h-4 w-px bg-border" />
          <div className="flex items-center text-sm">
            <User className="h-4 w-4 mr-1.5 text-muted-foreground" />
            <span className="text-muted-foreground">{config.testUser}</span>
          </div>
        </div>
      </div>

      {/* Capabilities Section */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <Zap className="h-5 w-5" />
            Capabilities
          </CardTitle>
          <Button 
            variant="outline" 
            className="gap-2"
            onClick={() => {
              setEditingCapability(undefined)
              setDialogOpen(true)
            }}
          >
            <Plus className="h-4 w-4" />
            Add Capability
          </Button>
        </CardHeader>
        <CardContent>
          <div className="rounded-md border">
            <div className="grid grid-cols-[1fr_2fr_1fr_1fr_auto] gap-4 p-4 border-b bg-muted/50">
              <div className="font-medium">Name</div>
              <div className="font-medium">Description</div>
              <div className="font-medium">Tags</div>
              <div className="font-medium">Parameters</div>
              <div className="w-[100px]"></div>
            </div>
            {capabilities.map((capability) => (
              <div key={capability.id} className="grid grid-cols-[1fr_2fr_1fr_1fr_auto] gap-4 p-4 border-b items-center">
                <div>{capability.name}</div>
                <div className="text-sm text-muted-foreground">{capability.description}</div>
                <div className="flex flex-wrap gap-1">
                  {capability.tags.map((tag) => (
                    <span key={tag} className="inline-flex items-center rounded-full bg-primary/10 px-2 py-0.5 text-xs font-medium text-primary-foreground">
                      {tag}
                    </span>
                  ))}
                </div>
                <div className="text-sm text-muted-foreground">
                  {capability.parameters.length} parameter{capability.parameters.length !== 1 ? 's' : ''}
                </div>
                <div className="flex items-center gap-2">
                  <Button 
                    variant="ghost" 
                    size="sm"
                    onClick={() => handleEdit(capability)}
                  >
                    Edit
                  </Button>
                  <Button 
                    variant="ghost" 
                    size="sm" 
                    className="text-destructive"
                    onClick={() => handleDeleteCapability(capability.id)}
                  >
                    Delete
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Settings Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Settings className="h-5 w-5" />
            Settings
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Button variant="outline" className="justify-start">
              Configure API Endpoints
            </Button>
            <Button variant="outline" className="justify-start">
              Manage User Preferences
            </Button>
            <Button variant="outline" className="justify-start">
              Update Notification Settings
            </Button>
            <Button variant="outline" className="justify-start">
              System Configuration
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Administration Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-5 w-5" />
            Administration
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Button variant="outline" className="justify-start">
              User Management
            </Button>
            <Button variant="outline" className="justify-start">
              Role Assignments
            </Button>
            <Button variant="outline" className="justify-start">
              Access Control
            </Button>
            <Button variant="outline" className="justify-start">
              Audit Logs
            </Button>
          </div>
        </CardContent>
      </Card>

      <CapabilityDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        onSubmit={editingCapability ? handleEditCapability : handleAddCapability}
        initialData={editingCapability}
      />
    </div>
  )
} 