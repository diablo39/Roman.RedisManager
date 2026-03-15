/**
 * Redis server groups API client
 */

import { apiBaseUrl } from './config'

/**
 * Represents a Redis server group from the backend.
 */
export interface RedisServerGroupDto {
  id: string
  name: string
  groupType: 'Standalone' | 'Cluster'
}

/**
 * Represents a Redis node returned in server-group detail responses.
 */
export interface RedisServerNodeDto {
  host: string
  port: number
  role: string
}

/**
 * Paginated query result for Redis server groups.
 */
export interface RedisServersQueryResult {
  serverGroups: RedisServerGroupDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

/**
 * Details for a single Redis server group.
 */
export interface RedisServerGroupDetailQueryResult {
  nodes: RedisServerNodeDto[]
}

interface ProblemDetails {
  title?: string | null
  detail?: string | null
}

async function parseErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const payload = (await response.json()) as ProblemDetails
    if (payload.detail) return payload.detail
    if (payload.title) return payload.title
    return fallback
  } catch {
    return fallback
  }
}

/**
 * Fetches Redis server groups with pagination.
 * @param pageNumber - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @param signal - Optional AbortSignal for request cancellation
 * @returns Promise resolving to paginated server-group list
 * @throws Error if the request fails
 */
export async function getRedisServers(
  pageNumber: number,
  pageSize: number,
  signal?: AbortSignal
): Promise<RedisServersQueryResult> {
  const params = new URLSearchParams({
    pageNumber: pageNumber.toString(),
    pageSize: pageSize.toString(),
  })
  const url = `${apiBaseUrl}api/redis-server-groups?${params.toString()}`

  const response = await fetch(url, { signal })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to fetch Redis server groups')
    throw new Error(message)
  }

  return await response.json()
}

/**
 * Fetches details for a single Redis server group.
 * @param id - Redis server group identifier
 * @param signal - Optional AbortSignal for request cancellation
 */
export async function getRedisServerGroupDetail(
  id: string,
  signal?: AbortSignal
): Promise<RedisServerGroupDetailQueryResult> {
  const url = `${apiBaseUrl}api/redis-server-groups/${encodeURIComponent(id)}`
  const response = await fetch(url, { signal })

  if (!response.ok) {
    const message = await parseErrorMessage(response, 'Failed to fetch Redis server group details')
    throw new Error(message)
  }

  return await response.json()
}
