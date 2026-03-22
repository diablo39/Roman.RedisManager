<template>
  <v-container class="fill-height d-flex align-center justify-center py-8">
    <v-card class="mx-auto" max-width="520" rounded="lg" variant="tonal" width="100%">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2" icon="mdi-shield-check" />
        Completing Sign-In
      </v-card-title>

      <v-divider class="mt-2" />

      <v-card-text>
        <div v-if="processing" class="d-flex flex-column align-center py-6">
          <v-progress-circular color="primary" indeterminate :size="40" />
          <div aria-live="polite" class="text-body-2 mt-4">
            Finalizing your authentication session...
          </div>
        </div>

        <v-alert v-else-if="error" aria-live="assertive" role="alert" type="error" variant="tonal">
          <div class="mb-3">{{ error }}</div>
          <div class="d-flex ga-2 flex-wrap">
            <v-btn color="primary" variant="tonal" @click="retrySignIn">Retry sign-in</v-btn>
            <v-btn color="primary" variant="text" @click="backToLogin">Back to sign in</v-btn>
          </div>
        </v-alert>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  import { LOGIN_PATH } from '@/router/auth'
  import { useAuthenticationStore } from '@/stores/authentication'

  const router = useRouter()
  const authenticationStore = useAuthenticationStore()

  const processing = ref(true)
  const error = ref<string | null>(null)

  async function handleCallback() {
    processing.value = true
    error.value = null

    try {
      const destination = await authenticationStore.handleCallback(window.location.href)
      await router.replace(destination)
    } catch (callbackError) {
      error.value =
        callbackError instanceof Error
          ? callbackError.message
          : 'Unable to complete sign-in. Please try again.'
      authenticationStore.setCallbackError(error.value)
    } finally {
      processing.value = false
    }
  }

  async function retrySignIn() {
    try {
      await authenticationStore.retryLastSignIn()
    } catch (retryError) {
      error.value =
        retryError instanceof Error
          ? retryError.message
          : 'Unable to retry sign-in. Please return to login.'
    }
  }

  async function backToLogin() {
    await router.replace({ path: LOGIN_PATH })
  }

  onMounted(handleCallback)
</script>
