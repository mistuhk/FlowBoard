import { CheckCircle2, FolderKanban } from "lucide-react";
import type { ReactNode } from "react";

/// Split-screen shell for the authentication pages: a brand panel on the left, the form on the right.
export function AuthLayout({ children }: { children: ReactNode }) {
  return (
    <div className="grid min-h-screen bg-background lg:grid-cols-2">
      <div className="relative hidden overflow-hidden bg-surface lg:block">
        <div className="absolute inset-0 bg-[radial-gradient(60%_60%_at_30%_20%,hsl(var(--primary)/0.25),transparent)]" />
        <div className="absolute inset-0 bg-[radial-gradient(50%_50%_at_80%_80%,hsl(var(--primary)/0.15),transparent)]" />
        <div className="relative flex h-full flex-col justify-between p-12">
          <div className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
              <FolderKanban className="h-5 w-5" />
            </div>
            <span className="text-xl font-bold tracking-tight">FlowBoard</span>
          </div>
          <div className="max-w-md space-y-4">
            <h1 className="text-3xl font-bold leading-tight">
              Where teams plan, track, and ship their best work.
            </h1>
            <p className="text-muted-foreground">
              Multi-tenant project management with boards, real-time collaboration, and a dashboard
              that keeps everyone aligned.
            </p>
            <div className="flex flex-col gap-2 pt-2 text-sm text-muted-foreground">
              {[
                "Kanban boards with priorities and assignees",
                "Comments, @mentions, and activity feeds",
                "Full-text search across everything",
              ].map((line) => (
                <span key={line} className="flex items-center gap-2">
                  <CheckCircle2 className="h-4 w-4 text-primary" /> {line}
                </span>
              ))}
            </div>
          </div>
          <p className="text-xs text-muted-foreground">© 2026 FlowBoard.</p>
        </div>
      </div>

      <div className="flex items-center justify-center p-6">
        <div className="w-full max-w-sm animate-slide-up">{children}</div>
      </div>
    </div>
  );
}

/// A field wrapper that shows a validation message beneath the control.
export function Field({
  label,
  htmlFor,
  error,
  children,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  children: ReactNode;
}) {
  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between">
        <label htmlFor={htmlFor} className="text-sm font-medium">
          {label}
        </label>
      </div>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}

/// A form-level error banner for non-field messages (for example invalid credentials).
export function FormBanner({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <div className="rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
      {message}
    </div>
  );
}
