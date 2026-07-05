import {
  Archive,
  ArchiveRestore,
  FolderKanban,
  Loader2,
  MoreHorizontal,
  Pencil,
  Plus,
  Search,
  Trash2,
} from "lucide-react";
import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { canManage } from "@/features/organisations/organisations";
import {
  type Project,
  useCreateProject,
  useDeleteProject,
  useProjects,
  useSetProjectArchived,
  useUpdateProject,
} from "@/features/projects/projects";
import { errorMessage } from "@/lib/problemDetails";

export function ProjectsPage() {
  const { currentOrg } = useCurrentOrg();
  const orgId = currentOrg.id;
  const manage = canManage(currentOrg.role);
  const { data: projects, isPending } = useProjects(orgId);
  const [query, setQuery] = useState("");
  const [creating, setCreating] = useState(false);
  const [editing, setEditing] = useState<Project | null>(null);
  const [deleting, setDeleting] = useState<Project | null>(null);

  const q = query.trim().toLowerCase();
  const filtered = (projects ?? []).filter(
    (p) =>
      !q || p.name.toLowerCase().includes(q) || (p.description ?? "").toLowerCase().includes(q),
  );
  const active = filtered.filter((p) => p.status === "active");
  const archived = filtered.filter((p) => p.status === "archived");

  return (
    <div className="mx-auto w-full max-w-7xl px-6 py-6">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-bold tracking-tight">Projects</h1>
        <div className="flex items-center gap-2">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search projects"
              className="h-9 w-56 pl-8"
            />
          </div>
          {manage && (
            <Button onClick={() => setCreating(true)}>
              <Plus className="h-4 w-4" /> New project
            </Button>
          )}
        </div>
      </div>

      {isPending ? (
        <div className="flex justify-center py-16">
          <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
        </div>
      ) : active.length === 0 && archived.length === 0 ? (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-dashed border-border p-12 text-center">
          <div className="rounded-full bg-secondary p-3 text-muted-foreground">
            <FolderKanban className="h-6 w-6" />
          </div>
          <p className="font-medium">{q ? "No matching projects" : "No projects yet"}</p>
          {manage && !q && (
            <Button variant="outline" size="sm" onClick={() => setCreating(true)}>
              <Plus className="h-4 w-4" /> Create the first project
            </Button>
          )}
        </div>
      ) : (
        <>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {active.map((p) => (
              <ProjectCard
                key={p.id}
                project={p}
                manage={manage}
                onEdit={setEditing}
                onDelete={setDeleting}
                orgId={orgId}
              />
            ))}
          </div>
          {archived.length > 0 && (
            <>
              <h2 className="mb-3 mt-8 text-sm font-semibold uppercase tracking-wider text-muted-foreground">
                Archived
              </h2>
              <div className="grid gap-4 opacity-70 sm:grid-cols-2 lg:grid-cols-3">
                {archived.map((p) => (
                  <ProjectCard
                    key={p.id}
                    project={p}
                    manage={manage}
                    onEdit={setEditing}
                    onDelete={setDeleting}
                    orgId={orgId}
                  />
                ))}
              </div>
            </>
          )}
        </>
      )}

      {creating && <ProjectFormDialog orgId={orgId} onClose={() => setCreating(false)} />}
      {editing && (
        <ProjectFormDialog orgId={orgId} project={editing} onClose={() => setEditing(null)} />
      )}
      {deleting && (
        <DeleteProjectDialog orgId={orgId} project={deleting} onClose={() => setDeleting(null)} />
      )}
    </div>
  );
}

function ProjectCard({
  project,
  manage,
  orgId,
  onEdit,
  onDelete,
}: {
  project: Project;
  manage: boolean;
  orgId: string;
  onEdit: (p: Project) => void;
  onDelete: (p: Project) => void;
}) {
  const setArchived = useSetProjectArchived(orgId);
  return (
    <div className="flex flex-col rounded-xl border border-border bg-card p-5">
      <div className="flex items-start justify-between">
        <span className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary/15 text-lg font-bold text-primary">
          {project.name[0]}
        </span>
        <div className="flex items-center gap-2">
          {project.status === "archived" && (
            <Badge variant="outline" className="gap-1">
              <Archive className="h-3 w-3" /> Archived
            </Badge>
          )}
          {manage && (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-8 w-8"
                  aria-label="Project actions"
                >
                  <MoreHorizontal className="h-4 w-4" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onSelect={() => onEdit(project)}>
                  <Pencil /> Rename
                </DropdownMenuItem>
                {project.status === "active" ? (
                  <DropdownMenuItem
                    onSelect={() => setArchived.mutate({ projectId: project.id, archived: true })}
                  >
                    <Archive /> Archive
                  </DropdownMenuItem>
                ) : (
                  <DropdownMenuItem
                    onSelect={() => setArchived.mutate({ projectId: project.id, archived: false })}
                  >
                    <ArchiveRestore /> Restore
                  </DropdownMenuItem>
                )}
                <DropdownMenuItem destructive onSelect={() => onDelete(project)}>
                  <Trash2 /> Delete
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </div>
      </div>
      <h3 className="mt-3 font-semibold">{project.name}</h3>
      <p className="mt-1 line-clamp-2 flex-1 text-sm text-muted-foreground">
        {project.description || "No description."}
      </p>
    </div>
  );
}

function ProjectFormDialog({
  orgId,
  project,
  onClose,
}: {
  orgId: string;
  project?: Project;
  onClose: () => void;
}) {
  const create = useCreateProject(orgId);
  const update = useUpdateProject(orgId, project?.id ?? "");
  const [name, setName] = useState(project?.name ?? "");
  const [description, setDescription] = useState(project?.description ?? "");
  const [error, setError] = useState<string | null>(null);
  const pending = create.isPending || update.isPending;

  const submit = async () => {
    setError(null);
    try {
      if (project) await update.mutateAsync({ name, description });
      else await create.mutateAsync({ name, description });
      onClose();
    } catch (e) {
      setError(errorMessage(e));
    }
  };

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent aria-describedby={undefined}>
        <DialogTitle>{project ? "Rename project" : "New project"}</DialogTitle>
        <div className="mt-4 space-y-4">
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="space-y-1.5">
            <Label htmlFor="p-name">Name</Label>
            <Input id="p-name" value={name} onChange={(e) => setName(e.target.value)} autoFocus />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="p-desc">Description</Label>
            <Textarea
              id="p-desc"
              value={description ?? ""}
              onChange={(e) => setDescription(e.target.value)}
            />
          </div>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={onClose}>
              Cancel
            </Button>
            <Button onClick={submit} disabled={pending || !name.trim()}>
              {pending && <Loader2 className="h-4 w-4 animate-spin" />}{" "}
              {project ? "Save" : "Create"}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

function DeleteProjectDialog({
  orgId,
  project,
  onClose,
}: {
  orgId: string;
  project: Project;
  onClose: () => void;
}) {
  const del = useDeleteProject(orgId);
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent aria-describedby={undefined}>
        <DialogTitle>Delete project</DialogTitle>
        <DialogDescription>
          Delete “{project.name}”? This cannot be undone. Its tasks will be removed.
        </DialogDescription>
        <div className="mt-5 flex justify-end gap-2">
          <Button variant="ghost" onClick={onClose}>
            Cancel
          </Button>
          <Button
            variant="destructive"
            disabled={del.isPending}
            onClick={async () => {
              await del.mutateAsync(project.id).catch(() => undefined);
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
