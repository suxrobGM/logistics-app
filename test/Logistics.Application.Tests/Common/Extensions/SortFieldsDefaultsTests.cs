using System.Reflection;
using Logistics.Application.Common.Extensions;
using Xunit;

namespace Logistics.Application.Tests.Common.Extensions;

/// <summary>A default naming an unregistered key would only fail when a client omits orderBy.</summary>
public class SortFieldsDefaultsTests
{
    public static TheoryData<string> SortFieldDeclarations()
    {
        var data = new TheoryData<string>();
        foreach (var field in DeclaredSortFields())
        {
            data.Add($"{field.DeclaringType!.Name}.{field.Name}");
        }

        return data;
    }

    private static IEnumerable<FieldInfo> DeclaredSortFields() =>
        typeof(SortFields<>).Assembly.GetTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
            .Where(f => f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(SortFields<>));

    [Theory]
    [MemberData(nameof(SortFieldDeclarations))]
    public void DefaultOrderBy_IsARegisteredKey(string declaration)
    {
        var field = DeclaredSortFields().Single(f => $"{f.DeclaringType!.Name}.{f.Name}" == declaration);
        var sorts = field.GetValue(null)!;
        var defaultOrderBy = (string?)sorts.GetType().GetProperty("DefaultOrderBy")!.GetValue(sorts);
        var keys = (IEnumerable<string>)sorts.GetType().GetProperty("Keys")!.GetValue(sorts)!;

        if (defaultOrderBy is not null)
        {
            Assert.Contains(defaultOrderBy.TrimStart('-'), keys);
        }
    }
}
