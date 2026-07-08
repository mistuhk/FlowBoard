using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Events;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Tasks.UnitTests.Domain;

public sealed class CommentTests
{
    private static TaskItem Task() =>
        TaskItem.Create(ProjectId.New(), OrganisationId.New(), "T", null, Priority.Medium, UserId.New());

    [Fact]
    public void CommentContent_accepts_1_to_10000_chars()
    {
        CommentContent.Create("hi").Value.Should().Be("hi");
        CommentContent.Create(new string('a', 10_000)).Value.Length.Should().Be(10_000);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CommentContent_rejects_empty(string value)
    {
        var act = () => CommentContent.Create(value);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CommentContent_rejects_over_10000_chars()
    {
        var act = () => CommentContent.Create(new string('a', 10_001));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddComment_adds_and_raises_CommentAddedEvent()
    {
        var task = Task();
        task.ClearDomainEvents();
        var author = UserId.New();

        var comment = task.AddComment(author, CommentContent.Create("hello"));

        task.Comments.Should().ContainSingle().Which.Id.Should().Be(comment.Id);
        comment.AuthorId.Should().Be(author);
        var ev = task.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<CommentAddedEvent>().Subject;
        ev.CommentId.Should().Be(comment.Id);
        ev.TaskId.Should().Be(task.Id);
        ev.AuthorId.Should().Be(author);
    }

    [Fact]
    public void EditComment_changes_the_content()
    {
        var task = Task();
        var c = task.AddComment(UserId.New(), CommentContent.Create("first"));

        task.EditComment(c.Id, CommentContent.Create("second"));

        task.Comments.Single(x => x.Id == c.Id).Content.Value.Should().Be("second");
    }

    [Fact]
    public void DeleteComment_soft_deletes()
    {
        var task = Task();
        var c = task.AddComment(UserId.New(), CommentContent.Create("x"));

        task.DeleteComment(c.Id);

        task.Comments.Single(x => x.Id == c.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Editing_or_deleting_an_unknown_or_deleted_comment_throws()
    {
        var task = Task();
        var c = task.AddComment(UserId.New(), CommentContent.Create("x"));
        task.DeleteComment(c.Id);

        var edit = () => task.EditComment(c.Id, CommentContent.Create("y"));   // already deleted
        var del = () => task.DeleteComment(CommentId.New());                    // unknown

        edit.Should().Throw<DomainException>();
        del.Should().Throw<DomainException>();
    }
}
