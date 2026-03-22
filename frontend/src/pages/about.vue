<template>
  <v-container class="fill-height">
    <v-card class="mx-auto" max-width="800" rounded="lg" variant="tonal">
      <v-card-title class="d-flex align-center">
        <v-icon class="mr-2" icon="mdi-information" />
        About Roman Redis Manager
      </v-card-title>

      <v-card-text>
        <h3 class="mb-2">Application Information</h3>
        <p class="mb-4">
          Roman Redis Manager is a modern frontend UI for managing Redis databases. It provides an
          intuitive interface for Redis connection management, data browsing, and database
          administration.
        </p>

        <v-divider class="my-4" />

        <h3 class="mb-2">Technology Stack</h3>
        <v-list density="compact">
          <v-list-item
            prepend-icon="mdi-vuejs"
            subtitle="Progressive JavaScript Framework"
            title="Vue 3"
          />
          <v-list-item
            prepend-icon="mdi-material-design"
            subtitle="Material Design Component Framework"
            title="Vuetify 3"
          />
          <v-list-item
            prepend-icon="mdi-language-typescript"
            subtitle="Type-safe Development"
            title="TypeScript"
          />
          <v-list-item
            prepend-icon="mdi-lightning-bolt"
            subtitle="Next Generation Frontend Tooling"
            title="Vite"
          />
        </v-list>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  // Minimal page logic
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'About'
</route>
