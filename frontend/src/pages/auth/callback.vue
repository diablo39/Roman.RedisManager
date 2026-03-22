<template>
  <v-container class="fill-height d-flex align-center justify-center">
    <v-card rounded="lg" width="420">
      <v-card-text>
        <!-- Processing: waiting for callback to complete -->
        <div v-if="processing" class="d-flex flex-column align-center py-6">
          <v-progress-circular class="mb-4" color="primary" indeterminate />
          <p class="text-body-1">Completing sign-in…</p>
        </div>

        <!-- Error: callback could not be completed -->
        <template v-else-if="callbackErrorMessage">
          <v-alert class="mb-4" :text="callbackErrorMessage" type="error" />
          <v-btn block color="error" variant="tonal" @click="router.push('/login')">
            Try again
          </v-btn>
        </template>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  const authStore = useAuthenticationStore()
  const router = useRouter()

  const processing = ref(true)
  const callbackErrorMessage = ref<string | null>(null)

  onMounted(async () => {
    try {
      const destination = await authStore.handleCallback()
      await router.replace(destination)
    } catch (error) {
      authStore.callbackError = error instanceof Error ? error.message : 'Sign-in could not be completed'
      callbackErrorMessage.value = authStore.callbackError
      processing.value = false
    }
  })
</script>

<route lang="yaml">
meta:
  layout: login
</route>
