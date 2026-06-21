using FluentValidation;

namespace FlowBoard.Modules.Projects.Application.Commands.RemoveProjectMember;

/// <summary>Validates <see cref="RemoveProjectMemberCommand"/>.</summary>
public sealed class RemoveProjectMemberCommandValidator : AbstractValidator<RemoveProjectMemberCommand>
{
    /// <summary>Configures the validation rules for removing a project member.</summary>
    public RemoveProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}
