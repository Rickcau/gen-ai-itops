export interface ExecutionMethod {
  type: string
  details: string
}

export interface Parameter {
  name: string
  type: string
  description: string
}

export interface Capability {
  id: string
  capabilityType: string
  name: string
  description: string
  tags: string[]
  parameters: Parameter[]
  executionMethod: ExecutionMethod
} 