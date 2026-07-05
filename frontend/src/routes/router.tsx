import { createBrowserRouter } from "react-router-dom";
import { OrgScopedLayout } from "@/components/AppShell";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { CreateOrganisationPage } from "@/pages/CreateOrganisationPage";
import { InvitationAcceptPage } from "@/pages/InvitationAcceptPage";
import { MembersPage } from "@/pages/MembersPage";
import { NotFoundPage } from "@/pages/NotFoundPage";
import { OrganisationSettingsPage } from "@/pages/OrganisationSettingsPage";
import { ProjectTasksPage } from "@/pages/ProjectTasksPage";
import { ProjectsPage } from "@/pages/ProjectsPage";
import { ForgotPasswordPage } from "@/pages/auth/ForgotPasswordPage";
import { LoginPage } from "@/pages/auth/LoginPage";
import { RegisterPage } from "@/pages/auth/RegisterPage";
import { ResetPasswordPage } from "@/pages/auth/ResetPasswordPage";
import { VerifyEmailPage } from "@/pages/auth/VerifyEmailPage";

export const router = createBrowserRouter([
  { path: "/login", element: <LoginPage /> },
  { path: "/register", element: <RegisterPage /> },
  { path: "/verify-email", element: <VerifyEmailPage /> },
  { path: "/forgot-password", element: <ForgotPasswordPage /> },
  { path: "/reset-password", element: <ResetPasswordPage /> },
  {
    element: <ProtectedRoute />,
    children: [
      { path: "/organisations/new", element: <CreateOrganisationPage /> },
      { path: "/invitations/accept", element: <InvitationAcceptPage /> },
      {
        element: <OrgScopedLayout />,
        children: [
          { path: "/", element: <ProjectsPage /> },
          { path: "/projects/:projectId", element: <ProjectTasksPage /> },
          { path: "/members", element: <MembersPage /> },
          { path: "/settings", element: <OrganisationSettingsPage /> },
        ],
      },
    ],
  },
  { path: "*", element: <NotFoundPage /> },
]);
