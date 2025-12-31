import { showToast } from '../components/Toast';

/**
 * Handles HTTP response errors and shows appropriate toast messages
 * @param response - The fetch Response object
 * @param defaultMessage - Default error message if response doesn't contain one
 * @throws Error with appropriate message
 */
export async function handleResponseError(response: Response, defaultMessage: string = 'Bir hata oluştu'): Promise<never> {
  // Handle 403 Forbidden - show permission error
  if (response.status === 403) {
    showToast('Bu işlemi yapmanız için yetkiniz yoktur.', 'error');
    throw new Error('Bu işlemi yapmanız için yetkiniz yoktur.');
  }

  // Try to get error message from response
  let errorMessage = defaultMessage;
  try {
    const errorData = await response.json().catch(() => ({}));
    errorMessage = errorData.message || errorData.detail || errorData.title || defaultMessage;
  } catch {
    // If JSON parsing fails, use default message
  }

  throw new Error(errorMessage);
}

