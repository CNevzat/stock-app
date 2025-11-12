const getApiBaseUrl = () => {
  if (import.meta.env.VITE_API_BASE_URL) {
    return import.meta.env.VITE_API_BASE_URL;
  }
  if (import.meta.env.PROD) {
    return `http://${window.location.hostname}:5134`;
  }
  return 'http://localhost:5134';
};

export interface ChatAskRequest {
  question: string;
}

export interface ChatResponse {
  answer: string;
  intent: string;
  suggestions?: string[];
}

export const chatService = {
  ask: async (question: string): Promise<ChatResponse> => {
    const API_BASE_URL = getApiBaseUrl();
    const response = await fetch(`${API_BASE_URL}/api/chat/ask`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ question }),
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({}));
      throw new Error(error.message || 'Sohbet isteği başarısız oldu.');
    }

    const data = await response.json();
    return {
      answer: data.answer,
      intent: data.intent,
      suggestions: data.suggestions,
    };
  },
};


