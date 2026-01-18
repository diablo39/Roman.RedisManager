<template>
  <v-card class="fill-height">
    <v-card-title class="d-flex align-center">
      <v-icon class="mr-2" icon="mdi-server" />
      Redis Servers
    </v-card-title>

    <v-card-text class="pa-0">
      <v-list>
        <v-list-item
          v-for="server in servers"
          :key="server"
          prepend-icon="mdi-database"
          :title="server"
          @click="selectServer(server)"
        />
      </v-list>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
// Mocked data - will be fetched from server in the future
const servers = ref(['Redis1', 'Redis2', 'Redis3'])

function selectServer(server: string) {
  console.log('Selected server:', server)
  // TODO: Handle server selection
}
</script>
