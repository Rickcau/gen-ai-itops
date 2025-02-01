"use client";

import { useAuth } from '@/components/providers/auth-provider';
import { ThemeToggle } from '@/components/theme-switcher';

export function UserHeader() {
  const { authState } = useAuth();

  return (
    <div className="flex items-center gap-4">
      <ThemeToggle />
      <div className="text-sm">
        {authState.user?.email}
      </div>
    </div>
  );
} 