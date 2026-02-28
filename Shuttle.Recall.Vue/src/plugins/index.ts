import vuetify from "./vuetify";
import pinia from "@/stores";
import router from "@/router";
import { loadLocaleMessages, i18n } from "@/i18n";

import type { App } from "vue";

import RecallContainer from "@/components/RecallContainer.vue";
import RecallDataTable from "@/components/RecallDataTable.vue";
import RecallDrawer from "@/components/RecallDrawer.vue";
import RecallFilterDrawer from "@/components/RecallFilterDrawer.vue";
import RecallFilterToggle from "@/components/RecallFilterToggle.vue";
import RecallStrip from "@/components/RecallStrip.vue";
import RecallTitle from "@/components/RecallTitle.vue";

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

await loadLocaleMessages(i18n, "en");

export function registerPlugins(app: App) {
  app.use(vuetify).use(router).use(pinia).use(i18n);

  app.component("r-container", RecallContainer);
  app.component("r-data-table", RecallDataTable);
  app.component("r-drawer", RecallDrawer);
  app.component("r-filter-drawer", RecallFilterDrawer);
  app.component("r-filter-toggle", RecallFilterToggle);
  app.component("r-strip", RecallStrip);
  app.component("r-title", RecallTitle);
}
