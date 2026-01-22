<template>
  <v-card class="fill-height">
    <v-expansion-panels
      v-model="openPanels"
      class="fill-height"
      density="comfortable"
      variant="accordion"
    >
      <v-expansion-panel :value="0" title="Redis Servers">
        <v-expansion-panel-text class="pa-0">
          <v-list>
            <v-list-item
              v-for="server in servers"
              :key="server"
              prepend-icon="mdi-database"
              :title="server"
              @click="selectServer(server)"
            />
          </v-list>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>
  </v-card>
</template>

<script setup lang="ts">
  const servers = ref(['Redis1', 'Redis2', 'Redis3'])
  const openPanels = ref([0])

  function selectServer(server: string) {
    console.log('Selected server:', server)
    // TODO: Replace with real selection handling when API is wired
  }
</script>
