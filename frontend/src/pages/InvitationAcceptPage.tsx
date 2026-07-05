import { CheckCircle2, Loader2, XCircle } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { useAcceptInvitation } from "@/features/organisations/organisations";
import { errorMessage } from "@/lib/problemDetails";

/// Accepts an invitation from an emailed link (`/invitations/accept?token=...`). The user must be
/// signed in (the route is protected); on success they land in the new organisation.
export function InvitationAcceptPage() {
  const [params] = useSearchParams();
  const token = params.get("token");
  const accept = useAcceptInvitation();
  const navigate = useNavigate();
  const [state, setState] = useState<"working" | "error">("working");
  const [message, setMessage] = useState("");
  const started = useRef(false);

  useEffect(() => {
    if (started.current) return;
    started.current = true;
    if (!token) {
      setMessage("This invitation link is missing its token.");
      setState("error");
      return;
    }
    void (async () => {
      try {
        const org = await accept.mutateAsync(token);
        localStorage.setItem("fb-current-org", org.id);
        navigate("/", { replace: true });
      } catch (e) {
        setMessage(errorMessage(e, "This invitation is invalid or has expired."));
        setState("error");
      }
    })();
  }, [token, accept, navigate]);

  return (
    <div className="flex min-h-screen items-center justify-center p-6 text-center">
      <div className="w-full max-w-sm space-y-4">
        {state === "working" ? (
          <>
            <Loader2 className="mx-auto h-8 w-8 animate-spin text-primary" />
            <p className="text-sm text-muted-foreground">Accepting your invitation…</p>
          </>
        ) : (
          <>
            <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-destructive/15 text-destructive">
              <XCircle className="h-7 w-7" />
            </div>
            <h1 className="text-xl font-bold tracking-tight">Invitation problem</h1>
            <p className="text-sm text-muted-foreground">{message}</p>
            <Button asChild variant="outline" className="w-full">
              <Link to="/">Go to FlowBoard</Link>
            </Button>
          </>
        )}
        <CheckCircle2 className="sr-only" />
      </div>
    </div>
  );
}
