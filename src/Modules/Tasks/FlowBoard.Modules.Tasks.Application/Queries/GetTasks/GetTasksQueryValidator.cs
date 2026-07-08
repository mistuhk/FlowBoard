using FluentValidation;

namespace FlowBoard.Modules.Tasks.Application.Queries.GetTasks;

/// <summary>Validates <see cref="GetTasksQuery"/> (filter values and page size).</summary>
public sealed class GetTasksQueryValidator : AbstractValidator<GetTasksQuery>
{
    private static readonly string[] Statuses = ["todo", "inprogress", "blocked", "done"];
    private static readonly string[] Priorities = ["low", "medium", "high", "critical"];

    /// <summary>Configures the validation rules for listing tasks.</summary>
    public GetTasksQueryValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project id is required.");

        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithMessage("Limit must be between 1 and 100.")
            .When(x => x.Limit is not null);

        RuleFor(x => x.Status!)
            .Must(s => Statuses.Contains(s.Trim().ToLowerInvariant()))
            .WithMessage("Status must be one of Todo, InProgress, Blocked, or Done.")
            .When(x => !string.IsNullOrWhiteSpace(x.Status));

        RuleFor(x => x.Priority!)
            .Must(p => Priorities.Contains(p.Trim().ToLowerInvariant()))
            .WithMessage("Priority must be one of Low, Medium, High, or Critical.")
            .When(x => !string.IsNullOrWhiteSpace(x.Priority));
    }
}
