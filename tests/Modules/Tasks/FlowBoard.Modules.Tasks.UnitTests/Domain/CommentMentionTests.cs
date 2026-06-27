using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Events;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Tasks.UnitTests.Domain;

public sealed class CommentMentionTests
{
    private static TaskItem Task() =>
        TaskItem.Create(ProjectId.New(), OrganisationId.New(), "T", null, Priority.Medium, UserId.New());

    [Fact]
    public void Extracts_a_single_mention()
    {
        CommentContent.Create("hey @ada take a look").Mentions.Should().Equal("ada");
    }

    [Fact]
    public void Extracts_multiple_distinct_mentions_lowercased()
    {
        CommentContent.Create("@Ada and @grace and @ada again").Mentions.Should().Equal("ada", "grace");
    }

    [Fact]
    public void Extracts_no_mentions_when_there_are_none()
    {
        CommentContent.Create("just some text, nothing tagged").Mentions.Should().BeEmpty();
    }

    [Fact]
    public void Does_not_treat_an_email_address_as_a_mention()
    {
        CommentContent.Create("write to ada@example.com about it").Mentions.Should().BeEmpty();
    }

    [Fact]
    public void Supports_dotted_handles()
    {
        CommentContent.Create("ping @ada.lovelace please").Mentions.Should().Equal("ada.lovelace");
    }

    [Fact]
    public void AddComment_raises_one_UserMentionedEvent_per_distinct_handle()
    {
        var task = Task();
        task.ClearDomainEvents();
        var author = UserId.New();

        task.AddComment(author, CommentContent.Create("@ada and @grace, see this"));

        var mentions = task.DomainEvents.OfType<UserMentionedEvent>().ToList();
        mentions.Should().HaveCount(2);
        mentions.Select(m => m.Handle).Should().BeEquivalentTo(["ada", "grace"]);
        mentions.Should().OnlyContain(m => m.AuthorId == author && m.TaskId == task.Id);
    }

    [Fact]
    public void AddComment_with_no_mentions_raises_no_UserMentionedEvent()
    {
        var task = Task();
        task.ClearDomainEvents();

        task.AddComment(UserId.New(), CommentContent.Create("no tags here"));

        task.DomainEvents.OfType<UserMentionedEvent>().Should().BeEmpty();
    }
}
