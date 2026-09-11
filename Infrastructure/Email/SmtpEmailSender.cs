using System.Net;
using System.Net.Mail;
using Application.Abstractions;
using Application.Options;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Email;

/// <summary>
/// Envoi SMTP (compatible Gmail via mot de passe d'application).
/// Si Email:Enabled est faux, l'e-mail est seulement journalisé : pratique en démo/dev.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(EmailOptions options, ILogger<SmtpEmailSender> logger)
    {
        _options = options;
        _logger = logger;
    }

    public async Task SendAsync(string destinataire, string sujet, string corpsHtml, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "[Email désactivé] À: {Destinataire} | Sujet: {Sujet}", destinataire, sujet);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = sujet,
            Body = corpsHtml,
            IsBodyHtml = true
        };
        message.To.Add(destinataire);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.User, _options.Password)
        };

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("E-mail envoyé à {Destinataire} (sujet : {Sujet}).", destinataire, sujet);
    }
}
