import {
  type AuthenticationBootstrapProvider,
  getAuthenticationBootstrap,
} from '@/api/authentication'
import { getOidcUser, getOidcUserManager } from '@/plugins/oidc'
import { DEFAULT_AUTHENTICATED_PATH, sanitizeReturnUrl } from '@/router/auth'

const AUTH_ACTIVE_PROVIDER_KEY = 'auth:active-provider-key'
const AUTH_RETURN_URL_KEY = 'auth:return-url'
const AUTH_IS_AUTHENTICATED_KEY = 'auth:is-authenticated'

const unavailableReasonMessages: Record<string, string> = {
  NO_ENABLED_PROVIDERS: 'No sign-in providers are currently enabled.',
  NO_SIGNIN_CAPABLE_PROVIDERS: 'Sign-in providers are configured but currently unavailable.',
  OIDC_CONFIGURATION_INCOMPLETE:
    'Sign-in is temporarily unavailable due to provider configuration.',
}

function mapUnavailableReasonCodes(reasonCodes: string[]): string {
  if (reasonCodes.length === 0) {
    return 'Sign-in is currently unavailable. Please try again later.'
  }

  const firstKnownMessage = reasonCodes.map(code => unavailableReasonMessages[code]).find(Boolean)

  return firstKnownMessage ?? 'Sign-in is currently unavailable. Please try again later.'
}

export const useAuthenticationStore = defineStore('authentication', {
  state: () => ({
    providers: [] as AuthenticationBootstrapProvider[],
    unavailableReasonCodes: [] as string[],
    bootstrapState: 'unavailable' as 'available' | 'unavailable',
    bootstrapLoading: false,
    bootstrapLoaded: false,
    isAuthenticated: false,
    activeProviderKey: null as string | null,
    returnUrl: null as string | null,
    error: null as string | null,
    callbackError: null as string | null,
  }),

  getters: {
    availableProviders(): AuthenticationBootstrapProvider[] {
      return this.providers
    },

    isSignInAvailable(): boolean {
      return this.bootstrapState === 'available' && this.providers.length > 0
    },

    unavailableMessage(): string {
      return mapUnavailableReasonCodes(this.unavailableReasonCodes)
    },
  },

  actions: {
    hydrateFromStorage() {
      this.activeProviderKey = sessionStorage.getItem(AUTH_ACTIVE_PROVIDER_KEY)
      this.returnUrl = sessionStorage.getItem(AUTH_RETURN_URL_KEY)
      this.isAuthenticated = sessionStorage.getItem(AUTH_IS_AUTHENTICATED_KEY) === 'true'
    },

    setReturnUrl(url: string | null | undefined) {
      const sanitized = sanitizeReturnUrl(url)
      this.returnUrl = sanitized === DEFAULT_AUTHENTICATED_PATH ? null : sanitized

      if (this.returnUrl) {
        sessionStorage.setItem(AUTH_RETURN_URL_KEY, this.returnUrl)
      } else {
        sessionStorage.removeItem(AUTH_RETURN_URL_KEY)
      }
    },

    clearReturnUrl() {
      this.returnUrl = null
      sessionStorage.removeItem(AUTH_RETURN_URL_KEY)
    },

    setActiveProviderKey(providerKey: string | null) {
      this.activeProviderKey = providerKey
      if (providerKey) {
        sessionStorage.setItem(AUTH_ACTIVE_PROVIDER_KEY, providerKey)
      } else {
        sessionStorage.removeItem(AUTH_ACTIVE_PROVIDER_KEY)
      }
    },

    setAuthenticated(isAuthenticated: boolean) {
      this.isAuthenticated = isAuthenticated
      sessionStorage.setItem(AUTH_IS_AUTHENTICATED_KEY, String(isAuthenticated))
    },

    async loadBootstrap(force = false) {
      if (this.bootstrapLoading) {
        return
      }
      if (this.bootstrapLoaded && !force) {
        return
      }

      this.bootstrapLoading = true
      this.error = null

      try {
        const response = await getAuthenticationBootstrap()
        this.bootstrapState = response.bootstrapState
        this.unavailableReasonCodes = response.unavailableReasonCodes
        this.providers = response.providers
        this.bootstrapLoaded = true
      } catch (error) {
        this.bootstrapState = 'unavailable'
        this.providers = []
        this.unavailableReasonCodes = []
        this.error =
          error instanceof Error ? error.message : 'Failed to load authentication providers'
      } finally {
        this.bootstrapLoading = false
      }
    },

    async prepareLogin() {
      this.hydrateFromStorage()
      await this.loadBootstrap()
    },

    async startSignIn(providerKey: string, returnUrl?: string) {
      if (returnUrl) {
        this.setReturnUrl(returnUrl)
      }

      if (!this.bootstrapLoaded) {
        await this.loadBootstrap()
      }

      const provider = this.providers.find(item => item.providerKey === providerKey)
      if (!provider) {
        throw new Error('Selected provider is no longer available. Please refresh and try again.')
      }

      this.setActiveProviderKey(providerKey)
      this.callbackError = null

      await getOidcUserManager(provider).signinRedirect({
        state: {
          providerKey,
          returnUrl: this.returnUrl,
        },
      })
    },

    async retryLastSignIn() {
      if (!this.activeProviderKey) {
        throw new Error('No previous provider available to retry sign-in.')
      }

      await this.startSignIn(this.activeProviderKey, this.returnUrl ?? undefined)
    },

    async handleCallback(currentUrl?: string): Promise<string> {
      this.hydrateFromStorage()
      this.callbackError = null

      if (!this.bootstrapLoaded) {
        await this.loadBootstrap()
      }

      const providerKey = this.activeProviderKey
      if (!providerKey) {
        throw new Error('Unable to determine sign-in provider from session state.')
      }

      const provider = this.providers.find(item => item.providerKey === providerKey)
      if (!provider) {
        throw new Error('Sign-in provider configuration is unavailable. Please try again.')
      }

      const user = await getOidcUserManager(provider).signinRedirectCallback(currentUrl)
      if (!user || user.expired) {
        this.invalidateSession()
        throw new Error('Sign-in session expired before completion. Please try again.')
      }

      this.setAuthenticated(true)
      const callbackState = (user.state ?? {}) as { returnUrl?: string }
      const destination = sanitizeReturnUrl(callbackState.returnUrl ?? this.returnUrl)
      this.clearReturnUrl()

      return destination
    },

    async ensureSessionValid(): Promise<boolean> {
      this.hydrateFromStorage()
      if (!this.isAuthenticated) {
        return false
      }

      if (!this.bootstrapLoaded) {
        await this.loadBootstrap()
      }

      if (!this.activeProviderKey) {
        this.invalidateSession()
        return false
      }

      const provider = this.providers.find(item => item.providerKey === this.activeProviderKey)
      if (!provider) {
        this.invalidateSession()
        return false
      }

      const user = await getOidcUser(provider)
      if (!user || user.expired) {
        await getOidcUserManager(provider).removeUser()
        this.invalidateSession()
        return false
      }

      this.setAuthenticated(true)
      return true
    },

    invalidateSession() {
      this.setAuthenticated(false)
      this.setActiveProviderKey(null)
      this.clearReturnUrl()
      this.callbackError = null
    },

    setCallbackError(message: string | null) {
      this.callbackError = message
    },
  },
})
