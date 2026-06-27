using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Notifications.UnitTests.Domain;

public sealed class NotificationTests
{
    [Fact]
    public void Create_makes_an_unread_notification()
    {
        var userId = UserId.New();
        var orgId = OrganisationId.New();
        var entityId = Guid.NewGuid();

        var notification = Notification.Create(
            userId, orgId, NotificationType.TaskAssigned, "You have been assigned a task.", "task", entityId);

        notification.UserId.Should().Be(userId);
        notification.OrganisationId.Should().Be(orgId);
        notification.Type.Should().Be(NotificationType.TaskAssigned);
        notification.EntityType.Should().Be("task");
        notification.EntityId.Should().Be(entityId);
        notification.IsRead.Should().BeFalse();
    }

    [Fact]
    public void MarkAsRead_sets_the_flag_and_is_idempotent()
    {
        var notification = Notification.Create(
            UserId.New(), OrganisationId.New(), NotificationType.CommentAdded, "msg", null, null);

        notification.MarkAsRead();
        notification.MarkAsRead();

        notification.IsRead.Should().BeTrue();
    }

    [Theory]
    [InlineData("task_assigned", "TaskAssigned")]
    [InlineData("task_status_changed", "TaskStatusChanged")]
    [InlineData("user_mentioned", "UserMentioned")]
    public void NotificationType_round_trips_from_persistence(string token, string expectedName)
    {
        NotificationType.FromPersistence(token).Name.Should().Be(expectedName);
    }

    [Fact]
    public void NotificationType_rejects_an_unknown_token()
    {
        var act = () => NotificationType.FromPersistence("nope");

        act.Should().Throw<DomainException>();
    }
}
