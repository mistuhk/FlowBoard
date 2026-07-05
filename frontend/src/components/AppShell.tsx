import {
  Bell,
  Check,
  ChevronsUpDown,
  FolderKanban,
  LogOut,
  Plus,
  Settings,
  Users as UsersIcon,
} from "lucide-react";
import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { ThemeToggle } from "@/components/theme";
import { UserAvatar } from "@/components/ui/avatar";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { useAuth } from "@/features/auth/useAuth";
import { NotificationBell } from "@/features/notifications/NotificationBell";
import { CurrentOrgProvider } from "@/features/organisations/CurrentOrgProvider";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { cn } from "@/lib/utils";

function OrgSwitcher() {
  const { organisations, currentOrg, setCurrentOrgId } = useCurrentOrg();
  const navigate = useNavigate();

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <button className="flex w-full items-center gap-2 rounded-lg border border-border bg-elevated/50 p-2 text-left transition-colors hover:bg-secondary">
          <span className="flex h-8 w-8 items-center justify-center rounded-md bg-primary text-sm font-bold text-primary-foreground">
            {currentOrg.name[0]}
          </span>
          <span className="min-w-0 flex-1">
            <span className="block truncate text-sm font-semibold">{currentOrg.name}</span>
            <span className="block truncate text-xs capitalize text-muted-foreground">
              {currentOrg.role}
            </span>
          </span>
          <ChevronsUpDown className="h-4 w-4 text-muted-foreground" />
        </button>
      </DropdownMenuTrigger>
      <DropdownMenuContent className="w-60">
        <DropdownMenuLabel>Organisations</DropdownMenuLabel>
        {organisations.map((o) => (
          <DropdownMenuItem key={o.id} onSelect={() => setCurrentOrgId(o.id)}>
            <span className="flex h-6 w-6 items-center justify-center rounded bg-secondary text-xs font-bold">
              {o.name[0]}
            </span>
            <span className="flex-1 truncate">{o.name}</span>
            {o.id === currentOrg.id && <Check className="h-4 w-4 text-primary" />}
          </DropdownMenuItem>
        ))}
        <DropdownMenuSeparator />
        <DropdownMenuItem onSelect={() => navigate("/organisations/new")}>
          <Plus /> New organisation
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

const linkClass = ({ isActive }: { isActive: boolean }) =>
  cn(
    "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
    isActive
      ? "bg-secondary text-foreground"
      : "text-muted-foreground hover:bg-secondary/60 hover:text-foreground",
  );

function Sidebar() {
  return (
    <aside className="flex h-full w-64 shrink-0 flex-col gap-4 border-r border-border bg-surface p-3">
      <div className="flex items-center gap-2 px-2 pt-1">
        <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <FolderKanban className="h-4 w-4" />
        </div>
        <span className="text-lg font-bold tracking-tight">FlowBoard</span>
      </div>

      <OrgSwitcher />

      <nav className="flex flex-col gap-0.5">
        <NavLink to="/" end className={linkClass}>
          <FolderKanban className="h-4 w-4" /> Projects
        </NavLink>
        <NavLink to="/notifications" className={linkClass}>
          <Bell className="h-4 w-4" /> Notifications
        </NavLink>
        <NavLink to="/members" className={linkClass}>
          <UsersIcon className="h-4 w-4" /> Members
        </NavLink>
        <NavLink to="/settings" className={linkClass}>
          <Settings className="h-4 w-4" /> Settings
        </NavLink>
      </nav>
    </aside>
  );
}

function Topbar() {
  const { user, logout } = useAuth();
  return (
    <header className="glass sticky top-0 z-30 flex h-14 items-center justify-end gap-1 border-b border-border px-6">
      <NotificationBell />
      <ThemeToggle />
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <button className="ml-1 rounded-full focus:outline-none focus-visible:ring-2 focus-visible:ring-ring">
            <UserAvatar name={user?.displayName ?? "?"} seed={user?.id} size="md" />
          </button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="w-56">
          <div className="flex items-center gap-2 p-2">
            <UserAvatar name={user?.displayName ?? "?"} seed={user?.id} size="md" />
            <div className="min-w-0">
              <p className="truncate text-sm font-medium">{user?.displayName}</p>
              <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
            </div>
          </div>
          <DropdownMenuSeparator />
          <DropdownMenuItem destructive onSelect={() => void logout()}>
            <LogOut /> Sign out
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  );
}

/// Authenticated, organisation-scoped layout.
export function AppShell() {
  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <Sidebar />
      <div className="flex min-w-0 flex-1 flex-col">
        <Topbar />
        <main className="min-h-0 flex-1 overflow-y-auto">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

/// The authenticated, organisation-scoped layout: loads the user's organisations then renders the shell.
export function OrgScopedLayout() {
  return (
    <CurrentOrgProvider>
      <AppShell />
    </CurrentOrgProvider>
  );
}
