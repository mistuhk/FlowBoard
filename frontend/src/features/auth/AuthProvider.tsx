import { useCallback, useEffect, useRef, useState } from "react";
import { authApi, type UserProfile } from "@/features/auth/authApi";
import { AuthContext, type AuthStatus } from "@/features/auth/authContext";
import { registerAuthHooks } from "@/lib/apiClient";

/// Holds authentication state. The access token lives ONLY in memory (a ref), never in
/// localStorage or sessionStorage; the refresh token is an httpOnly cookie the browser manages.
/// On load it attempts a silent refresh to restore the session; the axios interceptor calls back
/// here to refresh the token on a 401.
export function AuthProvider({ children }: { children: React.ReactNode }) {
  const accessToken = useRef<string | null>(null);
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [user, setUser] = useState<UserProfile | null>(null);

  const setSession = useCallback(async (token: string) => {
    accessToken.current = token;
    const profile = await authApi.me();
    setUser(profile);
    setStatus("authenticated");
  }, []);

  const clearSession = useCallback(() => {
    accessToken.current = null;
    setUser(null);
    setStatus("unauthenticated");
  }, []);

  // Register the token accessor and refresh callback with the shared API client.
  useEffect(() => {
    registerAuthHooks({
      getAccessToken: () => accessToken.current,
      refreshAccessToken: async () => {
        const token = await authApi.refresh();
        if (token) {
          accessToken.current = token;
          return token;
        }
        clearSession();
        return null;
      },
    });
  }, [clearSession]);

  // Attempt to restore an existing session once on load.
  useEffect(() => {
    let cancelled = false;
    void (async () => {
      const token = await authApi.refresh();
      if (cancelled) return;
      if (token) {
        try {
          await setSession(token);
        } catch {
          clearSession();
        }
      } else {
        clearSession();
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [setSession, clearSession]);

  const login = useCallback(
    async (email: string, password: string) => {
      const token = await authApi.login(email, password);
      await setSession(token);
    },
    [setSession],
  );

  const register = useCallback(async (displayName: string, email: string, password: string) => {
    await authApi.register(displayName, email, password);
  }, []);

  const logout = useCallback(async () => {
    await authApi.logout();
    clearSession();
  }, [clearSession]);

  return (
    <AuthContext.Provider value={{ status, user, login, register, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
