interface UpdateUserDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  userData: UserData | null
  onSubmit: (data: UserData) => Promise<void>
} 