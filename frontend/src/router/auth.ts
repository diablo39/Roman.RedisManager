export const LOGIN_PATH = '/login'
export const AUTH_CALLBACK_PATH = '/auth/callback'
export const DEFAULT_AUTHENTICATED_PATH = '/'

const PUBLIC_PATHS = new Set([LOGIN_PATH, AUTH_CALLBACK_PATH])

export function isPublicPath(path: string): boolean {
  return PUBLIC_PATHS.has(path)
}

export function sanitizeReturnUrl(returnUrl: string | null | undefined): string {
  if (!returnUrl || !returnUrl.startsWith('/')) {
    return DEFAULT_AUTHENTICATED_PATH
  }

  if (isPublicPath(returnUrl)) {
    return DEFAULT_AUTHENTICATED_PATH
  }

  return returnUrl
}
