<template>
  <v-layout>
    <v-main class="login-bg">
      <router-view />
    </v-main>
  </v-layout>
</template>

<style scoped>
  .login-bg {
    background: linear-gradient(135deg, #1976D2 0%, #1565C0 50%, #0D47A1 100%);
    min-height: 100vh;
  }
</style>
