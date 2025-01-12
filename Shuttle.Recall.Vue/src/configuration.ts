import type { Configuration } from "./recall";

const configuration: Configuration = {
  signInUrl: import.meta.env.VITE_SIGN_IN_URL,

  url: `${import.meta.env.VITE_API_URL}${
    import.meta.env.VITE_API_URL.endsWith("/") ? "" : "/"
  }`,

  accessUrl: `${import.meta.env.VITE_ACCESS_API_URL}${
    import.meta.env.VITE_ACCESS_API_URL.endsWith("/") ? "" : "/"
  }`,

  debugging() {
    return import.meta.env.DEV;
  },

  getApiUrl(path: string) {
    return this.url + path;
  },

  getAccessApiUrl(path: string) {
    return this.accessUrl + path;
  },
};

if (!import.meta.env.VITE_API_URL) {
  throw new Error("Configuration item 'VITE_API_URL' has not been set.");
}

if (!import.meta.env.VITE_ACCESS_API_URL) {
  throw new Error("Configuration item 'VITE_ACCESS_API_URL' has not been set.");
}

if (!import.meta.env.VITE_SIGN_IN_URL) {
  throw new Error("Configuration item 'VITE_SIGN_IN_URL' has not been set.");
}

if (Object.freeze) {
  Object.freeze(configuration);
}

export default configuration;
