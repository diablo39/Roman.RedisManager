<template>
  <v-container class="pa-6" fluid>
    <!-- Page header -->
    <div class="d-flex align-center justify-space-between flex-wrap ga-4 mb-6">
      <div>
        <div class="d-flex align-center ga-3">
          <div class="text-h5 font-weight-medium">
            {{ server?.name || 'Redis Server' }}
          </div>
          <v-chip
            v-if="server"
            :color="server.topology === 'Cluster' ? 'primary' : 'teal'"
            size="small"
            variant="tonal"
          >
            {{ server.topology }}
          </v-chip>
        </div>
        <div class="text-body-2 text-medium-emphasis">Server details and configuration</div>
      </div>

      <!-- Add key button with type menu -->
      <v-menu v-if="server">
        <template #activator="{ props: menuProps }">
          <v-btn color="success" prepend-icon="mdi-plus" v-bind="menuProps">
            Add
          </v-btn>
        </template>
        <v-list density="compact" nav>
          <v-list-item
            v-for="t in keyTypes"
            :key="t.value"
            :prepend-icon="t.icon"
            :title="t.label"
            @click="openCreateDialog(t.value)"
          />
        </v-list>
      </v-menu>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="d-flex justify-center py-12">
      <v-progress-circular :aria-label="`Loading server ${id}`" color="primary" indeterminate size="48" />
    </div>

    <!-- Error -->
    <v-card v-else-if="error" rounded="lg">
      <v-card-text>
        <v-alert type="error" variant="tonal">
          <div class="d-flex align-center justify-space-between flex-wrap ga-2">
            <span>{{ error }}</span>
            <v-btn color="primary" size="small" variant="text" @click="fetchServer">Retry</v-btn>
          </div>
        </v-alert>
      </v-card-text>
    </v-card>

    <!-- Content -->
    <template v-else-if="server">
      <v-card rounded="lg">
        <div class="card-header-separated">
          <v-tabs v-model="tab" class="card-header-tabs" color="primary" density="compact">
            <v-tab value="info">Server Info</v-tab>
            <v-tab value="keys">Keys</v-tab>
          </v-tabs>
        </div>

        <v-window v-model="tab">
          <v-window-item value="info">
            <v-card-text>
              <v-row>
                <!-- Basic info -->
                <v-col cols="12" md="6">
                  <div class="text-subtitle-2 font-weight-medium mb-3">General</div>
                  <v-list density="compact" variant="flat">
                    <v-list-item
                      prepend-icon="mdi-label-outline"
                      subtitle="Name"
                      :title="server.name"
                    />
                    <v-list-item
                      prepend-icon="mdi-lan-connect"
                      subtitle="Topology"
                      :title="server.topology"
                    />
                  </v-list>
                </v-col>

                <!-- Nodes -->
                <v-col cols="12" md="6">
                  <div class="text-subtitle-2 font-weight-medium mb-3">
                    Nodes ({{ server.nodes.length }})
                  </div>
                  <v-alert
                    v-if="server.nodes.length === 0"
                    type="info"
                    variant="tonal"
                  >
                    No nodes reported for this server.
                  </v-alert>
                  <v-list v-else density="compact" variant="flat">
                    <v-list-item
                      v-for="node in server.nodes"
                      :key="node.address"
                      prepend-icon="mdi-lan"
                      :subtitle="`Role: ${formatRole(node.role)}`"
                      :title="node.address"
                    />
                  </v-list>
                </v-col>
              </v-row>
            </v-card-text>
          </v-window-item>

          <v-window-item value="keys">
            <v-card-text>
              <RedisKeysExplorer ref="keysExplorer" :group-id="id" @open-key="onOpenKey" />
            </v-card-text>
          </v-window-item>
        </v-window>
      </v-card>
    </template>

    <div v-else class="text-body-2 text-medium-emphasis">No server selected.</div>

    <!-- String key detail dialog -->
    <StringKeyDetailDialog ref="stringKeyDialog" :group-id="id" @close="onKeyDialogClose" />

    <!-- Create key dialog -->
    <CreateKeyDialog ref="createKeyDialog" :group-id="id" @created="onKeyCreated" />

    <!-- Success toast -->
    <v-snackbar v-model="showToast" color="success" :timeout="3000">
      Key created successfully
    </v-snackbar>
  </v-container>
</template>

