import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"

interface ActionButtonsProps {
  onAction: (action: string) => void
}

export function ActionButtons({ onAction }: ActionButtonsProps) {
  const actions = [
    { 
      label: "Shut Down VMs", 
      value: "shutdown", 
      className: "bg-red-500 hover:bg-red-600 dark:bg-red-600 dark:hover:bg-red-700 text-white"
    },
    { 
      label: "Start VMs", 
      value: "start", 
      className: "bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700 text-white"
    },
    { 
      label: "List VMs", 
      value: "list", 
      className: "bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700 text-white"
    },
    { 
      label: "Restart VMs", 
      value: "restart", 
      className: "bg-orange-500 hover:bg-orange-600 dark:bg-orange-600 dark:hover:bg-orange-700 text-white"
    },
    // Add more actions here later
  ]

  return (
    <div className="flex flex-wrap gap-1.5 justify-center py-3 border-t">
      {actions.map((action) => (
        <Button
          key={action.value}
          onClick={() => onAction(action.value)}
          className={cn(
            "h-7 px-2.5 text-xs font-medium",
            action.className
          )}
        >
          {action.label}
        </Button>
      ))}
    </div>
  )
}

