import {
  type InfiniteData,
  useInfiniteQuery,
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

export interface AppNotification {
  id: string;
  type: string;
  message: string;
  entityType: string | null;
  entityId: string | null;
  isRead: boolean;
  organisationId: string;
  createdAt: string;
}

interface NotificationPage {
  items: AppNotification[];
  nextCursor: string | null;
}

const keys = {
  list: ["notifications", "list"] as const,
  unread: ["notifications", "unread-count"] as const,
};

/// Unread count, polled every 30 seconds so the bell badge stays current.
export function useUnreadCount() {
  return useQuery({
    queryKey: keys.unread,
    queryFn: async () =>
      (await apiClient.get<{ count: number }>("/notifications/unread-count")).data.count,
    refetchInterval: 30_000,
    refetchIntervalInBackground: true,
  });
}

export function useNotifications(enabled = true) {
  return useInfiniteQuery({
    queryKey: keys.list,
    enabled,
    initialPageParam: undefined as string | undefined,
    queryFn: async ({ pageParam }) =>
      (
        await apiClient.get<NotificationPage>("/notifications", {
          params: pageParam ? { cursor: pageParam } : {},
        })
      ).data,
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });
}

export function useMarkRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/notifications/${id}/read`);
    },
    onMutate: async (id) => {
      await qc.cancelQueries({ queryKey: keys.list });
      const snapshot = qc.getQueryData<InfiniteData<NotificationPage>>(keys.list);
      if (snapshot) {
        qc.setQueryData<InfiniteData<NotificationPage>>(keys.list, {
          ...snapshot,
          pages: snapshot.pages.map((p) => ({
            ...p,
            items: p.items.map((n) => (n.id === id ? { ...n, isRead: true } : n)),
          })),
        });
      }
      return { snapshot };
    },
    onError: (_e, _id, ctx) => ctx?.snapshot && qc.setQueryData(keys.list, ctx.snapshot),
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: keys.list });
      void qc.invalidateQueries({ queryKey: keys.unread });
    },
  });
}

export function useMarkAllRead() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      await apiClient.post("/notifications/read-all");
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: keys.list });
      void qc.invalidateQueries({ queryKey: keys.unread });
    },
  });
}
