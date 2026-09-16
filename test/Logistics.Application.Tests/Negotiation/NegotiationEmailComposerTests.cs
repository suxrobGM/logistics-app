using Logistics.Application.Abstractions.Email;
using Logistics.Application.Abstractions.Email.Models;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class NegotiationEmailComposerTests
{
    private readonly IEmailTemplateService _emailTemplateService = Substitute.For<IEmailTemplateService>();
    private readonly NegotiationEmailComposer _sut;
    private BrokerCounterOfferEmailModel? _lastRenderedModel;

    public NegotiationEmailComposerTests()
    {
        _emailTemplateService
            .RenderAsync("BrokerCounterOffer", Arg.Do<BrokerCounterOfferEmailModel>(m => _lastRenderedModel = m))
            .Returns("<html>rendered</html>");

        _sut = new NegotiationEmailComposer(_emailTemplateService);
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
        var result = await _sut.ComposeAsync(Request("<b>Hi</b> we can do <i>$2,150</i> total."));

        Assert.NotNull(_lastRenderedModel);
        Assert.DoesNotContain('<', _lastRenderedModel!.Message);
        Assert.DoesNotContain('>', _lastRenderedModel.Message);
        Assert.Equal("Hi we can do $2,150 total.", _lastRenderedModel.Message);
        Assert.Equal("<html>rendered</html>", result.HtmlBody);
    }

    [Fact]
    public async Task ComposeAsync_MessageWithControlCharsAndExtraWhitespace_CollapsesToSingleSpaces()
    {
        var raw = "Hi there,\n\n\tthis   works?";

        await _sut.ComposeAsync(Request(raw));

        Assert.Equal("Hi there, this works?", _lastRenderedModel!.Message);
    }

    [Fact]
    public async Task ComposeAsync_MessageOverMaxLength_ClampsToWholeWordAt800Chars()
    {
        var word = "lorem ";
        var longMessage = string.Concat(Enumerable.Repeat(word, 200));

        await _sut.ComposeAsync(Request(longMessage));

        var message = _lastRenderedModel!.Message;
        Assert.True(message.Length <= 803);
        Assert.EndsWith("...", message);
        Assert.DoesNotContain("  ", message.Replace("...", string.Empty));
    }

    [Fact]
    public async Task ComposeAsync_MessageUnderMaxLength_IsNotTruncated()
    {
        const string message = "We can move at this rate if pickup stays Monday.";

        await _sut.ComposeAsync(Request(message));

        Assert.Equal(message, _lastRenderedModel!.Message);
        Assert.DoesNotContain("...", _lastRenderedModel.Message);
    }

    #endregion

    #region Subject and reply address

    [Fact]
    public async Task ComposeAsync_BuildsSubjectWithOriginDestinationAndReference()
    {
        var result = await _sut.ComposeAsync(Request("Sounds good."));

        Assert.Equal("Rate offer: Chicago, IL -> Dallas, TX - RN-9F2A", result.Subject);
    }

    #endregion
}
