# Build System Domain Analysis

## Overview

The build system uses Vite 7 as the primary build tool with extensive plugin configuration for auto-imports, file-based routing, and Vuetify integration.

## Core Technologies

- **Vite** (v7.1.5) - Build tool and dev server
- **TypeScript** (v5.9.2) - Type system
- **vue-tsc** (v3.2.0) - Vue TypeScript compiler
- **ESLint** (v9.35.0) - Code linting

## Vite Configuration

### Main Config (vite.config.mts)

```typescript
// Plugins
import AutoImport from "unplugin-auto-import/vite";
import Components from "unplugin-vue-components/vite";
import Fonts from "unplugin-fonts/vite";
import Layouts from "vite-plugin-vue-layouts-next";
import Vue from "@vitejs/plugin-vue";
import VueRouter from "unplugin-vue-router/vite";
import { VueRouterAutoImports } from "unplugin-vue-router";
import Vuetify, { transformAssetUrls } from "vite-plugin-vuetify";

// Utilities
import { defineConfig } from "vite";
import { fileURLToPath, URL } from "node:url";

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [
    VueRouter({
      dts: "src/typed-router.d.ts",
    }),
    Layouts(),
    AutoImport({
      imports: [
        "vue",
        VueRouterAutoImports,
        {
          pinia: ["defineStore", "storeToRefs"],
        },
      ],
      dts: "src/auto-imports.d.ts",
      eslintrc: {
        enabled: true,
      },
      vueTemplate: true,
    }),
    Components({
      dts: "src/components.d.ts",
    }),
    Vue({
      template: { transformAssetUrls },
    }),
    // https://github.com/vuetifyjs/vuetify-loader/tree/master/packages/vite-plugin#readme
    Vuetify({
      autoImport: true,
      styles: {
        configFile: "src/styles/settings.scss",
      },
    }),
    Fonts({
      fontsource: {
        families: [
          {
            name: "Roboto",
            weights: [100, 300, 400, 500, 700, 900],
            styles: ["normal", "italic"],
          },
        ],
      },
    }),
  ],
  optimizeDeps: {
    exclude: [
      "vuetify",
      "vue-router",
      "unplugin-vue-router/runtime",
      "unplugin-vue-router/data-loaders",
      "unplugin-vue-router/data-loaders/basic",
    ],
  },
  define: { "process.env": {} },
  resolve: {
    alias: {
      "@": fileURLToPath(new URL("src", import.meta.url)),
    },
    extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"],
  },
  server: {
    port: 3000,
  },
});
```

## Plugin System

### 1. VueRouter Plugin (unplugin-vue-router)

- **Purpose**: File-based routing
- **Config**: Generates typed routes in `src/typed-router.d.ts`
- **Source**: `src/pages/` directory
- **Output**: Auto-generated routes from file structure

### 2. Layouts Plugin (vite-plugin-vue-layouts-next)

- **Purpose**: Layout system for Vue
- **Config**: Default configuration
- **Source**: `src/layouts/` directory
- **Integration**: Wraps routes with layouts

### 3. AutoImport Plugin (unplugin-auto-import)

- **Purpose**: Auto-imports Vue APIs and composables
- **Auto-imported APIs**:
  - All Vue APIs (ref, computed, watch, etc.)
  - Vue Router composables (useRouter, useRoute, useLink)
  - Pinia composables (defineStore, storeToRefs)
- **Output**: Type definitions in `src/auto-imports.d.ts`
- **ESLint Integration**: Generates `.eslintrc-auto-import.json`
- **Vue Template Support**: Auto-imports work in templates

### 4. Components Plugin (unplugin-vue-components)

- **Purpose**: Auto-imports Vue components
- **Source**: `src/components/` directory
- **Output**: Type definitions in `src/components.d.ts`
- **Result**: Components available without explicit imports

### 5. Vue Plugin (@vitejs/plugin-vue)

- **Purpose**: Vue 3 support
- **Config**: Asset URL transformation for Vuetify

### 6. Vuetify Plugin (vite-plugin-vuetify)

- **Purpose**: Vuetify integration
- **Features**:
  - Auto-import Vuetify components
  - SCSS configuration
- **Styles Config**: References `src/styles/settings.scss`

### 7. Fonts Plugin (unplugin-fonts)

- **Purpose**: Web font loading
- **Config**: Loads Roboto from fontsource
- **Weights**: 100, 300, 400, 500, 700, 900
- **Styles**: normal, italic

## TypeScript Configuration

### Root Config (tsconfig.json)

```jsonc
{
  "files": [],
  "references": [
    {
      "path": "./tsconfig.node.json",
    },
    {
      "path": "./tsconfig.app.json",
    },
  ],
}
```

Project uses TypeScript project references for better organization.

### Key TypeScript Features

- **Strict Type Checking**: Full TypeScript strict mode
- **Auto-generated Types**:
  - `src/auto-imports.d.ts` - Vue/Pinia API types
  - `src/components.d.ts` - Component types
  - `src/typed-router.d.ts` - Route types
  - `env.d.ts` - Environment types

## Build Scripts

### Package.json Scripts

```json
{
  "scripts": {
    "dev": "vite",
    "build": "run-p type-check \"build-only {@}\" --",
    "preview": "vite preview",
    "build-only": "vite build",
    "type-check": "vue-tsc --build --force",
    "lint": "eslint . --fix"
  }
}
```

### Script Breakdown

**Development**:

```bash
npm run dev
```

- Starts Vite dev server on port 3000
- Hot Module Replacement (HMR)
- Auto-imports enabled
- Fast refresh

