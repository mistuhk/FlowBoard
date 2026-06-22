using FlowBoard.Domain.Shared.Exceptions;
using FlowBoard.Modules.Tasks.Domain.ValueObjects;
using FluentAssertions;

namespace FlowBoard.Modules.Tasks.UnitTests.Domain;

public sealed class TaskItemStatusTests
{
    // Every allowed transition from the state machine.
    public static TheoryData<string, string> ValidTransitions() => new()
    {
        { "Todo", "InProgress" },
        { "Todo", "Blocked" },
        { "InProgress", "Blocked" },
        { "InProgress", "Done" },
        { "InProgress", "Todo" },
        { "Blocked", "InProgress" },
        { "Blocked", "Todo" },
        { "Done", "Todo" },
    };

    // A representative set of disallowed transitions, including self-transitions.
    public static TheoryData<string, string> InvalidTransitions() => new()
    {
        { "Todo", "Done" },
        { "Todo", "Todo" },
        { "InProgress", "InProgress" },
        { "Blocked", "Done" },
        { "Blocked", "Blocked" },
        { "Done", "InProgress" },
        { "Done", "Blocked" },
        { "Done", "Done" },
    };

    private static TaskItemStatus ByName(string name) => TaskItemStatus.All.Single(s => s.Name == name);

    [Theory]
    [MemberData(nameof(ValidTransitions))]
    public void CanTransitionTo_is_true_for_allowed_transitions(string from, string to)
    {
        ByName(from).CanTransitionTo(ByName(to)).Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(InvalidTransitions))]
    public void CanTransitionTo_is_false_for_disallowed_transitions(string from, string to)
    {
        ByName(from).CanTransitionTo(ByName(to)).Should().BeFalse();
    }

    [Theory]
    [InlineData("todo", "Todo")]
    [InlineData("in_progress", "InProgress")]
    [InlineData("blocked", "Blocked")]
    [InlineData("done", "Done")]
    public void FromPersistence_maps_the_stored_token_to_the_status(string token, string expectedName)
    {
        TaskItemStatus.FromPersistence(token).Name.Should().Be(expectedName);
    }

    [Fact]
    public void FromPersistence_rejects_an_unknown_token()
    {
        var act = () => TaskItemStatus.FromPersistence("archived");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void InProgress_persists_as_the_snake_case_token()
    {
        TaskItemStatus.InProgress.DbValue.Should().Be("in_progress");
    }
}
