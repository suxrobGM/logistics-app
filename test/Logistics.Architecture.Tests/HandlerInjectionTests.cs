using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Logistics.Architecture.Tests;

public class HandlerInjectionTests
{
    private const string forbiddenType = "Microsoft.AspNetCore.Http.IHttpContextAccessor";

    [Fact]
    public void No_handler_injects_IHttpContextAccessor()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Application)
            .And().HaveNameEndingWith("Handler")
            .Should().FollowCustomCondition(
                cls => cls.Dependencies.All(d => d.Target.FullName != forbiddenType),
                $"not inject {forbiddenType}",
                $"injects {forbiddenType}")
            .Check(AssemblyAnchors.Architecture);
    }
}
