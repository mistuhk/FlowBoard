import { FolderKanban, LogOut } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useAuth } from "@/features/auth/useAuth";

/// Minimal authenticated landing. The full dashboard and feature screens arrive in later sprints;
/// this confirms the signed-in session end to end.
export function HomePage() {
  const { user, logout } = useAuth();

  return (
    <div className="flex min-h-screen flex-col">
      <header className="glass sticky top-0 flex h-14 items-center justify-between border-b border-border px-6">
        <div className="flex items-center gap-2">
          <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <FolderKanban className="h-4 w-4" />
          </div>
          <span className="text-lg font-bold tracking-tight">FlowBoard</span>
        </div>
        <Button variant="ghost" size="sm" onClick={() => void logout()}>
          <LogOut className="h-4 w-4" /> Sign out
        </Button>
      </header>

      <main className="mx-auto flex max-w-2xl flex-1 flex-col items-center justify-center gap-4 p-8 text-center">
        <h1 className="text-3xl font-bold tracking-tight">
          Welcome back{user ? `, ${user.displayName.split(" ")[0]}` : ""}
        </h1>
        <p className="text-muted-foreground">
          You are signed in as {user?.email}. The dashboard and boards land in the next sprints.
        </p>
      </main>
    </div>
  );
}
