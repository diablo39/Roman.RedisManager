/**
 * API configuration module
 * Resolves the backend API base URL from environment variables
 */

const backendUrl = import.meta.env.VITE_BACKEND_URL?.trim()

const normalizeUrl = (url: string): string => (url.endsWith('/') ? url : `${url}/`)

// In development, force relative API paths so Vite proxy (`/api`) handles backend routing
// and the browser avoids direct cross-origin calls.
// In production, allow explicit backend override, otherwise default to same-origin deployment.
export const apiBaseUrl = import.meta.env.DEV ? '/' : backendUrl ? normalizeUrl(backendUrl) : '/'
