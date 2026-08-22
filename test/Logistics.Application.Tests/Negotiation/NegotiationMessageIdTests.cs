using Logistics.Application.Modules.Integrations.Negotiation;
using Xunit;

namespace Logistics.Application.Tests.Negotiation;

public class NegotiationMessageIdTests
{
    [Fact]
    public void Create_ReplyDomain_ReturnsBracketedMsgIdOnThatDomain()
    {
        var messageId = NegotiationMessageId.Create("mail.test.com");

        Assert.Matches(@"^<[^<>@\s]+@mail\.test\.com>$", messageId);
    }

    [Fact]
    public void Create_CalledTwice_ReturnsDifferentIds()
    {
        var sut = () => NegotiationMessageId.Create("mail.test.com");

        Assert.NotEqual(sut(), sut());
    }
}
