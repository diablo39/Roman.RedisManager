<template>
  <v-container class="fill-height">
    <v-card class="mx-auto" max-width="800" rounded="lg" variant="tonal">
      <v-card-title class="d-flex align-center">
        <v-icon icon="mdi-information" class="mr-2" />
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
            title="Vue 3"
            subtitle="Progressive JavaScript Framework"
          />
          <v-list-item
            prepend-icon="mdi-material-design"
            title="Vuetify 3"
            subtitle="Material Design Component Framework"
          />
          <v-list-item
            prepend-icon="mdi-language-typescript"
            title="TypeScript"
            subtitle="Type-safe Development"
          />
          <v-list-item
            prepend-icon="mdi-lightning-bolt"
            title="Vite"
            subtitle="Next Generation Frontend Tooling"
          />
        </v-list>
      </v-card-text>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
  // Minimal page logic
  const route = useRoute()
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'About'
</route>
