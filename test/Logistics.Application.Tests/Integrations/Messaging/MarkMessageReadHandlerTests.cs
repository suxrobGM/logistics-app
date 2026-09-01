using System.Linq.Expressions;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Integrations.Messaging.Commands;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.Messaging;

/// <summary>
/// The handler wrote a read receipt for whatever message id it was handed, with no check that the
/// caller belonged to that message's conversation. <c>SendMessageHandler</c> already gated the send
/// side this way; this is the read side.
/// </summary>
public class MarkMessageReadHandlerTests
{
    private static readonly Guid ConversationId = Guid.NewGuid();
    private static readonly Guid MessageId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();
    private static readonly Guid OutsiderId = Guid.NewGuid();

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IRealtimeMessagingService messagingService =
        Substitute.For<IRealtimeMessagingService>();

    private readonly ITenantRepository<Message, Guid> messageRepo =
        Substitute.For<ITenantRepository<Message, Guid>>();
    private readonly ITenantRepository<Conversation, Guid> conversationRepo =
        Substitute.For<ITenantRepository<Conversation, Guid>>();
    private readonly ITenantRepository<ConversationParticipant, Guid> participantRepo =
        Substitute.For<ITenantRepository<ConversationParticipant, Guid>>();
    private readonly ITenantRepository<MessageReadReceipt, Guid> receiptRepo =
        Substitute.For<ITenantRepository<MessageReadReceipt, Guid>>();

    private readonly MarkMessageReadHandler sut;

    public MarkMessageReadHandlerTests()
    {
        tenantUow.Repository<Message>().Returns(messageRepo);
        tenantUow.Repository<Conversation>().Returns(conversationRepo);
        tenantUow.Repository<ConversationParticipant>().Returns(participantRepo);
        tenantUow.Repository<MessageReadReceipt>().Returns(receiptRepo);

        messageRepo.GetByIdAsync(MessageId, Arg.Any<CancellationToken>())
            .Returns(Message.Create(ConversationId, Guid.NewGuid(), "hello"));

        Conversation(isTenantChat: false);

        // Run the handler's real predicate against the rows that exist rather than stubbing the
        // answer, so a predicate filtering on the wrong field would fail here.
        var participants = new List<ConversationParticipant>
        {
            new() { ConversationId = ConversationId, EmployeeId = ParticipantId }
        };
        participantRepo
            .GetAsync(Arg.Any<Expression<Func<ConversationParticipant, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call => participants.AsQueryable()
                .FirstOrDefault(call.Arg<Expression<Func<ConversationParticipant, bool>>>()));

        receiptRepo
            .GetAsync(Arg.Any<Expression<Func<MessageReadReceipt, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((MessageReadReceipt?)null);

        sut = new MarkMessageReadHandler(tenantUow, messagingService);
    }

    private void Conversation(bool isTenantChat) =>
        conversationRepo.GetByIdAsync(ConversationId, Arg.Any<CancellationToken>())
            .Returns(new Conversation { Id = ConversationId, IsTenantChat = isTenantChat });

    private Task<Logistics.Shared.Models.Result> Handle(Guid readById) =>
        sut.Handle(new MarkMessageReadCommand { MessageId = MessageId, ReadById = readById },
            CancellationToken.None);

    [Fact]
    public async Task Handle_CallerIsNotAParticipant_FailsAndWritesNothing()
    {
        var result = await Handle(OutsiderId);

        Assert.False(result.IsSuccess);
        await receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallerIsNotAParticipant_DoesNotBroadcast()
    {
        await Handle(OutsiderId);

        await messagingService.DidNotReceive().BroadcastMessageReadAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallerIsAParticipant_WritesTheReceiptAndBroadcasts()
    {
        var result = await Handle(ParticipantId);

        Assert.True(result.IsSuccess);
        await receiptRepo.Received(1).AddAsync(
            Arg.Is<MessageReadReceipt>(r => r.MessageId == MessageId && r.ReadById == ParticipantId),
            Arg.Any<CancellationToken>());
        await messagingService.Received(1).BroadcastMessageReadAsync(
            ConversationId, MessageId, ParticipantId.ToString(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tenant chat has no participant row until someone first posts, so gating on one would break
    /// marking a team-chat message read. Matches <c>ConversationAccess</c>'s carve-out.
    /// </summary>
    [Fact]
    public async Task Handle_TenantChatWithNoParticipantRow_IsStillAllowed()
    {
        Conversation(isTenantChat: true);

        var result = await Handle(OutsiderId);

        Assert.True(result.IsSuccess);
        await receiptRepo.Received(1).AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MessageNotFound_Fails()
    {
        messageRepo.GetByIdAsync(MessageId, Arg.Any<CancellationToken>()).Returns((Message?)null);

        Assert.False((await Handle(ParticipantId)).IsSuccess);
    }

    [Fact]
    public async Task Handle_ConversationNotFound_Fails()
    {
        conversationRepo.GetByIdAsync(ConversationId, Arg.Any<CancellationToken>())
            .Returns((Conversation?)null);

        var result = await Handle(ParticipantId);

        Assert.False(result.IsSuccess);
        await receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Re-marking must stay idempotent: a client can re-send after a reconnect.
    /// </summary>
    [Fact]
    public async Task Handle_AlreadyMarkedRead_SucceedsWithoutWritingASecondReceipt()
    {
        receiptRepo
            .GetAsync(Arg.Any<Expression<Func<MessageReadReceipt, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(new MessageReadReceipt { MessageId = MessageId, ReadById = ParticipantId });

        var result = await Handle(ParticipantId);

        Assert.True(result.IsSuccess);
        await receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }
}
