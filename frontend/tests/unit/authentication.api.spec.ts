import { describe, expect, it, vi } from 'vitest'
import { getAuthenticationBootstrap } from '@/api/authentication'

describe('authentication api', () => {
  it('returns bootstrap payload when request succeeds', async () => {
    const fetchMock = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        bootstrapState: 'available',
        unavailableReasonCodes: [],
        providers: [
          {
            providerKey: 'demo',
            displayName: 'Demo Provider',
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
      }),
    })

    vi.stubGlobal('fetch', fetchMock)

    const result = await getAuthenticationBootstrap()

    expect(result.bootstrapState).toBe('available')
    expect(result.providers).toHaveLength(1)
    expect(fetchMock).toHaveBeenCalledTimes(1)
  })

  it('throws parsed backend error details when request fails', async () => {
    vi.stubGlobal(
      'fetch',
      vi.fn().mockResolvedValue({
        ok: false,
        json: async () => ({
          detail: 'Bootstrap endpoint unavailable',
        }),
      })
    )

    await expect(getAuthenticationBootstrap()).rejects.toThrow('Bootstrap endpoint unavailable')
  })
})
