using Infrastructure.Data;
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

        // Les repositories, l'IUnitOfWork et les services applicatifs seront ajoutés au Jalon 3.
        return services;
    }
}
