namespace Logistics.Application.Abstractions.Communications.EmailModels;

public class PasswordResetEmailModel
{
    public string ResetUrl { get; set; } = string.Empty;
}
