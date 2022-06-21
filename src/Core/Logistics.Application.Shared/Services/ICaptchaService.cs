namespace Logistics.Application.Shared.Services;

public interface ICaptchaService
{
    Task<bool> VerifyCaptchaAsync(string captchaValue);
}