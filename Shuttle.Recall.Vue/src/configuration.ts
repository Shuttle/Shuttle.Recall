import type { Configuration, Env } from "@/types/app";
import { Api } from "./enums";
import axios from "axios";

let errorMessage: string;
let values: Env;
let isOk = true;

try {
  const env = async (): Promise<Env> => {
    if (import.meta.env.MODE === "production") {
      return (await axios.get<Env>("/env")).data;
    } else {
      return {
        VITE_ACCESS_API_URL: import.meta.env.VITE_ACCESS_API_URL,
        VITE_RECALL_API_URL: import.meta.env.VITE_RECALL_API_URL,
      };
    }
  };

  values = await env();
} catch (error: any) {
  isOk = false;
  errorMessage = error.toString();
}

const getConfiguration = (): Configuration => {
  return {
    isOk() {
      return isOk;
    },
    getErrorMessage() {
      return errorMessage;
    },
    getAccessUrl() {
      return isOk
        ? `${values.VITE_ACCESS_API_URL}${values.VITE_ACCESS_API_URL.endsWith("/") ? "" : "/"}`
        : "";
    },
    getRecallUrl() {
      return isOk
        ? `${values.VITE_RECALL_API_URL}${values.VITE_RECALL_API_URL.endsWith("/") ? "" : "/"}`
        : "";
    },
    isDebugging() {
      return import.meta.env.DEV;
    },
    getApiUrl(api: Api, path: string) {
      if (path.startsWith("/") && path.length < 2) {
        path = "";
      }

      const buildApiUrl = (baseUrl: string, path: string) => {
        return baseUrl + (path.startsWith("/") ? path.substring(1) : path);
      };

      switch (api) {
        case Api.Access: {
          return buildApiUrl(this.getAccessUrl(), path);
        }
        case Api.Recall: {
          return buildApiUrl(this.getRecallUrl(), path);
        }
        default: {
          throw `Unknown Api name '${api}'.`;
        }
      }
    },
  };
};

const configuration = getConfiguration();

if (Object.freeze) {
  Object.freeze(configuration);
}

export default configuration;
