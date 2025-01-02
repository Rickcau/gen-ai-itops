import type { Message } from '@/types/chat'

export const mockChatResponses: { [key: string]: Message[] } = {
  'hello': [
    {
      id: '1',
      role: 'assistant',
      content: 'Hello! How can I help you with VM management today?'
    }
  ],
  'default': [
    {
      id: '1',
      role: 'assistant',
      content: 'I understand you need help with VM management. Could you please be more specific about what you\'d like to do?'
    }
  ]
}

export const mockActionResponses: { [key: string]: Message[] } = {
  'Can you list all VMs?': [
    {
      id: '1',
      role: 'assistant',
      content: 'Here are your VMs:\n\n- VM-DEV-01 (Running)\n- VM-PROD-01 (Running)\n- VM-TEST-01 (Stopped)'
    }
  ],
  'Can you start VM {vmName} in resource group {resourceGroup}?': [
    {
      id: '1',
      role: 'assistant',
      content: 'Starting the VM. This may take a few minutes...'
    },
    {
      id: '2',
      role: 'assistant',
      content: 'VM started successfully!'
    }
  ],
  'Can you stop VM {vmName} in resource group {resourceGroup}?': [
    {
      id: '1',
      role: 'assistant',
      content: 'Stopping the VM. This may take a few minutes...'
    },
    {
      id: '2',
      role: 'assistant',
      content: 'VM stopped successfully!'
    }
  ],
  'Can you restart VM {vmName} in resource group {resourceGroup}?': [
    {
      id: '1',
      role: 'assistant',
      content: 'Restarting the VM. This may take a few minutes...'
    },
    {
      id: '2',
      role: 'assistant',
      content: 'VM restarted successfully!'
    }
  ],
  'default': [
    {
      id: '1',
      role: 'assistant',
      content: 'I\'ll help you manage that VM. Please wait while I process your request...'
    },
    {
      id: '2',
      role: 'assistant',
      content: 'Operation completed successfully!'
    }
  ]
}

