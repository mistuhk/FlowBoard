using System.Reflection;
using FlowBoard.Application.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Infrastructure.Persistence;
using NetArchTest.Rules;
using Xunit;

namespace FlowBoard.ArchitectureTests;

/// <summary>
/// Enforces the dependency rules of the FlowBoard modular monolith.
/// These tests run in CI on every push — violations fail the build immediately.
/// </summary>
public class DependencyTests
{
    // Assembly references
    private static readonly Assembly CoreDomainAssembly =
        typeof(Entity<>).Assembly;

    private static readonly Assembly CoreApplicationAssembly =
        typeof(IUnitOfWork).Assembly;

    private static readonly Assembly CoreInfrastructureAssembly =
        typeof(AppDbContext).Assembly;

    private static readonly Assembly IdentityDomainAssembly =
        typeof(FlowBoard.Modules.Identity.Domain.Aggregates.User).Assembly;

    private static readonly Assembly IdentityApplicationAssembly =
        typeof(FlowBoard.Modules.Identity.Infrastructure.DependencyInjection).Assembly
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .FirstOrDefault(a => a.FullName?.Contains("Identity.Application") == true)
        ?? typeof(FlowBoard.Modules.Identity.Infrastructure.DependencyInjection).Assembly;

    // Test 1: Core Domain has zero external NuGet dependencies
    [Fact(DisplayName = "Core Domain must not reference any external NuGet packages")]
    public void CoreDomain_Should_Not_Reference_External_Packages()
    {
        var result = Types
            .InAssembly(CoreDomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "MediatR",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "StackExchange.Redis",
                "Hangfire",
                "Newtonsoft.Json",
                "Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Core Domain references external packages — violations: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // Test 2: Identity Domain has zero external NuGet dependencies

    [Fact(DisplayName = "Identity Domain must not reference any external NuGet packages")]
    public void IdentityDomain_Should_Not_Reference_External_Packages()
    {
        var result = Types
            .InAssembly(IdentityDomainAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "MediatR",
                "FluentValidation",
                "Microsoft.EntityFrameworkCore",
                "StackExchange.Redis",
                "Hangfire",
                "Serilog")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Identity Domain references external packages — violations: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // Test 3: Application must not reference Infrastructure

    [Fact(DisplayName = "Application layer must not reference Infrastructure")]
    public void Application_Should_Not_Reference_Infrastructure()
    {
        var result = Types
            .InAssembly(CoreApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny(
                "FlowBoard.Infrastructure",
                "Microsoft.EntityFrameworkCore",
                "Npgsql",
                "StackExchange.Redis",
                "Hangfire")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Application layer references Infrastructure — violations: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    // Test 4: Domain events must inherit DomainEvent

    [Fact(DisplayName = "All domain event types must inherit DomainEvent base record")]
    public void DomainEvents_Should_Inherit_DomainEvent()
    {
        // Assemblies that contain domain event types
        var domainAssemblies = new[]
        {
            CoreDomainAssembly,
            IdentityDomainAssembly,
        };

        foreach (var assembly in domainAssemblies)
        {
            var result = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Event")
                .And()
                .AreNotAbstract()
                .Should()
                .Inherit(typeof(DomainEvent))
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"In {assembly.GetName().Name}: event types not inheriting DomainEvent — " +
                $"{string.Join(", ", result.FailingTypeNames ?? [])}");
        }
    }

    // Test 5: Controllers live only in Presentation layer

    [Fact(DisplayName = "Controllers must only reside in Presentation (Api) projects")]
    public void Controllers_Should_Only_Reside_In_Presentation_Layer()
    {
        // Domain and Application assemblies must never contain controllers
        var forbiddenAssemblies = new[] { CoreDomainAssembly, CoreApplicationAssembly };

        foreach (var assembly in forbiddenAssemblies)
        {
            var controllerTypes = Types
                .InAssembly(assembly)
                .That()
                .HaveNameEndingWith("Controller")
                .GetTypes()
                .ToList();

            Assert.True(controllerTypes.Count == 0,
                $"Controller found in non-Presentation assembly {assembly.GetName().Name}: " +
                $"{string.Join(", ", controllerTypes.Select(t => t.FullName))}");
        }
    }

    // Test 6: Value objects must inherit ValueObject base class

    [Fact(DisplayName = "All value object types must inherit the ValueObject base class")]
    public void ValueObjects_Should_Inherit_ValueObject()
    {
        var result = Types
            .InAssembly(IdentityDomainAssembly)
            .That()
            .ResideInNamespace("FlowBoard.Modules.Identity.Domain.ValueObjects")
            .Should()
            .Inherit(typeof(FlowBoard.Domain.Primitives.ValueObject))
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Value object types not inheriting ValueObject — violations: " +
            $"{string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
