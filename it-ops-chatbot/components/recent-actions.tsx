import { useState, useEffect } from 'react'
import { Button } from "@/components/ui/button"
import { Clock, X } from "lucide-react"
import { cn } from "@/lib/utils"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"

interface RecentAction {
  id: string
  label: string
  timestamp: number
  vmName?: string
  resourceGroup?: string
  className: string
  promptTemplate: string
}

interface RecentActionsProps {
  onActionClick: (prompt: string) => void
  onClear?: () => void
}

export function RecentActions({ onActionClick, onClear }: RecentActionsProps) {
  const [recentActions, setRecentActions] = useState<RecentAction[]>([])

  useEffect(() => {
    const stored = localStorage.getItem('recentActions')
    const actions = stored ? JSON.parse(stored) as RecentAction[] : []
    setRecentActions(actions)
  }, [])

  if (recentActions.length === 0) return null

  const sortedActions = recentActions.sort((a, b) => b.timestamp - a.timestamp).slice(0, 5)

  const handleClear = () => {
    localStorage.removeItem('recentActions')
    setRecentActions([])
    onClear?.()
  }

  return (
    <div className="flex flex-col gap-2 px-4 py-2 border-t">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Clock className="h-3 w-3" />
          <span>Recent Actions</span>
        </div>
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="h-5 w-5"
                onClick={handleClear}
              >
                <X className="h-3 w-3" />
              </Button>
            </TooltipTrigger>
            <TooltipContent>
              <p>Clear recent actions</p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      </div>
      <div className="flex flex-wrap gap-1.5">
        {sortedActions.map((action) => {
          const label = action.vmName 
            ? `${action.label}: ${action.vmName}`
            : action.label

          return (
            <Button
              key={`${action.id}-${action.timestamp}`}
              onClick={() => onActionClick(action.promptTemplate)}
              className={cn(
                "h-6 px-2 text-xs font-medium",
                action.className
              )}
              variant="ghost"
            >
              {label}
            </Button>
          )
        })}
      </div>
    </div>
  )
} 