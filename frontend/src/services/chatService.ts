// Helper function to add authorization header
import { authService } from './authService';
import { getApiBaseUrl } from '../utils/apiConfig';
import { handleResponseError } from '../utils/errorHandler';

const getAuthHeaders = () => {
  const token = authService.getToken();
  const headers: HeadersInit = {};
  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }
  return headers;
};

export interface ChatAskRequest {
  question: string;
}

export interface ChatResponse {
  answer: string;
  intent: string;
  suggestions?: string[];
}

export interface PublicChatResponse {
  answer: string;
  intent: string;
  suggestions?: string[];
  triggerSupportFlow?: boolean;
}

export interface CreateSupportRequestResult {
  id: number;
  email: string;
  subject: string;
}

export const chatService = {
  ask: async (question: string): Promise<ChatResponse> => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/chat/ask`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...getAuthHeaders(),
      },
      body: JSON.stringify({ question }),
    });

    if (!response.ok) {
      await handleResponseError(response, 'Sohbet isteği başarısız oldu');
    }

    const data = await response.json();
    return {
      answer: data.answer,
      intent: data.intent,
      suggestions: data.suggestions,
    };
  },

  askPublic: async (question: string): Promise<PublicChatResponse> => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/public-chat/ask`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ question }),
    });

    if (!response.ok) {
      await handleResponseError(response, 'Sohbet isteği başarısız oldu');
    }

    return response.json();
  },

  createSupportRequest: async (
    email: string,
    subject: string,
    detectedIntent?: string
  ): Promise<CreateSupportRequestResult> => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/public-chat/support-request`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, subject, detectedIntent }),
    });

    if (!response.ok) {
      await handleResponseError(response, 'Destek talebi oluşturulamadı');
    }

    return response.json();
  },
};



