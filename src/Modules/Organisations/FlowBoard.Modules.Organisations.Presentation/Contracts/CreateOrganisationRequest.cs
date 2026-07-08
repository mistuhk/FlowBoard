namespace FlowBoard.Modules.Organisations.Presentation.Contracts;

/// <summary>
/// Request body for creating an organisation.
/// </summary>
/// <param name="Name">The organisation's display name. Must be 2 to 100 characters.</param>
/// <param name="Slug">An optional explicit slug. When omitted, one is derived from the name.</param>
public sealed record CreateOrganisationRequest(string Name, string? Slug);
