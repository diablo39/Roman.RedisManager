/**
 * Unit tests for collection-type API functions in src/api/redisKeys.ts
 *
 * Tests the 6 new functions: getSetMembers, removeFromSet, getListRange,
 * removeFromList, getSortedSetRange, removeFromSortedSet.
 *
 * Validates URL construction, query parameters, request bodies, and response parsing.
 */

import { beforeEach, describe, expect, it, vi } from 'vitest'

import {
  getListRange,
  getSetMembers,
  getSortedSetRange,
  removeFromList,
  removeFromSet,
  removeFromSortedSet,
} from '../../src/api/redisKeys'

// Mock authentication and config modules before importing the API
vi.mock('../../src/api/authentication', () => ({
  getAuthHeaders: vi.fn().mockResolvedValue({ Authorization: 'Bearer test-token' }),
}))

vi.mock('../../src/api/config', () => ({
  apiBaseUrl: 'http://localhost:5000/',
}))

// Mock global fetch
const mockFetch = vi.fn()
globalThis.fetch = mockFetch

beforeEach(() => {
  mockFetch.mockReset()
})

// ---------------------------------------------------------------------------
// getSetMembers
// ---------------------------------------------------------------------------

describe('getSetMembers()', () => {
  it('builds URL with required params', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ members: ['a', 'b'], cursor: 0, hasMoreResults: false }),
    })

    await getSetMembers('group-1', 'myset')

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/sets?')
    expect(url).toContain('groupId=group-1')
    expect(url).toContain('key=myset')
    expect(url).not.toContain('cursor=')
    expect(url).not.toContain('pageSize=')
  })

  it('includes cursor and pageSize when provided', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ members: [], cursor: 0, hasMoreResults: false }),
    })

    await getSetMembers('group-1', 'myset', 42, 50)

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('cursor=42')
    expect(url).toContain('pageSize=50')
  })

  it('parses GetSetMembersResult correctly', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ members: ['x', 'y'], cursor: 99, hasMoreResults: true }),
    })

    const result = await getSetMembers('g', 'k')
    expect(result.members).toEqual(['x', 'y'])
    expect(result.cursor).toBe(99)
    expect(result.hasMoreResults).toBe(true)
  })

  it('throws on non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      json: () => Promise.resolve({ detail: 'Not found' }),
    })

    await expect(getSetMembers('g', 'k')).rejects.toThrow('Not found')
  })
})

// ---------------------------------------------------------------------------
// removeFromSet
// ---------------------------------------------------------------------------

describe('removeFromSet()', () => {
  it('posts correct JSON body', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ removedCount: 2 }),
    })

    await removeFromSet({ groupId: 'g1', key: 'k1', members: ['a', 'b'] })

    const [url, init] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/sets/remove')
    expect(init.method).toBe('POST')
    const body = JSON.parse(init.body as string)
    expect(body.groupId).toBe('g1')
    expect(body.key).toBe('k1')
    expect(body.members).toEqual(['a', 'b'])
  })
})

// ---------------------------------------------------------------------------
// getListRange
// ---------------------------------------------------------------------------

describe('getListRange()', () => {
  it('builds URL with required params only', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ values: ['item1'] }),
    })

    await getListRange('group-1', 'mylist')

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/lists?')
    expect(url).toContain('groupId=group-1')
    expect(url).toContain('key=mylist')
    expect(url).not.toContain('start=')
    expect(url).not.toContain('stop=')
  })

  it('includes start and stop when provided', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ values: [] }),
    })

    await getListRange('g', 'k', 100, 199)

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('start=100')
    expect(url).toContain('stop=199')
  })

  it('parses GetListRangeResult correctly', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ values: ['a', 'b', 'c'] }),
    })

    const result = await getListRange('g', 'k')
    expect(result.values).toEqual(['a', 'b', 'c'])
  })

  it('throws on non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      json: () => Promise.resolve({ title: 'Bad Request' }),
    })

    await expect(getListRange('g', 'k')).rejects.toThrow('Bad Request')
  })
})

// ---------------------------------------------------------------------------
// removeFromList
// ---------------------------------------------------------------------------

describe('removeFromList()', () => {
  it('posts correct JSON body', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ removedCount: 1 }),
    })

    await removeFromList({ groupId: 'g1', key: 'k1', value: 'item', count: 1 })

    const [url, init] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/lists/remove')
    expect(init.method).toBe('POST')
    const body = JSON.parse(init.body as string)
    expect(body.value).toBe('item')
    expect(body.count).toBe(1)
  })
})

// ---------------------------------------------------------------------------
// getSortedSetRange
// ---------------------------------------------------------------------------

describe('getSortedSetRange()', () => {
  it('builds URL with required params only', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ entries: [] }),
    })

    await getSortedSetRange('group-1', 'myzset')

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/sorted-sets?')
    expect(url).toContain('groupId=group-1')
    expect(url).toContain('key=myzset')
  })

  it('includes start and stop when provided', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ entries: [] }),
    })

    await getSortedSetRange('g', 'k', 50, 149)

    const [url] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('start=50')
    expect(url).toContain('stop=149')
  })

  it('parses SortedSetEntryDto[] response', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({
        entries: [
          { member: 'alice', score: 1.5 },
          { member: 'bob', score: 3 },
        ],
      }),
    })

    const result = await getSortedSetRange('g', 'k')
    expect(result.entries).toHaveLength(2)
    expect(result.entries[0]).toEqual({ member: 'alice', score: 1.5 })
    expect(result.entries[1]).toEqual({ member: 'bob', score: 3 })
  })

  it('throws on non-ok response', async () => {
    mockFetch.mockResolvedValue({
      ok: false,
      json: () => Promise.resolve({ detail: 'Server error' }),
    })

    await expect(getSortedSetRange('g', 'k')).rejects.toThrow('Server error')
  })
})

// ---------------------------------------------------------------------------
// removeFromSortedSet
// ---------------------------------------------------------------------------

describe('removeFromSortedSet()', () => {
  it('posts correct JSON body', async () => {
    mockFetch.mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ removedCount: 2 }),
    })

    await removeFromSortedSet({ groupId: 'g1', key: 'k1', members: ['alice', 'bob'] })

    const [url, init] = mockFetch.mock.calls[0] as [string, RequestInit]
    expect(url).toContain('api/redis/data/sorted-sets/remove')
    expect(init.method).toBe('POST')
    const body = JSON.parse(init.body as string)
    expect(body.members).toEqual(['alice', 'bob'])
  })
})
