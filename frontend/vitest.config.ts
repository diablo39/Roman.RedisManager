import { fileURLToPath, URL } from 'node:url'
import Vue from '@vitejs/plugin-vue'
import { defineConfig } from 'vitest/config'

// Minimal vitest config for unit testing stores and router utilities.
// Does NOT load Vuetify, auto-import, or layout plugins — unit tests should
// mock those boundaries rather than boot the full Vite plugin stack.
export default defineConfig({
  plugins: [Vue()],
  test: {
    environment: 'happy-dom',
    globals: true,
    setupFiles: ['./tests/setup.ts'],
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('src', import.meta.url)),
    },
    extensions: ['.js', '.json', '.jsx', '.mjs', '.ts', '.tsx', '.vue'],
  },
})
