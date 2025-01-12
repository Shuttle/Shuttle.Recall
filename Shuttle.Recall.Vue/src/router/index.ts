import type { RouteRecordRaw } from "vue-router";
import { createRouter, createWebHistory } from "vue-router";
import { useSessionStore } from "@/stores/session";
import { useAlertStore } from "@/stores/alert";
import { useBreadcrumbStore } from "@/stores/breadcrumb";
import type { Breadcrumb } from "@/stores";
import { i18n } from "@/i18n";

const ignoreBreadcrumbs = ["session", "oauth"];

const routes: Array<RouteRecordRaw> = [
  {
    path: "/events",
    name: "events",
    component: () => import("../views/Events.vue"),
  },
  {
    path: "/session/:token",
    name: "session",
    props: true,
    component: () => import("../views/Session.vue"),
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

router.beforeEach(async (to) => {
  const sessionStore = useSessionStore();

  if (!sessionStore.initialized) {
    return;
  }

  if (
    !!to.meta.permission &&
    !sessionStore.hasPermission(to.meta.permission as string)
  ) {
    useAlertStore().add({
      message: i18n.global.t("exceptions.insufficient-permission"),
      type: "info",
      name: "insufficient-permission",
    });

    return false;
  }

  if (!!to.meta.authenticated && !sessionStore.authenticated) {
    useAlertStore().add({
      message: i18n.global.t("exceptions.session-required"),
      type: "info",
      name: "insufficient-permission",
    });
  }
});

router.afterEach(async (to) => {
  const breadcrumbStore = useBreadcrumbStore();

  var name = typeof to.name === "string" ? to.name : undefined;

  if (!name || ignoreBreadcrumbs.includes(name)) {
    breadcrumbStore.clear();
    return;
  }

  if (name === "dashboard") {
    breadcrumbStore.clear();
  }

  const existingIndex = breadcrumbStore.breadcrumbs.findIndex(
    (route: Breadcrumb) => route.path === to.path
  );

  if (existingIndex === -1) {
    breadcrumbStore.addBreadcrumb({
      name: to.name,
      path: to.path,
    });
  } else {
    breadcrumbStore.removeBreadcrumbsAfter(existingIndex);
  }
});

export default router;
