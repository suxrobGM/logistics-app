using System.Linq.Expressions;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Integrations.Messaging.Commands;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.Messaging;

/// <summary>
/// The read side of the participant gate <c>SendMessageHandler</c> already applies on send.
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
    private readonly ITenantRepository<ConversationParticipant, Guid> participantRepo =
        Substitute.For<ITenantRepository<ConversationParticipant, Guid>>();
    private readonly ITenantRepository<MessageReadReceipt, Guid> receiptRepo =
        Substitute.For<ITenantRepository<MessageReadReceipt, Guid>>();

    private readonly Message message = Message.Create(ConversationId, Guid.NewGuid(), "hello");
    private readonly MarkMessageReadHandler sut;

    public MarkMessageReadHandlerTests()
    {
        tenantUow.Repository<Message>().Returns(messageRepo);
        tenantUow.Repository<ConversationParticipant>().Returns(participantRepo);
        tenantUow.Repository<MessageReadReceipt>().Returns(receiptRepo);

        messageRepo.GetByIdAsync(MessageId, Arg.Any<CancellationToken>()).Returns(message);

        Conversation(isTenantChat: false);

        // Run the handler's real predicate against the rows, so a wrong field fails here.
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
        message.Conversation = new Conversation { Id = ConversationId, IsTenantChat = isTenantChat };

    private Task<Result> Handle(Guid readById) =>
        sut.Handle(new MarkMessageReadCommand { MessageId = MessageId, ReadById = readById },
            CancellationToken.None);

    [Fact]
    public async Task Handle_CallerIsNotAParticipant_FailsWithoutWritingOrBroadcasting()
    {
        var result = await Handle(OutsiderId);

        Assert.False(result.IsSuccess);
        await receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
        await tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
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

    // Tenant chat has no participant row until someone posts, so it carves out of the gate.
    [Fact]
    public async Task Handle_TenantChatWithNoParticipantRow_IsStillAllowed()
    {
        Conversation(isTenantChat: true);

        var result = await Handle(OutsiderId);

        Assert.True(result.IsSuccess);
        await receiptRepo.Received(1).AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }

    // Clients re-send after a reconnect, so re-marking stays idempotent.
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
