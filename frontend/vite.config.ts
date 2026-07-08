import path from "node:path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// The API base URL for the dev proxy. Defaults to the local API's HTTP port (see
// src/FlowBoard.Api/Properties/launchSettings.json); override with VITE_API_PROXY_TARGET.
const apiProxyTarget = process.env.VITE_API_PROXY_TARGET ?? "http://localhost:5047";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    proxy: {
      // Forward API calls to the backend so the SPA and API share an origin in development,
      // which keeps the httpOnly refresh cookie first-party.
      "/api": {
        target: apiProxyTarget,
        changeOrigin: true,
        secure: false,
      },
    },
  },
});
