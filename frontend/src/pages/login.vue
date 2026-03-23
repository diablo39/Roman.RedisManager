<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-card rounded="lg" width="420">
      <v-card-title class="pt-6 pb-2 text-center text-h5">Sign in</v-card-title>

      <v-card-text>
        <!-- Loading: waiting for provider list -->
        <div v-if="bootstrapLoading" class="d-flex justify-center py-4">
          <v-progress-circular color="primary" indeterminate />
        </div>

        <!-- Error: bootstrap fetch failed -->
        <template v-else-if="error">
          <v-alert class="mb-4" :text="error" type="error" />
          <v-btn block color="error" variant="tonal" @click="authStore.loadBootstrap(true)">
            Retry
          </v-btn>
        </template>

        <!-- Unavailable: sign-in is currently disabled -->
        <v-alert v-else-if="!isSignInAvailable" :text="unavailableMessage" type="warning" />

        <!-- Available: show one button per provider -->
        <template v-else>
          <v-btn
            v-for="provider in availableProviders"
            :key="provider.providerKey"
            :aria-label="`Sign in with ${provider.displayName}`"
            block
            class="mb-3"
            color="primary"
            @click="authStore.startSignIn(provider.providerKey)"
          >
            {{ provider.displayName }}
          </v-btn>
        </template>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  import { sanitizeReturnUrl } from '@/router/auth'

  const route = useRoute()
  const authStore = useAuthenticationStore()
  const { bootstrapLoading, error, isSignInAvailable, unavailableMessage, availableProviders }
    = storeToRefs(authStore)

  onMounted(async () => {
    // Preserve the return destination passed in by the route guard
    const raw = route.query.returnUrl
    const url = Array.isArray(raw) ? raw[0] : raw
    if (url) {
      authStore.setReturnUrl(sanitizeReturnUrl(url))
    }

    await authStore.loadBootstrap()
  })
</script>

<route lang="yaml">
meta:
  layout: login
</route>
