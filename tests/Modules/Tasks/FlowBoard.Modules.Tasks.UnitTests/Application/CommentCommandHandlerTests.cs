using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.AddComment;
using FlowBoard.Modules.Tasks.Application.Commands.DeleteComment;
using FlowBoard.Modules.Tasks.Application.Commands.EditComment;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Entities;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class CommentCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IOrganisationMembershipReader> _membership = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly UserId _author = UserId.New();

    public CommentCommandHandlerTests() => _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);

    private (TaskItem task, Comment comment) SeedTaskWithComment()
    {
        var task = TaskItem.Create(ProjectId.New(), _orgId, "T", null, Priority.Medium, UserId.New());
        var comment = task.AddComment(_author, CommentContent.Create("original"));
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        return (task, comment);
    }

    private void Caller(UserId id) => _currentUser.Setup(c => c.UserId).Returns(id);
    private void Role(string? role) =>
        _membership.Setup(r => r.GetRoleNameAsync(_orgId, It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync(role);

    [Fact]
    public async Task Add_returns_not_found_when_task_missing()
    {
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>())).ReturnsAsync((TaskItem?)null);
        Caller(_author);
        var handler = new AddCommentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object);

        var result = await handler.Handle(new AddCommentCommand(Guid.NewGuid(), "hi"), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.NotFound);
    }

    [Fact]
    public async Task Edit_by_the_author_succeeds()
    {
        var (_, comment) = SeedTaskWithComment();
        Caller(_author);
        var handler = new EditCommentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var result = await handler.Handle(new EditCommentCommand(Guid.NewGuid(), comment.Id.Value, "edited"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("edited");
    }

    [Fact]
    public async Task Edit_by_a_non_author_non_admin_is_forbidden()
    {
        var (_, comment) = SeedTaskWithComment();
        Caller(UserId.New());
        Role("member");
        var handler = new EditCommentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var act = () => handler.Handle(new EditCommentCommand(Guid.NewGuid(), comment.Id.Value, "x"), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Delete_by_an_admin_who_is_not_the_author_succeeds()
    {
        var (task, comment) = SeedTaskWithComment();
        Caller(UserId.New());
        Role("admin");
        var handler = new DeleteCommentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var result = await handler.Handle(new DeleteCommentCommand(Guid.NewGuid(), comment.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Comments.Single().IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Edit_returns_comment_not_found_for_unknown_comment()
    {
        SeedTaskWithComment();
        Caller(_author);
        var handler = new EditCommentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var result = await handler.Handle(new EditCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "x"), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.CommentNotFound);
    }
}
