/**
 * Authentication API module
 *
 * Provides:
 * - TypeScript types for the bootstrap endpoint response
 * - A factory to create an oidc-client-ts UserManager from a provider config
 * - A module-level reference to the currently-active UserManager
 * - getAuthHeaders() for attaching the Bearer token to API requests
 * - getAuthenticationBootstrap() to fetch the list of sign-in providers
 */

import { UserManager, type UserManagerSettings, WebStorageStateStore } from 'oidc-client-ts'
import { apiBaseUrl } from './config'

// ---------------------------------------------------------------------------
// Types — mirror the backend bootstrap endpoint contract
// ---------------------------------------------------------------------------

export interface AuthenticationBootstrapProvider {
  providerKey: string
  displayName: string
  kind: number
  oidc: {
    authority: string
    clientId: string
    redirectUri: string
    scope: string
    responseType: string
    postLogoutRedirectUri: string | null
    silentRedirectUri: string | null
    automaticSilentRenew: boolean | null
    metadataOverrides: {
      issuer: string | null
      authorizationEndpoint: string | null
      tokenEndpoint: string | null
      userInfoEndpoint: string | null
      endSessionEndpoint: string | null
    } | null
  }
}

export interface AuthenticationBootstrapResult {
  bootstrapState: 'available' | 'unavailable'
  unavailableReasonCodes: string[]
  providers: AuthenticationBootstrapProvider[]
  generatedAtUtc: string
  version: string
}

// ---------------------------------------------------------------------------
// Module-level active UserManager
//
// Kept here (not in the store) so that getAuthHeaders() can read the current
// user without importing the Pinia store — which would create a circular dep
// with the API layer.
// ---------------------------------------------------------------------------

let activeUserManager: UserManager | null = null

export function setActiveUserManager(manager: UserManager | null): void {
  activeUserManager = manager
}

export function getActiveUserManager(): UserManager | null {
  return activeUserManager
}

// ---------------------------------------------------------------------------
// UserManager factory
//
// Creates a new oidc-client-ts UserManager from a provider's OIDC config,
// mapping camelCase backend fields to the snake_case settings object.
// ---------------------------------------------------------------------------

export function createUserManager(provider: AuthenticationBootstrapProvider): UserManager {
  const { oidc } = provider

  const settings: UserManagerSettings = {
    authority: oidc.authority,
    client_id: oidc.clientId,
    redirect_uri: oidc.redirectUri,
    scope: oidc.scope,
    response_type: oidc.responseType,
    automaticSilentRenew: oidc.automaticSilentRenew ?? false,
    // Store the user object in localStorage so sessions survive page refreshes
    userStore: new WebStorageStateStore({ store: window.localStorage }),
  }

  if (oidc.postLogoutRedirectUri != null) {
    settings.post_logout_redirect_uri = oidc.postLogoutRedirectUri
  }
  if (oidc.silentRedirectUri != null) {
    settings.silent_redirect_uri = oidc.silentRedirectUri
  }

  if (oidc.metadataOverrides != null) {
    const m = oidc.metadataOverrides
    settings.metadata = {
      ...(m.issuer != null && { issuer: m.issuer }),
      ...(m.authorizationEndpoint != null && { authorization_endpoint: m.authorizationEndpoint }),
      ...(m.tokenEndpoint != null && { token_endpoint: m.tokenEndpoint }),
      ...(m.userInfoEndpoint != null && { userinfo_endpoint: m.userInfoEndpoint }),
      ...(m.endSessionEndpoint != null && { end_session_endpoint: m.endSessionEndpoint }),
    }
  }

  return new UserManager(settings)
}

// ---------------------------------------------------------------------------
// Auth headers helper (FR-012)
//
// Call this before every protected fetch() to attach the Bearer token.
// Returns an empty object when no session is active — callers can safely
// spread the result into fetch headers regardless.
// ---------------------------------------------------------------------------

export async function getAuthHeaders(): Promise<Record<string, string>> {
  // Prefer the active UserManager when available
  if (activeUserManager) {
    const user = await activeUserManager.getUser()
    if (user && !user.expired) {
      return { Authorization: `Bearer ${user.access_token}` }
    }
  }

  // Fallback: some code paths call protected APIs before the
  // UserManager instance is reconstructed in memory. In that case the
  // token may still be present in localStorage under the oidc-client key
  // (format: "oidc.user:<authority>:<client_id>"). Try to read it.
  try {
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (!key) continue
      if (!key.startsWith('oidc.user')) continue

      const raw = localStorage.getItem(key)
      if (!raw) continue

      try {
        const parsed = JSON.parse(raw) as { access_token?: string; expired?: boolean; expires_at?: number }
        if (parsed.access_token) {
          // prefer explicit `expired` flag when present, otherwise use expires_at
          if (parsed.expired === true) continue
          if (typeof parsed.expires_at === 'number') {
            const now = Math.floor(Date.now() / 1000)
            if (parsed.expires_at <= now) continue
          }

          return { Authorization: `Bearer ${parsed.access_token}` }
        }
      } catch {
        // ignore parse errors and continue
      }
    }
  } catch {
    // localStorage might be unavailable in some environments — ignore
  }

  return {}
}

// ---------------------------------------------------------------------------
// Bootstrap API call
//
// Public endpoint — no auth needed. Fetches provider list and availability.
// ---------------------------------------------------------------------------

export async function getAuthenticationBootstrap(): Promise<AuthenticationBootstrapResult> {
  const response = await fetch(`${apiBaseUrl}api/authentication/bootstrap`)
  if (!response.ok) {
    throw new Error(`Failed to load sign-in providers (${response.status})`)
  }
  return response.json() as Promise<AuthenticationBootstrapResult>
}
