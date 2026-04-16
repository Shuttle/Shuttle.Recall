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

export type ApplicationRef = {
  name: string;
  ref: Ref<HTMLElement | null>;
};

export type Configuration = {
  isOk: () => boolean;
  getErrorMessage: () => string;
  getAccessUrl: () => string;
  getRecallUrl: () => string;
  isDebugging: () => boolean;
  getApiUrl: (api: Api, path: string) => string;
};

export type ConfirmationItem = {
  name: string;
  getConfirmationMessage: () => string;
  touched?: boolean;
  unwatch?: WatchHandle;
};

export type ConfirmationOptions = {
  item?: any;
  messageKey?: string;
  messageText?: string;
  titleKey?: string;
  titleText?: string;
};

export type ConfirmationResult = {
  confirmed: boolean;
  item?: any;
};

export type Credentials = {
  identityName: string;
  token?: string;
  password?: string;
  tenantId?: string;
};

export type DrawerOptions = {
  parentPath: string;
  refresh: () => Promise<void>;
};

export type DrawerSize = "compact" | "expanded" | "full" | undefined;

export interface EnumDefinition<
  T extends Record<string, number> = Record<string, number>,
> {
  name: string;
  values: T;
}

export type Env = {
  VITE_ACCESS_API_URL: string;
  VITE_RECALL_API_URL: string;
};

export type NavigationItem = {
  permission?: string;
  title: string;
  to?: string;
  ref?: string;
};

export type OAuthData = {
  code: string;
  state: string;
};

export type Permission = {
  id: string;
  name: string;
  status: number;
};

export type Title = {
  title: string;
  description?: string;
  closeDrawer?: boolean;
  closePath?: string;
  closeClick?: () => void;
};

export type SelectItem = {
  value: string;
  title: string;
};

export type Session = {
  id: string;
  identityId: string;
  identityName: string;
  identityDescription: string;
  permissions: Permission[];
  expiryDate?: Date;
  dateRegistered?: Date;
  tenantId?: string;
  tenantName?: string;
  tokenHash?: number[];
};

export type SessionResponse = {
  session?: Session;
  registrationRequested: boolean;
  result: string;
  token: string | null;
  tenants: Tenant[];
};

export type Tenant = {
  id: string;
  name: string;
  logoSvg?: string;
  logoUrl?: string;
};
