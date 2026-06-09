namespace HWAY.Payroll.Services.Email;

public interface IPaystubEmailService
{
    Task SendPaystubAsync(PaystubEmailMessage message);
}
