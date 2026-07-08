import { useContext } from "react";
import {
  CurrentOrgContext,
  type CurrentOrgContextValue,
} from "@/features/organisations/currentOrgContext";

/// Access the current-organisation context. Throws if used outside the provider. `currentOrg` is
/// non-null within the provider (the provider redirects when the user has no organisations).
export function useCurrentOrg(): CurrentOrgContextValue & {
  currentOrg: NonNullable<CurrentOrgContextValue["currentOrg"]>;
} {
  const context = useContext(CurrentOrgContext);
  if (!context || !context.currentOrg) {
    throw new Error(
      "useCurrentOrg must be used within CurrentOrgProvider with at least one organisation.",
    );
  }
  return context as CurrentOrgContextValue & {
    currentOrg: NonNullable<CurrentOrgContextValue["currentOrg"]>;
  };
}
