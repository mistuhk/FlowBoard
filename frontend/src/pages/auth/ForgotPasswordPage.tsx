import { zodResolver } from "@hookform/resolvers/zod";
import { ArrowLeft, Loader2, MailCheck } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link } from "react-router-dom";
import { AuthLayout, Field } from "@/components/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { authApi } from "@/features/auth/authApi";
import { type ForgotPasswordInput, forgotPasswordSchema } from "@/lib/validation";

export function ForgotPasswordPage() {
  const [sent, setSent] = useState(false);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordInput>({ resolver: zodResolver(forgotPasswordSchema) });

  const onSubmit = async (values: ForgotPasswordInput) => {
    // The response is intentionally the same whether or not the email exists, to avoid disclosing
    // which addresses have accounts.
    await authApi.forgotPassword(values.email).catch(() => undefined);
    setSent(true);
  };

  return (
    <AuthLayout>
      <Link
        to="/login"
        className="mb-6 inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground"
      >
        <ArrowLeft className="h-4 w-4" /> Back to sign in
      </Link>
      {sent ? (
        <div className="space-y-4 text-center">
          <div className="mx-auto flex h-14 w-14 items-center justify-center rounded-full bg-accent text-accent-foreground">
            <MailCheck className="h-7 w-7" />
          </div>
          <h2 className="text-2xl font-bold tracking-tight">Reset link sent</h2>
          <p className="text-sm text-muted-foreground">
            If an account exists for that email, you will receive a reset link shortly.
          </p>
        </div>
      ) : (
        <>
          <div className="mb-8 space-y-1.5">
            <h2 className="text-2xl font-bold tracking-tight">Reset your password</h2>
            <p className="text-sm text-muted-foreground">
              Enter your email and we will send a reset link.
            </p>
          </div>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
            <Field label="Email" htmlFor="email" error={errors.email?.message}>
              <Input id="email" type="email" autoComplete="email" {...register("email")} />
            </Field>
            <Button type="submit" className="w-full" disabled={isSubmitting}>
              {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />} Send reset link
            </Button>
          </form>
        </>
      )}
    </AuthLayout>
  );
}
