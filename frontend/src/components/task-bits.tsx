import { AlertTriangle, ArrowDown, ArrowUp, Minus } from "lucide-react";
import type { Priority, TaskStatus } from "@/features/tasks/tasks";
import { cn } from "@/lib/utils";

const statusStyle: Record<TaskStatus, { dot: string; text: string }> = {
  todo: { dot: "bg-muted-foreground", text: "text-muted-foreground" },
  in_progress: { dot: "bg-primary", text: "text-primary" },
  blocked: { dot: "bg-destructive", text: "text-destructive" },
  done: { dot: "bg-success", text: "text-success" },
};

const STATUS_TEXT: Record<TaskStatus, string> = {
  todo: "To do",
  in_progress: "In progress",
  blocked: "Blocked",
  done: "Done",
};

export function StatusDot({
  status,
  withLabel = false,
}: {
  status: TaskStatus;
  withLabel?: boolean;
}) {
  const s = statusStyle[status];
  return (
    <span className="inline-flex items-center gap-1.5">
      <span className={cn("h-2 w-2 rounded-full", s.dot)} />
      {withLabel && (
        <span className={cn("text-xs font-medium", s.text)}>{STATUS_TEXT[status]}</span>
      )}
    </span>
  );
}

const priorityMeta: Record<Priority, { label: string; className: string; Icon: typeof Minus }> = {
  low: { label: "Low", className: "text-muted-foreground", Icon: ArrowDown },
  medium: { label: "Medium", className: "text-primary", Icon: Minus },
  high: { label: "High", className: "text-warning", Icon: ArrowUp },
  critical: { label: "Critical", className: "text-destructive", Icon: AlertTriangle },
};

export function PriorityIndicator({
  priority,
  withLabel = false,
}: {
  priority: Priority;
  withLabel?: boolean;
}) {
  const { label, className, Icon } = priorityMeta[priority];
  return (
    <span className={cn("inline-flex items-center gap-1.5", className)}>
      <Icon className="h-3.5 w-3.5" />
      {withLabel && <span className="text-xs font-medium">{label}</span>}
    </span>
  );
}
