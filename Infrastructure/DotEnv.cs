namespace Infrastructure;

/// <summary>
/// Chargeur minimal de fichier .env (aucune dépendance NuGet).
/// Chaque ligne "CLE=valeur" est publiée en variable d'environnement, puis reprise
/// par la configuration .NET (convention "SECTION__CLE" → "Section:Cle").
/// </summary>
public static class DotEnv
{
    public static void Load(string? cheminBase = null)
    {
        // Recherche un .env dans le dossier courant puis en remontant l'arborescence.
        var dir = new DirectoryInfo(cheminBase ?? Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var candidat = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidat))
            {
                Appliquer(candidat);
                return;
            }
            dir = dir.Parent;
        }
    }

    private static void Appliquer(string chemin)
    {
        foreach (var ligneBrute in File.ReadAllLines(chemin))
        {
            var ligne = ligneBrute.Trim();
            if (ligne.Length == 0 || ligne.StartsWith('#'))
            {
                continue;
            }

            var i = ligne.IndexOf('=');
            if (i <= 0)
            {
                continue;
            }

            var cle = ligne[..i].Trim();
            var valeur = ligne[(i + 1)..].Trim().Trim('"');

            // Ne pas écraser une variable déjà définie par l'environnement réel.
            if (Environment.GetEnvironmentVariable(cle) is null)
            {
                Environment.SetEnvironmentVariable(cle, valeur);
            }
        }
    }
}
