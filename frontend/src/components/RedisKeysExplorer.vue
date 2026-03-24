<template>
  <div>
    <!-- Search form -->
    <div class="search-bar mb-4">
      <v-text-field
        v-model="pattern"
        density="compact"
        hide-details
        placeholder="Key pattern (e.g. user:* or *)"
        prepend-inner-icon="mdi-magnify"
        variant="solo-filled"
        flat
        @keydown.enter="search"
      >
        <template #append-inner>
          <v-btn
            color="primary"
            :loading="loading"
            rounded="lg"
            size="small"
            variant="elevated"
            @click="search"
          >
            <v-icon icon="mdi-arrow-right" size="18" />
          </v-btn>
        </template>
      </v-text-field>
    </div>

    <!-- Error -->
    <v-alert v-if="error" class="mb-4" closable type="error" variant="tonal" @click:close="error = null">
      <div class="d-flex align-center justify-space-between flex-wrap ga-2">
        <span>{{ error }}</span>
        <v-btn color="primary" size="small" variant="text" @click="search">Retry</v-btn>
      </div>
    </v-alert>

    <!-- Loading (first load) -->
    <v-skeleton-loader v-if="loading && keys.length === 0 && !error" type="table-thead, table-tbody" />

    <!-- Empty state -->
    <div
      v-else-if="!loading && keys.length === 0 && !error && searched"
      class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
    >
      <v-icon class="mb-3" icon="mdi-key-remove" size="48" />
      <div class="text-subtitle-1">No keys found</div>
      <div class="text-body-2">No keys match the pattern "{{ lastSearchedPattern }}"</div>
    </div>

    <!-- Results table -->
    <template v-else-if="keys.length > 0 || (loading && searched)">
      <v-progress-linear v-if="loading" color="primary" indeterminate />
      <v-table density="compact" hover>
        <thead>
          <tr>
            <th>Key</th>
            <th style="width: 120px;">Type</th>
            <th style="width: 140px;">TTL</th>
            <th class="text-end" style="width: 80px;">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in keys" :key="item.key">
            <td class="font-weight-medium" style="font-family: monospace; font-size: 0.85rem;">
              {{ item.key }}
            </td>
            <td>
              <v-chip :color="typeColor(item.type)" size="small" variant="tonal">
                {{ item.type }}
              </v-chip>
            </td>
            <td class="text-body-2 text-medium-emphasis">
              {{ formatTtl(item.ttlMilliseconds) }}
            </td>
            <td class="text-end">
              <v-btn
                color="error"
                icon="mdi-delete-outline"
                size="small"
                title="Delete key"
                variant="text"
                @click="confirmDelete(item)"
              />
            </td>
          </tr>
        </tbody>
      </v-table>

      <!-- Load more -->
      <div v-if="hasMoreResults" class="d-flex justify-center mt-4">
        <v-btn :loading="loading" variant="outlined" @click="loadMore">
          Load More
        </v-btn>
      </div>

      <!-- Result count -->
      <div class="text-body-2 text-medium-emphasis mt-3 text-center">
        {{ keys.length }} key{{ keys.length === 1 ? '' : 's' }} loaded{{ hasMoreResults ? ' (more available)' : '' }}
      </div>
    </template>

    <!-- Delete confirmation dialog -->
    <v-dialog v-model="showDeleteDialog" max-width="440">
      <v-card rounded="lg">
        <div class="card-header-separated">
          <div class="card-header-title">
            <v-icon color="error" icon="mdi-delete-alert" size="20" />
            Delete Key
          </div>
        </div>
        <v-card-text>
          Are you sure you want to delete
          <strong style="font-family: monospace;">{{ deleteTarget?.key }}</strong>?
          This action cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="cancelDelete">Cancel</v-btn>
          <v-btn color="error" :loading="deleting" variant="elevated" @click="executeDelete">
            Delete
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
  import { searchRedisKeys, deleteRedisKey } from '@/api/redisKeys'
  import type { RedisKeyDto } from '@/api/redisKeys'

  const props = defineProps<{ groupId: string }>()

  const pattern = ref('*')
  const keys = ref<RedisKeyDto[]>([])
  const continuationToken = ref<string | null>(null)
  const hasMoreResults = ref(false)
  const loading = ref(false)
  const error = ref<string | null>(null)
  const searched = ref(false)
  const lastSearchedPattern = ref('*')

  const deleteTarget = ref<RedisKeyDto | null>(null)
  const showDeleteDialog = ref(false)
  const deleting = ref(false)

  function typeColor (type: string): string {
    switch (type.toLowerCase()) {
      case 'string': return 'blue'
      case 'hash': return 'orange'
      case 'list': return 'green'
      case 'set': return 'purple'
      case 'zset': return 'teal'
      default: return 'grey'
    }
  }

  function formatTtl (ms: number | null): string {
    if (ms === null || ms < 0) return 'No expiry'
    if (ms < 1000) return '< 1s'

    const totalSeconds = Math.floor(ms / 1000)
    const days = Math.floor(totalSeconds / 86400)
    const hours = Math.floor((totalSeconds % 86400) / 3600)
    const minutes = Math.floor((totalSeconds % 3600) / 60)
    const seconds = totalSeconds % 60

    const parts: string[] = []
    if (days > 0) parts.push(`${days}d`)
    if (hours > 0) parts.push(`${hours}h`)
    if (minutes > 0) parts.push(`${minutes}m`)
    if (seconds > 0 || parts.length === 0) parts.push(`${seconds}s`)

    return parts.join(' ')
  }

  async function fetchKeys (reset: boolean) {
    loading.value = true
    error.value = null

    if (reset) {
      keys.value = []
      continuationToken.value = null
      hasMoreResults.value = false
    }

    try {
      const result = await searchRedisKeys(
        props.groupId,
        pattern.value || '*',
        continuationToken.value ?? undefined,
      )
      keys.value = reset ? result.keys : [...keys.value, ...result.keys]
      continuationToken.value = result.continuationToken
      hasMoreResults.value = result.hasMoreResults
      searched.value = true
      lastSearchedPattern.value = pattern.value || '*'
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to search keys'
    } finally {
      loading.value = false
    }
  }

  function search () {
    fetchKeys(true)
  }

  function loadMore () {
    fetchKeys(false)
  }

  function confirmDelete (item: RedisKeyDto) {
    deleteTarget.value = item
    showDeleteDialog.value = true
  }

  function cancelDelete () {
    showDeleteDialog.value = false
    deleteTarget.value = null
  }

  async function executeDelete () {
    if (!deleteTarget.value) return

    deleting.value = true
    try {
      await deleteRedisKey(deleteTarget.value.key, props.groupId)
      keys.value = keys.value.filter(k => k.key !== deleteTarget.value!.key)
      showDeleteDialog.value = false
      deleteTarget.value = null
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Failed to delete key'
      showDeleteDialog.value = false
    } finally {
      deleting.value = false
    }
  }

  function refresh () {
    search()
  }

  onMounted(() => search())

  watch(() => props.groupId, () => search())

  defineExpose({ refresh })
</script>

<style scoped>
  .search-bar :deep(.v-field) {
    background-color: #f8f9fa;
    border-radius: 12px;
  }

  .search-bar :deep(.v-field--focused) {
    background-color: #fff;
  }
</style>
