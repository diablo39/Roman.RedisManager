// Test environment polyfills for auto-imported globals used in the app
import { defineStore, storeToRefs } from 'pinia'
import { ref, computed, reactive, watch, watchEffect, onMounted, onUnmounted, nextTick } from 'vue'

Object.assign(globalThis, {
  defineStore,
  storeToRefs,
  ref,
  computed,
  reactive,
  watch,
  watchEffect,
  onMounted,
  onUnmounted,
  nextTick,
})

export {}
