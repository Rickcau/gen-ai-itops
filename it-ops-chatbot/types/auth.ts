export interface UserInfo {
  email: string;
  firstName?: string;
  lastName?: string;
}

export interface AuthState {
  user: UserInfo | null;
  isLoading: boolean;
  error: string | null;
  isAuthenticated: boolean;
}

export type AuthContextType = {
  authState: AuthState;
  login: () => Promise<void>;
  logout: () => Promise<void>;
} 
