export interface UserInfo {
  email: string
  firstName: string
  lastName: string
}

export interface UserData {
  userInfo: UserInfo
  role: string
  tier: string
  mockMode: boolean
  preferences: {
    theme: string
  }
} 