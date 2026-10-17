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
public class MarkConversationReadHandlerTests
{
    private static readonly Guid ConversationId = Guid.NewGuid();
    private static readonly Guid ParticipantId = Guid.NewGuid();
    private static readonly Guid OutsiderId = Guid.NewGuid();

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IRealtimeMessagingService _messagingService =
        Substitute.For<IRealtimeMessagingService>();

    private readonly ITenantRepository<Conversation, Guid> _conversationRepo =
        Substitute.For<ITenantRepository<Conversation, Guid>>();
    private readonly ITenantRepository<Message, Guid> _messageRepo =
        Substitute.For<ITenantRepository<Message, Guid>>();
    private readonly ITenantRepository<ConversationParticipant, Guid> _participantRepo =
        Substitute.For<ITenantRepository<ConversationParticipant, Guid>>();
    private readonly ITenantRepository<MessageReadReceipt, Guid> _receiptRepo =
        Substitute.For<ITenantRepository<MessageReadReceipt, Guid>>();

    private readonly ConversationParticipant _participant =
        new() { ConversationId = ConversationId, EmployeeId = ParticipantId };
    private readonly List<Message> _messages = [];
    private readonly MarkConversationReadHandler _sut;

    public MarkConversationReadHandlerTests()
    {
        _tenantUow.Repository<Conversation>().Returns(_conversationRepo);
        _tenantUow.Repository<Message>().Returns(_messageRepo);
        _tenantUow.Repository<ConversationParticipant>().Returns(_participantRepo);
        _tenantUow.Repository<MessageReadReceipt>().Returns(_receiptRepo);

        Conversation(isTenantChat: false);

        // Run the handler's real predicates against the rows, so a wrong field fails here.
        var participants = new List<ConversationParticipant> { _participant };
        _participantRepo
            .GetAsync(Arg.Any<Expression<Func<ConversationParticipant, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(call => participants.AsQueryable()
                .FirstOrDefault(call.Arg<Expression<Func<ConversationParticipant, bool>>>()));

        _messageRepo
            .GetListAsync(Arg.Any<Expression<Func<Message, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(call => _messages.AsQueryable()
                .Where(call.Arg<Expression<Func<Message, bool>>>())
                .ToList());

        _sut = new MarkConversationReadHandler(_tenantUow, _messagingService);
    }

    private void Conversation(bool isTenantChat) =>
        _conversationRepo.GetByIdAsync(ConversationId, Arg.Any<CancellationToken>())
            .Returns(new Conversation { Id = ConversationId, IsTenantChat = isTenantChat });

    private Message AddMessage(Guid senderId)
    {
        var message = Message.Create(ConversationId, senderId, "hello");
        _messages.Add(message);
        return message;
    }

    private Task<Result> Handle(Guid readById) =>
        _sut.Handle(new MarkConversationReadCommand { ConversationId = ConversationId, ReadById = readById },
            CancellationToken.None);

    [Fact]
    public async Task Handle_CallerIsNotAParticipant_FailsWithoutWritingOrBroadcasting()
    {
        AddMessage(Guid.NewGuid());

        var result = await Handle(OutsiderId);

        Assert.False(result.IsSuccess);
        await _receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _messagingService.DidNotReceive().BroadcastMessageReadAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CallerIsAParticipant_ReceiptsUnreadMessagesAndMovesLastReadAt()
    {
        var message = AddMessage(Guid.NewGuid());

        var result = await Handle(ParticipantId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(_participant.LastReadAt);
        await _receiptRepo.Received(1).AddAsync(
            Arg.Is<MessageReadReceipt>(r => r.MessageId == message.Id && r.ReadById == ParticipantId),
            Arg.Any<CancellationToken>());
        await _messagingService.Received(1).BroadcastMessageReadAsync(
            ConversationId, message.Id, ParticipantId.ToString(), Arg.Any<CancellationToken>());
    }

    // The badge counts by LastReadAt, so it must move even when another participant read everything first.
    [Fact]
    public async Task Handle_MessagesAlreadyReadByCaller_StillMovesLastReadAt()
    {
        var message = AddMessage(Guid.NewGuid());
        message.ReadReceipts.Add(new MessageReadReceipt { MessageId = message.Id, ReadById = ParticipantId });

        var result = await Handle(ParticipantId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(_participant.LastReadAt);
        await _receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnMessages_AreNotReceipted()
    {
        AddMessage(ParticipantId);

        var result = await Handle(ParticipantId);

        Assert.True(result.IsSuccess);
        await _receiptRepo.DidNotReceive().AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }

    // Tenant chat has no participant row until someone posts, so it carves out of the gate.
    [Fact]
    public async Task Handle_TenantChatWithNoParticipantRow_IsStillAllowed()
    {
        Conversation(isTenantChat: true);
        AddMessage(Guid.NewGuid());

        var result = await Handle(OutsiderId);

        Assert.True(result.IsSuccess);
        await _receiptRepo.Received(1).AddAsync(
            Arg.Any<MessageReadReceipt>(), Arg.Any<CancellationToken>());
    }
}
