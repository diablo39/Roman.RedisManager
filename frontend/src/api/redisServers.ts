/**
 * Redis Servers API client
 */

import { apiBaseUrl } from './config'

/**
 * Represents a Redis server from the backend
 */
export interface RedisServerDto {
  id: string
  name: string
}

/**
 * Paginated query result for Redis servers
 */
export interface RedisServersQueryResult {
  servers: RedisServerDto[]
  totalCount: number
  pageNumber: number
  pageSize: number
}

/**
 * Fetches Redis servers with pagination
 * @param pageNumber - Page number (1-indexed)
 * @param pageSize - Number of items per page
 * @param signal - Optional AbortSignal for request cancellation
 * @returns Promise resolving to paginated server list
 * @throws Error if the request fails
 */
export async function getRedisServers(
  pageNumber: number,
  pageSize: number,
  signal?: AbortSignal
): Promise<RedisServersQueryResult> {
  const url = `${apiBaseUrl}api/RedisServers?pageNumber=${pageNumber}&pageSize=${pageSize}`

  const response = await fetch(url, { signal })

  if (!response.ok) {
    throw new Error(`Failed to fetch servers: ${response.statusText}`)
  }

  return await response.json()
}
