import { z } from "zod";

// These schemas mirror the backend FluentValidation rules so the client rejects the same input the
// API would. The server remains the source of truth; its RFC 7807 field errors are still surfaced.

const email = z
  .string()
  .min(1, "Email address is required.")
  .email("A valid email address is required.")
  .max(254);

const password = z
  .string()
  .min(8, "Password must be at least 8 characters long.")
  .max(128, "Password must not exceed 128 characters.")
  .regex(/[A-Z]/, "Password must contain at least one uppercase letter.")
  .regex(/[a-z]/, "Password must contain at least one lowercase letter.")
  .regex(/[0-9]/, "Password must contain at least one digit.");

export const loginSchema = z.object({
  email,
  password: z.string().min(1, "Password is required."),
});

export const registerSchema = z.object({
  displayName: z
    .string()
    .min(1, "Display name is required.")
    .max(100, "Display name must not exceed 100 characters."),
  email,
  password,
});

export const forgotPasswordSchema = z.object({ email });

export const resetPasswordSchema = z
  .object({
    newPassword: password,
    confirmPassword: z.string().min(1, "Please confirm your password."),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match.",
    path: ["confirmPassword"],
  });

export type LoginInput = z.infer<typeof loginSchema>;
export type RegisterInput = z.infer<typeof registerSchema>;
export type ForgotPasswordInput = z.infer<typeof forgotPasswordSchema>;
export type ResetPasswordInput = z.infer<typeof resetPasswordSchema>;
