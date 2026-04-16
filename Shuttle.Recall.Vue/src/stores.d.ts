export type Alert = {
  message: string;
  name: string;
  type?: "error" | "success" | "warning" | "info" | undefined;
  expire?: boolean;
  expirySeconds?: number;
  dismissable?: boolean;
  key?: string;
  variant?: string;
};

export type AlertStoreState = {
  alerts: Alert[];
};

export type Breadcrumb = {
  name: RouteRecordNameGeneric;
  path: string;
};

export type BreadcrumbStoreState = {
  breadcrumbs: Breadcrumb[];
};

export type Configuration = {
  url: string;
  accessUrl: string;
  signInUrl: string;
  debugging: () => boolean;
  getApiUrl: (path: string) => string;
  getAccessApiUrl: (path: string) => string;
};

export type ConfirmationOptions = {
  item: any;
  title?: string;
  message?: string;
  onConfirm?: (item: any) => void;
};

export type ConfirmationStoreState = {
  isOpen: boolean;
  options?: ConfirmationOptions;
};

export type Credentials = {
  identityName: string;
  password?: string;
  token?: string;
};

export type NavigationItem = {
  permission?: string;
  title: string;
  to: string;
};

export type Session = {
  identityName: string;
  token: string;
  permissions: string[];
  expiryDate?: Date;
  dateRegistered?: Date;
};

export type SessionPermission = {
  type: string;
  permission: string;
};

export type SessionResponse = {
  identityName: string;
  permissions: string[];
  registrationRequested: boolean;
  result: string;
  token: string;
  tokenExpiryDate: string;
};

export type SessionStoreState = {
  authenticated: boolean;
  initialized: boolean;
  identityName?: string;
  token?: string;
  permissions: SessionPermission[];
};

export type OAuthData = {
  code: string;
  state: string;
};

export type OAuthProvider = {
  name: string;
  svg: string;
};
