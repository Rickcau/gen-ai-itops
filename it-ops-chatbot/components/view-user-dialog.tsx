import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Label } from "@/components/ui/label"
import { Button } from "@/components/ui/button"

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
}

export function ViewUserDialog({
  open,
  onOpenChange,
  userData
}: ViewUserDialogProps) {
  if (!userData) return null

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>User Details</DialogTitle>
        </DialogHeader>

        <div className="grid gap-6 py-4">
          {/* User Info Section */}
          <div className="grid gap-4">
            <div className="grid gap-2">
              <Label>Email</Label>
              <div className="p-2 rounded-md bg-muted/50">
                {userData.userInfo.email}
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div className="grid gap-2">
                <Label>First Name</Label>
                <div className="p-2 rounded-md bg-muted/50">
                  {userData.userInfo.firstName}
                </div>
              </div>
              <div className="grid gap-2">
                <Label>Last Name</Label>
                <div className="p-2 rounded-md bg-muted/50">
                  {userData.userInfo.lastName}
                </div>
              </div>
            </div>
          </div>

          {/* Role and Tier Section */}
          <div className="grid grid-cols-2 gap-4">
            <div className="grid gap-2">
              <Label>Role</Label>
              <div className="p-2 rounded-md bg-muted/50">
                {userData.role}
              </div>
            </div>
            <div className="grid gap-2">
              <Label>Tier</Label>
              <div className="p-2 rounded-md bg-muted/50">
                {userData.tier}
              </div>
            </div>
          </div>

          {/* Mock Mode Section */}
          <div className="grid gap-2">
            <Label>Mock Mode</Label>
            <div className="p-2 rounded-md bg-muted/50">
              {userData.mockMode ? "Enabled" : "Disabled"}
            </div>
          </div>

          {/* Theme Preference */}
          <div className="grid gap-2">
            <Label>Theme Preference</Label>
            <div className="p-2 rounded-md bg-muted/50">
              {userData.preferences.theme}
            </div>
          </div>

          {/* Close Button */}
          <div className="flex justify-end">
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Close
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
} 