<template>
  <v-main v-if="isPublicRoute">
    <router-view />
  </v-main>

  <v-layout v-else>
    <v-app-bar color="primary" dark title="Redis Manager" />

    <v-navigation-drawer>
      <Menu />
    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>

    <!-- Full-page loading overlay -->
    <v-overlay
      class="d-flex align-center justify-center"
      :model-value="loading && servers.length === 0"
      opacity="0.7"
      persistent
      scrim="black"
    >
      <v-card color="transparent" elevation="0">
        <v-card-text class="d-flex flex-column align-center">
          <v-progress-circular color="primary" indeterminate size="64" width="6" />
          <div class="text-h6 mt-4 text-white">Loading ...</div>
        </v-card-text>
      </v-card>
    </v-overlay>
  </v-layout>
</template>

<script lang="ts" setup>
  import { isPublicPath } from '@/router/auth'

  const route = useRoute()
  const isPublicRoute = computed(() => isPublicPath(route.path))

  const redisServersStore = useRedisServersStore()
  const { loading, servers } = storeToRefs(redisServersStore)
</script>
