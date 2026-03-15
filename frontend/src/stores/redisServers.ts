/**
 * Redis Servers Pinia Store
 * Manages Redis server list with pagination
 */

import type { RedisServerGroupDto } from '@/api/redisServers'
import { getRedisServers } from '@/api/redisServers'

export const useRedisServersStore = defineStore('redisServers', {
  state: () => ({
    servers: [] as RedisServerGroupDto[],
    pageNumber: 1,
    pageSize: 10,
    totalCount: 0,
    loading: false,
    error: null as string | null,
  }),

  getters: {
    /**
     * Check if previous page is available
     */
    hasPrev(): boolean {
      return this.pageNumber > 1
    },

    /**
     * Check if next page is available
     */
    hasNext(): boolean {
      return this.pageNumber * this.pageSize < this.totalCount
    },

    /**
     * Calculate total number of pages
     */
    totalPages(): number {
      return this.totalCount > 0 ? Math.ceil(this.totalCount / this.pageSize) : 0
    },
  },

  actions: {
    /**
     * Reset the list and fetch the first page
     */
    async reset() {
      this.servers = []
      this.pageNumber = 1
      this.totalCount = 0
      this.error = null
      await this.fetchServers(false)
    },

    /**
     * Load the next page of servers
     */
    async loadNextPage() {
      if (this.loading || !this.hasNext) return

      this.pageNumber++
      await this.fetchServers(true)
    },

    /**
     * Internal fetch implementation
     * @param append - Whether to append results to existing list
     */
    async fetchServers(append: boolean) {
      this.loading = true
      // error shouldn't be cleared here if we want to keep showing previous data + error toast,
      // but for now let's clear it to allow retries
      this.error = null

      try {
        const result = await getRedisServers(this.pageNumber, this.pageSize)

        if (append) {
          this.servers.push(...result.serverGroups)
        } else {
          this.servers = result.serverGroups
        }

        this.totalCount = result.totalCount
        // Check if backend returned a different page/size than requested
        this.pageNumber = result.pageNumber
        this.pageSize = result.pageSize
      } catch (err) {
        this.error = err instanceof Error ? err.message : 'Failed to load servers'
        console.error('Error fetching Redis servers:', err)
        // If appending failed, we might want to revert pageNumber so user can try again
        if (append) this.pageNumber--
        throw err // Re-throw for UI to handle (e.g. infinite scroll 'error' status)
      } finally {
        this.loading = false
      }
    },
  },
})
