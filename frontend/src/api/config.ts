/**
 * API configuration module
 * Resolves the backend API base URL from environment variables
 */

// Read from Vite environment variables (VITE_ prefix required)
const backendUrl = import.meta.env.VITE_BACKEND_URL

// Default to root if not set, ensure trailing slash
export const apiBaseUrl = backendUrl
  ? backendUrl.endsWith('/')
    ? backendUrl
    : `${backendUrl}/`
  : '/'
