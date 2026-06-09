namespace HWAY.Payroll.Services.Email;

public sealed class PaystubEmailMessage
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string AttachmentPath { get; set; } = string.Empty;
}
