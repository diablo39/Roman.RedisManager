import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getAuthenticationBootstrap } from '@/api/authentication'

import LoginPage from '@/pages/login.vue'
import vuetify from '@/plugins/vuetify'

vi.mock('vue-router', async () => {
  const actual = await vi.importActual('vue-router')

  return {
    ...actual,
    useRoute: () => ({ query: { returnUrl: '/redis/abc' } }),
  }
})

vi.mock('@/api/authentication', () => ({
  getAuthenticationBootstrap: vi.fn(),
}))

vi.mock('@/plugins/oidc', () => ({
  getOidcUserManager: vi.fn(() => ({
    signinRedirect: vi.fn(),
    signinRedirectCallback: vi.fn(),
    removeUser: vi.fn(),
  })),
  getOidcUser: vi.fn(() => null),
}))

describe('login page provider rendering', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    sessionStorage.clear()

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
  })

  it('renders provider actions returned by bootstrap endpoint', async () => {
    const wrapper = mount(LoginPage, {
      global: {
        plugins: [vuetify],
      },
    })
    await flushPromises()

    expect(wrapper.text()).toContain('Continue with Entra ID')
  })
})
