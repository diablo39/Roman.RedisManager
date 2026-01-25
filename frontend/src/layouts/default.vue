<template>
  <v-layout>
    <v-app-bar color="primary" dark title="Redis Manager" />

    <v-navigation-drawer>
      <Menu />
    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>

    <!-- Full-page loading overlay -->
    <v-overlay
      :model-value="loading && servers.length === 0"
      persistent
      class="d-flex align-center justify-center"
      scrim="black"
      opacity="0.7"
    >
      <v-card color="transparent" elevation="0">
        <v-card-text class="d-flex flex-column align-center">
          <v-progress-circular indeterminate size="64" width="6" color="primary" />
          <div class="text-h6 mt-4 text-white">Loading ...</div>
        </v-card-text>
      </v-card>
    </v-overlay>
  </v-layout>
</template>

<script lang="ts" setup>
  const redisServersStore = useRedisServersStore()
  const { loading, servers } = storeToRefs(redisServersStore)
</script>
