<template>
  <v-layout>
    <v-app-bar color="primary" dark>
      <template #title>Redis Manager</template>

      <v-spacer />

      <v-btn icon title="Clear tokens and re-login" variant="text" @click="onRelogin">
        <v-icon icon="mdi-logout" />
      </v-btn>
    </v-app-bar>

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
  const redisServersStore = useRedisServersStore()
  const { loading, servers } = storeToRefs(redisServersStore)
  const authStore = useAuthenticationStore()
  const router = useRouter()

  function onRelogin() {
    // Clear auth session and remove OIDC-localStorage entries so the
    // provider flow starts fresh. Then navigate to the login page.
    authStore.invalidateSession()

    try {
      for (let i = localStorage.length - 1; i >= 0; i--) {
        const key = localStorage.key(i)
        if (!key) continue
        const lower = key.toLowerCase()
        if (lower.includes('oidc') || lower.includes('user') || lower.includes('access_token')) {
          localStorage.removeItem(key)
        }
      }
    } catch (e) {
      // Ignore storage errors
    }

    router.push('/login')
  }
</script>
