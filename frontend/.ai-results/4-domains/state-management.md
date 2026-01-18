# State Management Domain Analysis

## Overview

The state management domain uses Pinia, the official Vue state management library, for managing global application state in the Roman Redis Manager.

## Core Technologies

- **Pinia** (v3.0.3) - Official Vue state management
- **TypeScript** - Fully typed stores
- **Auto-import** - defineStore and storeToRefs auto-imported

## Store Setup

### Pinia Instance (src/stores/index.ts)

```typescript
// Utilities
import { createPinia } from "pinia";

export default createPinia();
```

### Plugin Registration (src/plugins/index.ts)

```typescript
import pinia from "../stores";

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia); // Pinia registered after router
}
```

## Store Structure

### App Store (src/stores/app.ts)

```typescript
// Utilities
import { defineStore } from "pinia";

export const useAppStore = defineStore("app", {
  state: () => ({
    //
  }),
});
```

This is the base store structure used in the project. Currently minimal but ready for expansion.

## Key Conventions

### 1. Options API Syntax

Stores use Pinia's Options API syntax (not Setup Stores):

```typescript
export const useAppStore = defineStore("app", {
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

### 2. Store Naming Convention

- Store names: `useXxxStore` (e.g., `useAppStore`, `useRedisStore`)
- Store IDs: lowercase string matching purpose (e.g., `'app'`, `'redis'`)

### 3. Auto-Import Configuration

From vite.config.mts:

```typescript
AutoImport({
  imports: [
    "vue",
    VueRouterAutoImports,
    {
      pinia: ["defineStore", "storeToRefs"],
    },
  ],
});
```

This means:

- `defineStore` is available without imports
- `storeToRefs` is available without imports
- No need to `import { defineStore } from 'pinia'`

### 4. File Organization

All stores located in `src/stores/`:

```
src/stores/
  ├── index.ts        # Pinia instance
  ├── app.ts          # App store
  └── README.md       # Documentation
```

## Using Stores in Components

### Basic Store Usage

```typescript
<script setup lang="ts">
// defineStore is auto-imported
const appStore = useAppStore()

// Access state
const myValue = appStore.someState

// Call actions
appStore.someAction()
</script>
```

### With Reactivity (storeToRefs)

```typescript
<script setup lang="ts">
// Both are auto-imported
const appStore = useAppStore()

// Extract reactive properties
const { someState, anotherState } = storeToRefs(appStore)

// Actions don't need refs
const { someAction } = appStore
</script>
```

## Store Patterns

### State Definition

```typescript
export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [] as RedisConnection[],
    activeConnection: null as RedisConnection | null,
    keys: [] as string[],
    loading: false,
  }),
});
```

### Getters (Computed Properties)

```typescript
export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [] as RedisConnection[],
  }),

  getters: {
    activeConnections: (state) => {
      return state.connections.filter((c) => c.active);
    },

    connectionCount(): number {
      return this.connections.length;
    },
  },
});
```

### Actions (Methods)

```typescript
export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [] as RedisConnection[],
    loading: false,
  }),

  actions: {
    async fetchConnections() {
      this.loading = true;
      try {
        // API call
        const data = await api.getConnections();
        this.connections = data;
      } catch (error) {
        console.error("Failed to fetch connections:", error);
      } finally {
        this.loading = false;
      }
    },

    addConnection(connection: RedisConnection) {
      this.connections.push(connection);
    },
  },
});
```

## TypeScript Integration

### Typed Store

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
  }),

  getters: {
    // Return type inferred
    getConnectionById: (state) => {
      return (id: string): RedisConnection | undefined => {
        return state.connections.find((c) => c.id === id);
      };
    },
  },

  actions: {
    // Parameters typed
    updateConnection(id: string, updates: Partial<RedisConnection>) {
      const connection = this.connections.find((c) => c.id === id);
      if (connection) {
        Object.assign(connection, updates);
      }
    },
  },
});
```

## Store Composition

### Using Multiple Stores

```typescript
<script setup lang="ts">
const appStore = useAppStore()
const redisStore = useRedisStore()

// Stores can interact
const loadData = async () => {
  appStore.setLoading(true)
  await redisStore.fetchConnections()
  appStore.setLoading(false)
}
</script>
```

