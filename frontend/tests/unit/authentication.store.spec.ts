import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { getAuthenticationBootstrap } from '@/api/authentication'
import { useAuthenticationStore } from '@/stores/authentication'

vi.mock('@/api/authentication', () => ({
  getAuthenticationBootstrap: vi.fn(),
}))

const signinRedirectMock = vi.fn()
const getUserMock = vi.fn()
const signinRedirectCallbackMock = vi.fn()
const removeUserMock = vi.fn()

vi.mock('@/plugins/oidc', () => ({
  getOidcUserManager: vi.fn(() => ({
    signinRedirect: signinRedirectMock,
    signinRedirectCallback: signinRedirectCallbackMock,
    removeUser: removeUserMock,
  })),
  getOidcUser: vi.fn(() => getUserMock()),
}))

describe('authentication store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    sessionStorage.clear()
    signinRedirectMock.mockReset()
    signinRedirectCallbackMock.mockReset()
    removeUserMock.mockReset()
    getUserMock.mockReset()
    vi.mocked(getAuthenticationBootstrap).mockReset()
  })

  it('starts provider sign-in redirect with saved return url', async () => {
    vi.mocked(getAuthenticationBootstrap).mockResolvedValue({
      bootstrapState: 'available',
      unavailableReasonCodes: [],
      providers: [
        {
          providerKey: 'entra',
          displayName: 'Entra ID',
          kind: 0,
          oidc: {
            authority: 'https://authority.example',
            clientId: 'client-id',
            redirectUri: 'http://localhost:3000/auth/callback',
            scope: 'openid profile',
            responseType: 'code',
            postLogoutRedirectUri: null,
            silentRedirectUri: null,
            automaticSilentRenew: null,
            metadataOverrides: null,
          },
        },
      ],
      generatedAtUtc: '2026-01-01T00:00:00Z',
      version: '1.0.0',
    })

    const store = useAuthenticationStore()

    await store.startSignIn('entra', '/redis/1')

    expect(signinRedirectMock).toHaveBeenCalledTimes(1)
    expect(signinRedirectMock).toHaveBeenCalledWith({
      state: {
        providerKey: 'entra',
        returnUrl: '/redis/1',
      },
    })
    expect(store.activeProviderKey).toBe('entra')
  })

  it('invalidates authenticated session when user is missing or expired', async () => {
    vi.mocked(getAuthenticationBootstrap).mockResolvedValue({
      bootstrapState: 'available',
      unavailableReasonCodes: [],
      providers: [
        {
          providerKey: 'entra',
          displayName: 'Entra ID',
          kind: 0,
          oidc: {
            authority: 'https://authority.example',
            clientId: 'client-id',
            redirectUri: 'http://localhost:3000/auth/callback',
            scope: 'openid profile',
            responseType: 'code',
            postLogoutRedirectUri: null,
            silentRedirectUri: null,
            automaticSilentRenew: null,
            metadataOverrides: null,
          },
        },
      ],
      generatedAtUtc: '2026-01-01T00:00:00Z',
      version: '1.0.0',
    })

    getUserMock.mockResolvedValue({ expired: true })

    const store = useAuthenticationStore()
    store.setActiveProviderKey('entra')
    store.setAuthenticated(true)

    const result = await store.ensureSessionValid()

    expect(result).toBe(false)
    expect(store.isAuthenticated).toBe(false)
    expect(removeUserMock).toHaveBeenCalledTimes(1)
  })
})
