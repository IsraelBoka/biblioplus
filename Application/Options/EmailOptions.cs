namespace Application.Options;

/// <summary>
/// Paramètres SMTP (renseignés via le fichier .env → section de configuration "Email").
/// Tant que <see cref="Enabled"/> est faux, l'envoi est simplement journalisé (aucun mail réel).
/// </summary>
public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = "BiblioPlus <no-reply@biblioplus.local>";
    public bool EnableSsl { get; set; } = true;
}
