using NetArchTest.Rules;
using Xunit;

namespace Logistics.Architecture.Tests;

public class HandlerInjectionTests
{
    [Fact]
    public void No_handler_injects_IHttpContextAccessor()
    {
        var result = Types.InAssembly(AssemblyAnchors.Application.Assembly)
            .That().HaveNameEndingWith("Handler")
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore.Http.IHttpContextAccessor")
            .GetResult();

        Assert.True(result.IsSuccessful,
            result.FailingTypeNames is null
                ? "(no details)"
                : string.Join("\n  ", result.FailingTypeNames));
    }
}
