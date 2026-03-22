import { afterEach, vi } from 'vitest'

class MockResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
}

if (!globalThis.ResizeObserver) {
  globalThis.ResizeObserver = MockResizeObserver as typeof ResizeObserver
}

afterEach(() => {
  vi.restoreAllMocks()
  vi.clearAllMocks()
})
