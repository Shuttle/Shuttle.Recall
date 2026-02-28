import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";
import { loadLocaleMessages } from "@/i18n";
import { i18n } from "@/i18n";
import { useSessionStore } from "./stores/session";
import { useAlertStore } from "./stores/alert";

const app = createApp(App);

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

await loadLocaleMessages(i18n, "en");

app.use(i18n);
app.use(router);

registerPlugins(app);

const sessionStore = useSessionStore();

if (!sessionStore.isInitialized && window.location.pathname !== "/oauth") {
  try {
    await sessionStore.initialize();
  } catch (error: any) {
    useAlertStore().add({
      message: error.toString(),
      type: "error",
      name: "session-initialize",
    });
    if (!window.location.pathname.startsWith("/signin")) {
      router.push({ path: "/signin" });
    }
  }
}

if (window.location.pathname === "/") {
  router.push({ path: "/events" });
}

app.mount("#app");
