import { formatDistanceToNow } from "date-fns";
import { Bell, CheckCheck, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Spinner } from "@/components/ui/misc";
import {
  notificationIcon,
  useOpenNotification,
} from "@/features/notifications/notificationHelpers";
import {
  useMarkAllRead,
  useNotifications,
  useUnreadCount,
} from "@/features/notifications/notifications";
import { cn } from "@/lib/utils";

export function NotificationsPage() {
  const { data, isPending, fetchNextPage, hasNextPage, isFetchingNextPage } = useNotifications();
  const { data: unread } = useUnreadCount();
  const markAll = useMarkAllRead();
  const open = useOpenNotification();
  const items = data?.pages.flatMap((p) => p.items) ?? [];

  return (
    <div className="mx-auto w-full max-w-2xl px-6 py-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Notifications</h1>
          <p className="mt-1 text-sm text-muted-foreground">
            {unread ? `${unread} unread` : "You are all caught up"}
          </p>
        </div>
        <Button variant="outline" size="sm" disabled={!unread} onClick={() => markAll.mutate()}>
          <CheckCheck className="h-4 w-4" /> Mark all read
        </Button>
      </div>

      {isPending ? (
        <div className="flex justify-center py-16">
          <Spinner className="h-6 w-6" />
        </div>
      ) : items.length === 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed border-border p-12 text-center">
          <div className="rounded-full bg-secondary p-3 text-muted-foreground">
            <Bell className="h-6 w-6" />
          </div>
          <p className="font-medium">No notifications</p>
        </div>
      ) : (
        <Card className="divide-y divide-border">
          {items.map((n) => {
            const Icon = notificationIcon(n.type);
            return (
              <button
                key={n.id}
                onClick={() => open(n)}
                className={cn(
                  "flex w-full items-start gap-3 p-4 text-left transition-colors hover:bg-secondary/50",
                  !n.isRead && "bg-accent/40",
                )}
              >
                <div className="flex h-9 w-9 items-center justify-center rounded-full bg-secondary text-muted-foreground">
                  <Icon className="h-4 w-4" />
                </div>
                <div className="min-w-0 flex-1">
                  <p className="text-sm">{n.message}</p>
                  <p className="mt-0.5 text-xs text-muted-foreground">
                    {formatDistanceToNow(new Date(n.createdAt), { addSuffix: true })}
                  </p>
                </div>
                {!n.isRead && <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary" />}
              </button>
            );
          })}
        </Card>
      )}

      {hasNextPage && (
        <div className="mt-4 flex justify-center">
          <Button
            variant="outline"
            size="sm"
            onClick={() => void fetchNextPage()}
            disabled={isFetchingNextPage}
          >
            {isFetchingNextPage && <Loader2 className="h-4 w-4 animate-spin" />} Load more
          </Button>
        </div>
      )}
    </div>
  );
}
