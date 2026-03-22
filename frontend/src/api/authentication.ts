import { apiBaseUrl } from './config'

export type AuthenticationBootstrapState = 'available' | 'unavailable'

export interface AuthenticationBootstrapMetadataOverrides {
  issuer: string | null
  authorizationEndpoint: string | null
  tokenEndpoint: string | null
  userInfoEndpoint: string | null
  endSessionEndpoint: string | null
}

export interface AuthenticationBootstrapOidcProfile {
  authority: string
  clientId: string
  redirectUri: string
  scope: string
  responseType: string
  postLogoutRedirectUri: string | null
  silentRedirectUri: string | null
  automaticSilentRenew: boolean | null
  metadataOverrides: AuthenticationBootstrapMetadataOverrides | null
}

export interface AuthenticationBootstrapProvider {
  providerKey: string
  displayName: string
  kind: number
  oidc: AuthenticationBootstrapOidcProfile
}

export interface AuthenticationBootstrapResponse {
  bootstrapState: AuthenticationBootstrapState
  unavailableReasonCodes: string[]
  providers: AuthenticationBootstrapProvider[]
  generatedAtUtc: string
  version: string
}

interface ProblemDetails {
  title?: string | null
  detail?: string | null
}

async function parseErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const payload = (await response.json()) as ProblemDetails
    if (payload.detail) {
      return payload.detail
    }
    if (payload.title) {
      return payload.title
    }
    return fallback
  } catch {
    return fallback
  }
}

export async function getAuthenticationBootstrap(
  signal?: AbortSignal
): Promise<AuthenticationBootstrapResponse> {
  const response = await fetch(`${apiBaseUrl}api/authentication/bootstrap`, { signal })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to fetch authentication providers')
    throw new Error(message)
  }

  return await response.json()
}
