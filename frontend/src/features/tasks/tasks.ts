import {
  type InfiniteData,
  useInfiniteQuery,
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { isPast, parseISO } from "date-fns";
import { apiClient } from "@/lib/apiClient";

export type TaskStatus = "todo" | "in_progress" | "blocked" | "done";
export type Priority = "low" | "medium" | "high" | "critical";

export interface Task {
  id: string;
  projectId: string;
  organisationId: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: Priority;
  assigneeId: string | null;
  createdById: string;
  dueDate: string | null;
  createdAt: string;
}

interface TaskPage {
  items: Task[];
  nextCursor: string | null;
}

export interface TaskFilters {
  status?: TaskStatus;
  priority?: Priority;
  assigneeId?: string;
}

// Read endpoints return snake_case tokens; write endpoints expect the PascalCase name.
const STATUS_NAME: Record<TaskStatus, string> = {
  todo: "Todo",
  in_progress: "InProgress",
  blocked: "Blocked",
  done: "Done",
};
const priorityName = (p: Priority): string => p.charAt(0).toUpperCase() + p.slice(1);

export const STATUS_ORDER: TaskStatus[] = ["todo", "in_progress", "blocked", "done"];
export const PRIORITY_ORDER: Priority[] = ["low", "medium", "high", "critical"];
export const STATUS_LABEL: Record<TaskStatus, string> = {
  todo: "To do",
  in_progress: "In progress",
  blocked: "Blocked",
  done: "Done",
};

// Mirrors the backend state machine: only these transitions are offered in the UI.
const TRANSITIONS: Record<TaskStatus, TaskStatus[]> = {
  todo: ["in_progress", "blocked"],
  in_progress: ["blocked", "done", "todo"],
  blocked: ["in_progress", "todo"],
  done: ["todo"],
};
export const allowedTransitions = (from: TaskStatus): TaskStatus[] => TRANSITIONS[from];

/// Whether a task's due date has passed and it is not done.
export const isOverdue = (dueDate: string | null, status: TaskStatus): boolean =>
  dueDate != null && status !== "done" && isPast(parseISO(dueDate));

const base = (orgId: string, projectId: string) =>
  `/organisations/${orgId}/projects/${projectId}/tasks`;
const listKey = (orgId: string, projectId: string, filters: TaskFilters) =>
  ["tasks", orgId, projectId, filters] as const;

export function useTasks(orgId: string, projectId: string, filters: TaskFilters) {
  return useInfiniteQuery({
    queryKey: listKey(orgId, projectId, filters),
    initialPageParam: undefined as string | undefined,
    queryFn: async ({ pageParam }) => {
      const params: Record<string, string | number> = { limit: 25 };
      if (filters.status) params.status = STATUS_NAME[filters.status];
      if (filters.priority) params.priority = priorityName(filters.priority);
      if (filters.assigneeId) params.assigneeId = filters.assigneeId;
      if (pageParam) params.cursor = pageParam;
      return (await apiClient.get<TaskPage>(base(orgId, projectId), { params })).data;
    },
    getNextPageParam: (last) => last.nextCursor ?? undefined,
  });
}

export function useCreateTask(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: { title: string; description?: string | null; priority: Priority }) =>
      (
        await apiClient.post<Task>(base(orgId, projectId), {
          ...input,
          priority: priorityName(input.priority),
        })
      ).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks", orgId, projectId] }),
  });
}

export function useUpdateTask(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({
      taskId,
      ...input
    }: {
      taskId: string;
      title: string;
      description?: string | null;
      dueDate?: string | null;
    }) => (await apiClient.put<Task>(`${base(orgId, projectId)}/${taskId}`, input)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks", orgId, projectId] }),
  });
}

export function useChangeStatus(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ taskId, status }: { taskId: string; status: TaskStatus }) => {
      await apiClient.put(`${base(orgId, projectId)}/${taskId}/status`, {
        status: STATUS_NAME[status],
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks", orgId, projectId] }),
  });
}

export function useChangePriority(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ taskId, priority }: { taskId: string; priority: Priority }) => {
      await apiClient.put(`${base(orgId, projectId)}/${taskId}/priority`, {
        priority: priorityName(priority),
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks", orgId, projectId] }),
  });
}

export function useDeleteTask(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (taskId: string) => {
      await apiClient.delete(`${base(orgId, projectId)}/${taskId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks", orgId, projectId] }),
  });
}

// Assignment is optimistic: every cached task page for this project is patched immediately, then
// rolled back if the request fails.
export function useAssignTask(orgId: string, projectId: string) {
  const qc = useQueryClient();
  const prefix = ["tasks", orgId, projectId] as const;

  return useMutation({
    mutationFn: async ({ taskId, assigneeId }: { taskId: string; assigneeId: string | null }) => {
      const url = `${base(orgId, projectId)}/${taskId}/assignee`;
      if (assigneeId) await apiClient.put(url, { assigneeId });
      else await apiClient.delete(url);
    },
    onMutate: async ({ taskId, assigneeId }) => {
      await qc.cancelQueries({ queryKey: prefix });
      const snapshots = qc.getQueriesData<InfiniteData<TaskPage>>({ queryKey: prefix });
      for (const [key, data] of snapshots) {
        if (!data) continue;
        qc.setQueryData<InfiniteData<TaskPage>>(key, {
          ...data,
          pages: data.pages.map((page) => ({
            ...page,
            items: page.items.map((t) => (t.id === taskId ? { ...t, assigneeId } : t)),
          })),
        });
      }
      return { snapshots };
    },
    onError: (_e, _v, context) => {
      context?.snapshots.forEach(([key, data]) => qc.setQueryData(key, data));
    },
    onSettled: () => qc.invalidateQueries({ queryKey: prefix }),
  });
}
