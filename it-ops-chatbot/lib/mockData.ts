import { Message } from '@/types/chat'

export const mockChatResponses: { [key: string]: Message[] } = {
  default: [
    {
      id: '1',
      role: 'assistant',
      content: "I'm here to help with IT operations. You can ask me about managing VMs, checking system status, or use the quick action buttons below."
    }
  ],
  "help": [
    {
      id: '2',
      role: 'assistant',
      content: "I can help you with the following tasks:\n- Starting/stopping VMs\n- Checking VM status\n- Restarting VMs\n- Listing available VMs\n\nYou can either type your question or use the quick action buttons below."
    }
  ]
}

export const mockActionResponses: { [key: string]: Message[] } = {
  shutdown: [
    {
      id: '3',
      role: 'assistant',
      content: "I'll help you shut down the VMs. Connecting to IT Specialist..."
    },
    {
      id: '4',
      role: 'specialist',
      content: "VM shutdown process initiated. JobID: 789012. The VMs will be stopped safely."
    }
  ],
  start: [
    {
      id: '5',
      role: 'assistant',
      content: "I'll start the VMs for you. Connecting to IT Specialist..."
    },
    {
      id: '6',
      role: 'specialist',
      content: "VM start process initiated. JobID: 345678. The VMs will be online shortly."
    }
  ],
  list: [
    {
      id: '7',
      role: 'specialist',
      content: "Here are your VMs:\n1. VM-PROD-01 (Running)\n2. VM-PROD-02 (Stopped)\n3. VM-DEV-01 (Running)\n4. VM-TEST-01 (Running)"
    }
  ],
  restart: [
    {
      id: '8',
      role: 'assistant',
      content: "I'll restart the VMs for you. Connecting to IT Specialist..."
    },
    {
      id: '9',
      role: 'specialist',
      content: "VM restart process initiated. JobID: 901234. This will take a few minutes to complete safely."
    }
  ]
}

