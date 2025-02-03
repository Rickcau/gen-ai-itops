export interface ChatApiRequest {
  sessionId: string;
  userId: string;
  prompt: string;
  chatName: string;
}

export interface ChatApiResponse {
  chatResponse: string;
  assistantResponse?: string;
  specialistResponse?: string;
  weatherResponse?: string;
  sessionId?: string;
  chatName?: string;
}