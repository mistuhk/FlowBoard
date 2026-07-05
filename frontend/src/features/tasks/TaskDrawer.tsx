import * as DialogPrimitive from "@radix-ui/react-dialog";
import { format, parseISO } from "date-fns";
import { CalendarClock, Check, Trash2, X } from "lucide-react";
import { useEffect, useState } from "react";
import { PriorityIndicator, StatusDot } from "@/components/task-bits";
import { UserAvatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, SheetContent } from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import type { Member } from "@/features/organisations/organisations";
import {
  PRIORITY_ORDER,
  STATUS_LABEL,
  type Priority,
  type Task,
  type TaskStatus,
  allowedTransitions,
  isOverdue,
  useAssignTask,
  useChangePriority,
  useChangeStatus,
  useDeleteTask,
  useUpdateTask,
} from "@/features/tasks/tasks";
import { cn } from "@/lib/utils";

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="grid grid-cols-[100px_1fr] items-center gap-2">
      <span className="text-xs font-medium text-muted-foreground">{label}</span>
      <div>{children}</div>
    </div>
  );
}

export function TaskDrawer({
  task,
  orgId,
  projectId,
  members,
  onClose,
}: {
  task: Task;
  orgId: string;
  projectId: string;
  members: Member[];
  onClose: () => void;
}) {
  const changeStatus = useChangeStatus(orgId, projectId);
  const changePriority = useChangePriority(orgId, projectId);
  const assign = useAssignTask(orgId, projectId);
  const update = useUpdateTask(orgId, projectId);
  const del = useDeleteTask(orgId, projectId);

  const [title, setTitle] = useState(task.title);
  const [description, setDescription] = useState(task.description ?? "");
  useEffect(() => {
    setTitle(task.title);
    setDescription(task.description ?? "");
  }, [task.id, task.title, task.description]);

  const assignee = members.find((m) => m.userId === task.assigneeId) ?? null;
  const overdue = isOverdue(task.dueDate, task.status);
  const transitions = allowedTransitions(task.status);
  const dueValue = task.dueDate ? format(parseISO(task.dueDate), "yyyy-MM-dd") : "";

  const saveDetails = () => {
    if (title.trim() && (title !== task.title || description !== (task.description ?? ""))) {
      update.mutate({ taskId: task.id, title: title.trim(), description, dueDate: task.dueDate });
    }
  };

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <SheetContent aria-describedby={undefined}>
        <div className="flex items-center justify-between border-b border-border px-6 py-3">
          <span className="font-mono text-xs text-muted-foreground">
            {task.id.slice(0, 8).toUpperCase()}
          </span>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="icon"
              aria-label="Delete task"
              onClick={async () => {
                await del.mutateAsync(task.id).catch(() => undefined);
                onClose();
              }}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
            <DialogPrimitive.Close asChild>
              <Button variant="ghost" size="icon" aria-label="Close">
                <X className="h-4 w-4" />
              </Button>
            </DialogPrimitive.Close>
          </div>
        </div>

        <div className="flex-1 overflow-y-auto px-6 py-5">
          <Input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            onBlur={saveDetails}
            className="mb-5 h-auto border-0 bg-transparent px-0 text-xl font-semibold shadow-none focus-visible:ring-0"
          />

          <div className="space-y-3 rounded-lg border border-border bg-elevated/40 p-4">
            <Row label="Status">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <button className="inline-flex items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-secondary">
                    <StatusDot status={task.status} withLabel />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  <DropdownMenuLabel>Move to</DropdownMenuLabel>
                  {transitions.length === 0 && (
                    <DropdownMenuItem disabled>No transitions</DropdownMenuItem>
                  )}
                  {transitions.map((s) => (
                    <DropdownMenuItem
                      key={s}
                      onSelect={() =>
                        changeStatus.mutate({ taskId: task.id, status: s as TaskStatus })
                      }
                    >
                      <StatusDot status={s} />
                      <span className="flex-1">{STATUS_LABEL[s]}</span>
                    </DropdownMenuItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </Row>

            <Row label="Priority">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <button className="inline-flex items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-secondary">
                    <PriorityIndicator priority={task.priority} withLabel />
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  {PRIORITY_ORDER.map((p) => (
                    <DropdownMenuItem
                      key={p}
                      onSelect={() =>
                        changePriority.mutate({ taskId: task.id, priority: p as Priority })
                      }
                    >
                      <PriorityIndicator priority={p} withLabel />
                      {task.priority === p && <Check className="ml-auto h-4 w-4 text-primary" />}
                    </DropdownMenuItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </Row>

            <Row label="Assignee">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <button className="inline-flex items-center gap-2 rounded-md px-2 py-1 text-sm hover:bg-secondary">
                    {assignee ? (
                      <>
                        <UserAvatar name={assignee.displayName} seed={assignee.userId} size="xs" />{" "}
                        {assignee.displayName}
                      </>
                    ) : (
                      <span className="text-muted-foreground">Unassigned</span>
                    )}
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent className="max-h-72 overflow-y-auto">
                  <DropdownMenuItem
                    onSelect={() => assign.mutate({ taskId: task.id, assigneeId: null })}
                  >
                    <span className="text-muted-foreground">Unassigned</span>
                  </DropdownMenuItem>
                  {members.map((m) => (
                    <DropdownMenuItem
                      key={m.userId}
                      onSelect={() => assign.mutate({ taskId: task.id, assigneeId: m.userId })}
                    >
                      <UserAvatar name={m.displayName} seed={m.userId} size="xs" />
                      <span className="flex-1">{m.displayName}</span>
                      {task.assigneeId === m.userId && <Check className="h-4 w-4 text-primary" />}
                    </DropdownMenuItem>
                  ))}
                </DropdownMenuContent>
              </DropdownMenu>
            </Row>

            <Row label="Due date">
              <div className="flex items-center gap-2">
                <input
                  type="date"
                  value={dueValue}
                  onChange={(e) =>
                    update.mutate({
                      taskId: task.id,
                      title: task.title,
                      description: task.description,
                      dueDate: e.target.value ? new Date(e.target.value).toISOString() : null,
                    })
                  }
                  className={cn(
                    "rounded-md border border-input bg-background px-2 py-1 text-sm",
                    overdue && "border-destructive text-destructive",
                  )}
                />
                {overdue && (
                  <Badge variant="destructive" className="gap-1">
                    <CalendarClock className="h-3 w-3" /> Overdue
                  </Badge>
                )}
              </div>
            </Row>
          </div>

          <div className="mt-6">
            <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
              Description
            </h4>
            <Textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              onBlur={saveDetails}
              placeholder="Add a description."
              className="min-h-[120px] bg-background"
            />
          </div>
        </div>
      </SheetContent>
    </Dialog>
  );
}
