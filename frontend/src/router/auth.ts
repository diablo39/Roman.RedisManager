/**
 * Router authentication constants and helpers
 *
 * Centralises the public-path list and return-URL sanitisation so that
 * both the route guard and the login page share the same rules.
 */

export const LOGIN_PATH = '/login'
export const CALLBACK_PATH = '/auth/callback'
export const DEFAULT_AUTHENTICATED_PATH = '/'

/**
 * Returns true for paths that do not require authentication.
 * Every other path in the app is protected by default.
 */
export function isPublicPath (path: string): boolean {
  return path === LOGIN_PATH || path === CALLBACK_PATH
}

/**
 * Makes sure the return destination is a safe, in-app path.
 *
 * Returns '/' when:
 * - url is null or empty
 * - url looks like an external URL (starts with http/https or //)
 * - url is the login or callback path (prevents redirect loops)
 * - url is already the default landing page
 */
export function sanitizeReturnUrl (url: string | null | undefined): string {
  if (!url) {
    return DEFAULT_AUTHENTICATED_PATH
  }

  // Reject external URLs
  if (/^https?:\/\//i.test(url) || url.startsWith('//')) {
    return DEFAULT_AUTHENTICATED_PATH
  }

  // Reject login-flow paths (would cause redirect loops)
  if (
    url === LOGIN_PATH
    || url.startsWith(`${LOGIN_PATH}?`)
    || url === CALLBACK_PATH
    || url.startsWith(`${CALLBACK_PATH}?`)
  ) {
    return DEFAULT_AUTHENTICATED_PATH
  }

  return url
}
