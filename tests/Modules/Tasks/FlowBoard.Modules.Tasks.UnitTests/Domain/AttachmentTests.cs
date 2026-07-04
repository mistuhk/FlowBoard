using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Tasks.Domain.Aggregates;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Tasks.UnitTests.Domain;

public sealed class AttachmentTests
{
    private static TaskItem Task() =>
        TaskItem.Create(ProjectId.New(), OrganisationId.New(), "T", null, Priority.Medium, UserId.New());

    [Fact]
    public void AddAttachment_records_metadata()
    {
        var task = Task();
        var uploader = UserId.New();

        var attachment = task.AddAttachment(uploader, "spec.pdf", 1024, "application/pdf", "org/task/uuid/spec.pdf");

        task.Attachments.Should().ContainSingle().Which.Id.Should().Be(attachment.Id);
        attachment.UploadedById.Should().Be(uploader);
        attachment.FileName.Should().Be("spec.pdf");
        attachment.FileSizeBytes.Should().Be(1024);
        attachment.StorageKey.Should().Be("org/task/uuid/spec.pdf");
        attachment.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void RemoveAttachment_soft_deletes()
    {
        var task = Task();
        var a = task.AddAttachment(UserId.New(), "f.png", 10, "image/png", "k");

        task.RemoveAttachment(a.Id);

        task.Attachments.Single(x => x.Id == a.Id).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Removing_an_unknown_or_already_removed_attachment_throws()
    {
        var task = Task();
        var a = task.AddAttachment(UserId.New(), "f.png", 10, "image/png", "k");
        task.RemoveAttachment(a.Id);

        var again = () => task.RemoveAttachment(a.Id);             // already removed
        var unknown = () => task.RemoveAttachment(AttachmentId.New());

        again.Should().Throw<DomainException>();
        unknown.Should().Throw<DomainException>();
    }
}
