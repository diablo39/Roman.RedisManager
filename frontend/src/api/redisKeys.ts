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

export interface SetStringRequest {
  groupId: string
  key: string
  value: string
  ttl: string | null
}

export interface SetHashFieldsRequest {
  groupId: string
  key: string
  fields: Record<string, string>
  ttl?: string | null
}

export interface PushToListRequest {
  groupId: string
  key: string
  values: string[]
  direction?: number // 0 = left (LPUSH), 1 = right (RPUSH, default)
  ttl?: string | null
}

export interface AddToSetRequest {
  groupId: string
  key: string
  members: string[]
  ttl?: string | null
}

export interface AddToSortedSetRequest {
  groupId: string
  key: string
  entries: { member: string, score: number }[]
  ttl?: string | null
}

export interface HashFieldDto {
  field: string
  value: string
}

export interface GetHashFieldsResult {
  fields: HashFieldDto[]
  cursor: number
  hasMoreResults: boolean
}

export interface RemoveHashFieldsRequest {
  groupId: string
  key: string
  fields: string[]
}

export interface RemoveHashFieldsResult {
  removedCount: number
}

export interface GetSetMembersResult {
  members: string[]
  cursor: number
  hasMoreResults: boolean
}

export interface RemoveFromSetRequest {
  groupId: string
  key: string
  members: string[]
}

export interface RemoveFromSetResult {
  removedCount: number
}

export interface GetListRangeResult {
  values: string[]
}

export interface RemoveFromListRequest {
  groupId: string
  key: string
  value: string
  count: number
}

export interface RemoveFromListResult {
  removedCount: number
}

export interface SortedSetEntryDto {
  member: string
  score: number
}

export interface GetSortedSetRangeResult {
  entries: SortedSetEntryDto[]
}

export interface RemoveFromSortedSetRequest {
  groupId: string
  key: string
  members: string[]
}

export interface RemoveFromSortedSetResult {
  removedCount: number
}

export interface CommandResult {
  success: boolean
}

export interface GetStringQueryResult {
  value: string | null
}

export interface RedisKeyMetadataDto {
  type: string
  ttlMilliseconds: number | null
}

