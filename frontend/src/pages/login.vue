<template>
  <v-container class="fill-height d-flex align-center justify-center py-8">
    <v-card class="mx-auto" max-width="520" rounded="lg" variant="tonal" width="100%">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2" icon="mdi-shield-account" />
        Sign in to Roman Redis Manager
      </v-card-title>

      <v-card-subtitle>Choose one of the available identity providers to continue.</v-card-subtitle>

      <v-divider class="mt-2" />

      <v-card-text>
        <div v-if="bootstrapLoading" class="d-flex flex-column align-center py-6">
          <v-progress-circular color="primary" indeterminate :size="40" />
          <div aria-live="polite" class="text-body-2 mt-4">Loading sign-in providers...</div>
        </div>

        <v-alert v-else-if="error" class="mb-4" type="error" variant="tonal">
          <div class="d-flex align-center justify-space-between flex-wrap ga-2">
            <span>{{ error }}</span>
            <v-btn color="primary" size="small" variant="text" @click="reloadProviders">
              Retry
            </v-btn>
          </div>
        </v-alert>

        <v-alert v-else-if="!isSignInAvailable" class="mb-4" type="warning" variant="tonal">
          {{ unavailableMessage }}
        </v-alert>

        <template v-else>
          <v-btn
            v-for="provider in availableProviders"
            :key="provider.providerKey"
            block
            class="mb-2"
            color="primary"
            :disabled="Boolean(signingInProviderKey)"
            :loading="signingInProviderKey === provider.providerKey"
            prepend-icon="mdi-open-in-new"
            @click="signIn(provider.providerKey)"
          >
            Continue with {{ provider.displayName }}
          </v-btn>
        </template>

        <v-alert
          v-if="callbackError"
          aria-live="assertive"
          class="mt-4"
          role="alert"
          type="error"
          variant="tonal"
        >
          {{ callbackError }}
        </v-alert>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  import { useAuthenticationStore } from '@/stores/authentication'

  const route = useRoute()
  const authenticationStore = useAuthenticationStore()

  const {
    bootstrapLoading,
    availableProviders,
    error,
    isSignInAvailable,
    unavailableMessage,
    callbackError,
  } = storeToRefs(authenticationStore)

  const signingInProviderKey = ref<string | null>(null)

  const requestedReturnUrl = computed(() => {
    const queryReturnUrl = route.query.returnUrl
    return typeof queryReturnUrl === 'string' ? queryReturnUrl : undefined
  })

  async function reloadProviders() {
    await authenticationStore.loadBootstrap(true)
  }

  async function signIn(providerKey: string) {
    signingInProviderKey.value = providerKey

    try {
      await authenticationStore.startSignIn(providerKey, requestedReturnUrl.value)
    } catch (error) {
      authenticationStore.setCallbackError(
        error instanceof Error
          ? error.message
          : 'Unable to start sign-in. Please retry with a provider.'
      )
    } finally {
      signingInProviderKey.value = null
    }
  }

  onMounted(async () => {
    if (requestedReturnUrl.value) {
      authenticationStore.setReturnUrl(requestedReturnUrl.value)
    }

    await authenticationStore.prepareLogin()
  })
</script>
