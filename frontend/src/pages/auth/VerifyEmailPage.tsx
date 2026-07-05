import { CheckCircle2, Loader2, MailCheck, XCircle } from "lucide-react";
import { useEffect, useRef, useState } from "react";
import { Link, useLocation, useSearchParams } from "react-router-dom";
import { AuthLayout } from "@/components/AuthLayout";
import { Button } from "@/components/ui/button";
import { authApi } from "@/features/auth/authApi";
import { errorMessage } from "@/lib/problemDetails";

type State = "await-token" | "verifying" | "success" | "error";

export function VerifyEmailPage() {
  const [params] = useSearchParams();
  const token = params.get("token");
  const location = useLocation();
  const email = (location.state as { email?: string } | null)?.email;
  const [state, setState] = useState<State>(token ? "verifying" : "await-token");
  const [message, setMessage] = useState("");
  const started = useRef(false);

  useEffect(() => {
    if (!token || started.current) return;
    started.current = true;
    void (async () => {
      try {
        await authApi.verifyEmail(token);
        setState("success");
      } catch (error) {
        setMessage(errorMessage(error, "This verification link is invalid or has expired."));
        setState("error");
      }
    })();
  }, [token]);

  return (
    <AuthLayout>
      <div className="space-y-6 text-center">
        {state === "await-token" && (
          <>
            <Icon>
              <MailCheck className="h-7 w-7" />
            </Icon>
            <Heading
              title="Check your inbox"
              subtitle={`We have sent a verification link${email ? ` to ${email}` : ""}. Click it to activate your account.`}
            />
            <Button asChild className="w-full">
              <Link to="/login">Continue to sign in</Link>
            </Button>
          </>
        )}
        {state === "verifying" && (
          <>
            <Icon>
              <Loader2 className="h-7 w-7 animate-spin" />
            </Icon>
            <Heading title="Verifying your email" subtitle="This will only take a moment." />
          </>
        )}
        {state === "success" && (
          <>
            <Icon tone="success">
              <CheckCircle2 className="h-7 w-7" />
            </Icon>
            <Heading
              title="Email verified"
              subtitle="Your account is now active. You can sign in."
            />
            <Button asChild className="w-full">
              <Link to="/login">Sign in</Link>
            </Button>
          </>
        )}
        {state === "error" && (
          <>
            <Icon tone="destructive">
              <XCircle className="h-7 w-7" />
            </Icon>
            <Heading title="Verification failed" subtitle={message} />
            <Button asChild variant="outline" className="w-full">
              <Link to="/login">Back to sign in</Link>
            </Button>
          </>
        )}
      </div>
    </AuthLayout>
  );
}

function Icon({
  children,
  tone = "accent",
}: {
  children: React.ReactNode;
  tone?: "accent" | "success" | "destructive";
}) {
  const tones = {
    accent: "bg-accent text-accent-foreground",
    success: "bg-success/15 text-success",
    destructive: "bg-destructive/15 text-destructive",
  };
  return (
    <div
      className={`mx-auto flex h-14 w-14 items-center justify-center rounded-full ${tones[tone]}`}
    >
      {children}
    </div>
  );
}

function Heading({ title, subtitle }: { title: string; subtitle: string }) {
  return (
    <div className="space-y-1.5">
      <h2 className="text-2xl font-bold tracking-tight">{title}</h2>
      <p className="text-sm text-muted-foreground">{subtitle}</p>
    </div>
  );
}
