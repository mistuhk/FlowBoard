import axios from "axios";
import { apiClient } from "@/lib/apiClient";

/// The authenticated user's profile, from GET /users/me.
export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  isEmailVerified: boolean;
  createdAt: string;
  lastLoginAt: string | null;
}

interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

// A bare client for the refresh call only. It bypasses the shared client's interceptors so a failed
// refresh cannot recurse into another refresh attempt. It still sends the httpOnly refresh cookie.
const refreshClient = axios.create({ baseURL: "/api/v1", withCredentials: true });

export const authApi = {
  async login(email: string, password: string): Promise<string> {
    const { data } = await apiClient.post<LoginResponse>("/auth/login", { email, password });
    return data.accessToken;
  },

  async register(displayName: string, email: string, password: string): Promise<void> {
    await apiClient.post("/auth/register", { displayName, email, password });
  },

  async verifyEmail(token: string): Promise<void> {
    await apiClient.get("/auth/verify-email", { params: { token } });
  },

  async forgotPassword(email: string): Promise<void> {
    await apiClient.post("/auth/forgot-password", { email });
  },

  async resetPassword(token: string, newPassword: string): Promise<void> {
    await apiClient.post("/auth/reset-password", { token, newPassword });
  },

  /// Exchanges the refresh cookie for a new access token, or returns null when there is no valid session.
  async refresh(): Promise<string | null> {
    try {
      const { data } = await refreshClient.post<LoginResponse>("/auth/refresh");
      return data.accessToken;
    } catch {
      return null;
    }
  },

  async logout(): Promise<void> {
    try {
      await apiClient.post("/auth/logout");
    } catch {
      // Signing out locally must always succeed even if the server call fails.
    }
  },

  async me(): Promise<UserProfile> {
    const { data } = await apiClient.get<UserProfile>("/users/me");
    return data;
  },
};
