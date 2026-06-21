using FluentValidation;

namespace FlowBoard.Modules.Projects.Application.Commands.ArchiveProject;

/// <summary>Validates <see cref="ArchiveProjectCommand"/>.</summary>
public sealed class ArchiveProjectCommandValidator : AbstractValidator<ArchiveProjectCommand>
{
    /// <summary>Configures the validation rules for archiving a project.</summary>
    public ArchiveProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");
    }
}
