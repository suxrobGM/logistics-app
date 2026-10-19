using Logistics.Application.Modules.Platform.Notifications.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class TelegramRecipientsTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly List<TelegramChat> _chats = [];
    private readonly List<Employee> _employees = [];

    public TelegramRecipientsTests()
    {
        var chatRepo = Substitute.For<ITenantRepository<TelegramChat, Guid>>();
        chatRepo.Query().Returns(_ => _chats.BuildMock());
        _tenantUow.Repository<TelegramChat>().Returns(chatRepo);

        var employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();
        employeeRepo.Query().Returns(_ => _employees.BuildMock());
        _tenantUow.Repository<Employee>().Returns(employeeRepo);
    }

    [Theory]
    [InlineData(EmployeeStatus.Terminated)]
    [InlineData(EmployeeStatus.Suspended)]
    [InlineData(EmployeeStatus.OnLeave)]
    public async Task ListAsync_EmployeeNotActive_SkipsTheirChats(EmployeeStatus status)
    {
        var employee = AddEmployee(TenantRoles.Dispatcher, status);
        AddChat(employee.Id, TelegramChatType.Private);
        AddChat(employee.Id, TelegramChatType.Group);

        Assert.Empty(await TelegramRecipients.ListAsync(_tenantUow, null, CancellationToken.None));
    }

    [Fact]
    public async Task ListAsync_EmployeeRemoved_SkipsTheirChats()
    {
        AddChat(Guid.NewGuid(), TelegramChatType.Group);

        Assert.Empty(await TelegramRecipients.ListAsync(_tenantUow, null, CancellationToken.None));
    }

    /// <summary>The role comes from the employee now, so a promoted driver gets dispatcher alerts.</summary>
    [Fact]
    public async Task ListAsync_TargetRole_MatchesPrivateChatsOnTheEmployeesCurrentRole()
    {
        var driver = AddEmployee(TenantRoles.Driver, EmployeeStatus.Active);
        var dispatcher = AddEmployee(TenantRoles.Manager, EmployeeStatus.Active);
        AddChat(driver.Id, TelegramChatType.Private);
        var dispatcherChat = AddChat(dispatcher.Id, TelegramChatType.Private);
        var groupChat = AddChat(driver.Id, TelegramChatType.Group);

        var recipients = await TelegramRecipients.ListAsync(
            _tenantUow, TelegramChatRole.Dispatcher, CancellationToken.None);

        Assert.Equal([dispatcherChat, groupChat], recipients);
    }

    [Fact]
    public async Task ListAsync_NotificationsDisabled_SkipsTheChat()
    {
        var employee = AddEmployee(TenantRoles.Driver, EmployeeStatus.Active);
        AddChat(employee.Id, TelegramChatType.Private).NotificationsEnabled = false;

        Assert.Empty(await TelegramRecipients.ListAsync(_tenantUow, null, CancellationToken.None));
    }

    #region Command authorization

    [Fact]
    public async Task FindAuthorizedChatAsync_EmployeeActive_ReturnsTheChat()
    {
        var employee = AddEmployee(TenantRoles.Driver, EmployeeStatus.Active);
        var chat = AddChat(employee.Id, TelegramChatType.Private);

        Assert.Same(chat, await TelegramRecipients.FindAuthorizedChatAsync(_tenantUow, chat.ChatId, CancellationToken.None));
    }

    [Theory]
    [InlineData(EmployeeStatus.Terminated)]
    [InlineData(EmployeeStatus.Suspended)]
    [InlineData(EmployeeStatus.OnLeave)]
    public async Task FindAuthorizedChatAsync_EmployeeNotActive_ReturnsNull(EmployeeStatus status)
    {
        var employee = AddEmployee(TenantRoles.Dispatcher, status);
        var chat = AddChat(employee.Id, TelegramChatType.Group);

        Assert.Null(await TelegramRecipients.FindAuthorizedChatAsync(_tenantUow, chat.ChatId, CancellationToken.None));
    }

    [Fact]
    public async Task FindAuthorizedChatAsync_EmployeeRemoved_ReturnsNull()
    {
        var chat = AddChat(Guid.NewGuid(), TelegramChatType.Private);

        Assert.Null(await TelegramRecipients.FindAuthorizedChatAsync(_tenantUow, chat.ChatId, CancellationToken.None));
    }

    [Fact]
    public async Task FindAuthorizedChatAsync_ChatRowDeleted_ReturnsNull()
    {
        var employee = AddEmployee(TenantRoles.Driver, EmployeeStatus.Active);
        var chat = AddChat(employee.Id, TelegramChatType.Private);
        _chats.Remove(chat);

        Assert.Null(await TelegramRecipients.FindAuthorizedChatAsync(_tenantUow, chat.ChatId, CancellationToken.None));
    }

    #endregion

    private Employee AddEmployee(string roleName, EmployeeStatus status)
    {
        var employee = new Employee
        {
            Email = "e@example.com",
            FirstName = "E",
            LastName = "Mployee",
            Salary = new Money { Amount = 0, Currency = "USD" },
            Status = status,
            Role = new TenantRole(roleName)
        };
        _employees.Add(employee);
        return employee;
    }

    private TelegramChat AddChat(Guid userId, TelegramChatType type)
    {
        var chat = new TelegramChat { ChatId = _chats.Count + 1, ChatType = type, UserId = userId };
        _chats.Add(chat);
        return chat;
    }
}
