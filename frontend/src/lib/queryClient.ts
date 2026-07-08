import { QueryClient } from "@tanstack/react-query";

/// The shared TanStack Query client. A short stale time keeps dashboard-style data reasonably fresh
/// without refetching on every focus; failed queries retry once (auth 401s are handled by the axios
/// interceptor, not here).
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});
