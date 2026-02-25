/**
 * plugins/index.ts
 *
 * Automatically included in `./src/main.ts`
 */

// Plugins
import vuetify from "./vuetify";
import pinia from "@/stores";
import "@/styles/base.css";

// Types
import type { App } from "vue";

export function registerPlugins(app: App) {
  app.use(vuetify).use(pinia);
}
