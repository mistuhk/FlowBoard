import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

export type ProjectStatus = "active" | "archived";

export interface Project {
  id: string;
  organisationId: string;
  name: string;
  description: string | null;
  status: ProjectStatus;
  createdById: string;
}

const keys = {
  list: (orgId: string) => ["projects", orgId] as const,
  one: (orgId: string, projectId: string) => ["project", orgId, projectId] as const,
};

const base = (orgId: string) => `/organisations/${orgId}/projects`;

export function useProjects(orgId: string | null) {
  return useQuery({
    queryKey: keys.list(orgId ?? ""),
    enabled: Boolean(orgId),
    queryFn: async () => (await apiClient.get<Project[]>(base(orgId!))).data,
  });
}

export function useProject(orgId: string, projectId: string) {
  return useQuery({
    queryKey: keys.one(orgId, projectId),
    queryFn: async () => (await apiClient.get<Project>(`${base(orgId)}/${projectId}`)).data,
  });
}

export function useCreateProject(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: { name: string; description?: string | null }) =>
      (await apiClient.post<Project>(base(orgId), input)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.list(orgId) }),
  });
}

export function useUpdateProject(orgId: string, projectId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: { name: string; description?: string | null }) =>
      (await apiClient.put<Project>(`${base(orgId)}/${projectId}`, input)).data,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: keys.list(orgId) });
      void qc.invalidateQueries({ queryKey: keys.one(orgId, projectId) });
    },
  });
}

export function useSetProjectArchived(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ projectId, archived }: { projectId: string; archived: boolean }) => {
      await apiClient.put(`${base(orgId)}/${projectId}/${archived ? "archive" : "restore"}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.list(orgId) }),
  });
}

export function useDeleteProject(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (projectId: string) => {
      await apiClient.delete(`${base(orgId)}/${projectId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.list(orgId) }),
  });
}
