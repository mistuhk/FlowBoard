import axios, {
  type AxiosError,
  type AxiosRequestConfig,
  type InternalAxiosRequestConfig,
} from "axios";

/// Hooks the auth layer registers so the client can attach the in-memory access token and silently
/// refresh it on a 401. Kept as a registration seam (rather than importing React state) so the token
/// never leaves memory and the axios module has no dependency on the auth context.
export interface AuthHooks {
  /// Returns the current in-memory access token, or null when signed out.
  getAccessToken: () => string | null;
  /// Attempts to obtain a new access token using the httpOnly refresh cookie. Returns the new token,
  /// or null if refresh failed (the user must sign in again).
  refreshAccessToken: () => Promise<string | null>;
}

let authHooks: AuthHooks | null = null;

/// Registers the auth hooks. Called once by the auth provider on mount.
export function registerAuthHooks(hooks: AuthHooks): void {
  authHooks = hooks;
}

/// The shared API client. `withCredentials` sends the httpOnly refresh cookie; the base URL matches
/// the versioned API surface and is proxied to the backend in development (see vite.config.ts).
export const apiClient = axios.create({
  baseURL: "/api/v1",
  withCredentials: true,
  headers: { "Content-Type": "application/json" },
});

// Attach the bearer token to every request when signed in.
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = authHooks?.getAccessToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// A request that has already been retried after a refresh, so we do not loop.
interface RetriableConfig extends AxiosRequestConfig {
  _retried?: boolean;
}

// De-duplicate concurrent refreshes: many in-flight requests share one refresh attempt.
let refreshInFlight: Promise<string | null> | null = null;

// On a 401, refresh the access token once and replay the original request.
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as (RetriableConfig & InternalAxiosRequestConfig) | undefined;

    if (error.response?.status !== 401 || !original || original._retried || !authHooks) {
      return Promise.reject(error);
    }

    original._retried = true;
    refreshInFlight ??= authHooks.refreshAccessToken().finally(() => {
      refreshInFlight = null;
    });

    const newToken = await refreshInFlight;
    if (!newToken) {
      return Promise.reject(error);
    }

    original.headers = original.headers ?? {};
    (original.headers as Record<string, string>).Authorization = `Bearer ${newToken}`;
    return apiClient(original);
  },
);
