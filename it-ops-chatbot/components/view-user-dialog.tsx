import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Checkbox } from "@/components/ui/checkbox"
import { useState, useEffect } from "react"
import { Loader2, Pencil, X } from "lucide-react"

interface ViewUserDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  userData: {
    userInfo: {
      email: string
      firstName: string
      lastName: string
    }
    role: string
    tier: string
    mockMode: boolean
    preferences: {
      theme: string
    }
  } | null
  onSubmit?: (data: any) => Promise<void>
}

export function ViewUserDialog({
  open,
  onOpenChange,
  userData,
  onSubmit
}: ViewUserDialogProps) {
  const [isEditMode, setIsEditMode] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formData, setFormData] = useState(userData)

  useEffect(() => {
    setFormData(userData)
    setIsEditMode(false)
  }, [userData])

  if (!formData) return null

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!onSubmit) return

    setIsSubmitting(true)
    try {
      await onSubmit(formData)
      setIsEditMode(false)
    } catch (error) {
      console.error('Error updating user:', error)
    } finally {
      setIsSubmitting(false)
    }
  }

  const handleCancel = () => {
    setFormData(userData)
    setIsEditMode(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <div className="flex items-center justify-between pr-8">
            <DialogTitle>User Details</DialogTitle>
            {!isEditMode && onSubmit && (
              <Button
                variant="outline"
                size="sm"
                className="gap-2"
                onClick={() => setIsEditMode(true)}
              >
                <Pencil className="h-4 w-4" />
                Edit
              </Button>
            )}
          </div>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="grid gap-6 py-4">
          {/* User Info Section */}
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label>Email</Label>
              {isEditMode ? (
                <Input
                  type="email"
                  value={formData.userInfo.email}
                  onChange={(e) => setFormData({
                    ...formData,
                    userInfo: { ...formData.userInfo, email: e.target.value }
                  })}
                  required
                />
              ) : (
                <div className="p-2 rounded-md bg-muted/50">
                  {formData.userInfo.email}
                </div>
              )}
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label>First Name</Label>
                {isEditMode ? (
                  <Input
                    value={formData.userInfo.firstName}
                    onChange={(e) => setFormData({
                      ...formData,
                      userInfo: { ...formData.userInfo, firstName: e.target.value }
                    })}
                    required
                  />
                ) : (
                  <div className="p-2 rounded-md bg-muted/50">
                    {formData.userInfo.firstName}
                  </div>
                )}
              </div>
              <div className="grid gap-2">
                <Label>Last Name</Label>
                {isEditMode ? (
                  <Input
                    value={formData.userInfo.lastName}
                    onChange={(e) => setFormData({
                      ...formData,
                      userInfo: { ...formData.userInfo, lastName: e.target.value }
                    })}
                    required
                  />
                ) : (
                  <div className="p-2 rounded-md bg-muted/50">
                    {formData.userInfo.lastName}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Role and Tier Section */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label>Role</Label>
              {isEditMode ? (
                <select
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
              ) : (
                <div className="p-2 rounded-md bg-muted/50">
                  {formData.role}
                </div>
              )}
            </div>
            <div className="grid gap-2">
              <Label>Tier</Label>
              {isEditMode ? (
                <select
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                  value={formData.tier}
                  onChange={(e) => setFormData({ ...formData, tier: e.target.value })}
                  required
                >
                  <option value="pro">Pro</option>
                  <option value="trial">Trial</option>
                  <option value="simple">Simple</option>
                </select>
              ) : (
                <div className="p-2 rounded-md bg-muted/50">
                  {formData.tier}
                </div>
              )}
            </div>
          </div>

          {/* Mock Mode Section */}
          <div className="grid gap-2">
            <Label>Mock Mode</Label>
            {isEditMode ? (
              <div className="flex items-center space-x-2">
                <Checkbox
                  id="mockMode"
                  checked={formData.mockMode}
                  onCheckedChange={(checked) => 
                    setFormData({ ...formData, mockMode: checked as boolean })
                  }
                />
                <Label htmlFor="mockMode">Enable Mock Mode</Label>
              </div>
            ) : (
              <div className="p-2 rounded-md bg-muted/50">
                {formData.mockMode ? "Enabled" : "Disabled"}
              </div>
            )}
          </div>

          {/* Theme Preference */}
          <div className="grid gap-2">
            <Label>Theme Preference</Label>
            {isEditMode ? (
              <select
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
            ) : (
              <div className="p-2 rounded-md bg-muted/50">
                {formData.preferences.theme}
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