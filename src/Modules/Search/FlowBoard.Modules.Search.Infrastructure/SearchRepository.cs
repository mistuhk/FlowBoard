using System.Text;
using FlowBoard.Infrastructure.Persistence;
using FlowBoard.Modules.Search.Application;
using FlowBoard.Modules.Search.Application.Abstractions;
using FlowBoard.Modules.Search.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace FlowBoard.Modules.Search.Infrastructure;

/// <summary>
/// PostgreSQL full-text search expressed in EF Core / Npgsql LINQ. Tasks match against the maintained
/// <c>search_vector</c> generated column (GIN-indexed); projects match a tsvector computed over name
/// and description. Ranking uses <c>ts_rank</c>. Every query is scoped to the organisation. Highlighted
/// snippets are produced client-side, so no <c>ts_headline</c> (which Npgsql does not translate) is needed.
/// </summary>
internal sealed class SearchRepository(AppDbContext context) : ISearchRepository
{
    private const string Config = "english";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<TaskSearchResult>> SearchTasksAsync(
        Guid organisationId, string? query, TaskSearchFilters filters, int limit, CancellationToken cancellationToken = default)
    {
        var tasks = context.Set<TaskSearchDocument>()
            .AsNoTracking()
            .Where(t => t.OrganisationId == organisationId && t.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var status = ToToken(filters.Status);
            tasks = tasks.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(filters.Priority))
        {
            var priority = ToToken(filters.Priority);
            tasks = tasks.Where(t => t.Priority == priority);
        }

        if (filters.AssigneeId is { } assignee)
            tasks = tasks.Where(t => t.AssigneeId == assignee);
        if (filters.DueBefore is { } before)
        {
            var beforeUtc = AsUtc(before);
            tasks = tasks.Where(t => t.DueDate < beforeUtc);
        }

        if (filters.DueAfter is { } after)
        {
            var afterUtc = AsUtc(after);
            tasks = tasks.Where(t => t.DueDate >= afterUtc);
        }

        // With a text query, match and rank; otherwise return filtered results newest first, unranked.
        if (!string.IsNullOrWhiteSpace(query))
        {
            return await tasks
                .Where(t => t.SearchVector.Matches(EF.Functions.WebSearchToTsQuery(Config, query)))
                .OrderByDescending(t => t.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(Config, query)))
                .ThenByDescending(t => t.CreatedAt)
                .Take(limit)
                .Select(t => new TaskSearchResult(
                    t.Id, t.ProjectId, t.Title, t.Status, t.Priority,
                    (double)t.SearchVector.Rank(EF.Functions.WebSearchToTsQuery(Config, query))))
                .ToListAsync(cancellationToken);
        }

        return await tasks
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new TaskSearchResult(t.Id, t.ProjectId, t.Title, t.Status, t.Priority, 0d))
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProjectSearchResult>> SearchProjectsAsync(
        Guid organisationId, string? query, int limit, CancellationToken cancellationToken = default)
    {
        var projects = context.Set<ProjectSearchDocument>()
            .AsNoTracking()
            .Where(p => p.OrganisationId == organisationId && p.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(query))
        {
            return await projects
                .Where(p => EF.Functions.ToTsVector(Config, (p.Name ?? "") + " " + (p.Description ?? ""))
                    .Matches(EF.Functions.WebSearchToTsQuery(Config, query)))
                .OrderByDescending(p => EF.Functions.ToTsVector(Config, (p.Name ?? "") + " " + (p.Description ?? ""))
                    .Rank(EF.Functions.WebSearchToTsQuery(Config, query)))
                .ThenByDescending(p => p.CreatedAt)
                .Take(limit)
                .Select(p => new ProjectSearchResult(
                    p.Id, p.Name, p.Status,
                    (double)EF.Functions.ToTsVector(Config, (p.Name ?? "") + " " + (p.Description ?? ""))
                        .Rank(EF.Functions.WebSearchToTsQuery(Config, query))))
                .ToListAsync(cancellationToken);
        }

        return await projects
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new ProjectSearchResult(p.Id, p.Name, p.Status, 0d))
            .ToListAsync(cancellationToken);
    }

    // Maps an API token (e.g. "InProgress", "High") to the stored snake_case value ("in_progress", "high").
    private static string ToToken(string value)
    {
        var builder = new StringBuilder(value.Length + 4);
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsUpper(c) && i > 0)
                builder.Append('_');
            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }

    private static DateTime AsUtc(DateTime value) =>
        value.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(value, DateTimeKind.Utc) : value.ToUniversalTime();
}
