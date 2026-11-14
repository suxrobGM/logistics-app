using System.Reflection;
using Logistics.API.Controllers.Operations;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Logistics.Architecture.Tests;

/// <summary>Query strings share one vocabulary; see <c>.claude/rules/backend/api-design.md</c>.</summary>
public class QueryParameterNameTests
{
    private static IEnumerable<(string Owner, string Name)> QueryParameters()
    {
        var actions = typeof(LoadController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        foreach (var parameter in actions.SelectMany(a => a.GetParameters()))
        {
            if (parameter.GetCustomAttribute<FromQueryAttribute>() is null)
            {
                continue;
            }

            var type = parameter.ParameterType;
            if (type.IsClass && type != typeof(string))
            {
                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    yield return (type.Name, property.Name);
                }
            }
            else
            {
                yield return (parameter.Member.Name, parameter.Name!);
            }
        }
    }

    private static bool IsRetiredName(string name) =>
        name.Equals("StartDate", StringComparison.OrdinalIgnoreCase)
        || name.Equals("EndDate", StringComparison.OrdinalIgnoreCase)
        || name.StartsWith("Only", StringComparison.OrdinalIgnoreCase)
        || name.EndsWith("Name", StringComparison.Ordinal);

    [Fact]
    public void Query_parameters_use_the_shared_vocabulary()
    {
        var parameters = QueryParameters().ToArray();
        Assert.Contains(parameters, p => p.Name == "PageSize");

        var offenders = parameters
            .Where(p => IsRetiredName(p.Name))
            .Select(p => $"{p.Owner}.{p.Name}")
            .Distinct()
            .Order()
            .ToArray();

        Assert.Empty(offenders);
    }
}
