namespace HWAY.Payroll.Services.Email;

public interface IEmailSettingsService
{
    Task<EmailSettingsDto> GetSettingsAsync();
    Task SaveSettingsAsync(EmailSettingsDto settings);
}
