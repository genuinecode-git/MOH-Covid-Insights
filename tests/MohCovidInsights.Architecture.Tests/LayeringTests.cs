using System.Reflection;

using MohCovidInsights.Application.Abstractions.Interfaces;
using MohCovidInsights.Domain.ValueObjects;

using NetArchTest.Rules;

using Shouldly;

namespace MohCovidInsights.Architecture.Tests;

public class LayeringTests
{
    private static readonly Assembly Domain = typeof(EpiWeek).Assembly;
    private static readonly Assembly Application = typeof(IQueryDispatcher).Assembly;
    private static readonly Assembly Infrastructure =
        typeof(Infrastructure.DependencyInjection).Assembly;
    private const string ApplicationNs = "MohCovidInsights.Application";
    private const string InfrastructureNs = "MohCovidInsights.Infrastructure";
    private const string ApiNs = "MohCovidInsights.Api";

    [Fact]
    public void Domain_DependsOnNothing()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Domain must stay dependency-free. Offenders: {Describe(result)}");
    }

    [Fact]
    public void Domain_DoesNotDependOnEntityFramework()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Persistence concerns must not leak into Domain. Offenders: {Describe(result)}");
    }

    [Fact]
    public void Application_DoesNotDependOnInfrastructureOrApi()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application must depend inwards only. Offenders: {Describe(result)}");
    }

    [Fact]
    public void Application_DoesNotDependOnEntityFramework()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Application talks to ports, not EF Core. Offenders: {Describe(result)}");
    }

    [Fact]
    public void Infrastructure_DoesNotDependOnApi()
    {
        var result = Types.InAssembly(Infrastructure)
            .Should()
            .NotHaveDependencyOn(ApiNs)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Infrastructure must not reach into the web layer. Offenders: {Describe(result)}");
    }

    [Fact]
    public void QueryHandlers_AreSealedAndLiveInTheApplicationLayer()
    {
        var result = Types.InAssembly(Application)
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Query handlers should be sealed. Offenders: {Describe(result)}");
    }

   [Fact]
    public void Ports_AreInterfaces_NotConcreteTypes()
    {
        var result = Types.InAssembly(Application)
            .That()
            .ResideInNamespace($"{ApplicationNs}.Abstractions")
            .And()
            .AreNotClasses()
            .And()
            .AreNotNested()
            .Should()
            .BePublic()
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(
            $"Ports must be publicly consumable. Offenders: {Describe(result)}");
    }

    private static string Describe(TestResult result) =>
        result.FailingTypeNames is null
            ? "none reported"
            : string.Join(", ", result.FailingTypeNames);
}