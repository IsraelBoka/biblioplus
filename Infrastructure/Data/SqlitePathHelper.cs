using Microsoft.Data.Sqlite;

namespace Infrastructure.Data;

/// <summary>
/// Assure que l'Api, le Web et l'outil `dotnet ef` pointent tous vers le MÊME fichier
/// SQLite, placé à la racine de la solution (à côté de BiblioPlus.sln).
/// </summary>
public static class SqlitePathHelper
{
    /// <summary>Normalise une chaîne « Data Source=xxx.db » vers un chemin absolu partagé.</summary>
    public static string Normalize(string rawConnectionString)
    {
        var builder = new SqliteConnectionStringBuilder(rawConnectionString);
        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(ResolveSolutionDirectory(), builder.DataSource);
        }
        return builder.ToString();
    }

    private static string ResolveSolutionDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "BiblioPlus.sln")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? Directory.GetCurrentDirectory();
    }
}
