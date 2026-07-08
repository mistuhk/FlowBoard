using FluentValidation;

namespace FlowBoard.Modules.Projects.Application.Commands.AddProjectMember;

/// <summary>Validates <see cref="AddProjectMemberCommand"/>.</summary>
public sealed class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    /// <summary>Configures the validation rules for adding a project member.</summary>
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User id is required.");
    }
}