### Store Accessing Another Store

```typescript
export const useRedisStore = defineStore("redis", {
  actions: {
    async performAction() {
      const appStore = useAppStore();
      appStore.setLoading(true);

      // ... action logic

      appStore.setLoading(false);
    },
  },
});
```

## Common Patterns

### Loading States

```typescript
export const useAppStore = defineStore("app", {
  state: () => ({
    loading: false,
    error: null as string | null,
  }),

  actions: {
    async performAsyncOperation() {
      this.loading = true;
      this.error = null;

      try {
        // Async operation
      } catch (err) {
        this.error = err.message;
      } finally {
        this.loading = false;
      }
    },
  },
});
```

### Persisted State

For data that should persist across sessions:

```typescript
export const useSettingsStore = defineStore("settings", {
  state: () => ({
    theme: localStorage.getItem("theme") || "system",
    sidebarOpen: localStorage.getItem("sidebarOpen") === "true",
  }),

  actions: {
    setTheme(theme: string) {
      this.theme = theme;
      localStorage.setItem("theme", theme);
    },
  },
});
```

### Reset State

```typescript
export const useRedisStore = defineStore("redis", {
  state: () => ({
    connections: [],
    activeConnection: null,
  }),

  actions: {
    $reset() {
      this.connections = [];
      this.activeConnection = null;
    },
  },
});
```

## Best Practices

1. **Use Options API syntax** - Consistent with current store structure
2. **Type everything** - Define interfaces for complex state objects
3. **Leverage auto-imports** - No need to import `defineStore` or `storeToRefs`
4. **Use storeToRefs** - Extract reactive state properties while maintaining reactivity
5. **Keep stores focused** - One store per domain (redis, app, settings, etc.)
6. **Async in actions** - All async operations should be actions, not getters
7. **Direct action calls** - Actions don't need destructuring with `storeToRefs`

## Adding a New Store

To add a new store:

1. **Create store file** in `src/stores/`:

   ```typescript
   // src/stores/redis.ts
   export const useRedisStore = defineStore("redis", {
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

2. **Use in components** (no import needed):

   ```vue
   <script setup lang="ts">
   const redisStore = useRedisStore();
   const { connections } = storeToRefs(redisStore);
   </script>
   ```

3. **TypeScript types** defined inline or imported:

   ```typescript
   import type { RedisConnection } from "@/types";

   export const useRedisStore = defineStore("redis", {
     state: () => ({
       connections: [] as RedisConnection[],
     }),
   });
   ```

## Store Communication

### Option 1: Direct Store Import

```typescript
export const useRedisStore = defineStore("redis", {
  actions: {
    async connect() {
      const appStore = useAppStore();
      appStore.showNotification("Connecting...");
    },
  },
});
```

### Option 2: Composables

For complex store interactions, create composables:

```typescript
// composables/useRedisConnection.ts
export function useRedisConnection() {
  const redisStore = useRedisStore();
  const appStore = useAppStore();

  const connect = async (config: ConnectionConfig) => {
    appStore.setLoading(true);
    try {
      await redisStore.connect(config);
      appStore.showSuccess("Connected");
    } catch (error) {
      appStore.showError("Connection failed");
    } finally {
      appStore.setLoading(false);
    }
  };

  return { connect };
}
```

## DevTools Integration

Pinia integrates with Vue DevTools for:

- Viewing store state
- Time-travel debugging
- Action logging
- State mutation trackingnnection.ts
  export function useRedisConnection() {
  const redisStore = useRedisStore();
  const appStore = useAppStore();

  const connect = async (config: ConnectionConfig) => {
  appStore.setLoading(true);
  try {
  await redisStore.connect(config);
  appStore.showSuccess("Connected");
  } catch (error) {
  appStore.showError("Connection failed");
  } finally {
  appStore.setLoading(false);
  }
  };

  return { connect };
  }

```

## DevTools Integration

Pinia integrates with Vue DevTools for:

- Viewing store state
- Time-travel debugging
- Action logging
- State mutation tracking
```
