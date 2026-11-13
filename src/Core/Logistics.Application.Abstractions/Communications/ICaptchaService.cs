namespace Logistics.Application.Abstractions.Communications;

public interface ICaptchaService
{
    Task<bool> VerifyCaptchaAsync(string captchaValue);
}
