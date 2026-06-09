using System.Net;
using System.Net.Mail;
using System.Net.Mime;

namespace HWAY.Payroll.Services.Email;

public sealed class SmtpPaystubEmailService : IPaystubEmailService
{
    private readonly IEmailSettingsService _emailSettingsService;

    public SmtpPaystubEmailService(IEmailSettingsService emailSettingsService)
    {
        _emailSettingsService = emailSettingsService;
    }

    public async Task SendPaystubAsync(PaystubEmailMessage message)
    {
        var settings = await _emailSettingsService.GetSettingsAsync();

        if (!settings.IsConfigured)
            throw new InvalidOperationException("Email settings are incomplete.");

        if (!File.Exists(message.AttachmentPath))
            throw new FileNotFoundException("Paystub PDF was not found.", message.AttachmentPath);

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(settings.SenderEmail, GetSenderDisplayName(settings)),
            Subject = message.Subject,
            Body = message.Body
        };

        mailMessage.To.Add(new MailAddress(message.RecipientEmail, message.RecipientName));
        mailMessage.Attachments.Add(new Attachment(message.AttachmentPath, MediaTypeNames.Application.Pdf));

        using var smtpClient = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Credentials = new NetworkCredential(settings.Username, settings.Password)
        };

        await smtpClient.SendMailAsync(mailMessage);
    }

    private static string GetSenderDisplayName(EmailSettingsDto settings) =>
        string.IsNullOrWhiteSpace(settings.SenderName)
            ? settings.SenderEmail
            : settings.SenderName;
}
