'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ThemeToggle } from '@/components/theme-switcher'
import { ArrowLeft, Settings, Shield, Zap, User } from 'lucide-react'
import { config } from '@/lib/config'

export default function AdminPage() {
  const router = useRouter()

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

      {/* Capabilities Section */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Zap className="h-5 w-5" />
            Capabilities
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <Button variant="outline" className="justify-start">
              Agent Configuration
            </Button>
            <Button variant="outline" className="justify-start">
              Workflow Management
            </Button>
            <Button variant="outline" className="justify-start">
              Integration Settings
            </Button>
            <Button variant="outline" className="justify-start">
              Custom Actions
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
} 