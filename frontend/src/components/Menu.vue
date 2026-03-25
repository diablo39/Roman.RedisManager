<template>
  <div class="sidebar-wrapper">
    <!-- Brand -->
    <div class="sidebar-brand">
      <img src="@/assets/logo-sidebar.svg" alt="Roman Redis Manager" width="28" height="28" class="sidebar-brand-icon" />
      <span class="sidebar-brand-text">Redis Manager</span>
    </div>

    <v-divider />

    <!-- Dashboard link -->
    <v-list color="primary" density="compact" nav>
      <v-list-item
        prepend-icon="mdi-view-dashboard"
        title="Dashboard"
        to="/"
        exact
      />
    </v-list>

    <!-- Servers section -->
    <div class="sidebar-section-header">Servers</div>

    <div class="sidebar-servers">
      <v-list color="primary" density="compact" nav>
        <v-infinite-scroll :items="servers" @load="onLoad">
          <template v-for="server in servers" :key="server.id">
            <v-list-item
              prepend-icon="mdi-database"
              :title="server.name"
              :to="`/redis/${server.id}`"
            >
              <template #append>
                <v-chip
                  :color="server.groupType === 'Cluster' ? 'primary' : 'teal'"
                  size="x-small"
                  variant="tonal"
                >
                  {{ server.groupType }}
                </v-chip>
              </template>
            </v-list-item>
          </template>

          <template #empty />

          <template #error="{ props }">
            <v-alert class="ma-2" type="error" variant="tonal">
              <div class="text-caption">Failed to load</div>
              <v-btn v-bind="props" class="mt-2" size="small" variant="text">Retry</v-btn>
            </v-alert>
          </template>
        </v-infinite-scroll>
      </v-list>
    </div>

    <v-divider />

    <!-- Settings section -->
    <div class="sidebar-section-header">Settings</div>

    <v-list color="primary" density="compact" nav>
      <v-list-item
        prepend-icon="mdi-information-outline"
        title="About"
        to="/about"
      />
    </v-list>
  </div>
</template>

<script setup lang="ts">
  const redisServersStore = useRedisServersStore()
  const { servers, hasNext } = storeToRefs(redisServersStore)

  async function onLoad ({
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
    } catch {
      done('error')
    }
  }

  // Initial fetch on mount
  onMounted(() => {
    redisServersStore.reset()
  })
</script>

<style scoped>
  .sidebar-wrapper {
    display: flex;
    flex-direction: column;
    height: 100%;
  }

  .sidebar-servers {
    flex: 1;
    min-height: 0;
    overflow-y: auto;
  }
</style>
