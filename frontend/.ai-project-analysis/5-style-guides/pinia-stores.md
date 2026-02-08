# Pinia Stores Style Guide

## Overview

Pinia stores manage global application state using the Options API syntax with auto-imported composables.

## Store Structure

### Standard Template

```typescript
export const useXxxStore = defineStore("xxx", {
  state: () => ({
    // state properties
  }),

  getters: {
    // computed properties
  },

  actions: {
    // methods
  },
});
```

## Unique Conventions

### 1. No Imports Required

`defineStore` and `storeToRefs` are auto-imported:

```typescript
// ✅ Correct - no imports needed
export const useAppStore = defineStore("app", {
  state: () => ({
    //
  }),
});

// ❌ Incorrect - don't import
// import { defineStore } from 'pinia'
```

This is unique to this project via the auto-import configuration.

### 2. Options API Syntax

Stores use Options API, not Setup Stores:

```typescript
// ✅ Correct - Options API
export const useAppStore = defineStore("app", {
  state: () => ({
    count: 0,
  }),
  getters: {
    doubled(): number {
      return this.count * 2;
    },
  },
  actions: {
    increment() {
      this.count++;
    },
  },
});

// ❌ Not used in this project - Setup Store
// export const useAppStore = defineStore('app', () => {
//   const count = ref(0)
//   return { count }
// })
```

### 3. Minimal Base Store

The actual app store is extremely minimal:

```typescript
// From src/stores/app.ts
export const useAppStore = defineStore("app", {
  state: () => ({
    //
  }),
});
```

**Pattern**: Stores start minimal and grow as needed, not pre-populated with boilerplate.

### 4. State as Factory Function

State always returns an object from a function:

```typescript
// ✅ Correct
state: () => ({
  count: 0,
  items: [],
  user: null,
});

// ❌ Incorrect - not a function
// state: {
//   count: 0
// }
```

### 5. TypeScript Type Annotations

Type state properties inline:

```typescript
export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [] as RedisConnection[],
    activeConnection: null as RedisConnection | null,
    loading: false,
    error: null as string | null,
  }),
});
```

### 6. Getters with `this` Context

Getters can use `this` to access state:

```typescript
getters: {
  // Arrow function - parameter access
  activeConnections: (state) => {
    return state.connections.filter(c => c.active)
  },

  // Regular function - this access + type inference
  connectionCount(): number {
    return this.connections.length
  },

  // Getter returning function
  getConnectionById: (state) => {
    return (id: string): RedisConnection | undefined => {
      return state.connections.find(c => c.id === id)
    }
  },
}
```

### 7. Actions as Methods

Actions are regular methods that can modify state:

```typescript
actions: {
  // Synchronous action
  addConnection(connection: RedisConnection) {
    this.connections.push(connection)
  },

  // Async action
  async fetchConnections() {
    this.loading = true
    try {
      const data = await api.getConnections()
      this.connections = data
    } catch (error) {
      this.error = error.message
    } finally {
      this.loading = false
    }
  },

  // Action calling another action
  async refreshAndSelect(id: string) {
    await this.fetchConnections()
    this.selectConnection(id)
  },
}
```

## Store Examples

### Minimal Store (Actual)

```typescript
// src/stores/app.ts
import { defineStore } from "pinia";

export const useAppStore = defineStore("app", {
  state: () => ({
    //
  }),
});
```

### Complete Store Example

```typescript
interface RedisConnection {
  id: string;
  host: string;
  port: number;
  name: string;
  active: boolean;
}

export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [] as RedisConnection[],
    activeConnection: null as RedisConnection | null,
    loading: false,
    error: null as string | null,
  }),

  getters: {
    activeConnections: (state) => {
      return state.connections.filter((c) => c.active);
    },

    hasConnections(): boolean {
      return this.connections.length > 0;
    },

    getConnectionById: (state) => {
      return (id: string): RedisConnection | undefined => {
        return state.connections.find((c) => c.id === id);
      };
    },
  },

  actions: {
    async fetchConnections() {
      this.loading = true;
      this.error = null;

      try {
        const data = await fetch("/api/connections");
        this.connections = await data.json();
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },

    setActiveConnection(id: string) {
      const connection = this.connections.find((c) => c.id === id);
      if (connection) {
        this.activeConnection = connection;
      }
    },

    addConnection(connection: RedisConnection) {
      this.connections.push(connection);
    },

    removeConnection(id: string) {
      const index = this.connections.findIndex((c) => c.id === id);
      if (index !== -1) {
        this.connections.splice(index, 1);
      }
    },

    $reset() {
      this.connections = [];
      this.activeConnection = null;
      this.loading = false;
      this.error = null;
    },
  },
});
```

