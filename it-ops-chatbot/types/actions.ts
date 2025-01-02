import { LucideIcon } from "lucide-react"

export interface ActionParameter {
  name: string;
  label: string;
  placeholder: string;
  required: boolean;
}

export interface Action {
  id: string;
  label: string;
  value: string;
  className: string;
  requiresParameters: boolean;
  icon: LucideIcon;
  tooltip: string;
  parameters?: ActionParameter[];
  promptTemplate: string;
}

export interface ActionData {
  vmName?: string;
  resourceGroup?: string;
  [key: string]: string | undefined;
} 