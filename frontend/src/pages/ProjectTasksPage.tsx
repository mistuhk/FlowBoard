import { format, parseISO } from "date-fns";
import { ArrowLeft, Check, ChevronDown, ListFilter, Loader2, Plus, X } from "lucide-react";
import { useState } from "react";
import { Link, useParams, useSearchParams } from "react-router-dom";
import { PriorityIndicator, StatusDot } from "@/components/task-bits";
import { UserAvatar } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogTitle } from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Spinner } from "@/components/ui/misc";
import { Textarea } from "@/components/ui/textarea";
import { TaskDrawer } from "@/features/tasks/TaskDrawer";
import { useMembers } from "@/features/organisations/organisations";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { useProject } from "@/features/projects/projects";
import {
  PRIORITY_ORDER,
  STATUS_LABEL,
  STATUS_ORDER,
  type Priority,
  type TaskFilters,
  type TaskStatus,
  isOverdue,
  useCreateTask,
  useTasks,
} from "@/features/tasks/tasks";
import { cn } from "@/lib/utils";
import { errorMessage } from "@/lib/problemDetails";

function FilterMenu({
  label,
  active,
  onClear,
  children,
}: {
  label: string;
  active: boolean;
  onClear: () => void;
  children: React.ReactNode;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant={active ? "secondary" : "outline"} size="sm">
          {label} <ChevronDown className="h-3.5 w-3.5" />
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent>
        {children}
        {active && (
          <DropdownMenuItem onSelect={onClear}>
            <X /> Clear
          </DropdownMenuItem>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function ProjectTasksPage() {
  const { currentOrg } = useCurrentOrg();
  const orgId = currentOrg.id;
  const { projectId = "" } = useParams();
  const [params, setParams] = useSearchParams();
  const { data: project } = useProject(orgId, projectId);
  const { data: members } = useMembers(orgId);

  const status = (params.get("status") as TaskStatus | null) ?? undefined;
  const priority = (params.get("priority") as Priority | null) ?? undefined;
  const assigneeId = params.get("assignee") ?? undefined;
  const filters: TaskFilters = { status, priority, assigneeId };

  const { data, isPending, fetchNextPage, hasNextPage, isFetchingNextPage } = useTasks(
    orgId,
    projectId,
    filters,
  );
  const tasks = data?.pages.flatMap((p) => p.items) ?? [];
  const [creating, setCreating] = useState(false);

  const selectedTaskId = params.get("task");
  const selectedTask = tasks.find((t) => t.id === selectedTaskId) ?? null;

  const setParam = (key: string, value?: string) => {
    const next = new URLSearchParams(params);
    if (value) next.set(key, value);
    else next.delete(key);
    setParams(next, { replace: true });
  };

  const memberName = (id: string) =>
    members?.find((m) => m.userId === id)?.displayName ?? "Someone";

  return (
    <div className="flex h-full flex-col">
      <div className="border-b border-border px-6 py-4">
        <Link
          to="/"
          className="mb-2 inline-flex items-center gap-1 text-xs text-muted-foreground hover:text-foreground"
        >
          <ArrowLeft className="h-3.5 w-3.5" /> Projects
        </Link>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-xl font-bold tracking-tight">{project?.name ?? "Project"}</h1>
          <Button size="sm" onClick={() => setCreating(true)}>
            <Plus className="h-4 w-4" /> New task
          </Button>
        </div>

        <div className="mt-4 flex flex-wrap items-center gap-2">
          <ListFilter className="h-4 w-4 text-muted-foreground" />
          <FilterMenu
            label={status ? STATUS_LABEL[status] : "Status"}
            active={Boolean(status)}
            onClear={() => setParam("status")}
          >
            {STATUS_ORDER.map((s) => (
              <DropdownMenuItem key={s} onSelect={() => setParam("status", s)}>
                <StatusDot status={s} />
                <span className="flex-1">{STATUS_LABEL[s]}</span>
                {status === s && <Check className="h-4 w-4 text-primary" />}
              </DropdownMenuItem>
            ))}
          </FilterMenu>
          <FilterMenu
            label={priority ? priority : "Priority"}
            active={Boolean(priority)}
            onClear={() => setParam("priority")}
          >
            {PRIORITY_ORDER.map((p) => (
              <DropdownMenuItem
                key={p}
                onSelect={() => setParam("priority", p)}
                className="capitalize"
              >
                <PriorityIndicator priority={p} withLabel />
                {priority === p && <Check className="ml-auto h-4 w-4 text-primary" />}
              </DropdownMenuItem>
            ))}
          </FilterMenu>
          <FilterMenu
            label={assigneeId ? memberName(assigneeId) : "Assignee"}
            active={Boolean(assigneeId)}
            onClear={() => setParam("assignee")}
          >
            {members?.map((m) => (
              <DropdownMenuItem key={m.userId} onSelect={() => setParam("assignee", m.userId)}>
                <UserAvatar name={m.displayName} seed={m.userId} size="xs" />
                <span className="flex-1">{m.displayName}</span>
                {assigneeId === m.userId && <Check className="h-4 w-4 text-primary" />}
              </DropdownMenuItem>
            ))}
          </FilterMenu>
        </div>
      </div>

      <div className="min-h-0 flex-1 overflow-y-auto">
        <div className="mx-auto max-w-4xl px-6 py-4">
          {isPending ? (
            <div className="flex justify-center py-16">
              <Spinner className="h-6 w-6" />
            </div>
          ) : tasks.length === 0 ? (
            <p className="py-16 text-center text-sm text-muted-foreground">
              No tasks match these filters.
            </p>
          ) : (
            <ul className="divide-y divide-border overflow-hidden rounded-lg border border-border">
              {tasks.map((task) => {
                const assignee = members?.find((m) => m.userId === task.assigneeId);
                const overdue = isOverdue(task.dueDate, task.status);
                return (
                  <li key={task.id}>
                    <button
                      onClick={() => setParam("task", task.id)}
                      className="flex w-full items-center gap-3 px-4 py-3 text-left transition-colors hover:bg-secondary/50"
                    >
                      <StatusDot status={task.status} />
                      <span className="min-w-0 flex-1 truncate text-sm font-medium">
                        {task.title}
                      </span>
                      <PriorityIndicator priority={task.priority} />
                      {task.dueDate && (
                        <span
                          className={cn(
                            "text-xs",
                            overdue ? "text-destructive" : "text-muted-foreground",
                          )}
                        >
                          {format(parseISO(task.dueDate), "d MMM")}
                        </span>
                      )}
                      {assignee ? (
                        <UserAvatar name={assignee.displayName} seed={assignee.userId} size="sm" />
                      ) : (
                        <span className="h-6 w-6 rounded-full border border-dashed border-border" />
                      )}
                    </button>
                  </li>
                );
              })}
            </ul>
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
      </div>

      {creating && (
        <CreateTaskDialog orgId={orgId} projectId={projectId} onClose={() => setCreating(false)} />
      )}
      {selectedTask && members && (
        <TaskDrawer
          task={selectedTask}
          orgId={orgId}
          projectId={projectId}
          members={members}
          onClose={() => setParam("task")}
        />
      )}
    </div>
  );
}

function CreateTaskDialog({
  orgId,
  projectId,
  onClose,
}: {
  orgId: string;
  projectId: string;
  onClose: () => void;
}) {
  const create = useCreateTask(orgId, projectId);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [priority, setPriority] = useState<Priority>("medium");
  const [error, setError] = useState<string | null>(null);

  const submit = async () => {
    setError(null);
    try {
      await create.mutateAsync({ title, description, priority });
      onClose();
    } catch (e) {
      setError(errorMessage(e));
    }
  };

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent aria-describedby={undefined}>
        <DialogTitle>New task</DialogTitle>
        <div className="mt-4 space-y-4">
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="space-y-1.5">
            <Label htmlFor="t-title">Title</Label>
            <Input
              id="t-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              autoFocus
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="t-desc">Description</Label>
            <Textarea
              id="t-desc"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className="space-y-1.5">
            <Label>Priority</Label>
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  type="button"
                  variant="outline"
                  className="w-full justify-between capitalize"
                >
                  <PriorityIndicator priority={priority} withLabel />
                  <ChevronDown className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent>
                {PRIORITY_ORDER.map((p) => (
                  <DropdownMenuItem key={p} onSelect={() => setPriority(p)}>
                    <PriorityIndicator priority={p} withLabel />
                    {priority === p && <Check className="ml-auto h-4 w-4 text-primary" />}
                  </DropdownMenuItem>
                ))}
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button onClick={submit} disabled={create.isPending || !title.trim()}>
              {create.isPending && <Loader2 className="h-4 w-4 animate-spin" />} Create
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
