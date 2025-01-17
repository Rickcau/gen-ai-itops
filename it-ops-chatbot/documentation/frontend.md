# Frontend Documentation

## Overview
The IT Operations Assistant is a modern web application built with Next.js 13+, providing a chat-based interface for managing IT operations tasks. The application features a responsive design, real-time chat functionality, and supports both light and dark themes.

## Technical Stack

### Core Technologies
- **Framework**: Next.js 13+ (App Router)
- **Language**: TypeScript
- **UI Library**: React
- **Styling**: TailwindCSS with Shadcn UI components

### Key Libraries
- `nanoid`: Unique ID generation
- `lucide-react`: Icon components
- `tailwindcss`: Utility-first CSS framework
- `shadcn/ui`: Pre-built accessible UI components

## Architecture

### Component Structure
```
components/
├── ui/               # Base UI components (buttons, inputs, etc.)
├── action-buttons    # Pre-defined action buttons
├── message-bubble    # Chat message display
├── theme-switcher    # Dark/light mode toggle
└── endpoint-warning  # API endpoint warning dialog
```

### State Management
The application uses React's built-in state management solutions:
- **Local State**: `useState` hooks for component-level state
- **Effects**: `useEffect` for side effects and initialization
- **No Global State**: Given the application's scope, local state management is sufficient

### Key State Objects
```typescript
interface ChatState {
  messages: Message[]
  isLoading: boolean
}

interface Message {
  id: string
  content: string
  role: 'user' | 'assistant' | 'specialist'
}
```

## Features

### Chat Interface
- Real-time message display
- Support for multiple message types (user, assistant, specialist)
- Loading states with "Processing..." indicator
- Message history preservation
- Clear chat functionality

### Action System
- Pre-defined action buttons for common operations
- Parameter dialogs for actions requiring input
- Recent actions history
- Mock mode for development and testing

### Theme Support
- Light/Dark mode toggle
- System theme detection
- Persistent theme preference

## Components

### ChatInterface (Main Component)
- **Purpose**: Main application container
- **Location**: `app/page.tsx`
- **Responsibilities**:
  - Message handling
  - API communication
  - State management
  - UI layout

### ActionButtons
- **Purpose**: Provides quick access to common operations
- **Features**:
  - Customizable action buttons
  - Parameter collection dialogs
  - Recent actions tracking
  - Tooltips for action descriptions

### MessageBubble
- **Purpose**: Displays individual chat messages
- **Features**:
  - Role-based styling
  - Content formatting
  - Accessibility support

## State Management Details

### Message Flow
1. User input (text or action button)
2. State update with user message
3. API call with loading state
4. Response processing
5. State update with API response

### State Update Pattern
```typescript
setChatState((prev: ChatState) => ({
  isLoading: false,
  messages: [
    ...prev.messages,
    ...(data.assistantResponse ? [{...}] : []),
    ...(data.specialistResponse ? [{...}] : [])
  ]
}))
```

## API Integration

### Request Format
```typescript
interface ChatApiRequest {
  sessionId: string
  userId: string
  prompt: string
}
```

### Response Format
```typescript
interface ChatApiResponse {
  chatResponse: string
  assistantResponse?: string
  specialistResponse?: string
}
```

## Development Mode

### Mock Mode
- Toggle for API bypass
- Pre-defined mock responses
- Simulated API delay
- Error handling simulation

### Configuration
- Environment-based API endpoints
- Mock data configuration
- User settings
- API key management

## Best Practices

### State Updates
- Use functional updates to prevent stale state
- Preserve message history during updates
- Handle loading states appropriately
- Implement proper error boundaries

### UI/UX
- Responsive design
- Keyboard accessibility
- Loading indicators
- Error feedback
- Clear user feedback

### Performance
- Efficient state updates
- Optimized re-renders
- Proper error handling
- Responsive UI

## Future Improvements
1. Implement message persistence
2. Add message search functionality
3. Enhance error handling
4. Add user preferences
5. Implement message threading 