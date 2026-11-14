using System.Reflection;
using Logistics.API.Controllers.Operations;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Logistics.Architecture.Tests;

public class ControllerResponseTests
{
    /// <summary>Lists return <c>{items, pagination}</c>; a raw <see cref="PagedResult{T}"/> leaks <c>isSuccess</c> and <c>value</c>.</summary>
    [Fact]
    public void ListActions_DeclarePagedResponse_NotPagedResult()
    {
        var offenders = typeof(LoadController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(m => m.GetCustomAttributes<ProducesResponseTypeAttribute>()
                .Any(a => a.Type is { IsGenericType: true } type && type.GetGenericTypeDefinition() == typeof(PagedResult<>)))
            .Select(m => $"{m.DeclaringType!.Name}.{m.Name}")
            .ToArray();

        Assert.Empty(offenders);
    }
}