<script setup lang="ts">
  import type { RedisKeyType } from '@/components/CreateKeyDialog.vue'
  import { getRedisServerGroupDetail } from '@/api/redisServers'
  import CreateKeyDialog from '@/components/CreateKeyDialog.vue'
  import RedisKeysExplorer from '@/components/RedisKeysExplorer.vue'
  import StringKeyDetailDialog from '@/components/StringKeyDetailDialog.vue'

  type Topology = 'Cluster' | 'Standalone' | 'Unknown'
  type NodeRole = 'master' | 'slave' | 'replica' | 'unknown' | string

  interface ServerNode {
    address: string
    role: NodeRole
  }

  interface ServerDetail {
    id: string
    name: string
    topology: Topology
    nodes: ServerNode[]
  }

  const keyTypes: { value: RedisKeyType, label: string, icon: string }[] = [
    { value: 'string', label: 'String', icon: 'mdi-text' },
    { value: 'hash', label: 'Hash', icon: 'mdi-code-braces' },
    { value: 'list', label: 'List', icon: 'mdi-format-list-bulleted' },
    { value: 'set', label: 'Set', icon: 'mdi-set-all' },
    { value: 'zset', label: 'Sorted Set', icon: 'mdi-sort-numeric-ascending' },
  ]

  const route = useRoute()
  const router = useRouter()
  const id = computed(() => String((route.params as { id?: string }).id ?? ''))

  const initialTab = route.query.tab === 'keys' || route.query.key ? 'keys' : 'info'
  const tab = ref<'info' | 'keys'>(initialTab)
  const loading = ref(true)
  const error = ref<string | null>(null)
  const server = ref<ServerDetail | null>(null)
  const showToast = ref(false)

  const createKeyDialog = ref<InstanceType<typeof CreateKeyDialog> | null>(null)
  const keysExplorer = ref<InstanceType<typeof RedisKeysExplorer> | null>(null)
  const stringKeyDialog = ref<InstanceType<typeof StringKeyDetailDialog> | null>(null)

  const redisServersStore = useRedisServersStore()
  const { servers } = storeToRefs(redisServersStore)

  function formatRole (role: NodeRole): string {
    if (role === 'master') return 'Master'
    if (role === 'slave') return 'Slave'
    if (role === 'replica') return 'Replica'
    return 'Unknown'
  }

  function openCreateDialog (keyType: RedisKeyType) {
    createKeyDialog.value?.open(keyType)
  }

  function onKeyCreated () {
    showToast.value = true
    // Switch to keys tab and refresh
    tab.value = 'keys'
    nextTick(() => {
      keysExplorer.value?.refresh?.()
    })
  }

  async function fetchServer () {
    loading.value = true
    error.value = null

    try {
      if (!id.value) {
        throw new Error('Server id is required')
      }

      if (servers.value.length === 0) {
        await redisServersStore.reset()
      }

      const selected = servers.value.find(item => item.id === id.value)
      const details = await getRedisServerGroupDetail(id.value)

      server.value = {
        id: id.value,
        name: selected?.name ?? id.value,
        topology: selected?.groupType ?? 'Unknown',
        nodes: details.nodes.map(node => ({
          address: `${node.host}:${node.port}`,
          role: node.role,
        })),
      }
    } catch (error_) {
      error.value = error_ instanceof Error ? error_.message : 'Unable to load server details'
      server.value = null
    } finally {
      loading.value = false
    }
  }

  watch(tab, (newTab) => {
    const query: Record<string, string> = {}
    for (const [k, v] of Object.entries(route.query)) {
      if (typeof v === 'string') query[k] = v
    }
    if (newTab === 'keys') {
      query.tab = 'keys'
    } else {
      delete query.tab
    }
    router.replace({ query })
  })

  function onOpenKey (key: string, type: string) {
    if (type.toLowerCase() === 'string') {
      stringKeyDialog.value?.open(key)
      router.replace({ query: { ...route.query, key, type: 'string' } })
    }
  }

  function onKeyDialogClose () {
    const { key: _key, type: _type, ...rest } = route.query
    router.replace({ query: rest })
  }

  // Handle deep link: if URL has ?key=...&type=string, auto-open dialog
  function checkDeepLink () {
    const queryKey = route.query.key
    const queryType = route.query.type
    if (typeof queryKey === 'string' && queryType === 'string') {
      tab.value = 'keys'
      nextTick(() => {
        stringKeyDialog.value?.open(queryKey)
      })
    }
  }

  watch(id, fetchServer, { immediate: true })

  onMounted(() => {
    // Defer deep link check until server data is loaded
    const unwatch = watch(loading, (isLoading) => {
      if (!isLoading && server.value) {
        checkDeepLink()
        unwatch()
      }
    })
  })
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Redis Server'
</route>
