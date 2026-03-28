<template>
  <v-dialog v-model="visible" max-width="700" @update:model-value="onDialogChange">
    <v-card rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-text" size="20" />
          String Key Details
        </div>
        <v-btn icon="mdi-close" size="small" variant="text" @click="close" />
      </div>

      <!-- Loading state -->
      <v-card-text v-if="loading">
        <v-skeleton-loader class="mb-4" type="text" />
        <v-skeleton-loader class="mb-4" type="text" />
        <v-skeleton-loader class="mb-4" type="paragraph" />
        <v-skeleton-loader type="text" />
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

        <!-- Large value warning -->
        <v-card-text v-if="isLargeValue" class="pb-0">
          <v-alert type="warning" variant="tonal">
            This value is larger than 1 MB. Editor performance may be affected.
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

          <!-- Type (read-only chip) -->
          <div class="mb-4">
            <div class="text-body-2 font-weight-medium mb-1">Type</div>
            <v-chip color="blue" size="small" variant="tonal">string</v-chip>
          </div>

          <!-- Value (editable CodeMirror editor) -->
          <div class="mb-4">
            <div class="d-flex align-center justify-space-between mb-1">
              <div class="text-body-2 font-weight-medium">Value</div>
              <v-btn
                prepend-icon="mdi-code-json"
                size="small"
                variant="text"
                @click="formatCurrentJson"
              >
                Format JSON
              </v-btn>
            </div>
            <div class="codemirror-wrapper">
              <Codemirror
                v-model="currentValue"
                :extensions="editorExtensions"
                placeholder="(empty)"
                :style="{ minHeight: '120px', maxHeight: '400px' }"
              />
            </div>
          </div>

          <!-- TTL (editable) -->
          <div class="mb-4">
            <TtlPicker v-model="currentTtl" />
          </div>
        </v-card-text>

        <!-- Actions -->
        <v-card-actions class="card-footer-separated">
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
  import { createStringKey, getKeyMetadata, getStringKeyValue } from '@/api/redisKeys'

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
  const saving = ref(false)
  const saveError = ref<string | null>(null)

  // Data
  const currentValue = ref('')
  const currentTtl = ref<string | null>(null)
  const originalValue = ref('')
  const originalTtl = ref<string | null>(null)

  // Large value warning threshold: 1 MB
  const isLargeValue = computed(() => currentValue.value.length > 1_048_576)

  // JSON detection
  const isJson = computed(() => {
    if (!currentValue.value) return false
    try {
      JSON.parse(currentValue.value)
      return true
    } catch {
      return false
    }
  })

  // CodeMirror extensions: apply JSON language when value is valid JSON
  const editorExtensions = computed(() => {
    return isJson.value ? [json()] : []
  })

  // Dirty state tracking
  const isDirty = computed(() => {
    return currentValue.value !== originalValue.value || currentTtl.value !== originalTtl.value
  })

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

  function formatJsonIfValid (value: string): string {
    try {
      const parsed = JSON.parse(value)
      return JSON.stringify(parsed, null, 2)
    } catch {
      return value
    }
  }

  function formatCurrentJson () {
    try {
      const parsed = JSON.parse(currentValue.value)
      currentValue.value = JSON.stringify(parsed, null, 2)
      saveError.value = null
    } catch {
      saveError.value = 'Current value is not valid JSON'
    }
  }

  function resetState () {
    loading.value = true
    loadError.value = null
    saving.value = false
    saveError.value = null
    currentValue.value = ''
    currentTtl.value = null
    originalValue.value = ''
    originalTtl.value = null
  }

  async function fetchData () {
    loading.value = true
    loadError.value = null

    const controller = new AbortController()

    try {
      const [valueResult, metadataResult] = await Promise.all([
        getStringKeyValue(props.groupId, keyName.value, controller.signal),
        getKeyMetadata(keyName.value, props.groupId, controller.signal),
      ])

      const rawValue = valueResult.value ?? ''
      const formatted = formatJsonIfValid(rawValue)

      currentValue.value = formatted
      originalValue.value = formatted

      const ttl = msToTimespan(metadataResult.metadata.ttlMilliseconds)
      currentTtl.value = ttl
      originalTtl.value = ttl
    } catch (error) {
      loadError.value = error instanceof Error ? error.message : 'Failed to load key details'
    } finally {
      loading.value = false
    }
  }

  async function save () {
    saving.value = true
    saveError.value = null

    try {
      await createStringKey({
        groupId: props.groupId,
        key: keyName.value,
        value: currentValue.value,
        ttl: currentTtl.value,
      })

      close()
    } catch (error) {
      saveError.value = error instanceof Error ? error.message : 'Failed to save changes'
    } finally {
      saving.value = false
    }
  }

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
</style>
