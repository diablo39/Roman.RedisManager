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
    useRoute: () => ({ query: {} }),
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

describe('login page states', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    sessionStorage.clear()
  })

  it('shows loading message while bootstrap request is pending', async () => {
    vi.mocked(getAuthenticationBootstrap).mockImplementation(() => new Promise(() => undefined))

    const wrapper = mount(LoginPage, {
      global: {
        plugins: [vuetify],
      },
    })

    await Promise.resolve()

    expect(wrapper.text()).toContain('Loading sign-in providers...')
  })

  it('shows unavailable message when no sign-in providers are available', async () => {
    vi.mocked(getAuthenticationBootstrap).mockResolvedValue({
      bootstrapState: 'unavailable',
      unavailableReasonCodes: ['NO_SIGNIN_CAPABLE_PROVIDERS'],
      providers: [],
      generatedAtUtc: '2026-01-01T00:00:00Z',
      version: '1.0.0',
    })

    const wrapper = mount(LoginPage, {
      global: {
        plugins: [vuetify],
      },
    })
    await flushPromises()

    expect(wrapper.text()).toContain('Sign-in providers are configured but currently unavailable.')
  })
})
