using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;


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
