import { MessageRole } from '@/types/chat'
import { cn } from "@/lib/utils"

interface MessageBubbleProps {
  content: string
  role: MessageRole
}

export function MessageBubble({ content, role }: MessageBubbleProps) {
  return (
    <div
      className={cn(
        "flex flex-col rounded-lg p-4 mb-2",
        role === 'user' 
          ? "ml-auto mr-1 bg-primary text-primary-foreground max-w-[80%]"
          : role === 'assistant'
          ? "mr-auto ml-1 bg-secondary text-secondary-foreground max-w-[80%]"
          : "mr-auto ml-1 bg-accent text-accent-foreground max-w-[80%]"
      )}
    >
      {role !== 'user' && (
        <div className="font-semibold mb-2">
          {role === 'assistant' ? 'Assistant' : 'IT Specialist'}
        </div>
      )}
      <div className="whitespace-pre-wrap">{content}</div>
    </div>
  )
}

