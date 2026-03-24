<template>
  <v-container fluid class="pa-6">
    <!-- Page header -->
    <div class="mb-6">
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

    <!-- Loading -->
    <div v-if="loading" class="d-flex justify-center py-12">
      <v-progress-circular :aria-label="`Loading server ${id}`" color="primary" indeterminate size="48" />
    </div>

    <!-- Error -->
    <v-card v-else-if="error" elevation="1" rounded="lg">
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
      <v-card elevation="1" rounded="lg">
        <v-tabs v-model="tab" color="primary" density="compact">
          <v-tab value="info">Server Info</v-tab>
          <v-tab value="keys">Keys</v-tab>
        </v-tabs>

        <v-divider />

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
                      :subtitle="server.name"
                      title="Name"
                    />
                    <v-list-item
                      prepend-icon="mdi-lan-connect"
                      :subtitle="server.topology"
                      title="Topology"
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
              <div class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis">
                <v-icon class="mb-3" icon="mdi-key" size="48" />
                <div class="text-subtitle-1">Keys view coming soon</div>
                <div class="text-body-2">Browse and manage Redis keys will appear here.</div>
              </div>
            </v-card-text>
          </v-window-item>
        </v-window>
      </v-card>
    </template>

    <div v-else class="text-body-2 text-medium-emphasis">No server selected.</div>
  </v-container>
</template>

<script setup lang="ts">
  import { getRedisServerGroupDetail } from '@/api/redisServers'

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

  const route = useRoute()
  const id = computed(() => String((route.params as { id?: string }).id ?? ''))

  const tab = ref<'info' | 'keys'>('info')
  const loading = ref(true)
  const error = ref<string | null>(null)
  const server = ref<ServerDetail | null>(null)

  const redisServersStore = useRedisServersStore()
  const { servers } = storeToRefs(redisServersStore)

  function formatRole (role: NodeRole): string {
    if (role === 'master') return 'Master'
    if (role === 'slave') return 'Slave'
    if (role === 'replica') return 'Replica'
    return 'Unknown'
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

  watch(id, fetchServer, { immediate: true })
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Redis Server'
</route>
