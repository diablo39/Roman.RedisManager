/**
 * API configuration module
 * Resolves the backend API base URL from environment variables
 */

const devBackendUrl = 'https://localhost:7244/'
const backendUrl = import.meta.env.VITE_BACKEND_URL?.trim()

const normalizeUrl = (url: string): string => (url.endsWith('/') ? url : `${url}/`)

// In development use local backend by default.
// In production default to same-origin deployment.
export const apiBaseUrl = backendUrl
  ? normalizeUrl(backendUrl)
  : import.meta.env.DEV
    ? devBackendUrl
    : '/'
