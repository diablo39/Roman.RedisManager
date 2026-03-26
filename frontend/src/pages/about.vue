<template>
  <v-container class="pa-6" fluid>
    <div class="mb-6">
      <div class="text-h5 font-weight-medium">About</div>
      <div class="text-body-2 text-medium-emphasis">Application information</div>
    </div>

    <v-card max-width="800" rounded="lg">
      <div class="card-header-separated">
        <div class="card-header-title">
          <v-icon color="primary" icon="mdi-information" size="20" />
          About Roman Redis Manager
        </div>
      </div>

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
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'About'
</route>
