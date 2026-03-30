<template>
  <v-dialog v-model="visible" max-width="900" @update:model-value="onDialogChange">
    <v-card aria-label="Set key details" rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-set-all" size="20" />
          Set Key Details
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

          <!-- Type + member count -->
          <div class="d-flex align-center ga-4 mb-4">
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Type</div>
              <v-chip color="purple" size="small" variant="tonal">set</v-chip>
            </div>
            <div>
              <div class="text-body-2 font-weight-medium mb-1">Members</div>
              <span class="text-body-2">{{ activeMemberCount }}{{ hasMoreResults ? '+' : '' }}</span>
            </div>
          </div>

          <!-- Empty state -->
          <div
            v-if="members.length === 0 && !addingMember"
            class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
          >
            <v-icon class="mb-3" icon="mdi-set-all" size="48" />
            <div class="text-subtitle-1">No members</div>
            <div class="text-body-2">This set has no members</div>
          </div>

          <!-- Members table -->
          <template v-if="members.length > 0 || addingMember">
            <!-- Add member inline form -->
            <div v-if="addingMember" class="add-field-form mb-3 pa-3 rounded border">
              <div class="d-flex ga-2 align-start">
                <v-text-field
                  v-model="newMemberValue"
                  density="compact"
                  hide-details="auto"
                  label="Member value"
                  :rules="[v => !!v || 'Required']"
                  variant="outlined"
                />
                <v-btn
                  color="primary"
                  :disabled="!newMemberValue"
                  size="small"
                  variant="elevated"
                  @click="addMember"
                >
                  Add
                </v-btn>
                <v-btn size="small" variant="text" @click="cancelAddMember">Cancel</v-btn>
              </div>
              <div v-if="duplicateMemberWarning" class="text-caption text-warning mt-1">
                {{ duplicateMemberWarning }}
              </div>
            </div>

            <v-progress-linear v-if="loadingMore" color="primary" indeterminate />
            <v-table aria-label="Set members" density="compact" hover>
              <thead>
                <tr>
                  <th>Value</th>
                  <th class="text-end" style="width: 100px;">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(member, index) in members"
                  :key="index"
                  :class="{ 'deleted-row': pendingDeletions.has(member) }"
                >
                  <td>
                    <!-- Deleted member -->
                    <template v-if="pendingDeletions.has(member)">
                      <span class="text-medium-emphasis text-decoration-line-through text-body-2">
                        {{ truncateValue(member) }}
                      </span>
                    </template>
                    <!-- Edit mode -->
                    <template v-else-if="editingIndex === index">
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
                        :title="member.length > 500 ? 'Click to view/edit full value' : 'Click to edit'"
                        @click="startEdit(index)"
                      >
                        <template v-if="isJsonValue(member)">
                          <pre class="json-preview ma-0" style="white-space: pre-wrap; word-break: break-all; font-size: 0.8rem;">{{ truncateValue(formatJsonIfValid(member)) }}</pre>
                        </template>
                        <template v-else>
                          {{ truncateValue(member) }}
                        </template>
                      </div>
                    </template>
                  </td>
                  <td class="text-end">
                    <template v-if="pendingDeletions.has(member)">
                      <v-btn
                        color="primary"
                        icon="mdi-undo"
                        size="small"
                        title="Restore member"
                        variant="text"
                        @click="restoreField(member)"
                      />
                    </template>
                    <template v-else-if="editingIndex !== index">
                      <v-btn
                        color="error"
                        icon="mdi-delete-outline"
                        size="small"
                        title="Mark for deletion"
                        variant="text"
                        @click="markForDeletion(member, index)"
                      />
                    </template>
                  </td>
                </tr>
              </tbody>
            </v-table>

            <!-- Load more -->
            <div v-if="hasMoreResults" class="d-flex justify-center mt-4">
              <v-btn :loading="loadingMore" variant="outlined" @click="loadMore">
                Load More
              </v-btn>
            </div>

            <!-- Member count -->
            <div class="text-body-2 text-medium-emphasis mt-3 text-center">
              {{ activeMemberCount }} member{{ activeMemberCount === 1 ? '' : 's' }}{{ hasMoreResults ? ' (more available)' : '' }}
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
            @click="startAddMember"
          >
            Add Member
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
    createSetKey,
    getKeyMetadata,
    getSetMembers,
    removeFromSet,
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
  const members = ref<string[]>([])
  const originalMembers = ref<string[]>([])
  const cursor = ref(0)
  const hasMoreResults = ref(false)
  const currentTtl = ref<string | null>(null)
  const originalTtl = ref<string | null>(null)

  // Batch state
  const pendingDeletions = ref(new Set<string>())

  // Edit state (local only — no API call until Save)
  const editingIndex = ref<number | null>(null)
  const editValue = ref('')

  // Add member state
  const addingMember = ref(false)
  const newMemberValue = ref('')
  const duplicateMemberWarning = computed(() => {
    if (!newMemberValue.value) return null
    const existing = members.value.find(m => m === newMemberValue.value)
    if (existing !== undefined) {
      return `This member already exists in the set`
    }
    return null
  })

  // Abort controller for cancelling requests
  let abortController: AbortController | null = null

  // Dirty tracking
  const isDirty = computed(() => {
    if (pendingDeletions.value.size > 0) return true
    if (currentTtl.value !== originalTtl.value) return true
    if (members.value.length !== originalMembers.value.length) return true
    for (let i = 0; i < members.value.length; i++) {
      if (members.value[i] !== originalMembers.value[i]) return true
    }
    return false
  })

  const activeMemberCount = computed(() => {
    return members.value.length - pendingDeletions.value.size
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
    members.value = []
    originalMembers.value = []
    cursor.value = 0
    hasMoreResults.value = false
    currentTtl.value = null
    originalTtl.value = null
    pendingDeletions.value = new Set()
    editingIndex.value = null
    editValue.value = ''
    addingMember.value = false
    newMemberValue.value = ''
  }

  function snapshotOriginals () {
    originalMembers.value = [...members.value]
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
      const [membersResult, metadataResult] = await Promise.all([
        getSetMembers(props.groupId, keyName.value, 0, undefined, signal),
        getKeyMetadata(keyName.value, props.groupId, signal),
      ])

      members.value = [...membersResult.members]
      cursor.value = membersResult.cursor
      hasMoreResults.value = membersResult.hasMoreResults

      currentTtl.value = msToTimespan(metadataResult.metadata.ttlMilliseconds)
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      loadError.value = error instanceof Error ? error.message : 'Failed to load set key details'
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
      const result = await getSetMembers(
        props.groupId,
        keyName.value,
        cursor.value,
        undefined,
        signal,
      )

      members.value = [...members.value, ...result.members]
      cursor.value = result.cursor
      hasMoreResults.value = result.hasMoreResults
      snapshotOriginals()
    } catch (error) {
      if (signal.aborted) return
      saveError.value = error instanceof Error ? error.message : 'Failed to load more members'
    } finally {
      loadingMore.value = false
    }
  }

  // Edit operations — local only, no API call
  function startEdit (index: number) {
    const member = members.value[index]
    if (member === undefined || pendingDeletions.value.has(member)) return
    editingIndex.value = index
    editValue.value = isJsonValue(member) ? formatJsonIfValid(member) : member
  }

  function confirmEdit () {
    if (editingIndex.value === null || !editValue.value) return
    const idx = editingIndex.value
    if (idx >= 0 && idx < members.value.length) {
      const updated = [...members.value]
      updated[idx] = editValue.value
      members.value = updated
    }
    editingIndex.value = null
    editValue.value = ''
  }

  function cancelEdit () {
    editingIndex.value = null
    editValue.value = ''
  }

  // Add member — local only
  function startAddMember () {
    addingMember.value = true
    newMemberValue.value = ''
  }

  function cancelAddMember () {
    addingMember.value = false
    newMemberValue.value = ''
  }

  function addMember () {
    if (!newMemberValue.value) return

    members.value = [newMemberValue.value, ...members.value]

    // If it was marked for deletion, restore it
    pendingDeletions.value.delete(newMemberValue.value)

    addingMember.value = false
    newMemberValue.value = ''
  }

  // Delete — mark locally, actual delete on Save
  function markForDeletion (memberValue: string, index: number) {
    pendingDeletions.value = new Set([...pendingDeletions.value, memberValue])
    // If editing this member, cancel the edit
    if (editingIndex.value === index) {
      cancelEdit()
    }
  }

  function restoreField (memberValue: string) {
    const next = new Set(pendingDeletions.value)
    next.delete(memberValue)
    pendingDeletions.value = next
  }

  // Batch save — send all changes at once
  async function save () {
    saving.value = true
    saveError.value = null

    try {
      const promises: Promise<unknown>[] = []
      const originalSet = new Set(originalMembers.value)

      // Collect additions: members NOT in originalMembers AND NOT in pendingDeletions
      const additions: string[] = []
      // Collect edited values (new values replacing old ones)
      const editedNewValues: string[] = []
      const editedOldValues: string[] = []

      for (let i = 0; i < members.value.length; i++) {
        const member = members.value[i]
        if (member === undefined) continue
        if (pendingDeletions.value.has(member)) continue

        const original = originalMembers.value[i]
        if (!originalSet.has(member) && (original === undefined || original !== member)) {
          // This is either a new member or an edited member
          if (original !== undefined && original !== member) {
            // Edited: old value removed, new value added
            editedOldValues.push(original)
            editedNewValues.push(member)
          } else {
            additions.push(member)
          }
        }
      }

      // All members to add (new additions + new values from edits)
      const membersToAdd = [...additions, ...editedNewValues]
      if (membersToAdd.length > 0) {
        promises.push(
          createSetKey({
            groupId: props.groupId,
            key: keyName.value,
            members: membersToAdd,
            ttl: currentTtl.value,
          }),
        )
      } else if (currentTtl.value !== originalTtl.value) {
        // TTL-only change: send createSetKey with empty members to update TTL
        promises.push(
          createSetKey({
            groupId: props.groupId,
            key: keyName.value,
            members: [],
            ttl: currentTtl.value,
          }),
        )
      }

      // Collect removals: pendingDeletions that existed in original, PLUS old values of edited members
      const removals = [...pendingDeletions.value].filter(m => originalSet.has(m))
      const allRemovals = [...removals, ...editedOldValues]
      if (allRemovals.length > 0) {
        promises.push(
          removeFromSet({
            groupId: props.groupId,
            key: keyName.value,
            members: allRemovals,
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
