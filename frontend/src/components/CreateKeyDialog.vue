<template>
  <v-dialog v-model="dialogOpen" max-width="560" persistent>
    <v-card rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="success" icon="mdi-key-plus" size="20" />
          New {{ typeLabel }} Key
        </div>
        <v-btn icon="mdi-close" size="small" variant="text" @click="close" />
      </div>

      <v-card-text>
        <v-alert
          v-if="error"
          class="mb-4"
          closable
          type="error"
          variant="tonal"
          @click:close="error = null"
        >
          {{ error }}
        </v-alert>

        <!-- Key name (common to all types) -->
        <v-text-field
          v-model="keyName"
          class="mb-3"
          density="compact"
          hide-details="auto"
          label="Key name"
          :rules="[v => !!v || 'Key name is required']"
          variant="outlined"
        />

        <!-- String form -->
        <template v-if="type === 'string'">
          <div class="d-flex align-center justify-space-between mb-1">
            <div class="text-body-2 font-weight-medium">Value</div>
            <v-btn
              prepend-icon="mdi-code-json"
              size="small"
              variant="text"
              @click="formatStringJson"
            >
              Format JSON
            </v-btn>
          </div>

          <div class="codemirror-wrapper mb-3">
            <Codemirror
              v-model="stringValue"
              :extensions="stringEditorExtensions"
              placeholder="Enter value"
              :style="{ minHeight: '120px', maxHeight: '320px' }"
            />
          </div>
        </template>

        <!-- Hash form -->
        <template v-if="type === 'hash'">
          <div class="text-body-2 font-weight-medium mb-2">Fields</div>
          <div v-for="(field, i) in hashFields" :key="i" class="d-flex ga-2 mb-2 align-center">
            <v-text-field
              v-model="field.name"
              density="compact"
              hide-details
              label="Field"
              variant="outlined"
            />
            <v-text-field
              v-model="field.value"
              density="compact"
              hide-details
              label="Value"
              variant="outlined"
            />
            <v-btn
              :disabled="hashFields.length <= 1"
              icon="mdi-close"
              size="x-small"
              variant="text"
              @click="hashFields.splice(i, 1)"
            />
          </div>
          <v-btn
            class="mb-3"
            prepend-icon="mdi-plus"
            size="small"
            variant="text"
            @click="hashFields.push({ name: '', value: '' })"
          >
            Add Field
          </v-btn>
        </template>

        <!-- List form -->
        <template v-if="type === 'list'">
          <v-select
            v-model="listDirection"
            class="mb-3"
            density="compact"
            hide-details
            :items="[
              { title: 'Right (RPUSH)', value: 1 },
              { title: 'Left (LPUSH)', value: 0 },
            ]"
            label="Push direction"
            variant="outlined"
          />
          <div class="text-body-2 font-weight-medium mb-2">Values</div>
          <div v-for="(_, i) in listValues" :key="i" class="d-flex ga-2 mb-2 align-center">
            <v-text-field
              v-model="listValues[i]"
              density="compact"
              hide-details
              :label="`Value ${i + 1}`"
              variant="outlined"
            />
            <v-btn
              :disabled="listValues.length <= 1"
              icon="mdi-close"
              size="x-small"
              variant="text"
              @click="listValues.splice(i, 1)"
            />
          </div>
          <v-btn
            class="mb-3"
            prepend-icon="mdi-plus"
            size="small"
            variant="text"
            @click="listValues.push('')"
          >
            Add Value
          </v-btn>
        </template>

        <!-- Set form -->
        <template v-if="type === 'set'">
          <div class="text-body-2 font-weight-medium mb-2">Members</div>
          <div v-for="(_, i) in setMembers" :key="i" class="d-flex ga-2 mb-2 align-center">
            <v-text-field
              v-model="setMembers[i]"
              density="compact"
              hide-details
              :label="`Member ${i + 1}`"
              variant="outlined"
            />
            <v-btn
              :disabled="setMembers.length <= 1"
              icon="mdi-close"
              size="x-small"
              variant="text"
              @click="setMembers.splice(i, 1)"
            />
          </div>
          <v-btn
            class="mb-3"
            prepend-icon="mdi-plus"
            size="small"
            variant="text"
            @click="setMembers.push('')"
          >
            Add Member
          </v-btn>
        </template>

        <!-- Sorted Set form -->
        <template v-if="type === 'zset'">
          <div class="text-body-2 font-weight-medium mb-2">Entries</div>
          <div v-for="(entry, i) in zsetEntries" :key="i" class="d-flex ga-2 mb-2 align-center">
            <v-text-field
              v-model="entry.member"
              density="compact"
              hide-details
              label="Member"
              variant="outlined"
            />
            <v-text-field
              v-model.number="entry.score"
              density="compact"
              hide-details
              label="Score"
              style="max-width: 120px"
              type="number"
              variant="outlined"
            />
            <v-btn
              :disabled="zsetEntries.length <= 1"
              icon="mdi-close"
              size="x-small"
              variant="text"
              @click="zsetEntries.splice(i, 1)"
            />
          </div>
          <v-btn
            class="mb-3"
            prepend-icon="mdi-plus"
            size="small"
            variant="text"
            @click="zsetEntries.push({ member: '', score: 0 })"
          >
            Add Entry
          </v-btn>
        </template>

        <!-- TTL picker (common to all types) -->
        <TtlPicker v-model="ttl" />
      </v-card-text>

      <v-card-actions class="card-footer-separated">
        <v-spacer />
        <v-btn variant="text" @click="close">Cancel</v-btn>
        <v-btn
          color="success"
          :disabled="!isValid"
          :loading="submitting"
          variant="elevated"
          @click="submit"
        >
          Create
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
  import { json } from '@codemirror/lang-json'
  import { Codemirror } from 'vue-codemirror'
  import {
    createHashKey,
    createListKey,
    createSetKey,
    createSortedSetKey,
    createStringKey,
  } from '@/api/redisKeys'
  import TtlPicker from '@/components/TtlPicker.vue'

  export type RedisKeyType = 'string' | 'hash' | 'list' | 'set' | 'zset'

  const props = defineProps<{
    groupId: string
  }>()

  const emit = defineEmits<{
    created: []
  }>()

  const dialogOpen = ref(false)
  const type = ref<RedisKeyType>('string')
  const submitting = ref(false)
  const error = ref<string | null>(null)

  // Common fields
  const keyName = ref('')
  const ttl = ref<string | null>(null)

  // String
  const stringValue = ref('')

  // Hash
  const hashFields = ref([{ name: '', value: '' }])

  // List
  const listDirection = ref(1)
  const listValues = ref([''])

  // Set
  const setMembers = ref([''])

  // Sorted Set
  const zsetEntries = ref([{ member: '', score: 0 }])

  const typeLabel = computed(() => {
    const labels: Record<RedisKeyType, string> = {
      string: 'String',
      hash: 'Hash',
      list: 'List',
      set: 'Set',
      zset: 'Sorted Set',
    }
    return labels[type.value]
  })

  const isStringJson = computed(() => {
    if (!stringValue.value) return false

    try {
      JSON.parse(stringValue.value)
      return true
    } catch {
      return false
    }
  })

  const stringEditorExtensions = computed(() => {
    return type.value === 'string' && isStringJson.value ? [json()] : []
  })

  const isValid = computed(() => {
    if (!keyName.value.trim()) return false
    switch (type.value) {
      case 'string': {
        return !!stringValue.value
      }
      case 'hash': {
        return hashFields.value.some(f => f.name.trim() && f.value.trim())
      }
      case 'list': {
        return listValues.value.some(v => v.trim())
      }
      case 'set': {
        return setMembers.value.some(m => m.trim())
      }
      case 'zset': {
        return zsetEntries.value.some(e => e.member.trim())
      }
      default: {
        return false
      }
    }
  })

  function open (keyType: RedisKeyType) {
    type.value = keyType
    resetForm()
    dialogOpen.value = true
  }

  function close () {
    dialogOpen.value = false
    error.value = null
  }

  function resetForm () {
    keyName.value = ''
    ttl.value = null
    error.value = null
    stringValue.value = ''
    hashFields.value = [{ name: '', value: '' }]
    listDirection.value = 1
    listValues.value = ['']
    setMembers.value = ['']
    zsetEntries.value = [{ member: '', score: 0 }]
  }

  function formatStringJson () {
    try {
      const parsed = JSON.parse(stringValue.value)
      stringValue.value = JSON.stringify(parsed, null, 2)
      error.value = null
    } catch {
      error.value = 'String value is not valid JSON'
    }
  }

  async function submit () {
    submitting.value = true
    error.value = null

    try {
      const groupId = props.groupId
      const key = keyName.value.trim()

      switch (type.value) {
        case 'string': {
          await createStringKey({ groupId, key, value: stringValue.value, ttl: ttl.value })
          break
        }
        case 'hash': {
          const fields: Record<string, string> = {}
          for (const f of hashFields.value) {
            if (f.name.trim()) fields[f.name.trim()] = f.value
          }
          await createHashKey({ groupId, key, fields, ttl: ttl.value })
          break
        }
        case 'list': {
          await createListKey({
            groupId,
            key,
            values: listValues.value.filter(v => v.trim()),
            direction: listDirection.value,
            ttl: ttl.value,
          })
          break
        }
        case 'set': {
          await createSetKey({
            groupId,
            key,
            members: setMembers.value.filter(m => m.trim()),
            ttl: ttl.value,
          })
          break
        }
        case 'zset': {
          await createSortedSetKey({
            groupId,
            key,
            entries: zsetEntries.value.filter(e => e.member.trim()),
            ttl: ttl.value,
          })
          break
        }
      }

      emit('created')
      close()
    } catch (error_) {
      error.value = error_ instanceof Error ? error_.message : 'Failed to create key'
    } finally {
      submitting.value = false
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
