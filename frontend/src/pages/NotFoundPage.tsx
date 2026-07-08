import { Link } from "react-router-dom";
import { Button } from "@/components/ui/button";

/// Fallback page for unmatched routes.
export function NotFoundPage() {
  return (
    <main className="mx-auto flex min-h-screen max-w-md flex-col items-center justify-center gap-4 p-8 text-center">
      <p className="text-6xl font-bold">404</p>
      <p className="text-muted-foreground">This page could not be found.</p>
      <Button asChild variant="outline">
        <Link to="/">Back to home</Link>
      </Button>
    </main>
  );
}
