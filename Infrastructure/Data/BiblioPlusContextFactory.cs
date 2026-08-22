using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

/// <summary>
/// Factory utilisée uniquement au design-time par `dotnet ef` (migrations).
/// Évite de démarrer tout l'hôte web pour générer/appliquer une migration.
/// </summary>
public class BiblioPlusContextFactory : IDesignTimeDbContextFactory<BiblioPlusContext>
{
    public BiblioPlusContext CreateDbContext(string[] args)
    {
        var connectionString = SqlitePathHelper.Normalize("Data Source=biblioplus.db");

        var options = new DbContextOptionsBuilder<BiblioPlusContext>()
            .UseSqlite(connectionString)
            .Options;

        return new BiblioPlusContext(options);
    }
}
