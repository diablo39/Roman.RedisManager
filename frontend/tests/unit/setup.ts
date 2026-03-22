/**
 * Vitest global setup
 *
 * Makes auto-imported globals available in tests, replicating the runtime
 * behaviour of unplugin-auto-import which is not loaded in the minimal
 * vitest config.
 */
import { defineStore, storeToRefs } from 'pinia'
import {
  computed,
  nextTick,
  onMounted,
  onUnmounted,
  reactive,
  ref,
  watch,
  watchEffect,
} from 'vue'

Object.assign(globalThis, {
  // Pinia
  defineStore,
  storeToRefs,
  // Vue reactivity + lifecycle
  ref,
  computed,
  reactive,
  watch,
  watchEffect,
  onMounted,
  onUnmounted,
  nextTick,
})
