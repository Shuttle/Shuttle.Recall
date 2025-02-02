import { defineStore } from "pinia";
import axios, { type AxiosResponse } from "axios";
import configuration from "@/configuration";
import { i18n } from "@/i18n";
import type {
  Session,
  SessionStoreState,
  Credentials,
  SessionResponse,
} from "@/stores";

export const useSessionStore = defineStore("session", {
  state: (): SessionStoreState => {
    return {
      authenticated: false,
      initialized: false,
      identityName: "",
      token: "",
      permissions: [],
    };
  },
  getters: {
    status(): string {
      return !this.token ? "not-signed-in" : "signed-in";
    },
  },
  actions: {
    async initialize() {
      const self = this;

      if (this.initialized) {
        return;
      }

      const identityName = localStorage.getItem("shuttle-access.identityName");
      const token = localStorage.getItem("shuttle-access.token");

      if (!!identityName && !!token) {
        return self
          .signIn({ identityName: identityName, token: token })
          .then(function (response: any) {
            return response;
          })
          .finally(() => {
            self.initialized = true;
          });
      }

      return Promise.resolve();
    },
    addPermission(type: string, permission: string) {
      if (this.hasPermission(permission)) {
        return;
      }

      this.permissions.push({ type: type, permission: permission });
    },
    register(session: Session) {
      const self = this;

      if (
        !session ||
        !session.identityName ||
        !session.token ||
        !session.permissions
      ) {
        throw Error(i18n.global.t("exceptions.invalid-session"));
      }

      localStorage.setItem("shuttle-access.identityName", session.identityName);
      localStorage.setItem("shuttle-access.token", session.token);

      self.identityName = session.identityName;
      self.token = session.token;

      self.removePermissions("identity");

      session.permissions.forEach((item: string) => {
        self.addPermission("identity", item);
      });

      this.authenticated = true;
    },
    signInExchangeToken(
      exchangeToken: string
    ): Promise<AxiosResponse<Session>> {
      const self = this;

      return new Promise((resolve, reject) => {
        if (!exchangeToken) {
          reject(new Error(i18n.global.t("exceptions.missing-exchange-token")));
          return;
        }

        return axios
          .get(
            configuration.getAccessApiUrl(
              `v1/sessions/exchange/${exchangeToken}`
            )
          )
          .then(function (response) {
            self.register(response.data);

            resolve(response);
          })
          .catch(function (error) {
            reject(error);
          });
      });
    },
    signIn(credentials: Credentials): Promise<AxiosResponse<SessionResponse>> {
      const self = this;

      return new Promise((resolve, reject) => {
        if (!credentials || !credentials.identityName || !credentials.token) {
          reject(new Error(i18n.global.t("exceptions.missing-credentials")));
          return;
        }

        return axios
          .post(configuration.getAccessApiUrl("v1/sessions"), {
            identityName: credentials.identityName,
            token: credentials.token,
          })
          .then(function (response) {
            if (!response?.data) {
              throw new Error("Argument 'response.data' may not be undefined.");
            }

            const data = response.data;

            if (data.result === "Registered") {
              self.register({
                identityName: credentials.identityName,
                token: data.token,
                permissions: data.permissions,
              });
            }

            resolve(response);
          })
          .catch(function (error) {
            reject(error);
          });
      });
    },
    signOut() {
      this.identityName = undefined;
      this.token = undefined;

      localStorage.removeItem("shuttle-access.identityName");
      localStorage.removeItem("shuttle-access.token");

      this.removePermissions("identity");

      this.authenticated = false;
    },
    removePermissions(type: string) {
      this.permissions = this.permissions.filter(function (item) {
        return item.type !== type;
      });
    },
    hasSession() {
      return !!this.token;
    },
    hasPermission(permission: string) {
      let result = false;
      const permissionCompare = permission.toLowerCase();

      this.permissions.forEach(function (item) {
        if (result) {
          return;
        }

        result =
          item.permission === "*" ||
          item.permission.toLowerCase() === permissionCompare;
      });

      return result;
    },
  },
});
