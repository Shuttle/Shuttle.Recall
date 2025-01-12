import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";
import { loadLocaleMessages } from "@/i18n";

import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import { i18n } from "@/i18n";

const app = createApp(App);

document.querySelector("html")?.setAttribute("lang", i18n.global.locale.value);

await loadLocaleMessages(i18n, "en");

app.use(i18n);
app.use(router);

registerPlugins(app);

const alertStore = useAlertStore();
const sessionStore = useSessionStore();

await sessionStore.initialize().catch((error) => {
  alertStore.add({
    message: error.message,
    type: "error",
    name: "session-initialize",
  });
});

if (window.location.pathname === "/") {
  router.push({ path: "/events" });
}

app.mount("#app");
