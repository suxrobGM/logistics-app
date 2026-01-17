namespace Logistics.Infrastructure.Services.Email;

public class SmtpOptions
{
    public string? SenderEmail { get; set; }
    public string? SenderName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
}
