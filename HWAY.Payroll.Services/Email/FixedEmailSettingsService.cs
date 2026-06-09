namespace HWAY.Payroll.Services.Email;

public sealed class FixedEmailSettingsService : IEmailSettingsService
{
    private const string SmtpHost = "smtp.office365.com";
    private const int SmtpPort = 587;
    private const bool EnableSsl = true;
    private const string SenderEmail = "payroll@h-wayaccounting.com";
    private const string SenderName = "HWAY Payroll";
    private const string Username = "payroll@h-wayaccounting.com";
    private const string Password = "";

    public Task<EmailSettingsDto> GetSettingsAsync()
    {
        return Task.FromResult(new EmailSettingsDto
        {
            SmtpHost = SmtpHost,
            SmtpPort = SmtpPort,
            EnableSsl = EnableSsl,
            SenderEmail = SenderEmail,
            SenderName = SenderName,
            Username = Username,
            Password = Password
        });
    }

    public Task SaveSettingsAsync(EmailSettingsDto settings)
    {
        throw new InvalidOperationException("Email settings are fixed in the application.");
    }
}
