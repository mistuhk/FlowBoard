using FluentValidation;

namespace FlowBoard.Modules.Organisations.Application.Commands.InviteMember;

/// <summary>
/// Validates <see cref="InviteMemberCommand"/>. The role must be one that can be invited
/// (Owner is excluded). The <c>ValidationBehaviour</c> runs this before the handler.
/// </summary>
public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    private static readonly string[] InvitableRoles = ["admin", "member", "guest"];

    /// <summary>Configures the validation rules for inviting a member.</summary>
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.OrganisationId)
            .NotEmpty().WithMessage("Organisation id is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email address must not exceed 254 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => InvitableRoles.Contains(role.Trim().ToLowerInvariant()))
            .WithMessage("Role must be one of Admin, Member, or Guest.");
    }
}
