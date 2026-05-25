using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace ECommerceMVC.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings settings;

    public SmtpEmailService(IOptions<SmtpSettings> options)
    {
        settings = options.Value;
    }

    public bool TrySend(string toEmail, string subject, string htmlBody, out string? errorMessage)
    {
        errorMessage = null;
        subject = NormalizeVietnamese(subject);
        htmlBody = NormalizeVietnamese(htmlBody);

        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromEmail))
        {
            errorMessage = "Chưa cấu hình SMTP.";
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(string.IsNullOrWhiteSpace(settings.FromName)
                ? MailboxAddress.Parse(settings.FromEmail)
                : new MailboxAddress(settings.FromName, settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var htmlPart = new TextPart(TextFormat.Html)
            {
                Text = htmlBody
            };
            htmlPart.ContentType.Charset = "utf-8";
            message.Body = htmlPart;

            using var client = new SmtpClient();
            var secureSocket = settings.EnableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            client.Connect(settings.Host, settings.Port, secureSocket);

            if (!string.IsNullOrWhiteSpace(settings.UserName))
            {
                client.Authenticate(settings.UserName, settings.Password);
            }

            client.Send(message);
            client.Disconnect(true);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private static string NormalizeVietnamese(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return input;
        }

        if (input.Contains('Ã') || input.Contains('Ä') || input.Contains('Â') || input.Contains('�'))
        {
            try
            {
                var latin1 = Encoding.GetEncoding("ISO-8859-1");
                var bytes = latin1.GetBytes(input);
                var repaired = Encoding.UTF8.GetString(bytes);
                if (!string.IsNullOrWhiteSpace(repaired))
                {
                    return repaired;
                }
            }
            catch
            {
            }
        }

        return input;
    }
}
