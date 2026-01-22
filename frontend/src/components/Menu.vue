<template>
  <v-card class="fill-height">
    <v-expansion-panels v-model="openPanels" density="comfortable" variant="accordion">
      <v-expansion-panel :value="0" title="Redis Servers">
        <v-expansion-panel-text class="pa-0">
          <v-list>
            <v-list-item
              v-for="server in servers"
              :key="server"
              prepend-icon="mdi-database"
              :title="server"
              @click="selectServer(server)"
              nav
              to="/"
            />
          </v-list>
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
  const router = useRouter()
  const servers = ref(['Redis1', 'Redis2', 'Redis3'])
  const openPanels = ref([0])

  function selectServer(server: string) {
    console.log('Selected server:', server)
    // TODO: Replace with real selection handling when API is wired
  }
</script>
