import type { Router } from 'vue-router'
import pinia from '@/stores'
import { useAuthenticationStore } from '@/stores/authentication'
import { DEFAULT_AUTHENTICATED_PATH, isPublicPath, LOGIN_PATH, sanitizeReturnUrl } from './auth'

export function applyAuthenticationGuard(router: Router) {
  router.beforeEach(async to => {
    const authStore = useAuthenticationStore(pinia)

    if (isPublicPath(to.path) || to.meta.public === true) {
      if (to.path === LOGIN_PATH) {
        const isSessionValid = await authStore.ensureSessionValid()
        if (isSessionValid) {
          const requestedTarget =
            typeof to.query.returnUrl === 'string' ? to.query.returnUrl : authStore.returnUrl
          return sanitizeReturnUrl(requestedTarget)
        }
      }

      return true
    }

    const isSessionValid = await authStore.ensureSessionValid()
    if (isSessionValid) {
      return true
    }

    const requestedPath = sanitizeReturnUrl(to.fullPath)
    authStore.setReturnUrl(requestedPath)

    if (requestedPath === DEFAULT_AUTHENTICATED_PATH) {
      return { path: LOGIN_PATH }
    }

    return {
      path: LOGIN_PATH,
      query: {
        returnUrl: requestedPath,
      },
    }
  })
}
