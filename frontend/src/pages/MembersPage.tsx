import { Check, Loader2, Mail, MoreHorizontal, Shield, Trash2 } from "lucide-react";
import { useState } from "react";
import { UserAvatar } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Input } from "@/components/ui/input";
import { Spinner } from "@/components/ui/misc";
import {
  type Role,
  canManage,
  roleName,
  useChangeMemberRole,
  useInviteMember,
  useMembers,
  useRemoveMember,
} from "@/features/organisations/organisations";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { errorMessage } from "@/lib/problemDetails";

const ASSIGNABLE_ROLES: Role[] = ["admin", "member", "guest"];
const roleVariant: Record<Role, "primary" | "default" | "outline"> = {
  owner: "primary",
  admin: "primary",
  member: "default",
  guest: "outline",
};

export function MembersPage() {
  const { currentOrg } = useCurrentOrg();
  const orgId = currentOrg.id;
  const manage = canManage(currentOrg.role);
  const { data: members, isPending } = useMembers(orgId);
  const changeRole = useChangeMemberRole(orgId);
  const removeMember = useRemoveMember(orgId);

  return (
    <div className="mx-auto w-full max-w-4xl px-6 py-6">
      <div className="mb-6">
        <h1 className="text-2xl font-bold tracking-tight">Members</h1>
        <p className="mt-1 text-sm text-muted-foreground">
          People with access to {currentOrg.name}.
        </p>
      </div>

      {manage && <InviteForm orgId={orgId} />}

      <Card className="mt-4">
        {isPending ? (
          <div className="flex justify-center py-10">
            <Spinner />
          </div>
        ) : (
          <div className="divide-y divide-border">
            {members?.map((m) => (
              <div key={m.userId} className="flex items-center gap-3 px-5 py-3">
                <UserAvatar name={m.displayName} seed={m.userId} />
                <div className="min-w-0 flex-1">
                  <p className="text-sm font-medium">{m.displayName}</p>
                  <p className="truncate text-xs text-muted-foreground">{m.email}</p>
                </div>
                <Badge variant={roleVariant[m.role]} className="gap-1 capitalize">
                  <Shield className="h-3 w-3" /> {m.role}
                </Badge>
                {manage && m.role !== "owner" && (
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="ghost" size="icon" aria-label="Member actions">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuLabel>Change role</DropdownMenuLabel>
                      {ASSIGNABLE_ROLES.map((r) => (
                        <DropdownMenuItem
                          key={r}
                          onSelect={() => changeRole.mutate({ userId: m.userId, role: r })}
                        >
                          <span className="flex-1 capitalize">{r}</span>
                          {m.role === r && <Check className="h-4 w-4 text-primary" />}
                        </DropdownMenuItem>
                      ))}
                      <DropdownMenuSeparator />
                      <DropdownMenuItem destructive onSelect={() => removeMember.mutate(m.userId)}>
                        <Trash2 /> Remove
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                )}
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  );
}

function InviteForm({ orgId }: { orgId: string }) {
  const invite = useInviteMember(orgId);
  const [email, setEmail] = useState("");
  const [role, setRole] = useState<Role>("member");
  const [feedback, setFeedback] = useState<{ ok: boolean; message: string } | null>(null);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setFeedback(null);
    try {
      await invite.mutateAsync({ email, role });
      setFeedback({ ok: true, message: `Invitation sent to ${email}.` });
      setEmail("");
    } catch (err) {
      setFeedback({ ok: false, message: errorMessage(err) });
    }
  };

  return (
    <Card className="p-5">
      <form onSubmit={submit} className="flex flex-wrap items-end gap-3">
        <div className="flex-1 space-y-1.5">
          <label htmlFor="invite-email" className="text-sm font-medium">
            Invite a teammate
          </label>
          <Input
            id="invite-email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="teammate@company.com"
          />
        </div>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button type="button" variant="outline" className="capitalize">
              {role}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            {ASSIGNABLE_ROLES.map((r) => (
              <DropdownMenuItem key={r} onSelect={() => setRole(r)}>
                <span className="flex-1 capitalize">{r}</span>
                {role === r && <Check className="h-4 w-4 text-primary" />}
              </DropdownMenuItem>
            ))}
          </DropdownMenuContent>
        </DropdownMenu>
        <Button type="submit" disabled={invite.isPending || !email.trim()}>
          {invite.isPending ? (
            <Loader2 className="h-4 w-4 animate-spin" />
          ) : (
            <Mail className="h-4 w-4" />
          )}{" "}
          Send
        </Button>
      </form>
      {feedback && (
        <p className={`mt-2 text-sm ${feedback.ok ? "text-success" : "text-destructive"}`}>
          {feedback.message}
        </p>
      )}
      <p className="sr-only">
        Invite sends an email with a join link. Roles: {ASSIGNABLE_ROLES.map(roleName).join(", ")}.
      </p>
    </Card>
  );
}
