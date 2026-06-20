using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Modules.Organisations.Domain.Aggregates;
using FlowBoard.Modules.Organisations.Domain.Repositories;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using MediatR;

namespace FlowBoard.Modules.Organisations.Application.Commands.CreateOrganisation;

/// <summary>
/// Handles <see cref="CreateOrganisationCommand"/>: derives or validates the slug, enforces slug
/// uniqueness, and creates the <see cref="Organisation"/> aggregate with the caller as its sole
/// owner. The aggregate raises <c>OrganisationCreatedEvent</c>, dispatched via the outbox.
/// </summary>
public sealed class CreateOrganisationCommandHandler(
    ICurrentUserService currentUser,
    IOrganisationRepository organisations)
    : IRequestHandler<CreateOrganisationCommand, Result<OrganisationResponse>>
{
    /// <inheritdoc/>
    public async Task<Result<OrganisationResponse>> Handle(
        CreateOrganisationCommand request,
        CancellationToken cancellationToken)
    {
        var name = OrganisationName.Create(request.Name);

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? OrganisationSlug.FromName(name)
            : OrganisationSlug.Create(request.Slug);

        if (await organisations.ExistsBySlugAsync(slug, cancellationToken))
            return Result.Failure<OrganisationResponse>(OrganisationErrors.SlugAlreadyInUse);

        var organisation = Organisation.Create(name, slug, currentUser.UserId);
        await organisations.AddAsync(organisation, cancellationToken);

        return Result.Success(OrganisationResponse.From(organisation));
    }
}
