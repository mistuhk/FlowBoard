using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Application;
using FlowBoard.Modules.Tasks.Application.Commands.ConfirmAttachmentUpload;
using FlowBoard.Modules.Tasks.Application.Commands.RemoveAttachment;
using FlowBoard.Modules.Tasks.Application.Commands.RequestAttachmentUpload;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.Entities;
using FlowBoard.Modules.Tasks.Domain.Repositories;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FlowBoard.Modules.Tasks.UnitTests.Application;

public sealed class AttachmentCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ITenantContext> _tenant = new();
    private readonly Mock<ITaskRepository> _tasks = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<IOrganisationMembershipReader> _membership = new();
    private readonly OrganisationId _orgId = OrganisationId.New();
    private readonly Guid _taskGuid = Guid.NewGuid();

    public AttachmentCommandHandlerTests() => _tenant.Setup(t => t.CurrentOrganisationId).Returns(_orgId);

    private TaskItem SeedTask()
    {
        var task = TaskItem.Create(ProjectId.New(), _orgId, "T", null, Priority.Medium, UserId.New());
        _tasks.Setup(r => r.GetByIdAsync(It.IsAny<TaskId>(), _orgId, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        return task;
    }

    [Fact]
    public async Task Request_returns_an_upload_url_scoped_to_the_task()
    {
        SeedTask();
        _storage.Setup(s => s.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>())).ReturnsAsync("https://minio/put");
        var handler = new RequestAttachmentUploadCommandHandler(_tenant.Object, _tasks.Object, _storage.Object);

        var result = await handler.Handle(new RequestAttachmentUploadCommand(_taskGuid, "f.pdf", 100, "application/pdf"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.UploadUrl.Should().Be("https://minio/put");
        result.Value.StorageKey.Should().StartWith($"{_orgId.Value}/{_taskGuid}/");
    }

    [Fact]
    public async Task Confirm_rejects_a_storage_key_for_a_different_task()
    {
        SeedTask();
        var handler = new ConfirmAttachmentUploadCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _storage.Object);

        var result = await handler.Handle(
            new ConfirmAttachmentUploadCommand(_taskGuid, $"{_orgId.Value}/{Guid.NewGuid()}/x/f.pdf", "f.pdf", 100, "application/pdf"),
            CancellationToken.None);

        result.Error.Should().Be(TaskErrors.InvalidStorageKey);
    }

    [Fact]
    public async Task Confirm_fails_when_the_object_was_not_uploaded()
    {
        SeedTask();
        var key = $"{_orgId.Value}/{_taskGuid}/uuid/f.pdf";
        _storage.Setup(s => s.ExistsAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _currentUser.Setup(c => c.UserId).Returns(UserId.New());
        var handler = new ConfirmAttachmentUploadCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _storage.Object);

        var result = await handler.Handle(new ConfirmAttachmentUploadCommand(_taskGuid, key, "f.pdf", 100, "application/pdf"), CancellationToken.None);

        result.Error.Should().Be(TaskErrors.AttachmentNotUploaded);
    }

    [Fact]
    public async Task Confirm_records_the_attachment_when_the_object_exists()
    {
        var task = SeedTask();
        var key = $"{_orgId.Value}/{_taskGuid}/uuid/f.pdf";
        _storage.Setup(s => s.ExistsAsync(key, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _currentUser.Setup(c => c.UserId).Returns(UserId.New());
        var handler = new ConfirmAttachmentUploadCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _storage.Object);

        var result = await handler.Handle(new ConfirmAttachmentUploadCommand(_taskGuid, key, "f.pdf", 100, "application/pdf"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Attachments.Should().ContainSingle().Which.StorageKey.Should().Be(key);
    }

    [Fact]
    public async Task Remove_by_a_non_uploader_non_admin_is_forbidden()
    {
        var task = SeedTask();
        var attachment = task.AddAttachment(UserId.New(), "f.pdf", 100, "application/pdf", "k");
        _currentUser.Setup(c => c.UserId).Returns(UserId.New());
        _membership.Setup(r => r.GetRoleNameAsync(_orgId, It.IsAny<UserId>(), It.IsAny<CancellationToken>())).ReturnsAsync("member");
        var handler = new RemoveAttachmentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var act = () => handler.Handle(new RemoveAttachmentCommand(_taskGuid, attachment.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Remove_by_the_uploader_succeeds()
    {
        var task = SeedTask();
        var uploader = UserId.New();
        var attachment = task.AddAttachment(uploader, "f.pdf", 100, "application/pdf", "k");
        _currentUser.Setup(c => c.UserId).Returns(uploader);
        var handler = new RemoveAttachmentCommandHandler(_currentUser.Object, _tenant.Object, _tasks.Object, _membership.Object);

        var result = await handler.Handle(new RemoveAttachmentCommand(_taskGuid, attachment.Id.Value), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        task.Attachments.Single().IsDeleted.Should().BeTrue();
    }
}
