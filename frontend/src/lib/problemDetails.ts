import { AxiosError } from "axios";

/// An RFC 7807 problem-details response, as returned by the FlowBoard API on error. `errors` carries
/// per-field validation messages on a 422.
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

/// Extracts problem details from a failed request, or null if the error is not an API problem response.
export function toProblemDetails(error: unknown): ProblemDetails | null {
  if (
    error instanceof AxiosError &&
    error.response?.data &&
    typeof error.response.data === "object"
  ) {
    return error.response.data as ProblemDetails;
  }
  return null;
}

/// A human-readable summary for a failed request, falling back to a generic message.
export function errorMessage(
  error: unknown,
  fallback = "Something went wrong. Please try again.",
): string {
  const problem = toProblemDetails(error);
  return problem?.detail ?? problem?.title ?? fallback;
}
