<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-card elevation="8" rounded="xl" width="420">
      <!-- Brand -->
      <div class="d-flex flex-column align-center pt-8 pb-2">
        <img alt="Roman Redis Manager" height="48" src="@/assets/logo-sidebar.svg" width="48">
        <div class="text-h5 font-weight-bold mt-2">Redis Manager</div>
        <div class="text-body-2 text-medium-emphasis mt-1">Sign in to continue</div>
      </div>

      <v-card-text class="px-8 pb-8">
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
            rounded="lg"
            variant="elevated"
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
