using FlowBoard.Domain.Primitives;

namespace FlowBoard.Modules.Organisations.Application;

/// <summary>
/// Well-known <see cref="Error"/> values returned by Organisations command and query handlers.
/// Centralised so error codes stay stable and discoverable across the module.
/// </summary>
public static class OrganisationErrors
{
    /// <summary>Returned when an organisation cannot be created because the slug is already taken.</summary>
    public static readonly Error SlugAlreadyInUse = new(
        "Organisations.SlugAlreadyInUse",
        "An organisation with this slug already exists.");

    /// <summary>Returned when the requested organisation does not exist or is no longer active.</summary>
    public static readonly Error NotFound = new(
        "Organisations.NotFound",
        "The organisation could not be found.");
}
