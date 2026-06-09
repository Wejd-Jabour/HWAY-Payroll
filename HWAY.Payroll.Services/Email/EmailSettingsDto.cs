namespace HWAY.Payroll.Services.Email;

public sealed class EmailSettingsDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SmtpHost)
        && SmtpPort > 0
        && !string.IsNullOrWhiteSpace(SenderEmail)
        && !string.IsNullOrWhiteSpace(Username)
        && HasPassword;
}
