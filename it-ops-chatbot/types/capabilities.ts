export interface Capability {
  id: string;
  name: string;
  description: string;
  tags: string[];
  parameters: Parameter[];
}

export interface Parameter {
  name: string;
  type: 'string' | 'number' | 'boolean';
  description: string;
  required: boolean;
  defaultValue?: string | number | boolean;
} 