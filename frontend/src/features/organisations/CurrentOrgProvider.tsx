import { useEffect, useMemo, useState } from "react";
import { Navigate } from "react-router-dom";
import { Spinner } from "@/components/ui/misc";
import { CurrentOrgContext } from "@/features/organisations/currentOrgContext";
import { useOrganisations } from "@/features/organisations/organisations";

const STORAGE_KEY = "fb-current-org";

/// Loads the user's organisations and tracks the selected one (persisted). While loading it shows a
/// spinner; with no organisations it sends the user to create their first one.
export function CurrentOrgProvider({ children }: { children: React.ReactNode }) {
  const { data: organisations, isPending, isError } = useOrganisations();
  const [selectedId, setSelectedId] = useState<string | null>(() =>
    localStorage.getItem(STORAGE_KEY),
  );

  const currentOrg = useMemo(() => {
    if (!organisations || organisations.length === 0) return null;
    return organisations.find((o) => o.id === selectedId) ?? organisations[0];
  }, [organisations, selectedId]);

  useEffect(() => {
    if (currentOrg) localStorage.setItem(STORAGE_KEY, currentOrg.id);
  }, [currentOrg]);

  if (isPending) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Spinner className="h-6 w-6" />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="flex min-h-screen items-center justify-center text-sm text-muted-foreground">
        Could not load your organisations. Please try again.
      </div>
    );
  }

  if (organisations.length === 0) {
    return <Navigate to="/organisations/new" replace />;
  }

  return (
    <CurrentOrgContext.Provider
      value={{ organisations, currentOrg, setCurrentOrgId: setSelectedId }}
    >
      {children}
    </CurrentOrgContext.Provider>
  );
}
