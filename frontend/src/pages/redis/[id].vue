<template>
  <v-container class="py-6">
    <v-card rounded="lg" variant="tonal">
      <v-card-title class="d-flex align-center justify-space-between">
        <span class="text-subtitle-1">Redis Server</span>
        <v-chip v-if="server" color="primary" variant="tonal" size="small">
          {{ server.topology }}
        </v-chip>
      </v-card-title>

      <v-divider />

      <v-card-text>
        <div v-if="loading" class="d-flex justify-center py-6">
          <v-progress-circular indeterminate color="primary" :aria-label="`Loading server ${id}`" />
        </div>

        <v-alert v-else-if="error" type="error" variant="tonal" class="mb-4">
          <div class="d-flex align-center justify-space-between flex-wrap ga-2">
            <span>{{ error }}</span>
            <v-btn size="small" variant="text" color="primary" @click="fetchServer">Retry</v-btn>
          </div>
        </v-alert>

        <template v-else-if="server">
          <v-tabs v-model="tab" density="compact" class="mb-4">
            <v-tab value="info">Server info</v-tab>
            <v-tab value="keys">Keys</v-tab>
          </v-tabs>

          <v-window v-model="tab">
            <v-window-item value="info">
              <v-card variant="flat">
                <v-card-text class="pa-0">
                  <v-list density="compact" class="mb-4">
                    <v-list-item
                      title="Name"
                      :subtitle="server.name"
                      prepend-icon="mdi-label-outline"
                    />
                    <v-list-item
                      title="Topology"
                      :subtitle="server.topology"
                      prepend-icon="mdi-lan-connect"
                    />
                  </v-list>

                  <v-divider class="mb-4" />

                  <v-list density="comfortable">
                    <v-list-subheader>Nodes</v-list-subheader>
                    <v-alert v-if="!server.nodes.length" type="info" variant="tonal" class="ma-2">
                      No nodes reported for this server.
                    </v-alert>
                    <v-list-item
                      v-for="node in server.nodes"
                      :key="node.address"
                      :title="node.address"
                      :subtitle="`Role: ${formatRole(node.role)} • Hosted shards: ${node.hostedShards}`"
                      prepend-icon="mdi-lan"
                    />
                  </v-list>
                </v-card-text>
              </v-card>
            </v-window-item>

            <v-window-item value="keys">
              <div
                class="d-flex flex-column align-center justify-center py-10 text-medium-emphasis"
              >
                <v-icon icon="mdi-key" size="48" class="mb-3" />
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
  type Topology = 'Cluster' | 'Master-Slave' | 'Standalone'
  type NodeRole = 'master' | 'slave' | 'replica' | 'unknown'

  interface ServerNode {
    address: string
    role: NodeRole
    hostedShards: number
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

  const mockServers: Record<string, ServerDetail> = {
    '1': {
      id: '1',
      name: 'Primary Cluster',
      topology: 'Cluster',
      nodes: [
        { address: '10.0.0.1:6379', role: 'master', hostedShards: 8 },
        { address: '10.0.0.2:6379', role: 'replica', hostedShards: 8 },
      ],
    },
    '2': {
      id: '2',
      name: 'Analytics Master-Slave',
      topology: 'Master-Slave',
      nodes: [
        { address: '10.0.1.1:6379', role: 'master', hostedShards: 4 },
        { address: '10.0.1.2:6379', role: 'slave', hostedShards: 4 },
      ],
    },
    '3': {
      id: '3',
      name: 'Standalone Cache',
      topology: 'Standalone',
      nodes: [{ address: '10.0.2.1:6379', role: 'master', hostedShards: 1 }],
    },
  }

  const formatRole = (role: NodeRole): string => {
    if (role === 'master') return 'Master'
    if (role === 'slave') return 'Slave'
    if (role === 'replica') return 'Replica'
    return 'Unknown'
  }

  const fetchServer = async () => {
    loading.value = true
    error.value = null

    try {
      await new Promise(resolve => setTimeout(resolve, 300))
      const result = mockServers[id.value]

      if (!result) {
        throw new Error('Server not found')
      }

      server.value = result
    } catch (e) {
      error.value = 'Unable to load server details'
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
