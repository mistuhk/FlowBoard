import { AtSign, Bell, CheckCheck, MessageSquare, UserPlus } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useCurrentOrg } from "@/features/organisations/useCurrentOrg";
import { type AppNotification, useMarkRead } from "@/features/notifications/notifications";

/// The icon for a notification type.
export function notificationIcon(type: string) {
  switch (type) {
    case "user_mentioned":
      return AtSign;
    case "comment_added":
      return MessageSquare;
    case "task_status_changed":
      return CheckCheck;
    case "task_assigned":
    case "invitation":
    case "project_invited":
      return UserPlus;
    default:
      return Bell;
  }
}

/// Marks a notification read and navigates to a sensible destination for its organisation.
///
/// Note: notifications carry the entity id (for example a task) but not enough context (the project)
/// to deep-link straight to the task drawer, so this switches to the notification's organisation and
/// lands on its projects. Precise task deep-linking needs the project id on the notification payload
/// (tracked as a backend follow-up).
export function useOpenNotification() {
  const navigate = useNavigate();
  const markRead = useMarkRead();
  const { setCurrentOrgId } = useCurrentOrg();
  return (n: AppNotification) => {
    if (!n.isRead) markRead.mutate(n.id);
    if (n.organisationId) setCurrentOrgId(n.organisationId);
    navigate("/");
  };
}
