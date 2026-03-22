import { flushPromises, mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import CallbackPage from '@/pages/auth/callback.vue'

import vuetify from '@/plugins/vuetify'
import { useAuthenticationStore } from '@/stores/authentication'

const replaceMock = vi.fn()

vi.mock('vue-router', async () => {
  const actual = await vi.importActual('vue-router')

  return {
    ...actual,
    useRouter: () => ({
      replace: replaceMock,
    }),
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

describe('auth callback page', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    sessionStorage.clear()
    replaceMock.mockReset()
  })

  it('renders callback error and recovery actions when callback fails', async () => {
    const store = useAuthenticationStore()
    vi.spyOn(store, 'handleCallback').mockRejectedValue(new Error('Callback failed'))

    const wrapper = mount(CallbackPage, {
      global: {
        plugins: [vuetify],
      },
    })
    await flushPromises()

    expect(wrapper.text()).toContain('Callback failed')
    expect(wrapper.text()).toContain('Retry sign-in')
    expect(wrapper.text()).toContain('Back to sign in')
  })
})
