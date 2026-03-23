/**
 * Authentication route guard
 *
 * Protects every route except the public login flow.
 * Logic per task T009:
 *   1. Call ensureSessionValid() once and cache the result.
 *   2. If visiting /login while already authenticated → redirect to returnUrl.
 *   3. Otherwise, if path is public → allow through.
 *   4. If path is protected and session is valid → allow through.
 *   5. If path is protected and session is invalid → save destination, redirect to /login.
 */

import type { Router } from 'vue-router'
import { useAuthenticationStore } from '@/stores/authentication'
import { DEFAULT_AUTHENTICATED_PATH, isPublicPath, LOGIN_PATH, sanitizeReturnUrl } from './auth'

export function applyAuthenticationGuard (router: Router): void {
  router.beforeEach(async to => {
    const authStore = useAuthenticationStore()
    const sessionValid = await authStore.ensureSessionValid()

    // Already signed in and trying to visit /login → redirect away
    if (
      (isPublicPath(to.path) || to.meta['public'] === true)
      && to.path === LOGIN_PATH
      && sessionValid
    ) {
      return authStore.returnUrl ?? DEFAULT_AUTHENTICATED_PATH
    }

    // Public path (callback or login while not signed in) → allow through
    if (isPublicPath(to.path) || to.meta['public'] === true) {
      return true
    }

    // Protected path with a valid session → allow through
    if (sessionValid) {
      return true
    }

    // Protected path without a valid session → save destination and go to login
    authStore.setReturnUrl(sanitizeReturnUrl(to.fullPath))
    return { path: LOGIN_PATH, query: { returnUrl: to.fullPath } }
  })
}
