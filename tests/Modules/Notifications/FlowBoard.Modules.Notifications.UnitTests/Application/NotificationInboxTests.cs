using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Notifications.Application;
using FlowBoard.Modules.Notifications.Application.Commands.MarkAllAsRead;
using FlowBoard.Modules.Notifications.Application.Commands.MarkAsRead;
using FlowBoard.Modules.Notifications.Application.Queries.GetUnreadCount;
using FlowBoard.Modules.Notifications.Domain.Aggregates;
using FlowBoard.Modules.Notifications.Domain.Repositories;
using FlowBoard.Modules.Notifications.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Notifications.UnitTests.Application;

public sealed class NotificationInboxTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<INotificationRepository> _repo = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly UserId _userId = UserId.New();

    public NotificationInboxTests() => _currentUser.Setup(c => c.UserId).Returns(_userId);

    private string Key => NotificationCacheKeys.Unread(_userId.Value);

    private static Notification ANotification(UserId userId) =>
        Notification.Create(userId, OrganisationId.New(), NotificationType.TaskAssigned, "msg", "task", Guid.NewGuid());

    [Fact]
    public async Task MarkAsRead_returns_not_found_when_the_notification_is_not_the_users()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<NotificationId>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Notification?)null);
        var handler = new MarkAsReadCommandHandler(_currentUser.Object, _repo.Object, _cache.Object);

        var result = await handler.Handle(new MarkAsReadCommand(Guid.NewGuid()), CancellationToken.None);

        result.Error.Should().Be(NotificationErrors.NotFound);
        _cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsRead_marks_the_notification_and_invalidates_the_unread_cache()
    {
        var notification = ANotification(_userId);
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<NotificationId>(), _userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(notification);
        var handler = new MarkAsReadCommandHandler(_currentUser.Object, _repo.Object, _cache.Object);

        var result = await handler.Handle(new MarkAsReadCommand(notification.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        notification.IsRead.Should().BeTrue();
        _cache.Verify(c => c.RemoveAsync(Key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsRead_updates_via_the_repository_and_invalidates_the_cache()
    {
        var handler = new MarkAllAsReadCommandHandler(_currentUser.Object, _repo.Object, _cache.Object);

        var result = await handler.Handle(new MarkAllAsReadCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repo.Verify(r => r.MarkAllAsReadAsync(_userId, It.IsAny<CancellationToken>()), Times.Once);
        _cache.Verify(c => c.RemoveAsync(Key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnreadCount_returns_the_cached_value_without_hitting_the_database()
    {
        _cache.Setup(c => c.GetAsync<int?>(Key, It.IsAny<CancellationToken>())).ReturnsAsync(7);
        var handler = new GetUnreadCountQueryHandler(_currentUser.Object, _repo.Object, _cache.Object);

        var count = await handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        count.Should().Be(7);
        _repo.Verify(r => r.CountUnreadAsync(It.IsAny<UserId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnreadCount_computes_and_caches_on_a_miss()
    {
        _cache.Setup(c => c.GetAsync<int?>(Key, It.IsAny<CancellationToken>())).ReturnsAsync((int?)null);
        _repo.Setup(r => r.CountUnreadAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(3);
        var handler = new GetUnreadCountQueryHandler(_currentUser.Object, _repo.Object, _cache.Object);

        var count = await handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        count.Should().Be(3);
        _cache.Verify(c => c.SetAsync(Key, 3, It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
