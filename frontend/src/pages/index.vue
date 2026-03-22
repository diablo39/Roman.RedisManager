<template>
  <v-container fluid>
    <v-sheet
      border="dashed md"
      class="d-flex align-center justify-center"
      color="surface-light"
      height="400"
      rounded="lg"
    >
      <div class="text-h6">Main Content Area</div>
    </v-sheet>
  </v-container>
</template>

<script lang="ts" setup>
  // Protected home page. Route access is enforced by the global auth guard.
</script>

<route lang="yaml">
meta:
  layout: default
  title: 'Home'
</route>
