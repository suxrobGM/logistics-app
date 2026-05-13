namespace Logistics.Application.Abstractions.Captcha;

public interface ICaptchaService
{
    Task<bool> VerifyCaptchaAsync(string captchaValue);
}
