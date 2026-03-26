namespace Logistics.Application.Contracts.Models.Email;

public class PasswordResetEmailModel
{
    public string ResetUrl { get; set; } = string.Empty;
}
