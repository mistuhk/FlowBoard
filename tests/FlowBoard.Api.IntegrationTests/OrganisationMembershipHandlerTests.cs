using System.Security.Claims;
using FlowBoard.Api.Authorization;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Shared.ValueObjects;
using FlowBoard.Modules.Organisations.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FlowBoard.Api.IntegrationTests;

/// <summary>
/// Unit tests for the organisation membership authorisation handler. These exercise the policy
/// logic directly, without standing up the web host.
/// </summary>
public sealed class OrganisationMembershipHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IOrganisationMembershipReader> _reader = new();
    private readonly Guid _orgId = Guid.NewGuid();

    private async Task<bool> EvaluateAsync(OrganisationMembershipRequirement requirement, bool withOrgRoute = true)
    {
        var httpContext = new DefaultHttpContext();
        if (withOrgRoute)
            httpContext.Request.RouteValues["orgId"] = _orgId.ToString();

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);

        var handler = new OrganisationMembershipHandler(_currentUser.Object, _reader.Object, accessor.Object);
        var context = new AuthorizationHandlerContext([requirement], new ClaimsPrincipal(), resource: null);

        await handler.HandleAsync(context);
        return context.HasSucceeded;
    }

    private void Caller(bool authenticated) => _currentUser.Setup(c => c.IsAuthenticated).Returns(authenticated);

    private void Caller(bool authenticated, UserId userId)
    {
        _currentUser.Setup(c => c.IsAuthenticated).Returns(authenticated);
        _currentUser.Setup(c => c.UserId).Returns(userId);
    }

    private void RoleIs(string? roleName) =>
        _reader
            .Setup(r => r.GetRoleNameAsync(It.IsAny<OrganisationId>(), It.IsAny<UserId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleName);

    [Fact]
    public async Task Member_policy_succeeds_for_any_member()
    {
        Caller(true, UserId.New());
        RoleIs("guest");

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Guest));

        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Member_policy_fails_when_the_user_is_not_a_member()
    {
        Caller(true, UserId.New());
        RoleIs(null);

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Guest));

        succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Admin_policy_succeeds_for_an_owner()
    {
        Caller(true, UserId.New());
        RoleIs("owner");

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Admin));

        succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Admin_policy_fails_for_a_plain_member()
    {
        Caller(true, UserId.New());
        RoleIs("member");

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Admin));

        succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Fails_when_the_caller_is_not_authenticated()
    {
        Caller(false);

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Guest));

        succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task Fails_when_there_is_no_organisation_route_value()
    {
        Caller(true, UserId.New());
        RoleIs("owner");

        var succeeded = await EvaluateAsync(new OrganisationMembershipRequirement(MemberRole.Guest), withOrgRoute: false);

        succeeded.Should().BeFalse();
    }
}
