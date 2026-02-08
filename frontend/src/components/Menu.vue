<template>
  <v-card class="fill-height">
    <v-expansion-panels v-model="openPanels" density="comfortable" variant="accordion">
      <v-expansion-panel :value="0" title="Servers">
        <v-expansion-panel-text class="pa-0">
          <v-infinite-scroll :height="300" :items="servers" @load="onLoad">
            <template v-for="server in servers" :key="server.id">
              <v-list-item
                prepend-icon="mdi-database"
                :title="server.name"
                :to="`/redis/${server.id}`"
                nav
              />
            </template>

            <template #empty>
              <!-- <div class="pa-4 text-center text-caption text-medium-emphasis">No more servers</div> -->
            </template>

            <template #error="{ props }">
              <v-alert type="error" variant="tonal" class="ma-2">
                <div class="text-caption">Failed to load</div>
                <v-btn v-bind="props" size="small" variant="text" class="mt-2">Retry</v-btn>
              </v-alert>
            </template>
          </v-infinite-scroll>
        </v-expansion-panel-text>
      </v-expansion-panel>

      <v-expansion-panel :value="1" title="Settings">
        <v-expansion-panel-text class="pa-0">
          <v-list>
            <v-list-item prepend-icon="mdi-information" title="About" nav to="/about" />
          </v-list>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-card>
</template>

<script setup lang="ts">
  const redisServersStore = useRedisServersStore()
  const { servers, hasNext } = storeToRefs(redisServersStore)

  const openPanels = ref([0])

  async function onLoad({
    done,
  }: {
    done: (status: 'ok' | 'empty' | 'loading' | 'error') => void
  }) {
    if (!hasNext.value) {
      done('empty')
      return
    }

    try {
      await redisServersStore.loadNextPage()
      done('ok')
    } catch (e) {
      done('error')
    }
  }

  // Initial fetch on mount
  onMounted(() => {
    redisServersStore.reset()
  })
</script>
