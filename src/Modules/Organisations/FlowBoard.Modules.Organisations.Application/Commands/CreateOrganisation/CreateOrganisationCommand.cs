using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;

/// <summary>
/// Creates a new organisation owned by the authenticated caller, who becomes its sole owner.
/// </summary>
/// <param name="Name">The organisation's display name. Must be 2 to 100 characters.</param>
/// <param name="Slug">
/// An optional explicit slug. When omitted, a slug is derived from <paramref name="Name"/>.
/// </param>
public sealed record CreateOrganisationCommand(string Name, string? Slug)
    : ICommand<Result<OrganisationResponse>>;
