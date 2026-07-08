namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for renaming an organisation.
/// </summary>
/// <param name="Name">The new display name. Must be 2 to 100 characters.</param>
public sealed record RenameOrganisationRequest(string Name);
