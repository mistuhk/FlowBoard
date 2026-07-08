using FluentValidation;

namespace FlowBoard.Modules.Projects.Application.Commands.DeleteProject;

/// <summary>Validates <see cref="DeleteProjectCommand"/>.</summary>
public sealed class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    /// <summary>Configures the validation rules for deleting a project.</summary>
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");
    }
}
