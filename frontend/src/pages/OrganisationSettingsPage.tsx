import { Check, Loader2 } from "lucide-react";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogTitle } from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  isOwner,
  useDeleteOrganisation,
  useMembers,
  useRenameOrganisation,
  useTransferOwnership,
} from "@/features/organisations/organisations";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { errorMessage } from "@/lib/problemDetails";

export function OrganisationSettingsPage() {
  const { currentOrg, setCurrentOrgId, organisations } = useCurrentOrg();
  const owner = isOwner(currentOrg.role);

  if (!owner) {
    return (
      <div className="mx-auto w-full max-w-2xl px-6 py-6">
        <h1 className="text-2xl font-bold tracking-tight">Settings</h1>
        <p className="mt-4 text-sm text-muted-foreground">
          Only the organisation owner can change these settings.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto w-full max-w-2xl px-6 py-6">
      <h1 className="mb-6 text-2xl font-bold tracking-tight">Settings</h1>
      <div className="space-y-4">
        <RenameCard orgId={currentOrg.id} name={currentOrg.name} />
        <TransferCard orgId={currentOrg.id} />
        <DangerZone
          orgId={currentOrg.id}
          name={currentOrg.name}
          onDeleted={() => {
            const next = organisations.find((o) => o.id !== currentOrg.id);
            if (next) setCurrentOrgId(next.id);
          }}
        />
      </div>
    </div>
  );
}

function RenameCard({ orgId, name }: { orgId: string; name: string }) {
  const rename = useRenameOrganisation(orgId);
  const [value, setValue] = useState(name);
  const [saved, setSaved] = useState(false);
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Organisation name</CardTitle>
      </CardHeader>
      <CardContent className="flex items-end gap-3">
        <div className="flex-1 space-y-1.5">
          <Label htmlFor="org-name">Name</Label>
          <Input
            id="org-name"
            value={value}
            onChange={(e) => {
              setValue(e.target.value);
              setSaved(false);
            }}
          />
        </div>
        <Button
          disabled={rename.isPending || !value.trim() || value === name}
          onClick={async () => {
            await rename.mutateAsync(value).catch(() => undefined);
            setSaved(true);
          }}
        >
          {rename.isPending && <Loader2 className="h-4 w-4 animate-spin" />}{" "}
          {saved ? "Saved" : "Save"}
        </Button>
      </CardContent>
    </Card>
  );
}

function TransferCard({ orgId }: { orgId: string }) {
  const { data: members } = useMembers(orgId);
  const transfer = useTransferOwnership(orgId);
  const candidates = (members ?? []).filter((m) => m.role !== "owner");
  const [target, setTarget] = useState<{ userId: string; displayName: string } | null>(null);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-base">Transfer ownership</CardTitle>
        <CardDescription>
          Hand this organisation to another member. You become an Admin.
        </CardDescription>
      </CardHeader>
      <CardContent>
        {candidates.length === 0 ? (
          <p className="text-sm text-muted-foreground">Invite another member first.</p>
        ) : (
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline">Choose new owner</Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent>
              {candidates.map((m) => (
                <DropdownMenuItem
                  key={m.userId}
                  onSelect={() => setTarget({ userId: m.userId, displayName: m.displayName })}
                >
                  {m.displayName}
                </DropdownMenuItem>
              ))}
            </DropdownMenuContent>
          </DropdownMenu>
        )}
      </CardContent>

      {target && (
        <Dialog open onOpenChange={(o) => !o && setTarget(null)}>
          <DialogContent aria-describedby={undefined}>
            <DialogTitle>Transfer ownership</DialogTitle>
            <DialogDescription>
              Make {target.displayName} the owner? You will keep Admin access but lose owner rights.
            </DialogDescription>
            <div className="mt-5 flex justify-end gap-2">
              <Button variant="ghost" onClick={() => setTarget(null)}>
                Cancel
              </Button>
              <Button
                disabled={transfer.isPending}
                onClick={async () => {
                  await transfer.mutateAsync(target.userId).catch(() => undefined);
                  setTarget(null);
                }}
              >
                {transfer.isPending ? (
                  <Loader2 className="h-4 w-4 animate-spin" />
                ) : (
                  <Check className="h-4 w-4" />
                )}{" "}
                Transfer
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
}

function DangerZone({
  orgId,
  name,
  onDeleted,
}: {
  orgId: string;
  name: string;
  onDeleted: () => void;
}) {
  const del = useDeleteOrganisation();
  const navigate = useNavigate();
  const [open, setOpen] = useState(false);
  const [confirm, setConfirm] = useState("");
  const [error, setError] = useState<string | null>(null);

  return (
    <Card className="border-destructive/40">
      <CardHeader>
        <CardTitle className="text-base text-destructive">Danger zone</CardTitle>
        <CardDescription>
          Deleting an organisation removes its projects and tasks. This cannot be undone.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Button variant="destructive" onClick={() => setOpen(true)}>
          Delete organisation
        </Button>
      </CardContent>
      {open && (
        <Dialog open onOpenChange={(o) => !o && setOpen(false)}>
          <DialogContent aria-describedby={undefined}>
            <DialogTitle>Delete {name}</DialogTitle>
            <DialogDescription>Type the organisation name to confirm.</DialogDescription>
            <div className="mt-4 space-y-3">
              {error && <p className="text-sm text-destructive">{error}</p>}
              <Input
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                placeholder={name}
                autoFocus
              />
              <div className="flex justify-end gap-2">
                <Button variant="ghost" onClick={() => setOpen(false)}>
                  Cancel
                </Button>
                <Button
                  variant="destructive"
                  disabled={confirm !== name || del.isPending}
                  onClick={async () => {
                    setError(null);
                    try {
                      await del.mutateAsync(orgId);
                      onDeleted();
                      navigate("/");
                    } catch (e) {
                      setError(errorMessage(e));
                    }
                  }}
                >
                  {del.isPending && <Loader2 className="h-4 w-4 animate-spin" />} Delete
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>
      )}
    </Card>
  );
}
