import type { Message } from '@/types/chat'

export const mockChatResponses: Record<string, Message[]> = {
  'list workflows': [
    {
      id: 'mock-1',
      role: 'assistant',
      content: 'I am forwarding the request to the IT Specialist for handling.'
    },
    {
      id: 'mock-2',
      role: 'specialist',
      content: `Available workflows in rickcau/gen-ai-itops:
- Name: CI/CD Pipeline
  ID: 12345
  State: active
  Path: .github/workflows/ci-cd.yml`
    }
  ],
  'default': [
    {
      id: 'mock-1',
      role: 'assistant',
      content: 'I am forwarding the request to the IT Specialist for handling.'
    },
    {
      id: 'mock-2',
      role: 'specialist',
      content: 'I understand you need help with IT operations. Could you please provide more details about what you would like to do?'
    }
  ]
}

export const mockActionResponses: Record<string, Message[]> = {
  'list workflows': [
    {
      id: 'mock-1',
      role: 'assistant',
      content: 'I am forwarding the request to the IT Specialist for handling.'
    },
    {
      id: 'mock-2',
      role: 'specialist',
      content: `Available workflows in rickcau/gen-ai-itops:
- Name: CI/CD Pipeline
  ID: 12345
  State: active
  Path: .github/workflows/ci-cd.yml`
    }
  ],
  'default': [
    {
      id: 'mock-1',
      role: 'assistant',
      content: 'I am forwarding the request to the IT Specialist for handling.'
    },
    {
      id: 'mock-2',
      role: 'specialist',
      content: 'I understand you need help with IT operations. Could you please provide more details about what you would like to do?'
    }
  ]
}

