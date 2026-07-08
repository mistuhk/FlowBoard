using FlowBoard.Modules.Search.Infrastructure.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowBoard.Modules.Search.Infrastructure.Persistence;

/// <summary>
/// Maps the Search module's keyless read model to the existing <c>tasks</c> table for querying only.
/// Uses <c>ToView</c> so it is excluded from migrations (the table is owned by the Tasks module) and
/// does not conflict with that module's table mapping. Column names follow the snake_case convention.
/// </summary>
internal sealed class TaskSearchDocumentConfiguration : IEntityTypeConfiguration<TaskSearchDocument>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<TaskSearchDocument> builder)
    {
        builder.HasNoKey().ToView("tasks");
        builder.Property(t => t.SearchVector).HasColumnType("tsvector");
    }
}

/// <summary>Maps the Search module's keyless read model to the existing <c>projects</c> table for querying only.</summary>
internal sealed class ProjectSearchDocumentConfiguration : IEntityTypeConfiguration<ProjectSearchDocument>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<ProjectSearchDocument> builder) =>
        builder.HasNoKey().ToView("projects");
}
