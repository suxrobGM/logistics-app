namespace Logistics.Application.Common.Services;

public interface ICaptchaService
{
    Task<bool> VerifyCaptchaAsync(string captchaValue);
}