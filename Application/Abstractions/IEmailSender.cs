namespace Application.Abstractions;

/// <summary>
/// Envoi d'e-mails (alertes, confirmations). Implémentation SMTP dans l'Infrastructure.
/// L'envoi ne doit jamais faire échouer un cas d'usage métier : les erreurs sont journalisées.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string destinataire, string sujet, string corpsHtml, CancellationToken ct = default);
}
