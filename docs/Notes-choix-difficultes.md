# Notes — choix réalisés et difficultés rencontrées

## Choix réalisés

1. **SQLite plutôt que SQL Server.** Le projet est développé sur macOS, où SQL Server ne
   tourne pas nativement. SQLite est fichier-based, sans serveur, et rend la démonstration
   **100 % reproductible** (`dotnet ef database update` suffit). L'API, le Web et `dotnet ef`
   partagent le même fichier grâce à `SqlitePathHelper` (chemin résolu à la racine de la solution).

2. **.NET 7 / EF Core 7** au lieu de .NET 10 / EF 10 : c'est le SDK installé. Aucune
   fonctionnalité du sujet n'en dépend.

3. **`.sln` classique** au lieu de `.slnx` (ce format exige le SDK 9+).

4. **`Result` / `Result<T>` applicatif** (au lieu d'exceptions) pour distinguer explicitement
   réussite et refus métier, avec un `ErreurType` traduit en codes HTTP côté API
   (`Validation` → 400, `Introuvable` → 404, `Conflit` → 409).

5. **Suppression logique centralisée** : le filtre global de requête masque les entités
   supprimées, et l'override de `SaveChanges` convertit tout `Remove` en suppression logique +
   gère `UpdatedAt`. Un seul point de vérité.

6. **Unit of Work** exposant les six repositories autour d'un **seul `DbContext`**, avec un
   unique `SaveChangesAsync()` par cas d'usage réussi (atomicité).

7. **DTO systématiques** côté API ; les vues MVC utilisent un `ViewModel` dédié (messages de
   validation en français) plutôt que l'entité.

## Difficultés rencontrées

1. **Un seul fichier SQLite partagé.** Par défaut, `dotnet run` fixe le répertoire courant sur
   le projet, et `dotnet ef` sur la solution → trois emplacements différents pour `biblioplus.db`.
   Résolu avec `SqlitePathHelper` qui remonte jusqu'au dossier contenant `BiblioPlus.sln`.

2. **Index uniques + suppression logique.** Un index unique classique interdirait de recréer un
   code après suppression. Résolu par un index **filtré** `HasFilter("IsDeleted = 0")`
   (syntaxe SQLite, sans crochets contrairement à SQL Server).

3. **Recherche accent-insensible.** `LIKE` de SQLite ne replie pas les accents ; « etranger »
   ne matche pas « L'Étranger ». Documenté comme limite connue (non bloquant pour le socle).

4. **`RowVersion`/`[Timestamp]`.** Concept SQL Server : SQLite ne l'incrémente pas
   automatiquement. La colonne est conservée pour rester fidèle au modèle, sans impact fonctionnel.

5. **Profils de lancement.** `dotnet run` applique l'`applicationUrl` du profil `launchSettings`,
   qui l'emporte sur `ASPNETCORE_URLS`. Il faut `--launch-profile http` (ou `--no-launch-profile`)
   pour maîtriser le port.