**Production Build**:

```bash
npm run build
```

- Runs type checking first
- Then builds for production
- Output optimized for deployment
- Uses `npm-run-all2` for parallel execution

**Type Checking**:

```bash
npm run type-check
```

- Runs vue-tsc with --build flag
- Checks all TypeScript/Vue files
- Uses --force to rebuild everything

**Linting**:

```bash
npm run lint
```

- Runs ESLint with auto-fix
- Uses vuetify ESLint config
- Checks .vue, .ts, .js files

**Preview**:

```bash
npm run preview
```

- Serves production build locally
- For testing before deployment

## Module Resolution

### Path Aliases

```typescript
resolve: {
  alias: {
    '@': fileURLToPath(new URL('src', import.meta.url)),
  },
}
```

Usage:

```typescript
// Instead of: import Component from '../../components/Component.vue'
import Component from "@/components/Component.vue";

// Asset imports
import logo from "@/assets/logo.png";
```

### File Extensions

Auto-resolves these extensions:

```typescript
extensions: [".js", ".json", ".jsx", ".mjs", ".ts", ".tsx", ".vue"];
```

Can import without extension:

```typescript
import router from "@/router"; // Resolves to router/index.ts
import App from "@/App"; // Resolves to App.vue
```

## Development Server

### Server Configuration

```typescript
server: {
  port: 3000,
}
```

- **Port**: 3000
- **Hot Module Replacement**: Enabled
- **Auto-open**: Not configured (manual)
- **Access**: http://localhost:3000

## Optimization

### Dependency Exclusions

```typescript
optimizeDeps: {
  exclude: [
    'vuetify',
    'vue-router',
    'unplugin-vue-router/runtime',
    'unplugin-vue-router/data-loaders',
    'unplugin-vue-router/data-loaders/basic',
  ],
}
```

These packages are excluded from dependency pre-bundling to avoid issues.

### Process.env Polyfill

```typescript
define: { 'process.env': {} }
```

Provides empty process.env object for compatibility.

## ESLint Configuration

### Config File (eslint.config.js)

```javascript
import vuetify from "eslint-config-vuetify";

export default vuetify();
```

- Uses official Vuetify ESLint config
- Preconfigured rules for Vue 3 + Vuetify
- Auto-import support via `.eslintrc-auto-import.json`

## Build Output

### Production Build

- **Output Directory**: `dist/` (default)
- **Entry Point**: `index.html`
- **Assets**: Hashed filenames for caching
- **Chunks**: Code-split by route (via Vue Router)
- **Minification**: JavaScript and CSS minified
- **Source Maps**: Configurable

## Auto-Generated Files

### Do Not Edit Manually

These files are auto-generated and should not be edited:

1. **src/auto-imports.d.ts**
   - Generated by unplugin-auto-import
   - Contains type definitions for auto-imported APIs

2. **src/components.d.ts**
   - Generated by unplugin-vue-components
   - Contains type definitions for auto-imported components

3. **src/typed-router.d.ts**
   - Generated by unplugin-vue-router
   - Contains type definitions for routes

4. **.eslintrc-auto-import.json**
   - Generated for ESLint auto-import support
   - Prevents linting errors for auto-imported items

## Best Practices

1. **Use Auto-Imports**: Leverage the auto-import system instead of manual imports
2. **Follow File Structure**: Place files in correct directories for auto-discovery
3. **Run Type Check**: Before committing, run `npm run type-check`
4. **Use @ Alias**: For absolute imports from src directory
5. **Don't Edit Generated Files**: Never manually edit .d.ts files
6. **Run Linter**: Use `npm run lint` to fix code style issues
7. **Keep Config Clean**: Only customize what's necessary in vite.config.mts

## Adding New Build Features

### Adding a New Plugin

```typescript
// 1. Install plugin
npm install -D vite-plugin-example

// 2. Import in vite.config.mts
import Example from 'vite-plugin-example'

// 3. Add to plugins array
plugins: [
  // ... existing plugins
  Example({
    // configuration
  }),
]
```

### Customizing Build Output

```typescript
export default defineConfig({
  build: {
    outDir: "dist",
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ["vue", "vue-router", "pinia"],
          vuetify: ["vuetify"],
        },
      },
    },
  },
});
```

### Environment Variables

```typescript
// .env file
VITE_APP_TITLE=Redis Manager
VITE_API_URL=http://localhost:5000

// Access in code
const apiUrl = import.meta.env.VITE_API_URL
```

## Performance Optimizations

1. **Code Splitting**: Automatic route-based splitting
2. **Dependency Pre-bundling**: Vite optimizes dependencies
3. **HMR**: Fast hot module replacement during development
4. **Asset Optimization**: Images and fonts optimized automatically
5. **Tree Shaking**: Unused code removed in production
   output: {
   manualChunks: {
   vendor: ["vue", "vue-router", "pinia"],
   vuetify: ["vuetify"],
   },
   },
   },
   },
   });

````

### Environment Variables

```typescript
// .env file
VITE_APP_TITLE=Redis Manager
VITE_API_URL=http://localhost:5000

// Access in code
const apiUrl = import.meta.env.VITE_API_URL
````

## Performance Optimizations

1. **Code Splitting**: Automatic route-based splitting
2. **Dependency Pre-bundling**: Vite optimizes dependencies
3. **HMR**: Fast hot module replacement during development
4. **Asset Optimization**: Images and fonts optimized automatically
5. **Tree Shaking**: Unused code removed in production
