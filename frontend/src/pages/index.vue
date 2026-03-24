<template>
  <v-container fluid class="pa-6">
    <div class="d-flex align-center justify-space-between flex-wrap ga-4 mb-6">
      <div>
        <div class="text-h5 font-weight-medium">Dashboard</div>
        <div class="text-body-2 text-medium-emphasis">Server overview</div>
      </div>
      <v-text-field
        v-model="search"
        class="search-field"
        clearable
        density="compact"
        hide-details
        max-width="320"
        placeholder="Search servers..."
        prepend-inner-icon="mdi-magnify"
        rounded="lg"
        variant="outlined"
      />
    </div>

    <!-- Loading skeletons -->
    <v-row v-if="loading && servers.length === 0">
      <v-col v-for="i in 3" :key="i" cols="12" sm="6" md="4" lg="3">
        <v-skeleton-loader type="card" />
      </v-col>
    </v-row>

    <!-- Empty state -->
    <v-card
      v-else-if="!loading && servers.length === 0"
      class="text-center pa-8"
      elevation="1"
      rounded="lg"
    >
      <v-icon class="mb-3" color="primary" icon="mdi-database-off" size="48" />
      <div class="text-h6">No servers found</div>
      <div class="text-body-2 text-medium-emphasis">
        No Redis server groups are configured yet.
      </div>
    </v-card>

    <!-- No results -->
    <v-card
      v-else-if="filteredCards.length === 0 && search"
      class="text-center pa-8"
      rounded="lg"
    >
      <v-icon class="mb-3" color="medium-emphasis" icon="mdi-magnify" size="48" />
      <div class="text-h6">No matches</div>
      <div class="text-body-2 text-medium-emphasis">
        No servers match "{{ search }}"
      </div>
    </v-card>

    <!-- Server cards grid -->
    <v-row v-else>
      <v-col
        v-for="card in filteredCards"
        :key="card.id"
        cols="12"
        sm="6"
        md="4"
        lg="3"
      >
        <v-card
          elevation="1"
          hover
          rounded="lg"
          :to="`/redis/${card.id}`"
        >
          <v-card-title class="d-flex align-center justify-space-between">
            <span class="text-subtitle-1 font-weight-medium">{{ card.name }}</span>
            <v-chip
              :color="card.groupType === 'Cluster' ? 'primary' : 'teal'"
              size="x-small"
              variant="tonal"
            >
              {{ card.groupType }}
            </v-chip>
          </v-card-title>

          <v-card-text>
            <div class="d-flex align-center text-body-2 text-medium-emphasis">
              <v-icon class="mr-2" icon="mdi-server" size="18" />
              <template v-if="card.nodeCount === null">
                <v-progress-circular color="primary" indeterminate size="14" width="2" />
                <span class="ml-2">Loading nodes...</span>
              </template>
              <template v-else-if="card.nodeCount === -1">
                <span class="text-error">Failed to load</span>
              </template>
              <template v-else>
                {{ card.nodeCount }} {{ card.nodeCount === 1 ? 'node' : 'nodes' }}
              </template>
            </div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script lang="ts" setup>
  import { getRedisServerGroupDetail } from '@/api/redisServers'

  interface ServerCard {
    id: string
    name: string
    groupType: string
    nodeCount: number | null // null = loading, -1 = error
  }

  const redisServersStore = useRedisServersStore()
  const { servers, loading } = storeToRefs(redisServersStore)

  const search = ref('')
  const serverCards = ref<ServerCard[]>([])

  const filteredCards = computed(() => {
    const q = (search.value || '').trim().toLowerCase()
    if (!q) return serverCards.value
    return serverCards.value.filter(c =>
      c.name.toLowerCase().includes(q) || c.groupType.toLowerCase().includes(q),
    )
  })

  watch(servers, async (newServers) => {
    for (const srv of newServers) {
      if (serverCards.value.find(s => s.id === srv.id)) continue

      const card: ServerCard = reactive({
        id: srv.id,
        name: srv.name,
        groupType: srv.groupType,
        nodeCount: null,
      })
      serverCards.value.push(card)

      getRedisServerGroupDetail(srv.id)
        .then(detail => { card.nodeCount = detail.nodes.length })
        .catch(() => { card.nodeCount = -1 })
    }
  }, { immediate: true })

  onMounted(() => {
    if (servers.value.length === 0) {
      redisServersStore.reset()
    }
  })
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Dashboard'
</route>
