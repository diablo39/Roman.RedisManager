<template>
  <v-layout>
    <v-app-bar color="primary" dark title="Redis Manager" />

    <v-navigation-drawer>
      <ServerMenu />
    </v-navigation-drawer>

    <v-main>
      <router-view />
    </v-main>
  </v-layout>
</template>

<script lang="ts" setup></script>
