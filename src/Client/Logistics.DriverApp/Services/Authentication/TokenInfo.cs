namespace Logistics.DriverApp.Services.Authentication;

public record TokenInfo(string AccessToken, DateTimeOffset AccessTokenExpiration, string RefreshToken);