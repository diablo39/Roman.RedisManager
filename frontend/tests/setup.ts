// Test environment polyfills for auto-imported globals used in the app
import { defineStore, storeToRefs } from 'pinia'
import { computed, nextTick, onMounted, onUnmounted, reactive, ref, watch, watchEffect } from 'vue'

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
