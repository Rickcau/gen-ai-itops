import { cn } from "@/lib/utils"

interface MessageBubbleProps {
  content: string
  role: 'user' | 'assistant' | 'specialist' | 'weather'
}

export function MessageBubble({ content, role }: MessageBubbleProps) {
  const bubbleStyle = role === 'user' 
    ? "ml-auto mr-1 bg-primary text-primary-foreground"
    : role === 'assistant'
    ? "mr-auto ml-1 bg-secondary text-secondary-foreground"
    : role === 'weather'
    ? "mr-auto ml-1 bg-amber-100 text-amber-900"
    : "mr-auto ml-1 bg-accent text-accent-foreground"

  return (
    <div
      className={cn(
        "flex flex-col rounded-lg p-4 mb-2 max-w-[80%]",
        bubbleStyle
      )}
    >
      {role !== 'user' && (
        <div className="font-semibold mb-2">
          {role === 'assistant' 
            ? 'Assistant' 
            : role === 'weather'
            ? 'Weather Agent'
            : 'IT Specialist'}
        </div>
      )}
      <div className="whitespace-pre-wrap">{content}</div>
    </div>
  )
}

