import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/apiClient";

// Roles are returned by the read endpoints in lower case (as stored); the write endpoints expect the
// capitalised name. Keep the read form as the type and convert when sending.
export type Role = "owner" | "admin" | "member" | "guest";

export interface OrganisationSummary {
  id: string;
  name: string;
  slug: string;
  ownerId: string;
  role: Role;
}

export interface Member {
  userId: string;
  email: string;
  displayName: string;
  role: Role;
  joinedAt: string;
}

export const roleName = (role: Role): string => role.charAt(0).toUpperCase() + role.slice(1);
export const canManage = (role: Role): boolean => role === "owner" || role === "admin";
export const isOwner = (role: Role): boolean => role === "owner";

const keys = {
  all: ["organisations"] as const,
  one: (id: string) => ["organisation", id] as const,
  members: (orgId: string) => ["members", orgId] as const,
};

export function useOrganisations() {
  return useQuery({
    queryKey: keys.all,
    queryFn: async () => (await apiClient.get<OrganisationSummary[]>("/organisations")).data,
  });
}

export function useOrganisation(orgId: string | null) {
  return useQuery({
    queryKey: keys.one(orgId ?? ""),
    enabled: Boolean(orgId),
    queryFn: async () => (await apiClient.get<OrganisationSummary>(`/organisations/${orgId}`)).data,
  });
}

export function useMembers(orgId: string | null) {
  return useQuery({
    queryKey: keys.members(orgId ?? ""),
    enabled: Boolean(orgId),
    queryFn: async () => (await apiClient.get<Member[]>(`/organisations/${orgId}/members`)).data,
  });
}

export function useCreateOrganisation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (input: { name: string; slug?: string }) =>
      (await apiClient.post<OrganisationSummary>("/organisations", input)).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useRenameOrganisation(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (name: string) =>
      (await apiClient.put(`/organisations/${orgId}`, { name })).data,
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: keys.all });
      void qc.invalidateQueries({ queryKey: keys.one(orgId) });
    },
  });
}

export function useDeleteOrganisation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (orgId: string) => {
      await apiClient.delete(`/organisations/${orgId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}

export function useTransferOwnership(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (newOwnerId: string) => {
      await apiClient.put(`/organisations/${orgId}/owner`, { newOwnerId });
    },
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: keys.members(orgId) });
      void qc.invalidateQueries({ queryKey: keys.one(orgId) });
      void qc.invalidateQueries({ queryKey: keys.all });
    },
  });
}

export function useChangeMemberRole(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async ({ userId, role }: { userId: string; role: Role }) => {
      await apiClient.put(`/organisations/${orgId}/members/${userId}/role`, {
        role: roleName(role),
      });
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.members(orgId) }),
  });
}

export function useRemoveMember(orgId: string) {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (userId: string) => {
      await apiClient.delete(`/organisations/${orgId}/members/${userId}`);
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.members(orgId) }),
  });
}

export function useInviteMember(orgId: string) {
  return useMutation({
    mutationFn: async ({ email, role }: { email: string; role: Role }) =>
      (await apiClient.post(`/organisations/${orgId}/invitations`, { email, role: roleName(role) }))
        .data,
  });
}

export function useAcceptInvitation() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: async (token: string) =>
      (await apiClient.post<OrganisationSummary>("/invitations/accept", { token })).data,
    onSuccess: () => qc.invalidateQueries({ queryKey: keys.all }),
  });
}
