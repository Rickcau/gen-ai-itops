# IT Operations Chat Bot

A modern web interface for managing Azure VMs through natural language interactions. Built with Next.js 14, TypeScript, and Tailwind CSS.  This frontend is designed to be deployed to Azure App Service and can be modified to be used as a frontend for any GenAI ChatBot with modifications.

[![Next.js CI](https://github.com/Rickcau/it-ops-chat-bot/actions/workflows/nextjs.yml/badge.svg)](https://github.com/Rickcau/it-ops-chat-bot/actions/workflows/nextjs.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- 🤖 Natural language VM management
- 💻 Pre-built actions for common VM operations
- 🌙 Dark/Light theme support
- 💾 Recent actions history
- 🔄 Mock mode for development
- 🎯 Parameter validation for VM operations
- 🚀 Responsive design

## Tech Stack

- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **UI Components**: Shadcn UI
- **State Management**: React Hooks
- **Development Mode**: HTTP/HTTPS options

## Getting Started

### Prerequisites

- Node.js 18+ 
- npm or yarn
- Azure backend API (or use mock mode for development)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/Rickcau/it-ops-chat-bot.git
cd it-ops-chat-bot
```

2. Install dependencies:
```bash
npm install
```

3. Create a `.env.local` file:
```env
NEXT_PUBLIC_API_BASE_URL=https://localhost:7049
NEXT_PUBLIC_API_CONFIGURED=true
NEXT_PUBLIC_TEST_USER=testuser@myapp.com
NEXT_PUBLIC_API_KEY=1234
```

### Development

Start the development server:
```bash
npm run dev
```

You'll be prompted to choose:
1. HTTP (Port 3000)
2. HTTPS (Port 3443)

### Building for Production

```bash
npm run build
npm run start
```

## Environment Variables

- `NEXT_PUBLIC_API_BASE_URL`: Backend API URL
- `NEXT_PUBLIC_API_CONFIGURED`: Enable/disable mock mode
- `NEXT_PUBLIC_TEST_USER`: Test user email
- `NEXT_PUBLIC_API_KEY`: API authentication key

## Available VM Operations

- 🟢 Start VM
- 🔴 Stop VM
- 📋 List VMs
- 🔄 Restart VM

Each operation requiring parameters (VM name, resource group) will prompt for input.

## Mock Mode

Enable mock mode to test the interface without a backend:
- Toggle available in the UI
- Pre-configured responses
- No API connection required

## Deployment

The application is designed to be deployed to Azure App Service:
- Builds with `npm run build`
- Runs with `npm run start`
- Supports Azure Easy Auth
- Environment variables configured in App Service Configuration

## Project Structure

```
app/                  # Next.js app router pages
components/          # React components
  ├─ ui/            # Shadcn UI components
  └─ ...            # Custom components
lib/                # Utilities and configuration
types/              # TypeScript type definitions
```

## Contributing

1. Fork the repository
2. Create your feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details. 
