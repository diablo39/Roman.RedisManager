import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createMemoryHistory, createRouter } from 'vue-router'

import { applyAuthenticationGuard } from '@/router/authGuard'

const authStoreMock = {
  ensureSessionValid: vi.fn<() => Promise<boolean>>(),
  setReturnUrl: vi.fn<(path: string) => void>(),
  returnUrl: null as string | null,
}

vi.mock('@/stores/authentication', () => ({
  useAuthenticationStore: vi.fn(() => authStoreMock),
}))

describe('router auth guard', () => {
  beforeEach(() => {
    authStoreMock.ensureSessionValid.mockReset()
    authStoreMock.setReturnUrl.mockReset()
    authStoreMock.returnUrl = null
  })

  function createTestRouter() {
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/login', component: { template: '<div>login</div>' }, meta: { public: true } },
        {
          path: '/auth/callback',
          component: { template: '<div>callback</div>' },
          meta: { public: true },
        },
        { path: '/', component: { template: '<div>home</div>' } },
        { path: '/redis/:id', component: { template: '<div>redis</div>' } },
        { path: '/protected', component: { template: '<div>protected</div>' } },
      ],
    })

    applyAuthenticationGuard(router)

    return router
  }

  it('redirects unauthenticated users to login when navigating to protected route', async () => {
    authStoreMock.ensureSessionValid.mockResolvedValue(false)

    const router = createTestRouter()
    await router.push('/protected')

    expect(router.currentRoute.value.path).toBe('/login')
    expect(authStoreMock.setReturnUrl).toHaveBeenCalledWith('/protected')
  })

  it('allows authenticated users to open protected routes', async () => {
    authStoreMock.ensureSessionValid.mockResolvedValue(true)

    const router = createTestRouter()
    await router.push('/protected')

    expect(router.currentRoute.value.path).toBe('/protected')
  })

  it('restores returnUrl when authenticated user opens login route', async () => {
    authStoreMock.ensureSessionValid.mockResolvedValue(true)
    authStoreMock.returnUrl = '/redis/1'

    const router = createTestRouter()
    await router.push('/login?returnUrl=/redis/1')

    expect(router.currentRoute.value.path).toBe('/redis/1')
  })

  it('redirects to login after session expiration on protected navigation', async () => {
    authStoreMock.ensureSessionValid.mockResolvedValue(false)

    const router = createTestRouter()
    await router.push('/protected')

    expect(router.currentRoute.value.path).toBe('/login')
    expect(authStoreMock.setReturnUrl).toHaveBeenCalledWith('/protected')
  })
})
