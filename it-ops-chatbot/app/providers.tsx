'use client'

import { ThemeProvider as NextThemesProvider } from "next-themes"
import { type ReactNode } from "react"

interface ThemeProviderProps {
  children: ReactNode;
  forcedTheme?: string;
  theme?: string;
  attribute?: string;
  defaultTheme?: string;
  enableSystem?: boolean;
  disableTransitionOnChange?: boolean;
  storageKey?: string;
}

export function ThemeProvider({ children, ...props }: ThemeProviderProps) {
  return (
    <NextThemesProvider 
      attribute="class"
      defaultTheme="light"
      disableTransitionOnChange
      {...props}
    >
      {children}
    </NextThemesProvider>
  )
}

