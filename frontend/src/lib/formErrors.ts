import type { Path, UseFormSetError } from "react-hook-form";
import { toProblemDetails } from "@/lib/problemDetails";

/// Maps an RFC 7807 validation response onto react-hook-form fields, so server-side field errors
/// appear inline next to the matching input. The API returns PascalCase field keys (for example
/// "Email"); form field names are camelCase, so the first character is lower-cased.
/// Returns the top-level detail/title for any error that is not field-specific.
export function applyProblemToForm<TFieldValues extends Record<string, unknown>>(
  error: unknown,
  setError: UseFormSetError<TFieldValues>,
): string | null {
  const problem = toProblemDetails(error);
  if (!problem) return "Something went wrong. Please try again.";

  let matchedField = false;
  if (problem.errors) {
    for (const [key, messages] of Object.entries(problem.errors)) {
      const field = (key.charAt(0).toLowerCase() + key.slice(1)) as Path<TFieldValues>;
      setError(field, { type: "server", message: messages.join(" ") });
      matchedField = true;
    }
  }

  // If the problem was purely field errors, no banner message is needed.
  return matchedField ? null : (problem.detail ?? problem.title ?? "Something went wrong.");
}
