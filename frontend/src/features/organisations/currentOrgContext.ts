import { createContext } from "react";
import type { OrganisationSummary } from "@/features/organisations/organisations";

export interface CurrentOrgContextValue {
  organisations: OrganisationSummary[];
  currentOrg: OrganisationSummary | null;
  setCurrentOrgId: (id: string) => void;
}

export const CurrentOrgContext = createContext<CurrentOrgContextValue | null>(null);
