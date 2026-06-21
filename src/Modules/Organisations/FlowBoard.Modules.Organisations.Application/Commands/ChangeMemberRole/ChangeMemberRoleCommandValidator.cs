using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.ChangeMemberRole;

/// <summary>
/// Validates <see cref="ChangeMemberRoleCommand"/>. The role must be assignable (Owner is excluded;
/// ownership moves only via transfer).
/// </summary>
public sealed class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    private static readonly string[] AssignableRoles = ["admin", "member", "guest"];

    /// <summary>Configures the validation rules for changing a member's role.</summary>
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => AssignableRoles.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be one of Admin, Member, or Guest.");
    }
}
