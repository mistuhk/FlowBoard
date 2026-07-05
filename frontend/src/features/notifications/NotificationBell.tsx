import { formatDistanceToNow } from "date-fns";
import { Bell, CheckCheck } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
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

export function NotificationBell() {
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const { data: unread } = useUnreadCount();
  const { data } = useNotifications(open);
  const markAll = useMarkAllRead();
  const openNotification = useOpenNotification();
  const items = (data?.pages.flatMap((p) => p.items) ?? []).slice(0, 10);

  return (
    <DropdownMenu open={open} onOpenChange={setOpen}>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" aria-label="Notifications" className="relative">
          <Bell className="h-4 w-4" />
          {(unread ?? 0) > 0 && (
            <span className="absolute right-1.5 top-1.5 flex h-4 min-w-4 items-center justify-center rounded-full bg-primary px-1 text-[10px] font-bold text-primary-foreground">
              {unread}
            </span>
          )}
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-80 p-0">
        <div className="flex items-center justify-between border-b border-border px-3 py-2">
          <span className="text-sm font-semibold">Notifications</span>
          <button
            className="inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground disabled:opacity-50"
            disabled={!unread}
            onClick={() => markAll.mutate()}
          >
            <CheckCheck className="h-3.5 w-3.5" /> Mark all read
          </button>
        </div>
        <div className="max-h-80 overflow-y-auto">
          {items.length === 0 ? (
            <p className="px-3 py-8 text-center text-sm text-muted-foreground">
              You are all caught up.
            </p>
          ) : (
            items.map((n) => {
              const Icon = notificationIcon(n.type);
              return (
                <button
                  key={n.id}
                  onClick={() => {
                    setOpen(false);
                    openNotification(n);
                  }}
                  className={cn(
                    "flex w-full items-start gap-2.5 px-3 py-2.5 text-left transition-colors hover:bg-secondary/60",
                    !n.isRead && "bg-accent/40",
                  )}
                >
                  <Icon className="mt-0.5 h-4 w-4 shrink-0 text-muted-foreground" />
                  <span className="min-w-0 flex-1">
                    <span className="block text-sm">{n.message}</span>
                    <span className="block text-xs text-muted-foreground">
                      {formatDistanceToNow(new Date(n.createdAt), { addSuffix: true })}
                    </span>
                  </span>
                  {!n.isRead && (
                    <span className="mt-1.5 h-2 w-2 shrink-0 rounded-full bg-primary" />
                  )}
                </button>
              );
            })
          )}
        </div>
        <div className="border-t border-border p-1">
          <button
            className="w-full rounded-md px-2 py-1.5 text-center text-sm text-muted-foreground hover:bg-secondary hover:text-foreground"
            onClick={() => {
              setOpen(false);
              navigate("/notifications");
            }}
          >
            See all notifications
          </button>
        </div>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
