export interface ChatApiRequest {
  sessionId: string | null;
  userId: string | null;
  prompt: string;
  chatName: string;
}

export interface ChatApiResponse {
  chatResponse: string;
  assistantResponse?: string;
  specialistResponse?: string;
  weatherResponse?: string;
} 