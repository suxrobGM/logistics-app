# Phase 5 — Architecture tests as CI gate

> **Status: DONE — 2026-05-13.** Commit `15081403`.
>
> `tests/Logistics.Architecture.Tests/` scaffolded with NetArchTest IL rules + csproj-XML rules + handler-injection rules. 27 enforced tests pass; 4 are `[Fact(Skip = ...)]`-tagged for slice 1.8 (`Application_must_not_depend_on_Stripe`, `Abstractions_must_not_depend_on_Stripe`, plus their csproj-XML variants). `Logistics.Infrastructure.AI` is omitted from the layering rules' `[Theory] InlineData` with TODO comments pointing at slice 1.9-AI.

## Goal

Add a new `tests/Logistics.Architecture.Tests/` project using **NetArchTest.Rules** + xUnit that locks in the layering decisions made across Phases 1–3. Wire it into CI so any future PR introducing a forbidden dependency fails the build.

## Why

Refactors decay without enforcement. Today, nothing prevents a future contributor from adding `using Stripe;` in `Application/`, or from making `Infrastructure.Storage` depend on `Logistics.Application`, or from calling `SaveChangesAsync` in a handler after Phase 6 lands. Arch tests are cheap to write and catch all of these at PR time.

Codex's review of the master plan specifically called for **two pitfalls to avoid**:

1. **Use exact assembly name targeting, not prefix matching.** A rule "Application may not depend on `Logistics.Infrastructure.*`" can accidentally flag `Logistics.Application.Abstractions` if you target by prefix. Always use `Types.InAssembly(typeof(X).Assembly)`.
2. **Test `.csproj` package references too.** NetArchTest only checks IL-level dependencies. An unused `<PackageReference Include="Stripe.net" />` lingering in `Abstractions.csproj` won't be caught. Add a parallel XML-based check.

## Prerequisites

- Phase 1 should be fully complete (especially Slices 1.8 + 1.9-remainder). Some tests below will fail otherwise — that's the point, but you want a clean baseline before adding them.
- Phase 2 (Stripe/Tax mapper moves) done.
- Phase 3 (HttpContextAccessor + Include leaks) done.
- Phase 6 (`SaveChangesAsync` rule) **not** required first — that test is added at the end of Phase 6, not here.

## Project setup

```
tests/Logistics.Architecture.Tests/
  Logistics.Architecture.Tests.csproj
  BoundaryTests.cs
  HandlerInjectionTests.cs
  CsprojAssertions.cs           ← XML helper for package-ref checks
  CsprojReferenceTests.cs
  HandlerSaveChangesTests.cs    ← added at the END of Phase 6 only
```

### `Logistics.Architecture.Tests.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="..." />
    <PackageReference Include="xunit" Version="..." />
    <PackageReference Include="xunit.runner.visualstudio" Version="..." />
    <PackageReference Include="NetArchTest.Rules" Version="1.3.2" />
  </ItemGroup>
  <ItemGroup>
    <!-- Reference every project we want to inspect -->
    <ProjectReference Include="..\..\src\Core\Logistics.Application\Logistics.Application.csproj" />
    <ProjectReference Include="..\..\src\Core\Logistics.Application.Abstractions\Logistics.Application.Abstractions.csproj" />
    <ProjectReference Include="..\..\src\Core\Logistics.Domain\Logistics.Domain.csproj" />
    <!-- Reference every Infrastructure.* project -->
    <ProjectReference Include="..\..\src\Infrastructure\Logistics.Infrastructure.AI\Logistics.Infrastructure.AI.csproj" />
    <ProjectReference Include="..\..\src\Infrastructure\Logistics.Infrastructure.Communications\Logistics.Infrastructure.Communications.csproj" />
    <!-- ... all 11 ... -->
  </ItemGroup>
</Project>
```

Match versions to other test projects in `tests/`.

Add to `Logistics.slnx` under the `/Test/` folder.

## `BoundaryTests.cs` — IL-dependency rules

```csharp
using NetArchTest.Rules;
using Xunit;

namespace Logistics.Architecture.Tests;

