/**
 * T023 — Unit tests for src/stores/authentication.ts
 *
 * Tests US1 acceptance scenarios:
 * - loadBootstrap() populates providers and state on success
 * - loadBootstrap() sets error message on failure
 * - startSignIn() calls UserManager.signinRedirect() with the right config
 *
 * External dependencies (oidc-client-ts, fetch) are mocked so tests run
 * fully offline with no real network calls.
 */

import type { AuthenticationBootstrapResult } from '../../src/api/authentication'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { useAuthenticationStore } from '../../src/stores/authentication'

// ---------------------------------------------------------------------------
// Mock oidc-client-ts — we only care that UserManager is called correctly
// ---------------------------------------------------------------------------

const mockSigninRedirect = vi.fn()
const mockGetUser = vi.fn()

vi.mock('oidc-client-ts', () => ({
  UserManager: vi.fn().mockImplementation(function () {
    return {
      signinRedirect: mockSigninRedirect,
      signinRedirectCallback: vi.fn(),
      getUser: mockGetUser,
    }
  }),
  WebStorageStateStore: vi.fn().mockImplementation(function () {
    return {}
  }),
}))

// ---------------------------------------------------------------------------
// Mock bootstrap fetch
// ---------------------------------------------------------------------------

const mockBootstrapResult: AuthenticationBootstrapResult = {
  bootstrapState: 'available',
  unavailableReasonCodes: [],
  providers: [
    {
      providerKey: 'entra',
      displayName: 'Microsoft',
      kind: 0,
      oidc: {
        authority: 'https://login.microsoftonline.com/tenant/v2.0',
        clientId: 'test-client-id',
        redirectUri: 'https://app/login/callback',
        scope: 'openid profile',
        responseType: 'code',
        postLogoutRedirectUri: null,
        silentRedirectUri: null,
        automaticSilentRenew: false,
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

describe('useAuthenticationStore — loadBootstrap()', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('populates providers and sets bootstrapState on success', async () => {
    mockFetchSuccess(mockBootstrapResult)
    const store = useAuthenticationStore()

    await store.loadBootstrap()

    expect(store.providers).toHaveLength(1)
    expect(store.providers[0].providerKey).toBe('entra')
    expect(store.bootstrapState).toBe('available')
    expect(store.bootstrapLoaded).toBe(true)
    expect(store.error).toBeNull()
  })

  it('sets error and leaves bootstrapLoaded false when fetch fails', async () => {
    mockFetchFailure()
    const store = useAuthenticationStore()

    await store.loadBootstrap()

    expect(store.error).toMatch(/failed to load/i)
    expect(store.bootstrapLoaded).toBe(false)
    expect(store.providers).toHaveLength(0)
  })

  it('does not refetch when bootstrapLoaded is true and force is false', async () => {
    mockFetchSuccess(mockBootstrapResult)
    const store = useAuthenticationStore()
    // First load
    await store.loadBootstrap()
    // Second call without force — should NOT call fetch again
    await store.loadBootstrap()

    expect(global.fetch).toHaveBeenCalledTimes(1)
  })

  it('refetches and clears previous error when force is true', async () => {
    mockFetchFailure()
    const store = useAuthenticationStore()
    await store.loadBootstrap()
    expect(store.error).not.toBeNull()

    // Now simulate recovery
    mockFetchSuccess(mockBootstrapResult)
    await store.loadBootstrap(true)

    expect(store.error).toBeNull()
    expect(store.providers).toHaveLength(1)
  })
})

describe('useAuthenticationStore — getters', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('isSignInAvailable is false when bootstrapState is unavailable', () => {
    const store = useAuthenticationStore()
    store.bootstrapState = 'unavailable'
    expect(store.isSignInAvailable).toBe(false)
  })

  it('isSignInAvailable is true when state is available and providers exist', async () => {
    mockFetchSuccess(mockBootstrapResult)
    const store = useAuthenticationStore()
    await store.loadBootstrap()
    expect(store.isSignInAvailable).toBe(true)
  })

  it('unavailableMessage is non-empty when bootstrapState is unavailable', () => {
    const store = useAuthenticationStore()
    store.bootstrapState = 'unavailable'
    expect(store.unavailableMessage.length).toBeGreaterThan(0)
  })
})

describe('useAuthenticationStore — startSignIn()', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    mockFetchSuccess(mockBootstrapResult)
  })

  it('calls UserManager.signinRedirect() with the matching provider config', async () => {
    mockSigninRedirect.mockResolvedValue(undefined)
    const store = useAuthenticationStore()
    await store.loadBootstrap()

    await store.startSignIn('entra', '/some-page')

    expect(mockSigninRedirect).toHaveBeenCalledOnce()
    const callArg = mockSigninRedirect.mock.calls[0][0] as { state: { returnUrl: string } }
    expect(callArg.state.returnUrl).toBe('/some-page')
  })

  it('throws when the providerKey is not in the loaded providers', async () => {
    const store = useAuthenticationStore()
    await store.loadBootstrap()

    await expect(store.startSignIn('unknown-provider')).rejects.toThrow()
  })

  it('persists activeProviderKey to localStorage', async () => {
    mockSigninRedirect.mockResolvedValue(undefined)
    const store = useAuthenticationStore()
    await store.loadBootstrap()

    await store.startSignIn('entra')

    expect(localStorage.getItem('auth:activeProviderKey')).toBe('entra')
  })
})

describe('useAuthenticationStore — ensureSessionValid()', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    mockFetchSuccess(mockBootstrapResult)
    localStorage.clear()
  })

  it('returns false when no provider key is stored', async () => {
    const store = useAuthenticationStore()
    const valid = await store.ensureSessionValid()
    expect(valid).toBe(false)
  })

  it('returns false when stored user is expired', async () => {
    localStorage.setItem('auth:activeProviderKey', 'entra')
    mockGetUser.mockResolvedValue({ expired: true, access_token: 'old-token' })

    const store = useAuthenticationStore()
    const valid = await store.ensureSessionValid()

    expect(valid).toBe(false)
    expect(store.isAuthenticated).toBe(false)
  })

  it('returns true and sets isAuthenticated when user is valid', async () => {
    localStorage.setItem('auth:activeProviderKey', 'entra')
    mockGetUser.mockResolvedValue({ expired: false, access_token: 'valid-token' })

    const store = useAuthenticationStore()
    const valid = await store.ensureSessionValid()

    expect(valid).toBe(true)
    expect(store.isAuthenticated).toBe(true)
  })
})
