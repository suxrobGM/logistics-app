using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class NegotiationEmailComposerTests
{
    private readonly IEmailTemplateService emailTemplateService = Substitute.For<IEmailTemplateService>();
    private readonly NegotiationEmailComposer sut;
    private BrokerCounterOfferEmailModel? lastRenderedModel;

    public NegotiationEmailComposerTests()
    {
        emailTemplateService
            .RenderAsync("BrokerCounterOffer", Arg.Do<BrokerCounterOfferEmailModel>(m => lastRenderedModel = m))
            .Returns("<html>rendered</html>");

        sut = new NegotiationEmailComposer(emailTemplateService);
    }

    private static ComposeNegotiationEmailRequest Request(string agentMessage) => new()
    {
        OriginCity = "Chicago",
        OriginState = "IL",
        DestinationCity = "Dallas",
        DestinationState = "TX",
        PickupDate = new DateTime(2026, 9, 1),
        EquipmentType = "Dry Van",
        OfferAmount = 2150m,
        Currency = "USD",
        OfferPerMile = 2.15m,
        AgentMessage = agentMessage,
        CompanyName = "Acme Trucking",
        CompanyMcNumber = "123456",
        ThreadReference = "RN-9F2A",
        ReplyToAddress = "offer-abc123@reply.logisticsx.app"
    };

    #region Sanitization

    [Fact]
    public async Task ComposeAsync_MessageWithHtmlTags_StripsTags()
    {
        var result = await sut.ComposeAsync(Request("<b>Hi</b> we can do <i>$2,150</i> total."));

        Assert.NotNull(lastRenderedModel);
        Assert.DoesNotContain('<', lastRenderedModel!.Message);
        Assert.DoesNotContain('>', lastRenderedModel.Message);
        Assert.Equal("Hi we can do $2,150 total.", lastRenderedModel.Message);
        Assert.Equal("<html>rendered</html>", result.HtmlBody);
    }

    [Fact]
    public async Task ComposeAsync_MessageWithControlCharsAndExtraWhitespace_CollapsesToSingleSpaces()
    {
        var raw = "Hi there,\n\n\tthis   works?";

        await sut.ComposeAsync(Request(raw));

        Assert.Equal("Hi there, this works?", lastRenderedModel!.Message);
    }

    [Fact]
    public async Task ComposeAsync_MessageOverMaxLength_ClampsToWholeWordAt800Chars()
    {
        var word = "lorem ";
        var longMessage = string.Concat(Enumerable.Repeat(word, 200));

        await sut.ComposeAsync(Request(longMessage));

        var message = lastRenderedModel!.Message;
        Assert.True(message.Length <= 803);
        Assert.EndsWith("...", message);
        Assert.DoesNotContain("  ", message.Replace("...", string.Empty));
    }

    [Fact]
    public async Task ComposeAsync_MessageUnderMaxLength_IsNotTruncated()
    {
        const string message = "We can move at this rate if pickup stays Monday.";

        await sut.ComposeAsync(Request(message));

        Assert.Equal(message, lastRenderedModel!.Message);
        Assert.DoesNotContain("...", lastRenderedModel.Message);
    }

    #endregion

    #region Subject and reply address

    [Fact]
    public async Task ComposeAsync_BuildsSubjectWithOriginDestinationAndReference()
    {
        var result = await sut.ComposeAsync(Request("Sounds good."));

        Assert.Equal("Rate offer: Chicago, IL -> Dallas, TX - RN-9F2A", result.Subject);
    }

    [Fact]
    public async Task ComposeAsync_PassesThroughCallerSuppliedReplyAddress()
    {
        var result = await sut.ComposeAsync(Request("Sounds good."));

        Assert.Equal("offer-abc123@reply.logisticsx.app", result.ReplyToAddress);
    }

    #endregion
}
