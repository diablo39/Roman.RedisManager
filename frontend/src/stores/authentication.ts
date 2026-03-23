/**
 * Authentication Pinia store
 *
 * Holds the bootstrap result (available providers), current session state,
 * and the preserved return destination. Wraps oidc-client-ts UserManager
 * so Vue components never need to touch it directly.
 *
 * Data model: specs/008-frontend-authentication/data-model.md
 */

import {
  type AuthenticationBootstrapProvider,
  createUserManager,
  getActiveUserManager,
  getAuthenticationBootstrap,
  setActiveUserManager,
} from '@/api/authentication'
import { DEFAULT_AUTHENTICATED_PATH } from '@/router/auth'

// localStorage key for persisting the active provider across page reloads
const ACTIVE_PROVIDER_KEY = 'auth:activeProviderKey'

export const useAuthenticationStore = defineStore('authentication', {
  state: () => ({
    providers: [] as AuthenticationBootstrapProvider[],
    bootstrapState: 'unavailable' as 'available' | 'unavailable',
    bootstrapLoading: false,
    bootstrapLoaded: false,
    activeProviderKey: null as string | null,
    isAuthenticated: false,
    returnUrl: null as string | null,
    error: null as string | null,
    callbackError: null as string | null,
  }),

  getters: {
    availableProviders (): AuthenticationBootstrapProvider[] {
      return this.providers
    },

    isSignInAvailable (): boolean {
      return this.bootstrapState === 'available' && this.providers.length > 0
    },

    unavailableMessage (): string {
      if (this.bootstrapState === 'available') {
        return ''
      }
      return 'Sign-in is currently unavailable. Please try again later.'
    },
  },

  actions: {
    // -----------------------------------------------------------------------
    // Bootstrap — load the list of available sign-in providers
    // -----------------------------------------------------------------------

    async loadBootstrap (force = false): Promise<void> {
      if (this.bootstrapLoaded && !force) {
        return
      }
      this.bootstrapLoading = true
      this.error = null
      try {
        const result = await getAuthenticationBootstrap()
        this.providers = result.providers
        this.bootstrapState = result.bootstrapState
        this.bootstrapLoaded = true
      } catch (error) {
        this.error = error instanceof Error ? error.message : 'Failed to load sign-in providers'
      } finally {
        this.bootstrapLoading = false
      }
    },

    // -----------------------------------------------------------------------
    // startSignIn — redirect the browser to the chosen provider
    //
    // Encodes returnUrl in the OIDC state parameter so it survives the
    // full external redirect round-trip and comes back in handleCallback().
    // -----------------------------------------------------------------------

    async startSignIn (providerKey: string, returnUrl?: string): Promise<void> {
      const provider = this.providers.find(p => p.providerKey === providerKey)
      if (!provider) {
        throw new Error(`Unknown provider: ${providerKey}`)
      }

      const manager = createUserManager(provider)
      setActiveUserManager(manager)
      this.activeProviderKey = providerKey
      // Persist so ensureSessionValid() can reconstruct the manager on reload
      localStorage.setItem(ACTIVE_PROVIDER_KEY, providerKey)

      await manager.signinRedirect({
        state: { returnUrl: returnUrl ?? this.returnUrl ?? DEFAULT_AUTHENTICATED_PATH },
      })
    },

    // -----------------------------------------------------------------------
    // handleCallback — process the OIDC redirect back from the provider
    //
    // Called by the callback page (/login/callback).
    // Returns the post-login destination URL.
    // -----------------------------------------------------------------------

    async handleCallback (): Promise<string> {
      // First try the simple path: active provider key in memory or localStorage
      const providerKey = this.activeProviderKey ?? localStorage.getItem(ACTIVE_PROVIDER_KEY)

      if (providerKey) {
        if (!this.bootstrapLoaded) {
          await this.loadBootstrap()
        }

        const provider = this.providers.find(p => p.providerKey === providerKey)
        if (!provider) {
          throw new Error(`Provider not found: ${providerKey}`)
        }

        const manager = createUserManager(provider)
        setActiveUserManager(manager)

        const user = await manager.signinRedirectCallback()
        this.isAuthenticated = true
        this.activeProviderKey = providerKey

        const state = user.state as { returnUrl?: string } | null
        return state?.returnUrl ?? DEFAULT_AUTHENTICATED_PATH
      }

      // No provider key persisted — attempt a best-effort recovery by
      // reconstructing a UserManager for each known provider and trying
      // signinRedirectCallback() until one succeeds. This handles cases
      // where storage keys were not preserved but the provider returned
      // to our callback with state available in storage.
      if (!this.bootstrapLoaded) {
        await this.loadBootstrap()
      }

      for (const provider of this.providers) {
        const manager = createUserManager(provider)
        try {
          const user = await manager.signinRedirectCallback()
          // Success — record active provider and return destination
          setActiveUserManager(manager)
          this.isAuthenticated = true
          this.activeProviderKey = provider.providerKey
          localStorage.setItem(ACTIVE_PROVIDER_KEY, provider.providerKey)

          const state = user.state as { returnUrl?: string } | null
          return state?.returnUrl ?? DEFAULT_AUTHENTICATED_PATH
        } catch {
          // Not the right manager — continue to next
          // swallow error and try next provider
        }
      }

      throw new Error('No active provider found for callback')
    },

    // -----------------------------------------------------------------------
    // ensureSessionValid — check whether the current session is still alive
    //
    // Called by the route guard before every navigation. Returns true when
    // the user has a non-expired access token in localStorage. Reconstructs
    // the UserManager transparently when the page has just been reloaded.
    // -----------------------------------------------------------------------

    async ensureSessionValid (): Promise<boolean> {
      // Fast path: UserManager is already set (same page session, not a reload)
      let manager = getActiveUserManager()

      if (!manager) {
        // Page reload — try to restore UserManager from stored provider key
        const providerKey = this.activeProviderKey ?? localStorage.getItem(ACTIVE_PROVIDER_KEY)
        if (!providerKey) {
          return false
        }

        // We need the provider OIDC config to reconstruct UserManager settings
        if (!this.bootstrapLoaded) {
          try {
            await this.loadBootstrap()
          } catch {
            return false
          }
        }

        const provider = this.providers.find(p => p.providerKey === providerKey)
        if (!provider) {
          return false
        }

        manager = createUserManager(provider)
        setActiveUserManager(manager)
      }

      // Check stored user — oidc-client-ts reads from localStorage automatically
      const user = await manager.getUser()
      if (user && !user.expired) {
        this.isAuthenticated = true
        return true
      }

      this.invalidateSession()
      return false
    },

    // -----------------------------------------------------------------------
    // Session helpers
    // -----------------------------------------------------------------------

    setReturnUrl (url: string): void {
      this.returnUrl = url
    },

    clearReturnUrl (): void {
      this.returnUrl = null
    },

    setActiveProviderKey (key: string): void {
      this.activeProviderKey = key
    },

    setAuthenticated (flag: boolean): void {
      this.isAuthenticated = flag
    },

    invalidateSession (): void {
      this.isAuthenticated = false
      this.activeProviderKey = null
      localStorage.removeItem(ACTIVE_PROVIDER_KEY)
      setActiveUserManager(null)
    },
  },
})
