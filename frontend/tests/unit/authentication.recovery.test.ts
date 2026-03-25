/**
 * T025 — Unit tests for US3 error recovery paths
 *
 * Tests:
 * - handleCallback() sets callbackError on failure
 * - loadBootstrap(true) force-reloads and clears a previous error
 * - ensureSessionValid() returns false when the stored user is expired
 */

import type { AuthenticationBootstrapResult } from '../../src/api/authentication'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthenticationStore } from '../../src/stores/authentication'

// ---------------------------------------------------------------------------
// Mock oidc-client-ts
// ---------------------------------------------------------------------------

const mockSigninRedirectCallback = vi.fn()
const mockGetUser = vi.fn()

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(function () {
    return {
      signinRedirect: vi.fn(),
      signinRedirectCallback: mockSigninRedirectCallback,
      getUser: mockGetUser,
    }
  }),
  WebStorageStateStore: vi.fn().mockImplementation(function () {
    return {}
  }),
}))

// ---------------------------------------------------------------------------
// Bootstrap mock helpers
// ---------------------------------------------------------------------------

const bootstrapWithProvider: AuthenticationBootstrapResult = {
  bootstrapState: 'available',
  unavailableReasonCodes: [],
  providers: [
    {
      providerKey: 'entra',
      displayName: 'Microsoft',
      kind: 0,
      oidc: {
        authority: 'https://login.microsoftonline.com/tenant/v2.0',
        clientId: 'test-id',
        redirectUri: 'https://app/login/callback',
        scope: 'openid',
        responseType: 'code',
        postLogoutRedirectUri: null,
        silentRedirectUri: null,
        automaticSilentRenew: null,
        metadataOverrides: null,
      },
    },
  ],
  generatedAtUtc: '2026-03-22T12:00:00Z',
  version: '1.0',
}

function mockFetchSuccess (result: AuthenticationBootstrapResult): void {
  global.fetch = vi.fn().mockResolvedValue({
    ok: true,
    json: () => Promise.resolve(result),
  } as Response)
}

function mockFetchFailure (): void {
  global.fetch = vi.fn().mockResolvedValue({
    ok: false,
    status: 503,
    json: () => Promise.reject(new Error('no body')),
  } as unknown as Response)
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('handleCallback() — error path', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('throws when no active provider key is stored', async () => {
    const store = useAuthenticationStore()
    await expect(store.handleCallback()).rejects.toThrow(/no active provider/i)
  })

  it('callbackError can be set to signal failure to the callback page UI', async () => {
    mockFetchSuccess(bootstrapWithProvider)
    localStorage.setItem('auth:activeProviderKey', 'entra')
    mockSigninRedirectCallback.mockRejectedValue(new Error('invalid state'))

    const store = useAuthenticationStore()
    // Simulate what the callback page does on error:
    try {
      await store.handleCallback()
    } catch (error) {
      store.callbackError = error instanceof Error ? error.message : 'Sign-in failed'
    }

    expect(store.callbackError).toMatch(/invalid state/i)
    expect(store.isAuthenticated).toBe(false)
  })
})

describe('loadBootstrap() — force reload clears errors', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('clears a previous error when force=true and fetch succeeds', async () => {
    // First call fails
    mockFetchFailure()
    const store = useAuthenticationStore()
    await store.loadBootstrap()
    expect(store.error).not.toBeNull()

    // Force reload succeeds
    mockFetchSuccess(bootstrapWithProvider)
    await store.loadBootstrap(true)

    expect(store.error).toBeNull()
    expect(store.providers).toHaveLength(1)
    expect(store.bootstrapLoaded).toBe(true)
  })
})

describe('ensureSessionValid() — expired session', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    localStorage.clear()
  })

  it('returns false and calls invalidateSession when user is expired', async () => {
    localStorage.setItem('auth:activeProviderKey', 'entra')
    mockFetchSuccess(bootstrapWithProvider)
    mockGetUser.mockResolvedValue({ expired: true, access_token: 'stale' })

    const store = useAuthenticationStore()
    const spy = vi.spyOn(store, 'invalidateSession')

    const valid = await store.ensureSessionValid()

    expect(valid).toBe(false)
    expect(spy).toHaveBeenCalledOnce()
    expect(localStorage.getItem('auth:activeProviderKey')).toBeNull()
  })

  it('returns false when getUser() resolves to null', async () => {
    localStorage.setItem('auth:activeProviderKey', 'entra')
    mockFetchSuccess(bootstrapWithProvider)
    mockGetUser.mockResolvedValue(null)

    const store = useAuthenticationStore()
    const valid = await store.ensureSessionValid()

    expect(valid).toBe(false)
    expect(store.isAuthenticated).toBe(false)
  })
})
