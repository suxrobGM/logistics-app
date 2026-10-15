using System.Net;
using System.Text;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Integrations.Accounting;
using Logistics.Infrastructure.Integrations.Accounting.Providers.QuickBooks;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Logistics.Application.Tests.Accounting;

public class QuickBooksOnlineServiceTests
{
    private readonly StubHandler _handler = new();
    private readonly QuickBooksOnlineService _sut;

    public QuickBooksOnlineServiceTests()
    {
        var options = Options.Create(new AccountingOptions
        {
            QuickBooks = new QuickBooksOptions { ClientId = "id", ClientSecret = "secret", RedirectUri = "https://api.test/cb" }
        });
        var http = new HttpClient(_handler);
        var oauth = new QuickBooksOAuthClient(http, options, NullLogger<QuickBooksOAuthClient>.Instance);
        _sut = new QuickBooksOnlineService(http, oauth, options, NullLogger<QuickBooksOnlineService>.Instance);
        _sut.Initialize(new AccountingProviderConfiguration
        {
            ProviderType = AccountingProviderType.QuickBooksOnline,
            RealmId = "9130",
            AccessToken = "token"
        });
    }

    [Fact]
    public async Task UpsertInvoiceAsync_Create_SendsTheRequestId()
    {
        _handler.Respond(HttpStatusCode.OK, """{"Invoice":{"Id":"130","SyncToken":"0"}}""");

        var result = await _sut.UpsertInvoiceAsync(Invoice(), new QboWriteTarget(null, null, "req-1"));

        Assert.Equal("130", result.QboId);
        Assert.Contains("requestid=req-1", _handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task UpsertInvoiceAsync_StaleSyncToken_RereadsAndRetriesOnce()
    {
        _handler.Respond(HttpStatusCode.BadRequest,
            """{"Fault":{"Error":[{"Message":"Stale Object Error","Detail":"Stale Object Error : You and admin were working on this at the same time.","code":"5010"}],"type":"ValidationFault"}}""");
        _handler.Respond(HttpStatusCode.OK, """{"Invoice":{"Id":"130","SyncToken":"7"}}""");
        _handler.Respond(HttpStatusCode.OK, """{"Invoice":{"Id":"130","SyncToken":"8"}}""");

        var result = await _sut.UpsertInvoiceAsync(Invoice(), new QboWriteTarget("130", "2", "req-1"));

        Assert.Equal("8", result.SyncToken);
        Assert.Equal(HttpMethod.Get, _handler.Requests[1].Method);
        Assert.EndsWith("/invoice/130", _handler.Requests[1].RequestUri!.AbsolutePath);
        Assert.Contains("\"SyncToken\":\"7\"", _handler.Bodies[2]);
        Assert.DoesNotContain("requestid", _handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task VoidPaymentAsync_SendsASparseUpdateThatIncludesVoid()
    {
        _handler.Respond(HttpStatusCode.OK, """{"Payment":{"Id":"73","SyncToken":"3"}}""");

        var result = await _sut.VoidPaymentAsync(new QboWriteTarget("73", "2", "req-1"));

        Assert.Equal("3", result.SyncToken);
        Assert.EndsWith("/payment", _handler.Requests[0].RequestUri!.AbsolutePath);
        Assert.Contains("operation=update&include=void", _handler.Requests[0].RequestUri!.Query);
        Assert.Contains("\"sparse\":true", _handler.Bodies[0]);
    }

    [Fact]
    public async Task UpsertInvoiceAsync_ValidationFault_ThrowsWithFaultDetailAndIntuitTid()
    {
        _handler.Respond(HttpStatusCode.BadRequest,
            """{"Fault":{"Error":[{"Message":"Invalid Reference Id","Detail":"Invalid Reference Id : Item assigned to this transaction has been deleted","code":"2500"}],"type":"ValidationFault"}}""",
            intuitTid: "1-abc-tid");

        var ex = await Assert.ThrowsAnyAsync<Exception>(() =>
            _sut.UpsertInvoiceAsync(Invoice(), new QboWriteTarget(null, null, "req-1")));

        Assert.Contains("Item assigned to this transaction has been deleted", ex.Message);
        Assert.Contains("intuit_tid 1-abc-tid", ex.Message);
    }

    [Fact]
    public async Task UpsertInvoiceAsync_Unauthorized_RequiresReconnect()
    {
        _handler.Respond(HttpStatusCode.Unauthorized,
            """{"fault":{"error":[{"message":"message=AuthenticationFailed","detail":"Token revoked","code":"3200"}],"type":"AUTHENTICATION"}}""");

        await Assert.ThrowsAsync<AccountingReconnectRequiredException>(() =>
            _sut.UpsertInvoiceAsync(Invoice(), new QboWriteTarget("130", "1", "req-1")));
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidGrant_RequiresReconnect()
    {
        _handler.Respond(HttpStatusCode.BadRequest, """{"error":"invalid_grant"}""");

        await Assert.ThrowsAsync<AccountingReconnectRequiredException>(() => _sut.RefreshTokenAsync("refresh"));
    }

    [Fact]
    public async Task RefreshTokenAsync_Success_ReturnsRefreshTokenExpiry()
    {
        _handler.Respond(HttpStatusCode.OK,
            """{"access_token":"a","refresh_token":"r","expires_in":3600,"x_refresh_token_expires_in":8726400,"token_type":"bearer"}""");

        var token = await _sut.RefreshTokenAsync("refresh");

        Assert.Equal("r", token.RefreshToken);
        Assert.InRange(token.RefreshTokenExpiresAt!.Value, DateTime.UtcNow.AddDays(100).AddMinutes(-1), DateTime.UtcNow.AddDays(101));
    }

    [Fact]
    public async Task UpsertCustomerAsync_NameExistsInQuickBooks_LinksInsteadOfCreating()
    {
        _handler.Respond(HttpStatusCode.OK, """{"QueryResponse":{"Customer":[{"Id":"58","SyncToken":"3","DisplayName":"O'Neil Freight"}]}}""");
        _handler.Respond(HttpStatusCode.OK, """{"Customer":{"Id":"58","SyncToken":"4"}}""");

        var result = await _sut.UpsertCustomerAsync(
            new QboCustomerPayload { Name = "O'Neil Freight" }, new QboWriteTarget(null, null, "req-1"));

        Assert.Equal("58", result.QboId);
        Assert.Contains("O\\'Neil Freight", Uri.UnescapeDataString(_handler.Requests[0].RequestUri!.Query));
        Assert.Contains("\"Id\":\"58\"", _handler.Bodies[1]);
        Assert.Contains("\"sparse\":true", _handler.Bodies[1]);
        Assert.DoesNotContain("requestid", _handler.Requests[1].RequestUri!.Query);
    }

    private static QboInvoicePayload Invoice() => new()
    {
        QboCustomerId = "42",
        CurrencyCode = "USD",
        TxnDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
        Lines = [new QboInvoiceLinePayload { Description = "Line haul", Amount = 1000m }]
    };

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses = new();
        public List<HttpRequestMessage> Requests { get; } = [];
        public List<string> Bodies { get; } = [];

        public void Respond(HttpStatusCode status, string json, string? intuitTid = null)
        {
            var response = new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            if (intuitTid is not null)
            {
                response.Headers.Add("intuit_tid", intuitTid);
            }

            _responses.Enqueue(response);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Requests.Add(request);
            Bodies.Add(request.Content is null ? "" : await request.Content.ReadAsStringAsync(ct));
            return _responses.Dequeue();
        }
    }
}
