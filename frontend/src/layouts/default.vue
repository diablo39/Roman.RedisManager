<template>
  <v-layout>
    <v-navigation-drawer
      permanent
      :width="260"
      color="white"
      :elevation="0"
      border="e"
    >
      <Menu />
    </v-navigation-drawer>

    <v-app-bar color="white" :elevation="0">
      <v-app-bar-title class="text-body-1 font-weight-medium text-medium-emphasis">
        {{ pageTitle }}
      </v-app-bar-title>

      <v-spacer />

      <v-btn
        icon
        size="small"
        title="Logout"
        variant="text"
        @click="onRelogin"
      >
        <v-icon icon="mdi-logout" size="20" />
      </v-btn>
    </v-app-bar>

    <v-main style="background-color: #f5f5f5; min-height: 100vh;">
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
  const route = useRoute()

  const pageTitle = computed(() => {
    return (route.meta as { title?: string }).title || 'Dashboard'
  })

  function onRelogin() {
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
