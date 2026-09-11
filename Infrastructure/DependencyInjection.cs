using Application.Abstractions;
using Application.Options;
using Application.Services;
using Infrastructure.Data;
using Infrastructure.Email;
using Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Point d'entrée d'enregistrement de la couche d'infrastructure, partagé par l'Api et le Web.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddBiblioPlusPersistence(
        this IServiceCollection services, IConfiguration configuration)
    {
        var raw = configuration.GetConnectionString("BiblioPlus") ?? "Data Source=biblioplus.db";
        var connectionString = SqlitePathHelper.Normalize(raw);

        // Un DbContext par requête (portée Scoped par défaut d'AddDbContext).
        services.AddDbContext<BiblioPlusContext>(options => options.UseSqlite(connectionString));

        // Unit of Work (partage le DbContext de la requête) et service métier.
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICirculationService, CirculationService>();

        // Sécurité : hachage des mots de passe (PBKDF2, sans état → Singleton).
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        // E-mail : options liées à la section "Email" (alimentée par le .env) + expéditeur SMTP.
        var section = configuration.GetSection(EmailOptions.SectionName);
        var defauts = new EmailOptions();
        var emailOptions = new EmailOptions
        {
            Enabled = bool.TryParse(section["Enabled"], out var en) && en,
            Host = string.IsNullOrWhiteSpace(section["Host"]) ? defauts.Host : section["Host"]!,
            Port = int.TryParse(section["Port"], out var port) ? port : defauts.Port,
            User = section["User"] ?? string.Empty,
            Password = section["Password"] ?? string.Empty,
            From = string.IsNullOrWhiteSpace(section["From"]) ? defauts.From : section["From"]!,
            EnableSsl = !bool.TryParse(section["EnableSsl"], out var ssl) || ssl
        };
        services.AddSingleton(emailOptions);
        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
