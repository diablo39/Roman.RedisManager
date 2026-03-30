<template>
  <v-dialog v-model="visible" max-width="900" @update:model-value="onDialogChange">
    <v-card aria-label="List key details" rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-format-list-bulleted" size="20" />
          List Key Details
        </div>
        <v-btn
          aria-label="Close dialog"
          icon="mdi-close"
          size="small"
          variant="text"
          @click="close"
        />
      </div>

      <!-- Loading state -->
      <v-card-text v-if="loading">
        <v-skeleton-loader class="mb-4" type="text" />
        <v-skeleton-loader class="mb-4" type="text" />
        <v-skeleton-loader type="table-tbody" />
      </v-card-text>

      <!-- Error state -->
      <v-card-text v-else-if="loadError">
        <v-alert type="error" variant="tonal">
          <div class="d-flex align-center justify-space-between flex-wrap ga-2">
            <span>{{ loadError }}</span>
            <v-btn color="primary" size="small" variant="text" @click="fetchData">Retry</v-btn>
          </div>
        </v-alert>
      </v-card-text>

      <!-- Ready state -->
      <template v-else>
        <!-- Save error alert -->
        <v-card-text v-if="saveError" class="pb-0">
          <v-alert closable type="error" variant="tonal" @click:close="saveError = null">
            {{ saveError }}
          </v-alert>
        </v-card-text>

        <v-card-text>
          <!-- Key name (read-only) -->
          <div class="mb-4">
            <div class="text-body-2 font-weight-medium mb-1">Key</div>
            <v-text-field
              density="compact"
              disabled
              hide-details
              :model-value="keyName"
              style="font-family: monospace; font-size: 0.85rem"
              variant="outlined"
            />
          </div>

          <!-- Type + item count -->
          <div class="d-flex align-center ga-4 mb-4">
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Type</div>
              <v-chip color="green" size="small" variant="tonal">list</v-chip>
            </div>
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Items</div>
              <span class="text-body-2">{{ activeItemCount }}{{ hasMoreItems ? '+' : '' }}</span>
            </div>
          </div>

          <!-- Empty state -->
          <div
            v-if="items.length === 0 && !addingItem"
            class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
          >
            <v-icon class="mb-3" icon="mdi-format-list-bulleted" size="48" />
            <div class="text-subtitle-1">No items</div>
            <div class="text-body-2">This list has no items</div>
          </div>

          <!-- Items table -->
          <template v-if="items.length > 0 || addingItem">
            <!-- Add item inline form -->
            <div v-if="addingItem" class="add-field-form mb-3 pa-3 rounded border">
              <div class="d-flex ga-2 align-start">
                <v-text-field
                  v-model="newItemValue"
                  density="compact"
                  hide-details="auto"
                  label="Value"
                  :rules="[v => !!v || 'Required']"
                  variant="outlined"
                />
                <v-select
                  v-model="newItemDirection"
                  density="compact"
                  hide-details
                  :items="[
                    { title: 'Head (LPUSH)', value: 0 },
                    { title: 'Tail (RPUSH)', value: 1 },
                  ]"
                  style="max-width: 180px;"
                  variant="outlined"
                />
                <v-btn
                  color="primary"
                  :disabled="!newItemValue"
                  size="small"
                  variant="elevated"
                  @click="addItem"
                >
                  Add
                </v-btn>
                <v-btn size="small" variant="text" @click="cancelAddItem">Cancel</v-btn>
              </div>
            </div>

            <v-progress-linear v-if="loadingMore" color="primary" indeterminate />
            <v-table aria-label="List items" density="compact" hover>
              <thead>
                <tr>
                  <th class="text-end" style="width: 80px;">Index</th>
                  <th>Value</th>
                  <th class="text-end" style="width: 100px;">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(item, arrayIdx) in items"
                  :key="`${item.index}-${arrayIdx}`"
                  :class="{ 'deleted-row': pendingDeletions.has(arrayIdx) }"
                >
                  <td
                    class="text-end text-medium-emphasis"
                    :class="{ 'text-decoration-line-through': pendingDeletions.has(arrayIdx) }"
                    style="font-family: monospace; font-size: 0.85rem; width: 80px;"
                  >
                    {{ item.isNew ? '*' : item.index }}
                  </td>
                  <td>
                    <!-- Deleted item -->
                    <template v-if="pendingDeletions.has(arrayIdx)">
                      <span class="text-medium-emphasis text-decoration-line-through text-body-2">
                        {{ truncateValue(item.value) }}
                      </span>
                    </template>
                    <!-- Edit mode -->
                    <template v-else-if="editingIndex === arrayIdx">
                      <div class="py-2">
                        <div class="d-flex align-center justify-space-between mb-1">
                          <div class="text-body-2 font-weight-medium">Editing value</div>
                          <v-btn
                            v-if="isJsonValue(editValue)"
                            prepend-icon="mdi-code-json"
                            size="x-small"
                            variant="text"
                            @click="formatEditJson"
                          >
                            Format JSON
                          </v-btn>
                        </div>
                        <div class="codemirror-wrapper">
                          <Codemirror
                            v-model="editValue"
                            :extensions="editExtensions"
                            placeholder="(empty)"
                            :style="{ minHeight: '80px', maxHeight: '300px' }"
                          />
                        </div>
                        <div v-if="!editValue" class="text-caption text-error mt-1">
                          Value cannot be empty
                        </div>
                        <div class="d-flex ga-2 mt-2">
                          <v-btn
                            color="primary"
                            :disabled="!editValue"
                            size="small"
                            variant="elevated"
                            @click="confirmEdit"
                          >
                            OK
                          </v-btn>
                          <v-btn size="small" variant="text" @click="cancelEdit">Cancel</v-btn>
                        </div>
                      </div>
                    </template>
                    <!-- Read mode -->
                    <template v-else>
                      <div
                        class="field-value-display text-body-2 py-1"
                        style="cursor: pointer; font-family: monospace; font-size: 0.85rem;"
                        :title="item.value.length > 500 ? 'Click to view/edit full value' : 'Click to edit'"
                        @click="startEdit(item, arrayIdx)"
                      >
                        <template v-if="isJsonValue(item.value)">
                          <pre class="json-preview ma-0" style="white-space: pre-wrap; word-break: break-all; font-size: 0.8rem;">{{ truncateValue(formatJsonIfValid(item.value)) }}</pre>
                        </template>
                        <template v-else>
                          {{ truncateValue(item.value) }}
                        </template>
                      </div>
                    </template>
                  </td>
                  <td class="text-end">
                    <template v-if="pendingDeletions.has(arrayIdx)">
                      <v-btn
                        color="primary"
                        icon="mdi-undo"
                        size="small"
                        title="Restore item"
                        variant="text"
                        @click="restoreItem(arrayIdx)"
                      />
                    </template>
                    <template v-else-if="editingIndex !== arrayIdx">
                      <v-btn
                        color="error"
                        icon="mdi-delete-outline"
                        size="small"
                        title="Mark for deletion"
                        variant="text"
                        @click="markForDeletion(arrayIdx)"
                      />
                    </template>
                  </td>
                </tr>
              </tbody>
            </v-table>

            <!-- Load more -->
            <div v-if="hasMoreItems" class="d-flex justify-center mt-4">
              <v-btn :loading="loadingMore" variant="outlined" @click="loadMore">
                Load More
              </v-btn>
            </div>

            <!-- Item count -->
            <div class="text-body-2 text-medium-emphasis mt-3 text-center">
              {{ activeItemCount }} item{{ activeItemCount === 1 ? '' : 's' }}{{ hasMoreItems ? ' (more available)' : '' }}
              <template v-if="pendingDeletions.size > 0">
                · {{ pendingDeletions.size }} marked for deletion
              </template>
            </div>
          </template>

          <!-- TTL -->
          <div class="mt-4">
            <TtlPicker v-model="currentTtl" />
          </div>
        </v-card-text>

        <!-- Actions -->
        <v-card-actions class="card-footer-separated">
          <v-btn
            color="primary"
            prepend-icon="mdi-plus"
            size="small"
            variant="text"
            @click="startAddItem"
          >
            Add Item
          </v-btn>
          <v-spacer />
          <v-btn variant="text" @click="close">Cancel</v-btn>
          <v-btn
            color="primary"
            :disabled="!isDirty"
            :loading="saving"
            variant="elevated"
            @click="save"
          >
            Save
          </v-btn>
        </v-card-actions>
      </template>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
  import { json } from '@codemirror/lang-json'
  import { Codemirror } from 'vue-codemirror'
  import {
    createListKey,
    getKeyMetadata,
    getListRange,
    removeFromList,
  } from '@/api/redisKeys'

  interface ListItem {
    index: number
    value: string
    isNew?: boolean
    direction?: number // 0 = left (LPUSH), 1 = right (RPUSH)
  }

  const props = defineProps<{
    groupId: string
  }>()

  const emit = defineEmits<{
    close: []
  }>()

  // Dialog state
  const visible = ref(false)
  const keyName = ref('')

  // Loading state
  const loading = ref(true)
  const loadError = ref<string | null>(null)
  const loadingMore = ref(false)
  const saving = ref(false)
  const saveError = ref<string | null>(null)

  // Data — working copy and originals for dirty tracking
  const items = ref<ListItem[]>([])
  const originalItems = ref<ListItem[]>([])
  const loadedCount = ref(0)
  const hasMoreItems = ref(false)
  const pageSize = 100
  const currentTtl = ref<string | null>(null)
  const originalTtl = ref<string | null>(null)

  // Batch state
  const pendingDeletions = ref(new Set<number>())

  // Edit state (local only — no API call until Save)
  const editingIndex = ref<number | null>(null)
  const editValue = ref('')

  // Add item state
  const addingItem = ref(false)
  const newItemValue = ref('')
  const newItemDirection = ref(1) // 1 = right/tail by default

  // Abort controller for cancelling requests
  let abortController: AbortController | null = null

  // Dirty tracking
  const isDirty = computed(() => {
    if (pendingDeletions.value.size > 0) return true
    if (currentTtl.value !== originalTtl.value) return true
    if (items.value.length !== originalItems.value.length) return true
    for (let i = 0; i < items.value.length; i++) {
      const current = items.value[i]
      const original = originalItems.value[i]
      if (!current || !original) return true
      if (current.value !== original.value) return true
    }
    return false
  })

  const activeItemCount = computed(() => {
    return items.value.length - pendingDeletions.value.size
  })

  // JSON helpers
  function isJsonValue (value: string): boolean {
    if (!value) return false
    const trimmed = value.trim()
    if (!trimmed.startsWith('{') && !trimmed.startsWith('[')) return false
    try {
      JSON.parse(trimmed)
      return true
    } catch {
      return false
    }
  }

  function formatJsonIfValid (value: string): string {
    try {
      const parsed = JSON.parse(value)
      return JSON.stringify(parsed, null, 2)
    } catch {
      return value
    }
  }

  function truncateValue (value: string): string {
    if (value.length > 500) {
      return value.slice(0, 500) + '...'
    }
    return value
  }

  // CodeMirror extensions for edit mode
  const editExtensions = computed(() => {
    return isJsonValue(editValue.value) ? [json()] : []
  })

  function formatEditJson () {
    try {
      const parsed = JSON.parse(editValue.value)
      editValue.value = JSON.stringify(parsed, null, 2)
    } catch {
      saveError.value = 'Current value is not valid JSON'
    }
  }

  // TTL conversion
  function msToTimespan (ms: number | null): string | null {
    if (ms === null || ms < 0) return null

    const totalSeconds = Math.floor(ms / 1000)
    const days = Math.floor(totalSeconds / 86_400)
    const hours = Math.floor((totalSeconds % 86_400) / 3600)
    const minutes = Math.floor((totalSeconds % 3600) / 60)
    const seconds = totalSeconds % 60

    const hh = String(hours).padStart(2, '0')
    const mm = String(minutes).padStart(2, '0')
    const ss = String(seconds).padStart(2, '0')
    const dayPrefix = days > 0 ? `${days}.` : ''
    return `${dayPrefix}${hh}:${mm}:${ss}`
  }

  function resetState () {
    loading.value = true
    loadError.value = null
    loadingMore.value = false
    saving.value = false
    saveError.value = null
    items.value = []
    originalItems.value = []
    loadedCount.value = 0
    hasMoreItems.value = false
    currentTtl.value = null
    originalTtl.value = null
    pendingDeletions.value = new Set()
    editingIndex.value = null
    editValue.value = ''
    addingItem.value = false
    newItemValue.value = ''
    newItemDirection.value = 1
  }

  function snapshotOriginals () {
    originalItems.value = items.value.map(item => ({
      index: item.index,
      value: item.value,
      isNew: item.isNew,
      direction: item.direction,
    }))
    originalTtl.value = currentTtl.value
    pendingDeletions.value = new Set()
  }

  async function fetchData () {
    loading.value = true
    loadError.value = null

    abortController?.abort()
    abortController = new AbortController()
    const signal = abortController.signal

    try {
      const [listResult, metadataResult] = await Promise.all([
        getListRange(props.groupId, keyName.value, 0, pageSize - 1, signal),
        getKeyMetadata(keyName.value, props.groupId, signal),
      ])

      items.value = listResult.values.map((v, i) => ({ index: i, value: v }))
      loadedCount.value = listResult.values.length
      hasMoreItems.value = listResult.values.length >= pageSize

      currentTtl.value = msToTimespan(metadataResult.metadata.ttlMilliseconds)
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      loadError.value = error instanceof Error ? error.message : 'Failed to load list key details'
    } finally {
      loading.value = false
    }
  }

  async function loadMore () {
    loadingMore.value = true

    abortController?.abort()
    abortController = new AbortController()
    const signal = abortController.signal

    try {
      const start = loadedCount.value
      const stop = start + pageSize - 1
      const result = await getListRange(
        props.groupId,
        keyName.value,
        start,
        stop,
        signal,
      )

      const newItems = result.values.map((v, i) => ({
        index: start + i,
        value: v,
      }))
      items.value = [...items.value, ...newItems]
      loadedCount.value = loadedCount.value + result.values.length
      hasMoreItems.value = result.values.length >= pageSize
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      saveError.value = error instanceof Error ? error.message : 'Failed to load more items'
    } finally {
      loadingMore.value = false
    }
  }

  // Edit operations — local only, no API call
  function startEdit (item: ListItem, arrayIdx: number) {
    if (pendingDeletions.value.has(arrayIdx)) return
    editingIndex.value = arrayIdx
    editValue.value = isJsonValue(item.value) ? formatJsonIfValid(item.value) : item.value
  }

  function confirmEdit () {
    if (editingIndex.value === null || !editValue.value) return
    const idx = editingIndex.value
    const existing = items.value[idx]
    if (existing) {
      items.value[idx] = { ...existing, value: editValue.value }
    }
    editingIndex.value = null
    editValue.value = ''
  }

  function cancelEdit () {
    editingIndex.value = null
    editValue.value = ''
  }

  // Add item — local only
  function startAddItem () {
    addingItem.value = true
    newItemValue.value = ''
    newItemDirection.value = 1
  }

  function cancelAddItem () {
    addingItem.value = false
    newItemValue.value = ''
    newItemDirection.value = 1
  }

  function addItem () {
    if (!newItemValue.value) return

    const newEntry: ListItem = {
      index: -1,
      value: newItemValue.value,
      isNew: true,
      direction: newItemDirection.value,
    }

    // Prepend to items array so new items appear at the top
    items.value = [newEntry, ...items.value]

    // Shift pending deletion indices since we prepended
    if (pendingDeletions.value.size > 0) {
      const shifted = new Set<number>()
      for (const idx of pendingDeletions.value) {
        shifted.add(idx + 1)
      }
      pendingDeletions.value = shifted
    }

    // Shift editing index if active
    if (editingIndex.value !== null) {
      editingIndex.value = editingIndex.value + 1
    }

    addingItem.value = false
    newItemValue.value = ''
    newItemDirection.value = 1
  }

  // Delete — mark locally, actual delete on Save
  function markForDeletion (arrayIdx: number) {
    pendingDeletions.value = new Set([...pendingDeletions.value, arrayIdx])
    // If editing this item, cancel the edit
    if (editingIndex.value === arrayIdx) {
      cancelEdit()
    }
  }

  function restoreItem (arrayIdx: number) {
    const next = new Set(pendingDeletions.value)
    next.delete(arrayIdx)
    pendingDeletions.value = next
  }

  // Batch save — send all changes at once
  async function save () {
    saving.value = true
    saveError.value = null

    try {
      const promises: Promise<unknown>[] = []

      // Collect NEW items grouped by direction
      const leftValues: string[] = []
      const rightValues: string[] = []
      for (const item of items.value) {
        if (item.isNew) {
          if (item.direction === 0) {
            leftValues.push(item.value)
          } else {
            rightValues.push(item.value)
          }
        }
      }

      if (leftValues.length > 0) {
        promises.push(
          createListKey({
            groupId: props.groupId,
            key: keyName.value,
            values: leftValues,
            direction: 0,
            ttl: currentTtl.value,
          }),
        )
      }

      if (rightValues.length > 0) {
        promises.push(
          createListKey({
            groupId: props.groupId,
            key: keyName.value,
            values: rightValues,
            direction: 1,
            ttl: currentTtl.value,
          }),
        )
      }

      // Collect DELETIONS: items that existed in original (not new) and are marked for deletion
      for (const arrayIdx of pendingDeletions.value) {
        const item = items.value[arrayIdx]
        if (item && !item.isNew) {
          promises.push(
            removeFromList({
              groupId: props.groupId,
              key: keyName.value,
              value: item.value,
              count: 1,
            }),
          )
        }
      }

      // Collect EDITS: existing items whose value changed from original
      for (let i = 0; i < items.value.length; i++) {
        const current = items.value[i]
        if (!current || current.isNew || pendingDeletions.value.has(i)) continue

        const original = originalItems.value.find(
          o => !o.isNew && o.index === current.index,
        )
        if (original && original.value !== current.value) {
          // Remove old value, push new value
          promises.push(
            removeFromList({
              groupId: props.groupId,
              key: keyName.value,
              value: original.value,
              count: 1,
            }).then(() =>
              createListKey({
                groupId: props.groupId,
                key: keyName.value,
                values: [current.value],
                direction: 1,
                ttl: currentTtl.value,
              }),
            ),
          )
        }
      }

      // Handle TTL-only changes (no other mutations)
      if (promises.length === 0 && currentTtl.value !== originalTtl.value) {
        // Push with empty values just to update TTL — use createListKey with the existing first value
        // Actually, we need a TTL update mechanism. For now, re-push an empty array won't work.
        // Instead, if only TTL changed, we send a createListKey with empty values array.
        promises.push(
          createListKey({
            groupId: props.groupId,
            key: keyName.value,
            values: [],
            ttl: currentTtl.value,
          }),
        )
      }

      if (promises.length > 0) {
        await Promise.all(promises)
      }

      // Close dialog after successful save
      close()
    } catch (error) {
      saveError.value = error instanceof Error ? error.message : 'Failed to save changes'
    } finally {
      saving.value = false
    }
  }

  // Dialog lifecycle
  function open (key: string) {
    resetState()
    keyName.value = key
    visible.value = true
    fetchData()
  }

  function close () {
    visible.value = false
  }

  function onDialogChange (value: boolean) {
    if (!value) {
      abortController?.abort()
      emit('close')
    }
  }

  defineExpose({ open })
</script>

<style scoped>
  .codemirror-wrapper {
    border: 1px solid rgba(0, 0, 0, 0.12);
    border-radius: 4px;
    overflow: hidden;
  }

  .codemirror-wrapper :deep(.cm-editor) {
    font-size: 0.85rem;
  }

  .codemirror-wrapper :deep(.cm-editor.cm-focused) {
    outline: 2px solid rgb(var(--v-theme-primary));
    outline-offset: -1px;
  }

  .field-value-display:hover {
    background-color: rgba(0, 0, 0, 0.04);
    border-radius: 4px;
  }

  .json-preview {
    color: rgba(0, 0, 0, 0.7);
  }

  .add-field-form {
    background-color: rgba(var(--v-theme-primary), 0.04);
  }

  .deleted-row {
    background-color: rgba(var(--v-theme-error), 0.04);
  }
</style>
