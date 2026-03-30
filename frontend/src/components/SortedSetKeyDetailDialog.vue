<template>
  <v-dialog v-model="visible" max-width="900" @update:model-value="onDialogChange">
    <v-card aria-label="Sorted set key details" rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-sort-numeric-ascending" size="20" />
          Sorted Set Key Details
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

          <!-- Type + entry count -->
          <div class="d-flex align-center ga-4 mb-4">
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Type</div>
              <v-chip color="teal" size="small" variant="tonal">zset</v-chip>
            </div>
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Entries</div>
              <span class="text-body-2">{{ activeEntryCount }}{{ hasMoreEntries ? '+' : '' }}</span>
            </div>
          </div>

          <!-- Empty state -->
          <div
            v-if="entries.length === 0 && !addingEntry"
            class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
          >
            <v-icon class="mb-3" icon="mdi-sort-numeric-ascending" size="48" />
            <div class="text-subtitle-1">No entries</div>
            <div class="text-body-2">This sorted set has no entries</div>
          </div>

          <!-- Entries table -->
          <template v-if="entries.length > 0 || addingEntry">
            <!-- Add entry inline form -->
            <div v-if="addingEntry" class="add-field-form mb-3 pa-3 rounded border">
              <div class="d-flex ga-2 align-start">
                <v-text-field
                  v-model="newEntryMember"
                  density="compact"
                  hide-details="auto"
                  label="Member"
                  :rules="[v => !!v || 'Required']"
                  variant="outlined"
                />
                <v-text-field
                  v-model="newEntryScore"
                  density="compact"
                  hide-details="auto"
                  label="Score"
                  :rules="[v => !!v || 'Required', v => !Number.isNaN(Number(v)) || 'Must be a number']"
                  type="number"
                  variant="outlined"
                />
                <v-btn
                  color="primary"
                  :disabled="!newEntryMember || !newEntryScore || Number.isNaN(Number(newEntryScore))"
                  size="small"
                  variant="elevated"
                  @click="addEntry"
                >
                  Add
                </v-btn>
                <v-btn size="small" variant="text" @click="cancelAddEntry">Cancel</v-btn>
              </div>
              <div v-if="duplicateEntryWarning" class="text-caption text-warning mt-1">
                {{ duplicateEntryWarning }}
              </div>
            </div>

            <v-progress-linear v-if="loadingMore" color="primary" indeterminate />
            <v-table aria-label="Sorted set entries" density="compact" hover>
              <thead>
                <tr>
                  <th>Member</th>
                  <th class="text-end" style="width: 120px;">Score</th>
                  <th class="text-end" style="width: 100px;">Actions</th>
                </tr>
              </thead>
              <tbody>
                <template v-for="(item, index) in entries" :key="index">
                  <tr :class="{ 'deleted-row': pendingDeletions.has(item.member) }">
                    <td>
                      <!-- Deleted member -->
                      <template v-if="pendingDeletions.has(item.member)">
                        <span
                          class="text-medium-emphasis text-decoration-line-through text-body-2"
                          style="font-family: monospace; font-size: 0.85rem;"
                        >
                          {{ truncateValue(item.member) }}
                        </span>
                      </template>
                      <!-- Not editing member for this row -->
                      <template v-else-if="editingMemberIndex !== index">
                        <div
                          class="field-value-display text-body-2 py-1"
                          style="cursor: pointer; font-family: monospace; font-size: 0.85rem;"
                          :title="item.member.length > 500 ? 'Click to view/edit full value' : 'Click to edit'"
                          @click="startEditMember(index)"
                        >
                          <template v-if="isJsonValue(item.member)">
                            <pre class="json-preview ma-0" style="white-space: pre-wrap; word-break: break-all; font-size: 0.8rem;">{{ truncateValue(formatJsonIfValid(item.member)) }}</pre>
                          </template>
                          <template v-else>
                            {{ truncateValue(item.member) }}
                          </template>
                        </div>
                      </template>
                    </td>
                    <td class="text-end">
                      <!-- Deleted score -->
                      <template v-if="pendingDeletions.has(item.member)">
                        <span class="text-medium-emphasis text-decoration-line-through text-body-2">
                          {{ item.score }}
                        </span>
                      </template>
                      <!-- Editing score inline -->
                      <template v-else-if="editingScoreIndex === index">
                        <div class="d-flex align-center ga-1 justify-end">
                          <v-text-field
                            v-model="editScoreValue"
                            density="compact"
                            hide-details
                            style="max-width: 100px;"
                            type="number"
                            variant="outlined"
                            @keydown.enter="confirmEditScore"
                            @keydown.escape="cancelEditScore"
                          />
                          <v-btn
                            color="primary"
                            :disabled="editScoreValue === '' || Number.isNaN(Number(editScoreValue))"
                            icon="mdi-check"
                            size="x-small"
                            variant="text"
                            @click="confirmEditScore"
                          />
                          <v-btn
                            icon="mdi-close"
                            size="x-small"
                            variant="text"
                            @click="cancelEditScore"
                          />
                        </div>
                      </template>
                      <!-- Read mode score -->
                      <template v-else>
                        <span
                          class="text-body-2"
                          style="cursor: pointer;"
                          title="Click to edit score"
                          @click="startEditScore(index)"
                        >
                          {{ item.score }}
                        </span>
                      </template>
                    </td>
                    <td class="text-end">
                      <template v-if="pendingDeletions.has(item.member)">
                        <v-btn
                          color="primary"
                          icon="mdi-undo"
                          size="small"
                          title="Restore entry"
                          variant="text"
                          @click="restoreEntry(item.member)"
                        />
                      </template>
                      <template v-else-if="editingMemberIndex !== index && editingScoreIndex !== index">
                        <v-btn
                          color="error"
                          icon="mdi-delete-outline"
                          size="small"
                          title="Mark for deletion"
                          variant="text"
                          @click="markForDeletion(item.member)"
                        />
                      </template>
                    </td>
                  </tr>
                  <!-- Edit member mode (expanded row below) -->
                  <tr v-if="editingMemberIndex === index && !pendingDeletions.has(item.member)">
                    <td colspan="3">
                      <div class="py-2">
                        <div class="d-flex align-center justify-space-between mb-1">
                          <div class="text-body-2 font-weight-medium">Editing member</div>
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
                          Member cannot be empty
                        </div>
                        <div class="d-flex ga-2 mt-2">
                          <v-btn
                            color="primary"
                            :disabled="!editValue"
                            size="small"
                            variant="elevated"
                            @click="confirmEditMember"
                          >
                            OK
                          </v-btn>
                          <v-btn size="small" variant="text" @click="cancelEditMember">Cancel</v-btn>
                        </div>
                      </div>
                    </td>
                  </tr>
                </template>
              </tbody>
            </v-table>

            <!-- Load more -->
            <div v-if="hasMoreEntries" class="d-flex justify-center mt-4">
              <v-btn :loading="loadingMore" variant="outlined" @click="loadMore">
                Load More
              </v-btn>
            </div>

            <!-- Entry count -->
            <div class="text-body-2 text-medium-emphasis mt-3 text-center">
              {{ activeEntryCount }} entr{{ activeEntryCount === 1 ? 'y' : 'ies' }}{{ hasMoreEntries ? ' (more available)' : '' }}
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
            @click="startAddEntry"
          >
            Add Entry
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
    createSortedSetKey,
    getKeyMetadata,
    getSortedSetRange,
    removeFromSortedSet,
  } from '@/api/redisKeys'

  interface SortedSetEntry {
    member: string
    score: number
    isNew?: boolean
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
  const entries = ref<SortedSetEntry[]>([])
  const originalEntries = ref<SortedSetEntry[]>([])
  const loadedCount = ref(0)
  const hasMoreEntries = ref(false)
  const pageSize = 100
  const currentTtl = ref<string | null>(null)
  const originalTtl = ref<string | null>(null)

  // Batch state
  const pendingDeletions = ref(new Set<string>())

  // Edit member state
  const editingMemberIndex = ref<number | null>(null)
  const editValue = ref('')

  // Edit score state
  const editingScoreIndex = ref<number | null>(null)
  const editScoreValue = ref('')

  // Add entry state
  const addingEntry = ref(false)
  const newEntryMember = ref('')
  const newEntryScore = ref('')
  const duplicateEntryWarning = computed(() => {
    if (!newEntryMember.value) return null
    const existing = entries.value.find(e => e.member === newEntryMember.value)
    if (existing) {
      return `This will update the score for existing member '${newEntryMember.value}'`
    }
    return null
  })

  // Abort controller for cancelling requests
  let abortController: AbortController | null = null

  // Dirty tracking
  const isDirty = computed(() => {
    if (pendingDeletions.value.size > 0) return true
    if (currentTtl.value !== originalTtl.value) return true
    if (entries.value.length !== originalEntries.value.length) return true
    for (let i = 0; i < entries.value.length; i++) {
      const current = entries.value[i]
      const original = originalEntries.value[i]
      if (!current || !original) return true
      if (current.member !== original.member || current.score !== original.score) return true
    }
    return false
  })

  const activeEntryCount = computed(() => {
    return entries.value.length - pendingDeletions.value.size
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
    entries.value = []
    originalEntries.value = []
    loadedCount.value = 0
    hasMoreEntries.value = false
    currentTtl.value = null
    originalTtl.value = null
    pendingDeletions.value = new Set()
    editingMemberIndex.value = null
    editValue.value = ''
    editingScoreIndex.value = null
    editScoreValue.value = ''
    addingEntry.value = false
    newEntryMember.value = ''
    newEntryScore.value = ''
  }

  function snapshotOriginals () {
    originalEntries.value = entries.value.map(e => ({ member: e.member, score: e.score }))
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
      const [rangeResult, metadataResult] = await Promise.all([
        getSortedSetRange(props.groupId, keyName.value, 0, pageSize - 1, signal),
        getKeyMetadata(keyName.value, props.groupId, signal),
      ])

      entries.value = rangeResult.entries.map(e => ({ member: e.member, score: e.score }))
      loadedCount.value = rangeResult.entries.length
      hasMoreEntries.value = rangeResult.entries.length >= pageSize

      currentTtl.value = msToTimespan(metadataResult.metadata.ttlMilliseconds)
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      loadError.value = error instanceof Error ? error.message : 'Failed to load sorted set details'
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
      const result = await getSortedSetRange(
        props.groupId,
        keyName.value,
        loadedCount.value,
        loadedCount.value + pageSize - 1,
        signal,
      )

      const newEntries = result.entries.map(e => ({ member: e.member, score: e.score }))
      entries.value = [...entries.value, ...newEntries]
      loadedCount.value += newEntries.length
      hasMoreEntries.value = newEntries.length >= pageSize
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      saveError.value = error instanceof Error ? error.message : 'Failed to load more entries'
    } finally {
      loadingMore.value = false
    }
  }

  // Edit member operations
  function startEditMember (index: number) {
    const entry = entries.value[index]
    if (!entry || pendingDeletions.value.has(entry.member)) return
    cancelEditScore()
    editingMemberIndex.value = index
    editValue.value = isJsonValue(entry.member) ? formatJsonIfValid(entry.member) : entry.member
  }

  function confirmEditMember () {
    if (editingMemberIndex.value === null || !editValue.value) return
    const entry = entries.value[editingMemberIndex.value]
    if (entry) {
      entries.value[editingMemberIndex.value] = {
        ...entry,
        member: editValue.value,
      }
    }
    editingMemberIndex.value = null
    editValue.value = ''
  }

  function cancelEditMember () {
    editingMemberIndex.value = null
    editValue.value = ''
  }

  // Edit score operations
  function startEditScore (index: number) {
    const entry = entries.value[index]
    if (!entry || pendingDeletions.value.has(entry.member)) return
    cancelEditMember()
    editingScoreIndex.value = index
    editScoreValue.value = String(entry.score)
  }

  function confirmEditScore () {
    if (editingScoreIndex.value === null) return
    const parsed = Number(editScoreValue.value)
    if (Number.isNaN(parsed)) return
    const entry = entries.value[editingScoreIndex.value]
    if (entry) {
      entries.value[editingScoreIndex.value] = {
        ...entry,
        score: parsed,
      }
    }
    editingScoreIndex.value = null
    editScoreValue.value = ''
  }

  function cancelEditScore () {
    editingScoreIndex.value = null
    editScoreValue.value = ''
  }

  // Add entry — local only
  function startAddEntry () {
    addingEntry.value = true
    newEntryMember.value = ''
    newEntryScore.value = ''
  }

  function cancelAddEntry () {
    addingEntry.value = false
    newEntryMember.value = ''
    newEntryScore.value = ''
  }

  function addEntry () {
    if (!newEntryMember.value || !newEntryScore.value) return
    const score = Number(newEntryScore.value)
    if (Number.isNaN(score)) return

    const existingIdx = entries.value.findIndex(e => e.member === newEntryMember.value)
    if (existingIdx === -1) {
      entries.value = [{ member: newEntryMember.value, score, isNew: true }, ...entries.value]
    } else {
      entries.value[existingIdx] = {
        ...entries.value[existingIdx]!,
        member: newEntryMember.value,
        score,
      }
    }

    // If it was marked for deletion, restore it
    pendingDeletions.value.delete(newEntryMember.value)

    addingEntry.value = false
    newEntryMember.value = ''
    newEntryScore.value = ''
  }

  // Delete — mark locally, actual delete on Save
  function markForDeletion (memberName: string) {
    pendingDeletions.value = new Set([...pendingDeletions.value, memberName])
    // If editing this member, cancel the edit
    const entry = entries.value.find(e => e.member === memberName)
    if (entry) {
      const idx = entries.value.indexOf(entry)
      if (editingMemberIndex.value === idx) {
        cancelEditMember()
      }
      if (editingScoreIndex.value === idx) {
        cancelEditScore()
      }
    }
  }

  function restoreEntry (memberName: string) {
    const next = new Set(pendingDeletions.value)
    next.delete(memberName)
    pendingDeletions.value = next
  }

  // Batch save — send all changes at once
  async function save () {
    saving.value = true
    saveError.value = null

    try {
      const promises: Promise<unknown>[] = []

      // Collect upserts: new entries + modified entries (excluding deletions)
      // Also handle member renames: if member changed, we need to remove old + add new
      const upserts: { member: string, score: number }[] = []
      const memberRemovals: string[] = []

      for (let i = 0; i < entries.value.length; i++) {
        const entry = entries.value[i]!
        if (pendingDeletions.value.has(entry.member)) continue

        const original = originalEntries.value[i]
        if (!original) {
          // New entry (added or shifted)
          upserts.push({ member: entry.member, score: entry.score })
        } else if (original.member !== entry.member) {
          // Member was renamed — remove old, add new
          memberRemovals.push(original.member)
          upserts.push({ member: entry.member, score: entry.score })
        } else if (original.score !== entry.score) {
          // Score changed
          upserts.push({ member: entry.member, score: entry.score })
        }
      }

      if (upserts.length > 0) {
        promises.push(
          createSortedSetKey({
            groupId: props.groupId,
            key: keyName.value,
            entries: upserts,
            ttl: currentTtl.value,
          }),
        )
      }

      // Collect deletions: pending deletions that existed in original + member renames
      const deletions = [
        ...[...pendingDeletions.value].filter(
          name => originalEntries.value.some(o => o.member === name),
        ),
        ...memberRemovals,
      ]
      if (deletions.length > 0) {
        promises.push(
          removeFromSortedSet({
            groupId: props.groupId,
            key: keyName.value,
            members: deletions,
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
