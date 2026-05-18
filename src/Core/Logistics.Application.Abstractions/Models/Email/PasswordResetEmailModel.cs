namespace Logistics.Application.Abstractions.Models.Email;

public class PasswordResetEmailModel
{
    public string ResetUrl { get; set; } = string.Empty;
}