export interface GetKeyMetadataQueryResult {
  metadata: RedisKeyMetadataDto
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
  if (pattern) {
    params.set('pattern', pattern)
  }
  if (continuationToken) {
    params.set('continuationToken', continuationToken)
  }
  if (pageSize) {
    params.set('pageSize', pageSize.toString())
  }

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

async function postJson<T> (path: string, body: unknown, signal?: AbortSignal): Promise<T> {
  const url = `${apiBaseUrl}${path}`
  const authHeaders = await getAuthHeaders()

  const response = await fetch(url, {
    method: 'POST',
    headers: { ...authHeaders, 'Content-Type': 'application/json' },
    body: JSON.stringify(body),
    signal,
  })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Request failed')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Gets a Redis string value by key.
 */
export async function getStringKeyValue (
  groupId: string,
  key: string,
  signal?: AbortSignal,
): Promise<GetStringQueryResult> {
  const params = new URLSearchParams({ groupId, key })
  const url = `${apiBaseUrl}api/redis/data/strings?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get string value')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Retrieves metadata for a Redis key.
 */
export async function getKeyMetadata (
  key: string,
  groupId: string,
  signal?: AbortSignal,
): Promise<GetKeyMetadataQueryResult> {
  const params = new URLSearchParams({ groupId })
  const url = `${apiBaseUrl}api/redis-keys/${encodeURIComponent(key)}/metadata?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get key metadata')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Fetches hash fields with cursor-based pagination.
 */
export async function getHashFields (
  groupId: string,
  key: string,
  cursor?: number,
  pageSize?: number,
  signal?: AbortSignal,
): Promise<GetHashFieldsResult> {
  const params = new URLSearchParams({ groupId, key })
  if (cursor !== undefined && cursor !== 0) {
    params.set('cursor', cursor.toString())
  }
  if (pageSize !== undefined) {
    params.set('pageSize', pageSize.toString())
  }

  const url = `${apiBaseUrl}api/redis/data/hashes?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get hash fields')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Removes one or more fields from a Redis hash.
 */
export async function removeHashFields (
  request: RemoveHashFieldsRequest,
  signal?: AbortSignal,
): Promise<RemoveHashFieldsResult> {
  return postJson<RemoveHashFieldsResult>('api/redis/data/hashes/remove', request, signal)
}

export function createStringKey (request: SetStringRequest, signal?: AbortSignal) {
  return postJson<CommandResult>('api/redis/data/strings', request, signal)
}

export function createHashKey (request: SetHashFieldsRequest, signal?: AbortSignal) {
  return postJson<CommandResult>('api/redis/data/hashes', request, signal)
}

export function createListKey (request: PushToListRequest, signal?: AbortSignal) {
  return postJson<CommandResult>('api/redis/data/lists', request, signal)
}

export function createSetKey (request: AddToSetRequest, signal?: AbortSignal) {
  return postJson<CommandResult>('api/redis/data/sets', request, signal)
}

export function createSortedSetKey (request: AddToSortedSetRequest, signal?: AbortSignal) {
  return postJson<CommandResult>('api/redis/data/sorted-sets', request, signal)
}

/**
 * Fetches set members with cursor-based pagination.
 */
export async function getSetMembers (
  groupId: string,
  key: string,
  cursor?: number,
  pageSize?: number,
  signal?: AbortSignal,
): Promise<GetSetMembersResult> {
  const params = new URLSearchParams({ groupId, key })
  if (cursor !== undefined && cursor !== 0) {
    params.set('cursor', cursor.toString())
  }
  if (pageSize !== undefined) {
    params.set('pageSize', pageSize.toString())
  }

  const url = `${apiBaseUrl}api/redis/data/sets?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get set members')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Removes one or more members from a Redis set.
 */
export function removeFromSet (request: RemoveFromSetRequest, signal?: AbortSignal) {
  return postJson<RemoveFromSetResult>('api/redis/data/sets/remove', request, signal)
}

/**
 * Fetches list items by index range.
 */
export async function getListRange (
  groupId: string,
  key: string,
  start?: number,
  stop?: number,
  signal?: AbortSignal,
): Promise<GetListRangeResult> {
  const params = new URLSearchParams({ groupId, key })
  if (start !== undefined && start !== 0) {
    params.set('start', start.toString())
  }
  if (stop !== undefined && stop !== -1) {
    params.set('stop', stop.toString())
  }

  const url = `${apiBaseUrl}api/redis/data/lists?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get list items')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Removes items from a Redis list by value.
 */
export function removeFromList (request: RemoveFromListRequest, signal?: AbortSignal) {
  return postJson<RemoveFromListResult>('api/redis/data/lists/remove', request, signal)
}

/**
 * Fetches sorted set entries by index range (sorted by score).
 */
export async function getSortedSetRange (
  groupId: string,
  key: string,
  start?: number,
  stop?: number,
  signal?: AbortSignal,
): Promise<GetSortedSetRangeResult> {
  const params = new URLSearchParams({ groupId, key })
  if (start !== undefined && start !== 0) {
    params.set('start', start.toString())
  }
  if (stop !== undefined && stop !== -1) {
    params.set('stop', stop.toString())
  }

  const url = `${apiBaseUrl}api/redis/data/sorted-sets?${params.toString()}`
  const response = await fetch(url, { signal, headers: await getAuthHeaders() })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to get sorted set entries')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Removes one or more members from a Redis sorted set.
 */
export function removeFromSortedSet (request: RemoveFromSortedSetRequest, signal?: AbortSignal) {
  return postJson<RemoveFromSortedSetResult>('api/redis/data/sorted-sets/remove', request, signal)
}
