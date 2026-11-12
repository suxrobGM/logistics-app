using ArchUnitNET.xUnitV3;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Logistics.Architecture.Tests;

public class HandlerInjectionTests
{
    private const string ForbiddenType = "Microsoft.AspNetCore.Http.IHttpContextAccessor";

    [Fact]
    public void No_handler_injects_IHttpContextAccessor()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Application)
            .And().HaveNameEndingWith("Handler")
            .Should().FollowCustomCondition(
                cls => cls.Dependencies.All(d => d.Target.FullName != ForbiddenType),
                $"not inject {ForbiddenType}",
                $"injects {ForbiddenType}")
            .Check(AssemblyAnchors.Architecture);
    }
}
