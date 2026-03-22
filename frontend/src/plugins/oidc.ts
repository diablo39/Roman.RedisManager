import type { AuthenticationBootstrapProvider } from '@/api/authentication'
import {
  type User,
  UserManager,
  type UserManagerSettings,
  WebStorageStateStore,
} from 'oidc-client-ts'

const oidcManagers = new Map<string, UserManager>()

function createUserManagerSettings(provider: AuthenticationBootstrapProvider): UserManagerSettings {
  const metadata = provider.oidc.metadataOverrides
    ? {
        issuer: provider.oidc.metadataOverrides.issuer ?? undefined,
        authorization_endpoint: provider.oidc.metadataOverrides.authorizationEndpoint ?? undefined,
        token_endpoint: provider.oidc.metadataOverrides.tokenEndpoint ?? undefined,
        userinfo_endpoint: provider.oidc.metadataOverrides.userInfoEndpoint ?? undefined,
        end_session_endpoint: provider.oidc.metadataOverrides.endSessionEndpoint ?? undefined,
      }
    : undefined

  return {
    authority: provider.oidc.authority,
    client_id: provider.oidc.clientId,
    redirect_uri: provider.oidc.redirectUri,
    response_type: provider.oidc.responseType,
    scope: provider.oidc.scope,
    post_logout_redirect_uri: provider.oidc.postLogoutRedirectUri ?? undefined,
    silent_redirect_uri: provider.oidc.silentRedirectUri ?? undefined,
    automaticSilentRenew: provider.oidc.automaticSilentRenew ?? false,
    metadata,
    userStore: new WebStorageStateStore({ store: window.sessionStorage }),
  }
}

export function getOidcUserManager(provider: AuthenticationBootstrapProvider): UserManager {
  const cachedManager = oidcManagers.get(provider.providerKey)
  if (cachedManager) {
    return cachedManager
  }

  const manager = new UserManager(createUserManagerSettings(provider))
  oidcManagers.set(provider.providerKey, manager)

  return manager
}

export async function getOidcUser(provider: AuthenticationBootstrapProvider): Promise<User | null> {
  return await getOidcUserManager(provider).getUser()
}

export function initializeOidcPlugin() {
  // Placeholder hook so plugin registration can initialize any future OIDC runtime setup.
}
