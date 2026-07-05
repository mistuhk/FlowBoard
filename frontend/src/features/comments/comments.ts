import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

export interface Comment {
  id: string;
  taskId: string;
  authorId: string;
  content: string;
  mentions: string[];
  createdAt: string;
  updatedAt: string;
}

const base = (orgId: string, projectId: string, taskId: string) =>
  `/organisations/${orgId}/projects/${projectId}/tasks/${taskId}/comments`;
const key = (taskId: string) => ["comments", taskId] as const;

export function useComments(orgId: string, projectId: string, taskId: string) {
  return useQuery({
    queryKey: key(taskId),
    queryFn: async () => (await apiClient.get<Comment[]>(base(orgId, projectId, taskId))).data,
  });
}

export function useAddComment(orgId: string, projectId: string, taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (content: string) =>
      (await apiClient.post<Comment>(base(orgId, projectId, taskId), { content })).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: key(taskId) }),
  });
}

export function useEditComment(orgId: string, projectId: string, taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ commentId, content }: { commentId: string; content: string }) =>
      (await apiClient.put<Comment>(`${base(orgId, projectId, taskId)}/${commentId}`, { content }))
        .data,
    onSuccess: () => qc.invalidateQueries({ queryKey: key(taskId) }),
  });
}

export function useDeleteComment(orgId: string, projectId: string, taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (commentId: string) => {
      await apiClient.delete(`${base(orgId, projectId, taskId)}/${commentId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key(taskId) }),
  });
}