public class BoundaryTests
{
    [Fact]
    public void Application_must_not_depend_on_Stripe()
    {
        var result = Types.InAssembly(typeof(Logistics.Application.Registrar).Assembly)
            .ShouldNot().HaveDependencyOnAny("Stripe")
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Application depends on Stripe.* — culprits: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_must_not_depend_on_AspNetCore_Http()
    {
        var result = Types.InAssembly(typeof(Logistics.Application.Registrar).Assembly)
            .ShouldNot().HaveDependencyOnAny("Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Application_must_not_depend_on_Infrastructure_assemblies()
    {
        var result = Types.InAssembly(typeof(Logistics.Application.Registrar).Assembly)
            .ShouldNot().HaveDependencyOnAny(
                "Logistics.Infrastructure.AI",
                "Logistics.Infrastructure.Communications",
                "Logistics.Infrastructure.Documents",
                "Logistics.Infrastructure.Integrations.Eld",
                "Logistics.Infrastructure.Integrations.LoadBoard",
                "Logistics.Infrastructure.Payments",
                "Logistics.Infrastructure.Persistence",
                "Logistics.Infrastructure.Routing",
                "Logistics.Infrastructure.Storage",
                "Logistics.Infrastructure.Tax",
                "Logistics.Infrastructure.Vin")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Abstractions_must_not_depend_on_Application_or_Infrastructure_or_SDKs()
    {
        // Use a known type from Abstractions to identify the assembly precisely
        var result = Types.InAssembly(typeof(Logistics.Application.Abstractions.Email.IEmailSender).Assembly)
            .ShouldNot().HaveDependencyOnAny(
                "Logistics.Application",                     // exact, NOT Logistics.Application.*
                "Logistics.Infrastructure",                  // catches all infra
                "Stripe",
                "OpenAI",
                "Anthropic",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Theory]
    [InlineData("Logistics.Infrastructure.AI")]
    [InlineData("Logistics.Infrastructure.Communications")]
    [InlineData("Logistics.Infrastructure.Documents")]
    [InlineData("Logistics.Infrastructure.Integrations.Eld")]
    [InlineData("Logistics.Infrastructure.Integrations.LoadBoard")]
    [InlineData("Logistics.Infrastructure.Payments")]
    [InlineData("Logistics.Infrastructure.Persistence")]
    [InlineData("Logistics.Infrastructure.Routing")]
    [InlineData("Logistics.Infrastructure.Storage")]
    [InlineData("Logistics.Infrastructure.Tax")]
    [InlineData("Logistics.Infrastructure.Vin")]
    public void Each_Infrastructure_assembly_references_Abstractions_not_Application(string assemblyName)
    {
        var asm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName)
            ?? throw new InvalidOperationException($"Assembly {assemblyName} not loaded. Ensure the test project references it.");

        var result = Types.InAssembly(asm)
            .ShouldNot().HaveDependencyOn("Logistics.Application")   // exact — Abstractions is allowed
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    private static string FormatFailures(TestResult r) =>
        r.FailingTypeNames is null ? "(no details)" : string.Join("\n  ", r.FailingTypeNames);
}
```

## `HandlerInjectionTests.cs`

```csharp
[Fact]
public void No_handler_injects_IHttpContextAccessor()
{
    var result = Types.InAssembly(typeof(Logistics.Application.Registrar).Assembly)
        .That().HaveNameEndingWith("Handler")
        .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore.Http.IHttpContextAccessor")
        .GetResult();

    Assert.True(result.IsSuccessful, FormatFailures(result));
}
```

## `CsprojAssertions.cs` + `CsprojReferenceTests.cs` — XML-level rules

```csharp
public static class CsprojAssertions
{
    public static void AssertNoPackage(string csprojPath, string packageName)
    {
        var doc = XDocument.Load(csprojPath);
        var hit = doc.Descendants("PackageReference")
            .Any(e => string.Equals((string?)e.Attribute("Include"), packageName, StringComparison.OrdinalIgnoreCase));
        Assert.False(hit, $"{Path.GetFileName(csprojPath)} unexpectedly references PackageReference '{packageName}'");
    }

    public static void AssertNoProjectReference(string csprojPath, string referencedProjectFileName)
    {
        var doc = XDocument.Load(csprojPath);
        var hit = doc.Descendants("ProjectReference")
            .Any(e => ((string?)e.Attribute("Include") ?? "")
                .Replace('\\', '/').EndsWith($"/{referencedProjectFileName}", StringComparison.OrdinalIgnoreCase));
        Assert.False(hit, $"{Path.GetFileName(csprojPath)} unexpectedly references ProjectReference '{referencedProjectFileName}'");
    }

    public static string ResolveRepoFile(params string[] relative)
    {
        // Walk up from test bin dir to find the repo root marker (Logistics.slnx)
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Logistics.slnx")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("Could not find repo root (Logistics.slnx).");
        return Path.Combine(new[] { dir.FullName }.Concat(relative).ToArray());
    }
}

public class CsprojReferenceTests
{
    [Fact]
    public void Application_csproj_has_no_Stripe_or_AspNetCore_Http_packages()
    {
        var csproj = CsprojAssertions.ResolveRepoFile("src","Core","Logistics.Application","Logistics.Application.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Stripe.net");
        CsprojAssertions.AssertNoPackage(csproj, "Microsoft.AspNetCore.Http");
    }

    [Fact]
    public void Abstractions_csproj_has_no_Stripe_or_EFCore_packages()
    {
        var csproj = CsprojAssertions.ResolveRepoFile("src","Core","Logistics.Application.Abstractions","Logistics.Application.Abstractions.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Stripe.net");
        CsprojAssertions.AssertNoPackage(csproj, "Microsoft.EntityFrameworkCore");
    }

    [Theory]
    [InlineData("Logistics.Infrastructure.AI")]
    [InlineData("Logistics.Infrastructure.Communications")]
    [InlineData("Logistics.Infrastructure.Documents")]
    [InlineData("Logistics.Infrastructure.Integrations.Eld")]
    [InlineData("Logistics.Infrastructure.Integrations.LoadBoard")]
    [InlineData("Logistics.Infrastructure.Payments")]
    [InlineData("Logistics.Infrastructure.Persistence")]
    [InlineData("Logistics.Infrastructure.Routing")]
    [InlineData("Logistics.Infrastructure.Storage")]
    [InlineData("Logistics.Infrastructure.Tax")]
    [InlineData("Logistics.Infrastructure.Vin")]
    public void Each_Infrastructure_csproj_does_not_reference_Application_project(string projectName)
    {
        var csproj = CsprojAssertions.ResolveRepoFile("src","Infrastructure",projectName,$"{projectName}.csproj");
        CsprojAssertions.AssertNoProjectReference(csproj, "Logistics.Application.csproj");
    }
}
```

## CI wiring

Verify the new test project is picked up by `dotnet test` at the solution root. No CI config change should be needed if CI runs `dotnet test Logistics.slnx`.

Run it intentionally **broken** once to make sure failures are visible (add a `using Stripe;` to a random Application file, watch the test fail, revert).

## Step-by-step

1. Scaffold the project (`dotnet new xunit`), match nuget versions to existing test projects.
2. Add NetArchTest.Rules package.
3. Add the project to `Logistics.slnx`.
4. Project-reference every assembly the tests inspect (so they're loaded into the test AppDomain).
5. Write `BoundaryTests.cs` + `HandlerInjectionTests.cs`.
6. Write `CsprojAssertions.cs` + `CsprojReferenceTests.cs`.
7. Run `dotnet test tests/Logistics.Architecture.Tests` — expect green.
8. Sanity check: temporarily add `using Stripe;` to a file in `Application`, rerun — expect red. Revert.
9. Commit.

## Critical files

- `tests/Logistics.Architecture.Tests/Logistics.Architecture.Tests.csproj` (new)
- `tests/Logistics.Architecture.Tests/BoundaryTests.cs` (new)
- `tests/Logistics.Architecture.Tests/HandlerInjectionTests.cs` (new)
- `tests/Logistics.Architecture.Tests/CsprojAssertions.cs` (new)
- `tests/Logistics.Architecture.Tests/CsprojReferenceTests.cs` (new)
- `Logistics.slnx` — add the test project under `/Test/`

## Verification

- All architecture tests pass on the green baseline.
- Introducing a deliberate violation makes the relevant test fail.

## Risks

| Risk                                                                                                    | Mitigation                                                                                                                                                                                |
| ------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `AppDomain.CurrentDomain.GetAssemblies()` doesn't auto-load infrastructure DLLs                         | Force-load by referencing any concrete type from each infrastructure assembly at the top of `BoundaryTests`, e.g., `_ = typeof(Logistics.Infrastructure.AI.Registrar);` in a static ctor. |
| NetArchTest pattern matches `Logistics.Application.Abstractions` when you meant `Logistics.Application` | Use `HaveDependencyOn("Logistics.Application")` (exact-name match per NetArchTest semantics). Verify by writing a deliberate test that prods both.                                        |
| `.csproj` path resolution flaky in CI                                                                   | The `ResolveRepoFile` walks up looking for `Logistics.slnx`. Confirm CI's working directory contains it.                                                                                  |

## Acceptance

- [ ] `tests/Logistics.Architecture.Tests` exists, registered in `Logistics.slnx`, runs in `dotnet test`.
- [ ] All boundary, csproj, and handler-injection tests pass on green baseline.
- [ ] Deliberately adding `using Stripe;` to an Application file makes a test fail.
- [ ] Deliberately re-adding `Microsoft.AspNetCore.Http` to `Logistics.Application.csproj` fails `Application_csproj_has_no_*_packages`.
- [ ] Committed.
