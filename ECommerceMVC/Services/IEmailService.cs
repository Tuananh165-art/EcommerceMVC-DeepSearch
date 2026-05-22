namespace ECommerceMVC.Services;

public interface IEmailService
{
    bool TrySend(string toEmail, string subject, string htmlBody, out string? errorMessage);
}