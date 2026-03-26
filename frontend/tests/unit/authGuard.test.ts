/**
 * T024 — Unit tests for src/router/authGuard.ts and src/router/auth.ts
 *
 * Tests US2 acceptance scenarios:
 * - Public paths are allowed through without a valid session
 * - Protected paths redirect to /login when session is invalid
 * - /login redirects to returnUrl when session is already valid
 * - sanitizeReturnUrl rejects null, external, and login-flow paths
 */

import type { RouteRecordRaw } from 'vue-router'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { createMemoryHistory, createRouter } from 'vue-router'
import { CALLBACK_PATH, LOGIN_PATH, sanitizeReturnUrl } from '../../src/router/auth'
import { applyAuthenticationGuard } from '../../src/router/authGuard'
import { useAuthenticationStore } from '../../src/stores/authentication'

// ---------------------------------------------------------------------------
// sanitizeReturnUrl — no mocking needed, pure function
// ---------------------------------------------------------------------------

describe('sanitizeReturnUrl()', () => {
  it('returns "/" for null', () => {
    expect(sanitizeReturnUrl(null)).toBe('/')
  })

  it('returns "/" for undefined', () => {
    expect(sanitizeReturnUrl(undefined)).toBe('/')
  })

  it('returns "/" for empty string', () => {
    expect(sanitizeReturnUrl('')).toBe('/')
  })

  it('returns "/" for external http URL', () => {
    expect(sanitizeReturnUrl('https://evil.com/steal')).toBe('/')
  })

  it('returns "/" for external https URL', () => {
    expect(sanitizeReturnUrl('http://attacker.example.com/')).toBe('/')
  })

  it('returns "/" for protocol-relative URL', () => {
    expect(sanitizeReturnUrl('//evil.com')).toBe('/')
  })

  it('returns "/" for the login path (prevents redirect loop)', () => {
    expect(sanitizeReturnUrl(LOGIN_PATH)).toBe('/')
  })

  it('returns "/" for login path with query string', () => {
    expect(sanitizeReturnUrl(`${LOGIN_PATH}?returnUrl=/other`)).toBe('/')
  })

  it('returns "/" for the callback path', () => {
    expect(sanitizeReturnUrl(CALLBACK_PATH)).toBe('/')
  })

  it('returns the path unchanged for a safe in-app path', () => {
    expect(sanitizeReturnUrl('/redis/some-id')).toBe('/redis/some-id')
  })

  it('returns the path unchanged for the root path', () => {
    expect(sanitizeReturnUrl('/')).toBe('/')
  })
})

// ---------------------------------------------------------------------------
// Guard integration — mock the authentication store
// ---------------------------------------------------------------------------

// We test the guard logic through the RouteLocation shape rather than
// mounting a full app. The guard returns false/true/redirect objects.

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(() => ({
    getUser: vi.fn().mockResolvedValue(null),
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
  })),
  WebStorageStateStore: vi.fn().mockImplementation(() => ({})),
}))

// Mock the bootstrap fetch so the store does not need a real server
global.fetch = vi.fn().mockResolvedValue({
  ok: true,
  json: () =>
    Promise.resolve({
      bootstrapState: 'available',
      unavailableReasonCodes: [],
      providers: [],
      generatedAtUtc: '2026-03-22T12:00:00Z',
      version: '1.0',
    }),
} as Response)

function createTestRouter (ensureValidReturn: boolean) {
  const routes: RouteRecordRaw[] = [
    { path: '/', component: { template: '<div>home</div>' } },
    { path: '/login', component: { template: '<div>login</div>' }, meta: { public: true } },
    { path: '/auth/callback', component: { template: '<div>cb</div>' }, meta: { public: true } },
  ]
  const router = createRouter({ history: createMemoryHistory(), routes })

  // Stub ensureSessionValid on the store instance
  const store = useAuthenticationStore()
  vi.spyOn(store, 'ensureSessionValid').mockResolvedValue(ensureValidReturn)

  applyAuthenticationGuard(router)
  return { router, store }
}

describe('applyAuthenticationGuard()', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('allows navigation to /login when session is invalid', async () => {
    const { router } = createTestRouter(false)
    await router.push('/login')
    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('allows navigation to /auth/callback when session is invalid', async () => {
    const { router } = createTestRouter(false)
    await router.push('/auth/callback')
    expect(router.currentRoute.value.path).toBe('/auth/callback')
  })

  it('redirects unauthenticated user from / to /login', async () => {
    const { router } = createTestRouter(false)
    await router.push('/')
    expect(router.currentRoute.value.path).toBe('/login')
  })

  it('allows authenticated user to navigate to protected route', async () => {
    const { router } = createTestRouter(true)
    await router.push('/')
    expect(router.currentRoute.value.path).toBe('/')
  })

  it('redirects authenticated user away from /login to returnUrl', async () => {
    const { router, store } = createTestRouter(true)
    store.returnUrl = '/redis/my-server'

    await router.push('/login')

    expect(router.currentRoute.value.path).toBe('/redis/my-server')
  })

  it('saves the intended destination when redirecting unauthenticated user', async () => {
    const { router, store } = createTestRouter(false)
    await router.push('/')

    // The guard should have stored the attempted path
    expect(store.returnUrl).not.toBeNull()
  })
})
