<template>
  <v-container class="py-6">
    <v-card rounded="lg" variant="tonal">
      <v-card-title class="d-flex align-center justify-space-between">
        <span class="text-subtitle-1">Redis Server</span>
        <v-chip v-if="server" color="primary" size="small" variant="tonal">
          {{ server.topology }}
        </v-chip>
      </v-card-title>

      <v-divider />

      <v-card-text>
        <div v-if="loading" class="d-flex justify-center py-6">
          <v-progress-circular :aria-label="`Loading server ${id}`" color="primary" indeterminate />
        </div>

        <v-alert v-else-if="error" class="mb-4" type="error" variant="tonal">
          <div class="d-flex align-center justify-space-between flex-wrap ga-2">
            <span>{{ error }}</span>
            <v-btn color="primary" size="small" variant="text" @click="fetchServer">Retry</v-btn>
          </div>
        </v-alert>

        <template v-else-if="server">
          <v-tabs v-model="tab" class="mb-4" density="compact">
            <v-tab value="info">Server info</v-tab>
            <v-tab value="keys">Keys</v-tab>
          </v-tabs>

          <v-window v-model="tab">
            <v-window-item value="info">
              <v-card variant="flat">
                <v-card-text class="pa-0">
                  <v-list class="mb-4" density="compact">
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

                  <v-divider class="mb-4" />

                  <v-list density="comfortable">
                    <v-list-subheader>Nodes</v-list-subheader>
                    <v-alert v-if="server.nodes.length === 0" class="ma-2" type="info" variant="tonal">
                      No nodes reported for this server.
                    </v-alert>
                    <v-list-item
                      v-for="node in server.nodes"
                      :key="node.address"
                      prepend-icon="mdi-lan"
                      :subtitle="`Role: ${formatRole(node.role)}`"
                      :title="node.address"
                    />
                  </v-list>
                </v-card-text>
              </v-card>
            </v-window-item>

            <v-window-item value="keys">
              <div
                class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
              >
                <v-icon class="mb-3" icon="mdi-key" size="48" />
                <div class="text-subtitle-1">Keys view coming soon</div>
                <div class="text-body-2">Browse and manage Redis keys will appear here.</div>
              </div>
            </v-window-item>
          </v-window>
        </template>

        <div v-else class="text-body-2 text-medium-emphasis">No server selected.</div>
      </v-card-text>
    </v-card>
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
