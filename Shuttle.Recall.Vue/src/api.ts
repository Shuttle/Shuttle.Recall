import axios, { type AxiosInstance } from "axios";
import { useAlertStore } from "@/stores/alert";
import { useSessionStore } from "@/stores/session";
import configuration from "./configuration";
import { i18n } from "@/i18n";

const configure = (api: AxiosInstance): AxiosInstance => {
  api.interceptors.request.use(function (config) {
    const sessionStore = useSessionStore();

    if (sessionStore.isAuthenticated) {
      config.headers["Authorization"] =
        `Shuttle.Access token=${sessionStore.token}`;
    }

    return config;
  });

  api.interceptors.response.use(
    (response) => response,
    (error) => {
      const alertStore = useAlertStore();

      if (error.response?.status === 401) {
        alertStore.add({
          message: i18n.global.t("exceptions.unauthorized"),
          type: "error",
          name: "api-error",
        });

        return error;
      }

      alertStore.add({
        message:
          error.response?.data ||
          error.response?.statusText ||
          "(unknown communication/network error)",
        type: "error",
        name: "api-error",
      });

      return Promise.reject(error);
    },
  );

  return api;
};

const accessApi = configure(
  axios.create({ baseURL: configuration.getAccessUrl() }),
);
const recallApi = configure(
  axios.create({ baseURL: configuration.getRecallUrl() }),
);

export { accessApi, recallApi };
