/**
 * Redis keys API client
 */

import { getAuthHeaders } from './authentication'
import { apiBaseUrl } from './config'

export interface RedisKeyDto {
  key: string
  type: string
  ttlMilliseconds: number | null
  hasExpiration: boolean
}

export interface RedisKeysSearchResult {
  keys: RedisKeyDto[]
  hasMoreResults: boolean
  continuationToken: string | null
}

export interface DeleteKeyResult {
  deleted: boolean
}

interface ProblemDetails {
  title?: string | null
  detail?: string | null
}

async function parseErrorMessage (response: Response, fallback: string): Promise<string> {
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

/**
 * Searches Redis keys by glob pattern with continuation-token pagination.
 */
export async function searchRedisKeys (
  groupId: string,
  pattern?: string,
  continuationToken?: string,
  pageSize?: number,
  signal?: AbortSignal,
): Promise<RedisKeysSearchResult> {
  const params = new URLSearchParams({ groupId })
  if (pattern) params.set('pattern', pattern)
  if (continuationToken) params.set('continuationToken', continuationToken)
  if (pageSize) params.set('pageSize', pageSize.toString())

  const url = `${apiBaseUrl}api/redis-keys?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to search Redis keys')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Deletes a Redis key.
 */
export async function deleteRedisKey (
  key: string,
  groupId: string,
  signal?: AbortSignal,
): Promise<DeleteKeyResult> {
  const params = new URLSearchParams({ groupId })
  const url = `${apiBaseUrl}api/redis-keys/${encodeURIComponent(key)}?${params.toString()}`

  const response = await fetch(url, {
    method: 'DELETE',
    signal,
    headers: await getAuthHeaders(),
  })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to delete Redis key')
    throw new Error(message)
  }

  return await response.json()
}
