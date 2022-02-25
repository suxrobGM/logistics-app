namespace Logistics.Application.Options;

public class EmailSenderOptions
{
    public string? SenderAddress { get; set; }
    public string? SenderDisplayName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; } = 537;
}
