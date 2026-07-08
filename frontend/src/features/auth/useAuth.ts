import { useContext } from "react";
import { AuthContext, type AuthContextValue } from "@/features/auth/authContext";

/// Access the authentication context. Throws if used outside the provider.
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }
  return context;
}
