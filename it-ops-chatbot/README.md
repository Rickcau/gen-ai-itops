# it-ops-chat-bot
THis is the Frontend for the backend API, it is written in NextJS.

## App Structure (NextJS 14 App Router)
![frontend app structure](./images/frontend-app-structure.jpg)

## Loading Flow
- When you visit the app, NextJS loads app/layout.tsx first (this sets up the theme provider)
- Then loads app/page.tsx which contains the main chat interface
- The chat interface initializes with:
   - Empty message list
   - Action buttons at the bottom
   - Input field for typing messages

## Action Flow (when you click a VM action button)
    User clicks "Start VM" →
    ↓
    ActionButtons component checks if parameters needed
    ↓
    Opens parameter dialogh (ActionParametersDialog)
    ↓
    User enters VM Name & resource group
    ↓
    Submits form →
    ↓
    Saves to recent action (localStorage)
    ↓
    Sends request to API
    ↓
    Shows response in chat

## Recent Actions Flow:
   Action completed →
   ↓
   Saves to localStorage
   ↓
   RecentActions component reads localStorage
   ↓
   Shows last 5 actions below main buttons

## API Communication:
- In development/mock mode:
    Action → Mock response from mockData.ts

- In produciton:
    Action → 
        ↓
        Creates API payload with:
        - sessionId (generated on page load)
        - userId (testuser@myapp.com)
        - prompt (e.g., "Can you start VM my-vm in resource group my-rg?")
        ↓
        Sends POST request to API with:
        - Headers: api-key, Content-Type: application/json
        - Body: payload
        ↓
        API responds with:
        { "chatResponse": "Starting VM my-vm..." }
        ↓
        Response converted to Message format and added to chat

## State Management:

- Chat messages stored in React state
- Recent actions stored in localStorage
- Theme preference stored in localStorage
- Session ID generated on page load

## Key Features
- Dark/Light theme switching
- Mock mode for testing
- Persistent action history
- Parameter validation
- Tooltips for button descriptions
- Error handling with fallbacks

## Important Notes
This is a modern NextJS app using the new app router (introduced in NextJS 13/14), which is quite different from the older pages router. The main differences are:
- Server components by default
- More intuitive routing structure
- Better performance optimization
- Simplified data fetching