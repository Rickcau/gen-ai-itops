import { cn } from "@/lib/utils"

interface MessageBubbleProps {
  content: string
  role: 'user' | 'assistant' | 'specialist' | 'weather'
}

export function MessageBubble({ content, role }: MessageBubbleProps) {
  const bubbleClass = {
    user: 'bg-blue-100 text-blue-900 ml-auto',
    assistant: 'bg-green-500 text-white',
    specialist: 'bg-purple-500 text-white',
    weather: 'bg-sky-500 text-white'
  }[role]

  const titleClass = {
    user: 'text-blue-700',
    assistant: 'text-green-100',
    specialist: 'text-purple-100',
    weather: 'text-sky-100'
  }[role]

  const title = {
    user: 'You',
    assistant: 'Assistant',
    specialist: 'IT Specialist',
    weather: 'Weather'
  }[role]

  return (
    <div className={`rounded-lg p-4 max-w-[80%] space-y-1 ${bubbleClass}`}>
      {role !== 'user' && (
        <div className={`text-sm font-medium ${titleClass}`}>{title}</div>
      )}
      <div className="whitespace-pre-wrap">{content}</div>
    </div>
  )
}