## Using Stores in Components

### Basic Usage

```vue
<script setup lang="ts">
// ✅ Auto-imported
const appStore = useAppStore();

// Access state directly
console.log(appStore.loading);

// Call actions
appStore.someAction();
</script>
```

### With Reactivity (storeToRefs)

```vue
<script setup lang="ts">
// ✅ Both auto-imported
const redisStore = useRedisStore();

// Extract reactive state
const { connections, loading, error } = storeToRefs(redisStore);

// Actions don't need refs
const { fetchConnections, addConnection } = redisStore;
</script>

<template>
  <div v-if="loading">Loading...</div>
  <div v-else-if="error">{{ error }}</div>
  <div v-else>
    <div v-for="conn in connections" :key="conn.id">
      {{ conn.name }}
    </div>
  </div>
</template>
```

**Key point**: Use `storeToRefs` for state/getters, direct destructure for actions.

## Store Composition

### Store Calling Another Store

```typescript
export const useRedisStore = defineStore("redis", {
  actions: {
    async connect(config: ConnectionConfig) {
      const appStore = useAppStore();

      appStore.setLoading(true);

      try {
        await api.connect(config);
        appStore.showSuccess("Connected");
      } catch (error) {
        appStore.showError("Connection failed");
      } finally {
        appStore.setLoading(false);
      }
    },
  },
});
```

## Pinia Instance

### Store Registry (src/stores/index.ts)

```typescript
// Utilities
import { createPinia } from "pinia";

export default createPinia();
```

**Unique aspect**: Minimal export of Pinia instance, imported in plugin registration.

### Plugin Registration

```typescript
// src/plugins/index.ts
import pinia from "../stores";

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia); // Registered after router
}
```

## What Makes Stores Unique in This Project

1. **No Imports**: `defineStore` and `storeToRefs` are auto-imported
2. **Options API**: Uses Options syntax, not Setup Stores
3. **Minimal Start**: Stores start empty and grow organically
4. **TypeScript Inline**: Types defined inline with `as` type assertions
5. **Standard Structure**: All stores follow same state/getters/actions pattern

## Store Creation Checklist

- [ ] Create `.ts` file in `src/stores/` directory
- [ ] Export store with `useXxxStore` naming convention
- [ ] Use Options API syntax (state/getters/actions)
- [ ] Don't import `defineStore` (auto-imported)
- [ ] Type state properties with TypeScript
- [ ] Use `state: () => ({})` factory function
- [ ] Put computed logic in getters
- [ ] Put state mutations in actions
- [ ] Async operations go in actions

## Common Patterns

### Loading State Pattern

```typescript
export const useDataStore = defineStore("data", {
  state: () => ({
    items: [] as Item[],
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async fetchItems() {
      this.loading = true;
      this.error = null;

      try {
        this.items = await api.getItems();
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
  },
});
```

### Persistent State Pattern

```typescript
export const useSettingsStore = defineStore("settings", {
  state: () => ({
    theme: (localStorage.getItem("theme") || "system") as string,
    sidebarOpen: localStorage.getItem("sidebarOpen") === "true",
  }),

  actions: {
    setTheme(theme: string) {
      this.theme = theme;
      localStorage.setItem("theme", theme);
    },

    toggleSidebar() {
      this.sidebarOpen = !this.sidebarOpen;
      localStorage.setItem("sidebarOpen", String(this.sidebarOpen));
    },
  },
});
```

### Reset Pattern

```typescript
actions: {
  $reset() {
    this.connections = []
    this.activeConnection = null
    this.loading = false
    this.error = null
  },
}
```

### Batch Operations

```typescript
actions: {
  async batchUpdate(updates: Update[]) {
    this.loading = true

    try {
      for (const update of updates) {
        await this.updateItem(update)
      }
    } finally {
      this.loading = false
    }
  },
}
```

### Batch Operations

```typescript
actions: {
  async batchUpdate(updates: Update[]) {
    this.loading = true

    try {
      for (const update of updates) {
        await this.updateItem(update)
      }
    } finally {
      this.loading = false
    }
  },
}
```
