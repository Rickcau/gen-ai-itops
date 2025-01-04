import { useState } from 'react'
import { Button } from "@/components/ui/button"
import { ActionParametersDialog } from "@/components/action-parameters-dialog"
import { RecentActions } from "@/components/recent-actions"
import { cn } from "@/lib/utils"
import {
  PlayCircle,
  StopCircle,
  ListChecks,
  RefreshCw,
  GitBranch,
  List,
} from "lucide-react"
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { Action, ActionData } from "@/types/actions"

interface ActionButtonsProps {
  onAction: (prompt: string) => void
}

export function ActionButtons({ onAction }: ActionButtonsProps) {
  const [selectedAction, setSelectedAction] = useState<Action | null>(null)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [recentActionsKey, setRecentActionsKey] = useState(0)

  const saveToRecents = (action: Action, data?: ActionData) => {
    const recentActions = JSON.parse(localStorage.getItem('recentActions') || '[]')
    const newAction = {
      id: action.id,
      label: action.label,
      timestamp: Date.now(),
      className: action.className,
      promptTemplate: data 
        ? action.promptTemplate
            .replace('{vmName}', data.vmName || '')
            .replace('{resourceGroup}', data.resourceGroup || '')
        : action.promptTemplate,
      ...(data && { vmName: data.vmName, resourceGroup: data.resourceGroup })
    }

    // Add to start, remove duplicates, keep only last 10
    const updatedActions = [
      newAction,
      ...recentActions.filter((a: typeof newAction) => 
        a.id !== action.id || 
        a.vmName !== newAction.vmName || 
        a.resourceGroup !== newAction.resourceGroup
      )
    ].slice(0, 10)

    localStorage.setItem('recentActions', JSON.stringify(updatedActions))
    setRecentActionsKey(prev => prev + 1) // Force RecentActions to re-render
  }

  const actions: Action[] = [
    { 
      id: 'start-vm',
      label: "Start VM", 
      value: "start", 
      className: "bg-green-500 hover:bg-green-600 dark:bg-green-600 dark:hover:bg-green-700 text-white",
      requiresParameters: true,
      icon: PlayCircle,
      tooltip: "Start a specific virtual machine",
      parameters: [
        {
          name: 'vmName',
          label: 'VM Name',
          placeholder: 'Enter VM name',
          required: true
        },
        {
          name: 'resourceGroup',
          label: 'Resource Group',
          placeholder: 'Enter resource group',
          required: true
        }
      ],
      promptTemplate: 'Can you start VM {vmName} in resource group {resourceGroup}?'
    },
    { 
      id: 'stop-vm',
      label: "Stop VM", 
      value: "shutdown", 
      className: "bg-red-500 hover:bg-red-600 dark:bg-red-600 dark:hover:bg-red-700 text-white",
      requiresParameters: true,
      icon: StopCircle,
      tooltip: "Stop a running virtual machine",
      parameters: [
        {
          name: 'vmName',
          label: 'VM Name',
          placeholder: 'Enter VM name',
          required: true
        },
        {
          name: 'resourceGroup',
          label: 'Resource Group',
          placeholder: 'Enter resource group',
          required: true
        }
      ],
      promptTemplate: 'Can you stop VM {vmName} in resource group {resourceGroup}?'
    },
    { 
      id: 'list-vms',
      label: "List VMs", 
      value: "list", 
      className: "bg-blue-500 hover:bg-blue-600 dark:bg-blue-600 dark:hover:bg-blue-700 text-white",
      requiresParameters: false,
      icon: ListChecks,
      tooltip: "Show all virtual machines in your subscription",
      promptTemplate: 'Can you list all VMs?'
    },
    { 
      id: 'restart-vm',
      label: "Restart VM", 
      value: "restart", 
      className: "bg-orange-500 hover:bg-orange-600 dark:bg-orange-600 dark:hover:bg-orange-700 text-white",
      requiresParameters: true,
      icon: RefreshCw,
      tooltip: "Restart a specific virtual machine",
      parameters: [
        {
          name: 'vmName',
          label: 'VM Name',
          placeholder: 'Enter VM name',
          required: true
        },
        {
          name: 'resourceGroup',
          label: 'Resource Group',
          placeholder: 'Enter resource group',
          required: true
        }
      ],
      promptTemplate: 'Can you restart VM {vmName} in resource group {resourceGroup}?'
    },
    { 
      id: 'list-workflows',
      label: "List Workflows", 
      value: "list-workflows", 
      className: "bg-gray-500 hover:bg-gray-600 dark:bg-gray-600 dark:hover:bg-gray-700 text-white",
      requiresParameters: false,
      icon: List,
      tooltip: "Show available GitHub workflows",
      promptTemplate: 'list workflows'
    },
    { 
      id: 'run-cicd',
      label: "Run CI/CD Pipeline", 
      value: "run-cicd", 
      className: "bg-purple-500 hover:bg-purple-600 dark:bg-purple-600 dark:hover:bg-purple-700 text-white",
      requiresParameters: false,
      icon: GitBranch,
      tooltip: "Trigger the CI/CD pipeline workflow",
      promptTemplate: 'trigger workflow CI/CD Pipeline'
    }
  ]

  const handleActionClick = (action: Action) => {
    if (action.requiresParameters) {
      setSelectedAction(action)
      setDialogOpen(true)
    } else {
      onAction(action.promptTemplate)
      saveToRecents(action)
    }
  }

  const handleParameterSubmit = (action: Action, data: ActionData) => {
    let prompt = action.promptTemplate
    if (data.vmName) {
      prompt = prompt.replace('{vmName}', data.vmName)
    }
    if (data.resourceGroup) {
      prompt = prompt.replace('{resourceGroup}', data.resourceGroup)
    }
    onAction(prompt)
    saveToRecents(action, data)
  }

  return (
    <>
      <div className="flex flex-col">
        <div className="flex flex-wrap gap-1.5 justify-center py-3 border-t">
          <TooltipProvider>
            {actions.map((action) => {
              const Icon = action.icon
              return (
                <Tooltip key={action.id}>
                  <TooltipTrigger asChild>
                    <Button
                      onClick={() => handleActionClick(action)}
                      className={cn(
                        "h-7 px-2.5 text-xs font-medium gap-1.5",
                        action.className
                      )}
                    >
                      <Icon size={14} />
                      {action.label}
                    </Button>
                  </TooltipTrigger>
                  <TooltipContent>
                    <p>{action.tooltip}</p>
                  </TooltipContent>
                </Tooltip>
              )
            })}
          </TooltipProvider>
        </div>

        <RecentActions 
          key={recentActionsKey}
          onActionClick={onAction}
          onClear={() => setRecentActionsKey(prev => prev + 1)}
        />
      </div>

      {selectedAction && (
        <ActionParametersDialog
          action={selectedAction}
          open={dialogOpen}
          onOpenChange={setDialogOpen}
          onSubmit={handleParameterSubmit}
        />
      )}
    </>
  )
}

