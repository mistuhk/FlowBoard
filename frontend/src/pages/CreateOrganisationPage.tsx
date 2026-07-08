import { FolderKanban, Loader2 } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useCreateOrganisation } from "@/features/organisations/organisations";
import { errorMessage } from "@/lib/problemDetails";

/// Creating an organisation. Reached from the org switcher, or automatically when the signed-in user
/// belongs to none.
export function CreateOrganisationPage() {
  const create = useCreateOrganisation();
  const navigate = useNavigate();
  const [name, setName] = useState("");
  const [error, setError] = useState<string | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await create.mutateAsync({ name });
      navigate("/", { replace: true });
    } catch (err) {
      setError(errorMessage(err));
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-background p-6">
      <div className="w-full max-w-sm animate-slide-up">
        <div className="mb-6 flex items-center gap-2">
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <FolderKanban className="h-5 w-5" />
          </div>
          <span className="text-lg font-bold tracking-tight">FlowBoard</span>
        </div>
        <h1 className="text-2xl font-bold tracking-tight">Create an organisation</h1>
        <p className="mt-1.5 text-sm text-muted-foreground">
          Organisations hold your projects and team. You can create more later.
        </p>
        <form onSubmit={submit} className="mt-6 space-y-4" noValidate>
          {error && <p className="text-sm text-destructive">{error}</p>}
          <div className="space-y-1.5">
            <Label htmlFor="name">Organisation name</Label>
            <Input
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Acme Inc"
              autoFocus
            />
          </div>
          <Button type="submit" className="w-full" disabled={create.isPending || !name.trim()}>
            {create.isPending && <Loader2 className="h-4 w-4 animate-spin" />} Create organisation
          </Button>
        </form>
      </div>
    </div>
  );
}
