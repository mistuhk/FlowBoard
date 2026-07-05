import { formatDistanceToNow } from "date-fns";
import { Loader2, Pencil, Send, Trash2, X } from "lucide-react";
import { useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { UserAvatar } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import {
  type Comment,
  useAddComment,
  useComments,
  useDeleteComment,
  useEditComment,
} from "@/features/comments/comments";
import type { Member } from "@/features/organisations/organisations";
import { useAuth } from "@/features/auth/useAuth";

// Renders comment text as GitHub-flavoured markdown, with @mentions given a subtle highlight. Markdown
// parsing and sanitisation are handled by react-markdown; the mention pass only styles bare @handles.
function CommentBody({ content }: { content: string }) {
  return (
    <div className="prose-comment text-sm leading-relaxed text-foreground/90 [&_a]:text-primary [&_a]:underline [&_code]:rounded [&_code]:bg-secondary [&_code]:px-1 [&_p]:my-1">
      <ReactMarkdown
        remarkPlugins={[remarkGfm]}
        components={{
          p: ({ children }) => <p>{highlightMentions(children)}</p>,
          li: ({ children }) => <li>{highlightMentions(children)}</li>,
        }}
      >
        {content}
      </ReactMarkdown>
    </div>
  );
}

// Wraps @handle tokens found in plain-text children with a highlighted span.
function highlightMentions(children: React.ReactNode): React.ReactNode {
  return (Array.isArray(children) ? children : [children]).map((child, i) => {
    if (typeof child !== "string") return <span key={i}>{child}</span>;
    const parts = child.split(/(@[a-z0-9_.-]+)/gi);
    return parts.map((part, j) =>
      /^@[a-z0-9_.-]+$/i.test(part) ? (
        <span
          key={`${i}-${j}`}
          className="rounded bg-accent px-1 font-medium text-accent-foreground"
        >
          {part}
        </span>
      ) : (
        <span key={`${i}-${j}`}>{part}</span>
      ),
    );
  });
}

export function CommentThread({
  orgId,
  projectId,
  taskId,
  members,
}: {
  orgId: string;
  projectId: string;
  taskId: string;
  members: Member[];
}) {
  const { user } = useAuth();
  const { data: comments, isPending } = useComments(orgId, projectId, taskId);
  const add = useAddComment(orgId, projectId, taskId);
  const [draft, setDraft] = useState("");
  const [deleting, setDeleting] = useState<Comment | null>(null);

  const authorOf = (id: string) => members.find((m) => m.userId === id);

  return (
    <div>
      <h4 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground">
        Comments {comments ? `· ${comments.length}` : ""}
      </h4>

      {isPending ? (
        <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
      ) : (
        <div className="space-y-4">
          {comments?.map((c) => (
            <CommentRow
              key={c.id}
              comment={c}
              author={authorOf(c.authorId)}
              isOwn={c.authorId === user?.id}
              orgId={orgId}
              projectId={projectId}
              taskId={taskId}
              onDelete={() => setDeleting(c)}
            />
          ))}
          {comments?.length === 0 && (
            <p className="text-sm text-muted-foreground">No comments yet.</p>
          )}
        </div>
      )}

      <div className="mt-4 flex items-end gap-2">
        <Textarea
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          placeholder="Write a comment. Use @ to mention a teammate, and Markdown for formatting."
          className="min-h-[44px] bg-background"
        />
        <Button
          size="icon"
          aria-label="Send comment"
          disabled={!draft.trim() || add.isPending}
          onClick={async () => {
            await add.mutateAsync(draft.trim()).catch(() => undefined);
            setDraft("");
          }}
        >
          {add.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Send className="h-4 w-4" />
          )}
        </Button>
      </div>

      {deleting && (
        <DeleteCommentDialog
          orgId={orgId}
          projectId={projectId}
          taskId={taskId}
          comment={deleting}
          onClose={() => setDeleting(null)}
        />
      )}
    </div>
  );
}

function CommentRow({
  comment,
  author,
  isOwn,
  orgId,
  projectId,
  taskId,
  onDelete,
}: {
  comment: Comment;
  author?: Member;
  isOwn: boolean;
  orgId: string;
  projectId: string;
  taskId: string;
  onDelete: () => void;
}) {
  const edit = useEditComment(orgId, projectId, taskId);
  const [editing, setEditing] = useState(false);
  const [value, setValue] = useState(comment.content);
  const edited = comment.updatedAt !== comment.createdAt;

  return (
    <div className="flex gap-3">
      <UserAvatar name={author?.displayName ?? "?"} seed={comment.authorId} size="sm" />
      <div className="min-w-0 flex-1">
        <div className="flex items-baseline gap-2">
          <span className="text-sm font-medium">{author?.displayName ?? "Unknown"}</span>
          <span className="text-xs text-muted-foreground">
            {formatDistanceToNow(new Date(comment.createdAt), { addSuffix: true })}
            {edited && " · edited"}
          </span>
          {isOwn && !editing && (
            <span className="ml-auto flex items-center gap-1">
              <button
                className="text-muted-foreground hover:text-foreground"
                aria-label="Edit comment"
                onClick={() => setEditing(true)}
              >
                <Pencil className="h-3.5 w-3.5" />
              </button>
              <button
                className="text-muted-foreground hover:text-destructive"
                aria-label="Delete comment"
                onClick={onDelete}
              >
                <Trash2 className="h-3.5 w-3.5" />
              </button>
            </span>
          )}
        </div>
        {editing ? (
          <div className="mt-1 space-y-2">
            <Textarea
              value={value}
              onChange={(e) => setValue(e.target.value)}
              className="min-h-[60px] bg-background"
            />
            <div className="flex gap-2">
              <Button
                size="sm"
                disabled={edit.isPending || !value.trim()}
                onClick={async () => {
                  await edit
                    .mutateAsync({ commentId: comment.id, content: value.trim() })
                    .catch(() => undefined);
                  setEditing(false);
                }}
              >
                Save
              </Button>
              <Button
                size="sm"
                variant="ghost"
                onClick={() => {
                  setValue(comment.content);
                  setEditing(false);
                }}
              >
                <X className="h-4 w-4" /> Cancel
              </Button>
            </div>
          </div>
        ) : (
          <CommentBody content={comment.content} />
        )}
      </div>
    </div>
  );
}

function DeleteCommentDialog({
  orgId,
  projectId,
  taskId,
  comment,
  onClose,
}: {
  orgId: string;
  projectId: string;
  taskId: string;
  comment: Comment;
  onClose: () => void;
}) {
  const del = useDeleteComment(orgId, projectId, taskId);
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent aria-describedby={undefined}>
        <DialogTitle>Delete comment</DialogTitle>
        <DialogDescription>This comment will be permanently removed.</DialogDescription>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            disabled={del.isPending}
            onClick={async () => {
              await del.mutateAsync(comment.id).catch(() => undefined);
              onClose();
            }}
          >
            {del.isPending && <Loader2 className="h-4 w-4 animate-spin" />} Delete
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  );
}
