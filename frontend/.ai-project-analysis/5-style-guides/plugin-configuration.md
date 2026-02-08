# Plugin Configuration Style Guide

## Overview

Plugins in this project include Vuetify configuration and the plugin registration system.

## Plugin Structure

### Plugin Index (src/plugins/index.ts)

```typescript
/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Plugins
import vuetify from "./vuetify";
import pinia from "../stores";
import router from "../router";

// Types
import type { App } from "vue";

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia);
}
```

## Unique Conventions

### 1. Single Registration Function

All plugins registered through one function:

```typescript
// ✅ Central registration
export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia);
}

// ❌ Not scattered in main.ts
```

### 2. Specific Plugin Order

Order matters for this project:

```typescript
app
  .use(vuetify) // 1. Vuetify first
  .use(router) // 2. Router second
  .use(pinia); // 3. Pinia last
```

### 3. Vuetify Configuration (src/plugins/vuetify.ts)

```typescript
/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */

// Styles
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";

// Composables
import { createVuetify } from "vuetify";

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
    defaultTheme: "system",
  },
});
```

**Key points**:

- Import MDI icons CSS
- Import Vuetify base styles
- Minimal theme configuration
- Default theme is 'system' (auto light/dark)

### 4. Style Imports in Plugin

Vuetify plugin imports all necessary styles:

```typescript
// ✅ Styles imported in plugin
import "@mdi/font/css/materialdesignicons.css";
import "vuetify/styles";

// ❌ Not in main.ts or components
```

### 5. Plugin File Comments

All plugin files have JSDoc-style headers:

```typescript
/**
 * plugins/vuetify.ts
 *
 * Framework documentation: https://vuetifyjs.com`
 */
```

## Vuetify Configuration Patterns

### Theme Customization

```typescript
export default createVuetify({
  theme: {
    defaultTheme: "light",
    themes: {
      light: {
        colors: {
          primary: "#1976D2",
          secondary: "#424242",
          accent: "#82B1FF",
          error: "#FF5252",
          info: "#2196F3",
          success: "#4CAF50",
          warning: "#FB8C00",
        },
      },
      dark: {
        colors: {
          primary: "#2196F3",
          secondary: "#424242",
        },
      },
    },
  },
});
```

### Defaults Configuration

```typescript
export default createVuetify({
  defaults: {
    VBtn: {
      variant: "elevated",
      color: "primary",
    },
    VCard: {
      elevation: 2,
    },
  },
});
```

### Icons Configuration

```typescript
export default createVuetify({
  icons: {
    defaultSet: "mdi",
    sets: {
      mdi,
    },
  },
});
```

### Display Configuration

```typescript
export default createVuetify({
  display: {
    mobileBreakpoint: "sm",
    thresholds: {
      xs: 0,
      sm: 600,
      md: 960,
      lg: 1280,
      xl: 1920,
    },
  },
});
```

## Adding New Plugins

### Step 1: Create Plugin File

```typescript
// src/plugins/i18n.ts
import { createI18n } from "vue-i18n";

export default createI18n({
  locale: "en",
  fallbackLocale: "en",
  messages: {
    en: {
      // translations
    },
  },
});
```

### Step 2: Add to Registration

```typescript
// src/plugins/index.ts
import vuetify from "./vuetify";
import pinia from "../stores";
import router from "../router";
import i18n from "./i18n"; // ← Add import

export function registerPlugins(app: App) {
  app
    .use(vuetify)
    .use(router)
    .use(i18n) // ← Add to chain
    .use(pinia);
}
```

## Main.ts Integration

### Application Bootstrap (src/main.ts)

```typescript
/**
 * main.ts
 *
 * Bootstraps Vuetify and other plugins then mounts the App`
 */

// Plugins
import { registerPlugins } from "@/plugins";

// Components
import App from "./App.vue";

// Composables
import { createApp } from "vue";

// Styles
import "unfonts.css";

const app = createApp(App);

registerPlugins(app);

app.mount("#app");
```

**Pattern**:

- Create app
- Register plugins via single function
- Mount app

## What Makes Plugin Config Unique

1. **Central Registration**: All plugins through one function
2. **Specific Order**: Vuetify → Router → Pinia
3. **Minimal Vuetify Config**: Only theme configuration
4. **System Theme Default**: Auto light/dark mode
5. **Style Imports in Plugin**: MDI and Vuetify styles imported in plugin file
6. **JSDoc Headers**: All plugin files have documentation headers

## Plugin Configuration Checklist

- [ ] Create plugin file in `src/plugins/`
- [ ] Export plugin instance as default
- [ ] Add JSDoc header comment
- [ ] Import in `src/plugins/index.ts`
- [ ] Add to `registerPlugins()` function
- [ ] Consider plugin registration order
- [ ] Import required styles if needed
- [ ] Keep configuration minimal

## Common Plugin Additions

### Vue I18n

```typescript
// src/plugins/i18n.ts
import { createI18n } from "vue-i18n";

export default createI18n({
  legacy: false,
  locale: "en",
  fallbackLocale: "en",
  messages: {
    en: require("@/locales/en.json"),
    es: require("@/locales/es.json"),
  },
});
```

### Vue Query

```typescript
// src/plugins/vue-query.ts
import { VueQueryPlugin } from "@tanstack/vue-query";

export default {
  install(app: App) {
    app.use(VueQueryPlugin, {
      queryClientConfig: {
        defaultOptions: {
          queries: {
            staleTime: 60000,
          },
        },
      },
    });
  },
};
```

### Day.js

```typescript
// src/plugins/dayjs.ts
import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

dayjs.extend(relativeTime);

export default {
  install(app: App) {
    app.config.globalProperties.$dayjs = dayjs;
  },
};
```

## Vuetify Customization Examples

### Custom Theme

```typescript
export default createVuetify({
  theme: {
    defaultTheme: "redisManagerTheme",
    themes: {
      redisManagerTheme: {
        dark: false,
        colors: {
          primary: "#DC382D", // Redis red
          secondary: "#424242",
          accent: "#82B1FF",
        },
      },
    },
  },
});
```

### SSR Configuration

```typescript
export default createVuetify({
  ssr: true,
});
```

### Custom Aliases

````typescript
export default createVuetify({
  aliases: {
    VRedisCard: VCard,
  },
});
``` SSR Configuration

```typescript
export default createVuetify({
  ssr: true,
});
````

### Custom Aliases

```typescript
export default createVuetify({
  aliases: {
    VRedisCard: VCard,
  },
});
```
