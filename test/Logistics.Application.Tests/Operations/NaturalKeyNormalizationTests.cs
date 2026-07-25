using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations;

/// <summary>
/// Both keys carry a case-sensitive unique index, so "mscu1234567" and "MSCU1234567" used to be two
/// rows and lookups picked one arbitrarily. Canonicalising in the setter is what closes it, so no
/// writer - handler, seeder or importer - can bypass it.
/// </summary>
public class NaturalKeyNormalizationTests
{
    private static Container NewContainer(string number) =>
        new() { Number = number, IsoType = ContainerIsoType.Gp20 };

    private static Terminal NewTerminal(string code) => new()
    {
        Name = "Test",
        Code = code,
        CountryCode = "US",
        Type = TerminalType.SeaPort,
        Address = new Address
        {
            Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US"
        }
    };

    [Theory]
    [InlineData("mscu1234567", "MSCU1234567")]
    [InlineData("  MSCU1234567  ", "MSCU1234567")]
    [InlineData("MsCu1234567", "MSCU1234567")]
    [InlineData("MSCU1234567", "MSCU1234567")]
    public void ContainerNumber_IsCanonicalisedOnWrite(string input, string expected)
    {
        Assert.Equal(expected, NewContainer(input).Number);
    }

    [Theory]
    [InlineData("uslax", "USLAX")]
    [InlineData(" uslax ", "USLAX")]
    [InlineData("UsLaX", "USLAX")]
    public void TerminalCode_IsCanonicalisedOnWrite(string input, string expected)
    {
        Assert.Equal(expected, NewTerminal(input).Code);
    }

    /// <summary>A later assignment must normalise too, not just the initialiser.</summary>
    [Fact]
    public void ReassigningTheKey_NormalisesAgain()
    {
        var container = NewContainer("MSCU1234567");
        container.Number = " tclu7654321 ";
        Assert.Equal("TCLU7654321", container.Number);

        var terminal = NewTerminal("USLAX");
        terminal.Code = " deham ";
        Assert.Equal("DEHAM", terminal.Code);
    }

    /// <summary>
    /// Two spellings now compare equal, so the handlers' uniqueness check finds the conflict.
    /// </summary>
    [Fact]
    public void TwoSpellingsOfTheSameKey_CollapseToOneValue()
    {
        Assert.Equal(NewContainer("mscu1234567").Number, NewContainer("MSCU1234567").Number);
        Assert.Equal(NewTerminal("uslax").Code, NewTerminal("USLAX").Code);
    }

    /// <summary>
    /// Search terms go through the same helper - the only reason the tools can drop `upper()` from
    /// the WHERE clause and still match.
    /// </summary>
    [Fact]
    public void NormalizeHelpers_MatchWhatTheSetterStores()
    {
        Assert.Equal(NewContainer("mscu1234567").Number, Container.NormalizeNumber("mscu1234567"));
        Assert.Equal(NewTerminal("uslax").Code, Terminal.NormalizeCode("uslax"));
        Assert.Equal("", Container.NormalizeNumber(null));
        Assert.Equal("", Terminal.NormalizeCode(null));
    }
}
