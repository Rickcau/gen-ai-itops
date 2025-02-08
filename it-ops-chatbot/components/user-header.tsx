"use client";

import { User } from 'lucide-react';
import { useAuth } from '@/components/providers/auth-provider';
import { ThemeToggle } from '@/components/theme-switcher';

export function UserHeader() {
  const { authState } = useAuth();

  return (
    <div className="flex items-center gap-2">
      <ThemeToggle />
      <div className="h-4 w-px bg-border mx-2" />
      <div className="flex items-center gap-2 text-sm text-muted-foreground">
        <User className="h-4 w-4" />
        {authState.user?.email}
      </div>
    </div>
  );
} 

