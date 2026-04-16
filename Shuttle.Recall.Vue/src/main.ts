import { createApp } from "vue";
import { registerPlugins } from "@/plugins";

import App from "./App.vue";
import router from "./router";

import { useSessionStore } from "./stores/session";
import { useAlertStore } from "./stores/alert";

const app = createApp(App);

registerPlugins(app);

const sessionStore = useSessionStore();
let signin = false;

if (!sessionStore.isInitialized && window.location.pathname !== "/oauth") {
  try {
    const sessionResponse = await sessionStore.initialize();

    if (sessionResponse?.result === "Registered") {
      router.push({ path: "/events" });
    } else {
      signin = true;
    }
  } catch (error: any) {
    useAlertStore().add({
      message: error.toString(),
      type: "error",
      name: "session-initialize",
    });
    signin = true;
  }
}

if (signin && !window.location.pathname.startsWith("/sign-in")) {
  router.push({ path: "/sign-in" });
}

app.mount("#app");
