import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import axios from "axios";
import { apiClient } from "@/lib/apiClient";

export interface Attachment {
  id: string;
  taskId: string;
  uploadedById: string;
  fileName: string;
  fileSizeBytes: number;
  mimeType: string;
  createdAt: string;
}

interface UploadUrlResponse {
  storageKey: string;
  uploadUrl: string;
  expiresAtUtc: string;
}

// Mirrors the backend AttachmentPolicy so oversized or disallowed files are rejected before any
// request is made.
export const MAX_SIZE_BYTES = 25 * 1024 * 1024;
const ALLOWED_MIME = new Set([
  "image/png",
  "image/jpeg",
  "image/gif",
  "image/webp",
  "application/pdf",
  "text/plain",
  "text/csv",
  "application/zip",
  "application/msword",
  "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  "application/vnd.ms-excel",
  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
]);

export function validateFile(file: File): string | null {
  if (file.size > MAX_SIZE_BYTES) return "File exceeds the 25 MB limit.";
  if (!ALLOWED_MIME.has(file.type)) return "That file type is not allowed.";
  return null;
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

const base = (orgId: string, projectId: string, taskId: string) =>
  `/organisations/${orgId}/projects/${projectId}/tasks/${taskId}/attachments`;
const key = (taskId: string) => ["attachments", taskId] as const;

export function useAttachments(orgId: string, projectId: string, taskId: string) {
  return useQuery({
    queryKey: key(taskId),
    queryFn: async () => (await apiClient.get<Attachment[]>(base(orgId, projectId, taskId))).data,
  });
}

/// The pre-signed upload flow: request a URL, PUT the bytes directly to storage (bytes never touch
/// the API), then confirm so the metadata is recorded. Progress is reported for the direct upload.
export function useUploadAttachment(orgId: string, projectId: string, taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({
      file,
      onProgress,
    }: {
      file: File;
      onProgress?: (percent: number) => void;
    }) => {
      const meta = { fileName: file.name, fileSizeBytes: file.size, mimeType: file.type };
      const { data: url } = await apiClient.post<UploadUrlResponse>(
        `${base(orgId, projectId, taskId)}/upload-url`,
        meta,
      );

      // Direct-to-storage PUT, outside the API client so no bearer or base URL is applied.
      await axios.put(url.uploadUrl, file, {
        headers: { "Content-Type": file.type },
        onUploadProgress: (e) => {
          if (onProgress && e.total) onProgress(Math.round((e.loaded / e.total) * 100));
        },
      });

      await apiClient.post(`${base(orgId, projectId, taskId)}/confirm`, {
        storageKey: url.storageKey,
        ...meta,
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key(taskId) }),
  });
}

export function useDeleteAttachment(orgId: string, projectId: string, taskId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (attachmentId: string) => {
      await apiClient.delete(`${base(orgId, projectId, taskId)}/${attachmentId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: key(taskId) }),
  });
}

export async function fetchDownloadUrl(
  orgId: string,
  projectId: string,
  taskId: string,
  attachmentId: string,
) {
  const { data } = await apiClient.get<{ url: string }>(
    `${base(orgId, projectId, taskId)}/${attachmentId}/download-url`,
  );
  return data.url;
}
