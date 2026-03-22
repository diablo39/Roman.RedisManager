/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Types
import type { App } from 'vue'
import router from '../router'
import pinia from '../stores'

// Plugins
import { initializeOidcPlugin } from './oidc'
import vuetify from './vuetify'

export function registerPlugins(app: App) {
  initializeOidcPlugin()

  app.use(vuetify).use(router).use(pinia)
}
