'use client'

import { useState } from 'react'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Input } from '@/components/ui/input'
import { Search, PlayCircle, AlertCircle } from 'lucide-react'

interface RunbookExecution {
  jobId: string
  runbookName: string
  status: 'running' | 'completed' | 'failed'
  timestamp: string
}

interface RunbookSidebarProps {
  sessionId: string
  executions: RunbookExecution[]
  onExecutionClick: (runbookName: string, jobId: string) => void
}

export function RunbookSidebar({ sessionId, executions, onExecutionClick }: RunbookSidebarProps) {
  const [isOpen, setIsOpen] = useState(false)
  const [searchQuery, setSearchQuery] = useState('')

  const filteredExecutions = executions
    .filter((execution) => 
      execution.runbookName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      execution.jobId.toLowerCase().includes(searchQuery.toLowerCase())
    )
    .sort((a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime())

  const getStatusIcon = (status: RunbookExecution['status']) => {
    switch (status) {
      case 'running':
        return <PlayCircle className="h-4 w-4 text-blue-500 animate-pulse" />
      case 'completed':
        return <PlayCircle className="h-4 w-4 text-green-500" />
      case 'failed':
        return <AlertCircle className="h-4 w-4 text-red-500" />
    }
  }

  return (
    <>
      {/* Hover trigger area */}
      <div
        className="fixed right-0 top-0 w-24 h-full z-40 bg-gradient-to-l from-transparent to-transparent"
        onMouseEnter={() => setIsOpen(true)}
      />

      {/* Sidebar */}
      <div
        className={`fixed right-0 top-4 bottom-4 bg-background border border-border rounded-l-xl w-80 transform transition-transform duration-300 ease-in-out z-50 ${
          isOpen ? 'translate-x-0' : 'translate-x-full'
        }`}
        onMouseLeave={() => setIsOpen(false)}
      >
        <div className="flex flex-col h-full">
          {/* Header */}
          <div className="p-4 border-b border-border">
            <h2 className="font-semibold">Runbook Executions</h2>
            <div className="text-sm text-muted-foreground">Session: {sessionId.slice(0, 8)}</div>
          </div>

          {/* Search */}
          <div className="p-4">
            <div className="relative">
              <Search className="absolute left-3 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search executions..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9 bg-muted/50"
              />
            </div>
          </div>

          {/* Executions List */}
          <ScrollArea className="flex-1">
            <div className="p-4 space-y-2">
              {filteredExecutions.length === 0 ? (
                <div className="text-center py-4 text-muted-foreground">
                  No runbook executions found
                </div>
              ) : (
                filteredExecutions.map((execution) => (
                  <div
                    key={execution.jobId}
                    className="group relative flex items-center space-x-2 rounded-md border border-border p-3 hover:bg-muted/50 cursor-pointer"
                    onClick={() => onExecutionClick(execution.runbookName, execution.jobId)}
                    role="button"
                    tabIndex={0}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        onExecutionClick(execution.runbookName, execution.jobId);
                      }
                    }}
                  >
                    {getStatusIcon(execution.status)}
                    <div className="flex-1 overflow-hidden">
                      <div className="font-medium truncate">{execution.runbookName}</div>
                      <div className="text-xs text-muted-foreground">
                        Job ID: {execution.jobId}
                      </div>
                      <div className="text-xs text-muted-foreground">
                        {new Date(execution.timestamp).toLocaleString()}
                      </div>
                    </div>
                  </div>
                ))
              )}
            </div>
          </ScrollArea>
        </div>
      </div>
    </>
  )
} 