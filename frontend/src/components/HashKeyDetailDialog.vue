<template>
  <v-dialog v-model="visible" max-width="900" @update:model-value="onDialogChange">
    <v-card aria-label="Hash key details" rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-code-braces" size="20" />
          Hash Key Details
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

          <!-- Type + field count -->
          <div class="d-flex align-center ga-4 mb-4">
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Type</div>
              <v-chip color="orange" size="small" variant="tonal">hash</v-chip>
            </div>
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Fields</div>
              <span class="text-body-2">{{ activeFieldCount }}{{ hasMoreFields ? '+' : '' }}</span>
            </div>
          </div>

          <!-- Empty state -->
          <div
            v-if="fields.length === 0 && !addingField"
            class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
          >
            <v-icon class="mb-3" icon="mdi-code-braces-box" size="48" />
            <div class="text-subtitle-1">No fields</div>
            <div class="text-body-2">This hash key has no fields</div>
          </div>

          <!-- Fields table -->
          <template v-if="fields.length > 0 || addingField">
            <!-- Add field inline form -->
            <div v-if="addingField" class="add-field-form mb-3 pa-3 rounded border">
              <div class="d-flex ga-2 align-start">
                <v-text-field
                  v-model="newFieldName"
                  density="compact"
                  hide-details="auto"
                  label="Field name"
                  :rules="[v => !!v || 'Required']"
                  variant="outlined"
                />
                <v-text-field
                  v-model="newFieldValue"
                  density="compact"
                  hide-details="auto"
                  label="Field value"
                  :rules="[v => !!v || 'Required']"
                  variant="outlined"
                />
                <v-btn
                  color="primary"
                  :disabled="!newFieldName || !newFieldValue"
                  size="small"
                  variant="elevated"
                  @click="addField"
                >
                  Add
                </v-btn>
                <v-btn size="small" variant="text" @click="cancelAddField">Cancel</v-btn>
              </div>
              <div v-if="duplicateFieldWarning" class="text-caption text-warning mt-1">
                {{ duplicateFieldWarning }}
              </div>
            </div>

            <v-progress-linear v-if="loadingMore" color="primary" indeterminate />
            <v-table aria-label="Hash fields" density="compact" hover>
              <thead>
                <tr>
                  <th style="width: 200px;">Field</th>
                  <th>Value</th>
                  <th class="text-end" style="width: 100px;">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="item in fields"
                  :key="item.field"
                  :class="{ 'deleted-row': pendingDeletions.has(item.field) }"
                >
                  <td
                    class="font-weight-medium"
                    :class="{ 'text-decoration-line-through text-medium-emphasis': pendingDeletions.has(item.field) }"
                    style="font-family: monospace; font-size: 0.85rem;"
                  >
                    {{ item.field }}
                  </td>
                  <td>
                    <!-- Deleted field -->
                    <template v-if="pendingDeletions.has(item.field)">
                      <span class="text-medium-emphasis text-decoration-line-through text-body-2">
                        {{ truncateValue(item.value) }}
                      </span>
                    </template>
                    <!-- Edit mode -->
                    <template v-else-if="editingField === item.field">
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
                        @click="startEdit(item)"
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
                    <template v-if="pendingDeletions.has(item.field)">
                      <v-btn
                        color="primary"
                        icon="mdi-undo"
                        size="small"
                        title="Restore field"
                        variant="text"
                        @click="restoreField(item.field)"
                      />
                    </template>
                    <template v-else-if="editingField !== item.field">
                      <v-btn
                        color="error"
                        icon="mdi-delete-outline"
                        size="small"
                        title="Mark for deletion"
                        variant="text"
                        @click="markForDeletion(item.field)"
                      />
                    </template>
                  </td>
                </tr>
              </tbody>
            </v-table>

            <!-- Load more -->
            <div v-if="hasMoreFields" class="d-flex justify-center mt-4">
              <v-btn :loading="loadingMore" variant="outlined" @click="loadMore">
                Load More
              </v-btn>
            </div>

            <!-- Field count -->
            <div class="text-body-2 text-medium-emphasis mt-3 text-center">
              {{ activeFieldCount }} field{{ activeFieldCount === 1 ? '' : 's' }}{{ hasMoreFields ? ' (more available)' : '' }}
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
            @click="startAddField"
          >
            Add Field
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
  import type { HashFieldDto } from '@/api/redisKeys'
  import { json } from '@codemirror/lang-json'
  import { Codemirror } from 'vue-codemirror'
  import {
    createHashKey,
    getHashFields,
    getKeyMetadata,
    removeHashFields,
  } from '@/api/redisKeys'

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
  const fields = ref<HashFieldDto[]>([])
  const originalFields = ref<HashFieldDto[]>([])
  const cursor = ref(0)
  const hasMoreFields = ref(false)
  const currentTtl = ref<string | null>(null)
  const originalTtl = ref<string | null>(null)

  // Batch state
  const pendingDeletions = ref(new Set<string>())

  // Edit state (local only — no API call until Save)
  const editingField = ref<string | null>(null)
  const editValue = ref('')

  // Add field state
  const addingField = ref(false)
  const newFieldName = ref('')
  const newFieldValue = ref('')
  const duplicateFieldWarning = computed(() => {
    if (!newFieldName.value) return null
    const existing = fields.value.find(f => f.field === newFieldName.value)
    if (existing) {
      return `This will overwrite the existing value for '${newFieldName.value}'`
    }
    return null
  })

  // Abort controller for cancelling requests
  let abortController: AbortController | null = null

  // Dirty tracking
  const isDirty = computed(() => {
    if (pendingDeletions.value.size > 0) return true
    if (currentTtl.value !== originalTtl.value) return true
    if (fields.value.length !== originalFields.value.length) return true
    for (let i = 0; i < fields.value.length; i++) {
      const current = fields.value[i]
      const original = originalFields.value[i]
      if (!current || !original) return true
      if (current.field !== original.field || current.value !== original.value) return true
    }
    return false
  })

  const activeFieldCount = computed(() => {
    return fields.value.length - pendingDeletions.value.size
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
    fields.value = []
    originalFields.value = []
    cursor.value = 0
    hasMoreFields.value = false
    currentTtl.value = null
    originalTtl.value = null
    pendingDeletions.value = new Set()
    editingField.value = null
    editValue.value = ''
    addingField.value = false
    newFieldName.value = ''
    newFieldValue.value = ''
  }

  function snapshotOriginals () {
    originalFields.value = fields.value.map(f => ({ field: f.field, value: f.value }))
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
      const [fieldsResult, metadataResult] = await Promise.all([
        getHashFields(props.groupId, keyName.value, 0, undefined, signal),
        getKeyMetadata(keyName.value, props.groupId, signal),
      ])

      fields.value = fieldsResult.fields.map(f => ({ field: f.field, value: f.value }))
      cursor.value = fieldsResult.cursor
      hasMoreFields.value = fieldsResult.hasMoreResults

      currentTtl.value = msToTimespan(metadataResult.metadata.ttlMilliseconds)
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      loadError.value = error instanceof Error ? error.message : 'Failed to load hash key details'
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
      const result = await getHashFields(
        props.groupId,
        keyName.value,
        cursor.value,
        undefined,
        signal,
      )

      fields.value = [...fields.value, ...result.fields.map(f => ({ field: f.field, value: f.value }))]
      cursor.value = result.cursor
      hasMoreFields.value = result.hasMoreResults
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      saveError.value = error instanceof Error ? error.message : 'Failed to load more fields'
    } finally {
      loadingMore.value = false
    }
  }

  // Edit operations — local only, no API call
  function startEdit (item: HashFieldDto) {
    if (pendingDeletions.value.has(item.field)) return
    editingField.value = item.field
    editValue.value = isJsonValue(item.value) ? formatJsonIfValid(item.value) : item.value
  }

  function confirmEdit () {
    if (!editingField.value || !editValue.value) return
    const idx = fields.value.findIndex(f => f.field === editingField.value)
    const existing = fields.value[idx]
    if (idx !== -1 && existing) {
      fields.value[idx] = { field: existing.field, value: editValue.value }
    }
    editingField.value = null
    editValue.value = ''
  }

  function cancelEdit () {
    editingField.value = null
    editValue.value = ''
  }

  // Add field — local only
  function startAddField () {
    addingField.value = true
    newFieldName.value = ''
    newFieldValue.value = ''
  }

  function cancelAddField () {
    addingField.value = false
    newFieldName.value = ''
    newFieldValue.value = ''
  }

  function addField () {
    if (!newFieldName.value || !newFieldValue.value) return

    const existingIdx = fields.value.findIndex(f => f.field === newFieldName.value)
    if (existingIdx === -1) {
      fields.value = [{ field: newFieldName.value, value: newFieldValue.value }, ...fields.value]
    } else {
      fields.value[existingIdx] = { field: newFieldName.value, value: newFieldValue.value }
    }

    // If it was marked for deletion, restore it
    pendingDeletions.value.delete(newFieldName.value)

    addingField.value = false
    newFieldName.value = ''
    newFieldValue.value = ''
  }

  // Delete — mark locally, actual delete on Save
  function markForDeletion (fieldName: string) {
    pendingDeletions.value = new Set([...pendingDeletions.value, fieldName])
    // If editing this field, cancel the edit
    if (editingField.value === fieldName) {
      cancelEdit()
    }
  }

  function restoreField (fieldName: string) {
    const next = new Set(pendingDeletions.value)
    next.delete(fieldName)
    pendingDeletions.value = next
  }

  // Batch save — send all changes at once
  async function save () {
    saving.value = true
    saveError.value = null

    try {
      const promises: Promise<unknown>[] = []

      // Collect upserts: new fields + modified fields (excluding deletions)
      const upserts: Record<string, string> = {}
      for (const field of fields.value) {
        if (pendingDeletions.value.has(field.field)) continue
        const original = originalFields.value.find(o => o.field === field.field)
        if (!original || original.value !== field.value) {
          upserts[field.field] = field.value
        }
      }

      if (Object.keys(upserts).length > 0) {
        promises.push(
          createHashKey({
            groupId: props.groupId,
            key: keyName.value,
            fields: upserts,
            ttl: currentTtl.value,
          }),
        )
      }

      // Collect deletions: only fields that existed in original
      const deletions = [...pendingDeletions.value].filter(
        name => originalFields.value.some(o => o.field === name),
      )
      if (deletions.length > 0) {
        promises.push(
          removeHashFields({
            groupId: props.groupId,
            key: keyName.value,
            fields: deletions,
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
