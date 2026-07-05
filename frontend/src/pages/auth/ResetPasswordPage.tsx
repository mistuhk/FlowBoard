import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { AuthLayout, Field, FormBanner } from "@/components/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { authApi } from "@/features/auth/authApi";
import { applyProblemToForm } from "@/lib/formErrors";
import { type ResetPasswordInput, resetPasswordSchema } from "@/lib/validation";

export function ResetPasswordPage() {
  const [params] = useSearchParams();
  const token = params.get("token");
  const navigate = useNavigate();
  const [banner, setBanner] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<ResetPasswordInput>({ resolver: zodResolver(resetPasswordSchema) });

  if (!token) {
    return (
      <AuthLayout>
        <div className="space-y-4 text-center">
          <h2 className="text-2xl font-bold tracking-tight">Invalid reset link</h2>
          <p className="text-sm text-muted-foreground">
            This link is missing its token or has expired.
          </p>
          <Button asChild variant="outline" className="w-full">
            <Link to="/forgot-password">Request a new link</Link>
          </Button>
        </div>
      </AuthLayout>
    );
  }

  const onSubmit = async (values: ResetPasswordInput) => {
    setBanner(null);
    try {
      await authApi.resetPassword(token, values.newPassword);
      navigate("/login", { replace: true });
    } catch (error) {
      setBanner(applyProblemToForm(error, setError));
    }
  };

  return (
    <AuthLayout>
      <div className="mb-8 space-y-1.5">
        <h2 className="text-2xl font-bold tracking-tight">Choose a new password</h2>
        <p className="text-sm text-muted-foreground">
          Your new password must differ from previous ones.
        </p>
      </div>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
        <FormBanner message={banner} />
        <Field label="New password" htmlFor="newPassword" error={errors.newPassword?.message}>
          <Input
            id="newPassword"
            type="password"
            autoComplete="new-password"
            {...register("newPassword")}
          />
        </Field>
        <Field
          label="Confirm password"
          htmlFor="confirmPassword"
          error={errors.confirmPassword?.message}
        >
          <Input
            id="confirmPassword"
            type="password"
            autoComplete="new-password"
            {...register("confirmPassword")}
          />
        </Field>
        <Button type="submit" className="w-full" disabled={isSubmitting}>
          {isSubmitting && <Loader2 className="h-4 w-4 animate-spin" />} Update password
        </Button>
      </form>
    </AuthLayout>
  );
}
