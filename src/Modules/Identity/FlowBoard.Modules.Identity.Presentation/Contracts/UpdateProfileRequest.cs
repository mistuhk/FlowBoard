namespace FlowBoard.Modules.Identity.Presentation.Contracts;

/// <summary>The request body for <c>PUT /api/v1/users/me</c>.</summary>
/// <param name="DisplayName">The new display name.</param>
public sealed record UpdateProfileRequest(string DisplayName);
