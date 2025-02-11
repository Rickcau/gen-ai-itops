import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { useState } from "react"
import type { UserData } from "@/types/user"

interface CreateUserDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSubmit: (data: UserData) => Promise<void>
}

export function CreateUserDialog({
  open,
  onOpenChange,
  onSubmit
}: CreateUserDialogProps) {
  const [formData, setFormData] = useState({
    userInfo: {
      email: "",
      firstName: "",
      lastName: ""
    },
    role: "admin",
    tier: "pro",
    mockMode: false,
    preferences: {
      theme: "light"
    }
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    onSubmit(formData)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>Create New User</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="grid gap-6 py-4">
          {/* User Info Section */}
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                value={formData.userInfo.email}
                onChange={(e) => setFormData({
                  ...formData,
                  userInfo: { ...formData.userInfo, email: e.target.value }
                })}
                required
              />
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label htmlFor="firstName">First Name</Label>
                <Input
                  id="firstName"
                  value={formData.userInfo.firstName}
                  onChange={(e) => setFormData({
                    ...formData,
                    userInfo: { ...formData.userInfo, firstName: e.target.value }
                  })}
                  required
                />
              </div>
              <div className="grid gap-2">
                <Label htmlFor="lastName">Last Name</Label>
                <Input
                  id="lastName"
                  value={formData.userInfo.lastName}
                  onChange={(e) => setFormData({
                    ...formData,
                    userInfo: { ...formData.userInfo, lastName: e.target.value }
                  })}
                  required
                />
              </div>
            </div>
          </div>

          {/* Role and Tier Section */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label htmlFor="role">Role</Label>
              <select
                id="role"
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={formData.role}
                onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                required
              >
                <option value="admin">Admin</option>
                <option value="user">User</option>
                <option value="readonly">Read Only</option>
                <option value="support">Support</option>
              </select>
            </div>
            <div className="grid gap-2">
              <Label htmlFor="tier">Tier</Label>
              <select
                id="tier"
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                value={formData.tier}
                onChange={(e) => setFormData({ ...formData, tier: e.target.value })}
                required
              >
                <option value="pro">Pro</option>
                <option value="trial">Trial</option>
                <option value="simple">Simple</option>
              </select>
            </div>
          </div>

          {/* Mock Mode Section */}
          <div className="flex items-center space-x-2">
            <Checkbox
              id="mockMode"
              checked={formData.mockMode}
              onCheckedChange={(checked) => 
                setFormData({ ...formData, mockMode: checked as boolean })
              }
            />
            <Label htmlFor="mockMode">Mock Mode</Label>
          </div>

          {/* Theme Preference */}
          <div className="grid gap-2">
            <Label htmlFor="theme">Theme Preference</Label>
            <select
              id="theme"
              className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
              value={formData.preferences.theme}
              onChange={(e) => setFormData({
                ...formData,
                preferences: { ...formData.preferences, theme: e.target.value }
              })}
              required
            >
              <option value="light">Light</option>
              <option value="dark">Dark</option>
            </select>
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
              Create User
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  )
} 