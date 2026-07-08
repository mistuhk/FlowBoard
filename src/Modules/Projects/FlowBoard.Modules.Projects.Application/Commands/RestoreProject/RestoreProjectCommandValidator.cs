using FluentValidation;

namespace FlowBoard.Modules.Projects.Application.Commands.RestoreProject;

/// <summary>Validates <see cref="RestoreProjectCommand"/>.</summary>
public sealed class RestoreProjectCommandValidator : AbstractValidator<RestoreProjectCommand>
{
    /// <summary>Configures the validation rules for restoring a project.</summary>
    public RestoreProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");
    }
}
