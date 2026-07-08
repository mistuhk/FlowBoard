import { Download, Loader2, Paperclip, Trash2, UploadCloud } from "lucide-react";
import { useRef, useState } from "react";
import { Button } from "@/components/ui/button";
import { Progress } from "@/components/ui/misc";
import {
  type Attachment,
  fetchDownloadUrl,
  formatBytes,
  useAttachments,
  useDeleteAttachment,
  useUploadAttachment,
  validateFile,
} from "@/features/attachments/attachments";
import { cn } from "@/lib/utils";
import { errorMessage } from "@/lib/problemDetails";

export function AttachmentPanel({
  orgId,
  projectId,
  taskId,
}: {
  orgId: string;
  projectId: string;
  taskId: string;
}) {
  const { data: attachments } = useAttachments(orgId, projectId, taskId);
  const upload = useUploadAttachment(orgId, projectId, taskId);
  const remove = useDeleteAttachment(orgId, projectId, taskId);
  const inputRef = useRef<HTMLInputElement>(null);
  const [dragging, setDragging] = useState(false);
  const [progress, setProgress] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleFiles = async (files: FileList | null) => {
    setError(null);
    const file = files?.[0];
    if (!file) return;
    const invalid = validateFile(file);
    if (invalid) {
      setError(invalid);
      return;
    }
    try {
      setProgress(0);
      await upload.mutateAsync({ file, onProgress: setProgress });
    } catch (e) {
      setError(errorMessage(e, "Upload failed. Please try again."));
    } finally {
      setProgress(null);
    }
  };

  const download = async (a: Attachment) => {
    const url = await fetchDownloadUrl(orgId, projectId, taskId, a.id).catch(() => null);
    if (url) window.open(url, "_blank", "noopener");
  };

  return (
    <div>
      <h4 className="mb-2 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
        Attachments {attachments ? `· ${attachments.length}` : ""}
      </h4>

      <div className="space-y-2">
        {attachments?.map((a) => (
          <div
            key={a.id}
            className="flex items-center gap-3 rounded-lg border border-border bg-elevated/40 p-2.5"
          >
            <div className="flex h-9 w-9 items-center justify-center rounded-md bg-secondary text-muted-foreground">
              <Paperclip className="h-4 w-4" />
            </div>
            <div className="min-w-0 flex-1">
              <p className="truncate text-sm font-medium">{a.fileName}</p>
              <p className="text-xs text-muted-foreground">{formatBytes(a.fileSizeBytes)}</p>
            </div>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Download"
              onClick={() => void download(a)}
            >
              <Download className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              aria-label="Remove"
              onClick={() => remove.mutate(a.id)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ))}
      </div>

      <div
        onDragOver={(e) => {
          e.preventDefault();
          setDragging(true);
        }}
        onDragLeave={() => setDragging(false)}
        onDrop={(e) => {
          e.preventDefault();
          setDragging(false);
          void handleFiles(e.dataTransfer.files);
        }}
        className={cn(
          "mt-2 flex flex-col items-center gap-1 rounded-lg border border-dashed border-border p-4 text-center transition-colors",
          dragging && "border-primary bg-primary/5",
        )}
      >
        {progress !== null ? (
          <div className="w-full space-y-2">
            <p className="flex items-center justify-center gap-2 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" /> Uploading… {progress}%
            </p>
            <Progress value={progress} />
          </div>
        ) : (
          <>
            <UploadCloud className="h-5 w-5 text-muted-foreground" />
            <p className="text-sm text-muted-foreground">
              Drag a file here, or{" "}
              <button
                className="font-medium text-primary hover:underline"
                onClick={() => inputRef.current?.click()}
              >
                browse
              </button>
            </p>
            <p className="text-xs text-muted-foreground">Up to 25 MB</p>
          </>
        )}
        <input
          ref={inputRef}
          type="file"
          className="hidden"
          onChange={(e) => {
            void handleFiles(e.target.files);
            e.target.value = "";
          }}
        />
      </div>
      {error && <p className="mt-1.5 text-sm text-destructive">{error}</p>}
    </div>
  );
}
