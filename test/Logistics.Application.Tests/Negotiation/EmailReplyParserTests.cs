using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class EmailReplyParserTests
{
    [Fact]
    public void Strip_OnWroteQuoteBlock_KeepsOnlyTheReply()
    {
        var body = """
            We can do 2100, not 2200.

            On Mon, 3 Mar 2026 at 09:12, Dispatch <dispatch@carrier.com> wrote:
            > Our offer is 2200 all in.
            > Let us know.
            """;

        Assert.Equal("We can do 2100, not 2200.", EmailReplyParser.Strip(body));
    }

    [Fact]
    public void Strip_OriginalMessageSeparator_CutsThere()
    {
        var body = """
            2100 works.

            -----Original Message-----
            From: dispatch@carrier.com
            Our offer is 2200.
            """;

        Assert.Equal("2100 works.", EmailReplyParser.Strip(body));
    }

    [Fact]
    public void Strip_SignatureDelimiter_CutsThere()
    {
        var body = """
            Agreed at 2200.

            --
            Pat Broker
            Freight Co
            """;

        Assert.Equal("Agreed at 2200.", EmailReplyParser.Strip(body));
    }

    [Fact]
    public void Strip_MobileFooter_CutsThere()
    {
        var body = "Sounds good.\n\nSent from my iPhone";

        Assert.Equal("Sounds good.", EmailReplyParser.Strip(body));
    }

    [Fact]
    public void Strip_QuotedLinesOnly_FallsBackToTheWholeBody()
    {
        var body = "> Our offer is 2200 all in.";

        Assert.Equal(body, EmailReplyParser.Strip(body));
    }

    [Fact]
    public void Strip_LongBody_IsClamped()
    {
        var body = new string('a', 9000);

        var result = EmailReplyParser.Strip(body);

        Assert.True(result.Length < 9000);
        Assert.EndsWith("...", result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Strip_EmptyBody_ReturnsEmpty(string body)
    {
        Assert.Equal("", EmailReplyParser.Strip(body));
    }
}
