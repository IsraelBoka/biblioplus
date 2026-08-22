# BiblioPlus

Gestion d'une bibliothèque : exemplaires physiques, emprunts, retours et pénalités de retard.
Projet individuel réalisé en **architecture en couches** (Domain / Application / Infrastructure /
Web MVC / API REST) avec **ASP.NET Core** et **Entity Framework Core**.

- **Auteur** : Israël Boka *(à ajuster si besoin)*
- **Sujet** : BiblioPlus (voir l'énoncé et `docs/conception.md`)

---

## 1. Prérequis et versions

| Outil | Version utilisée |
|---|---|
| SDK .NET | **7.0** |
| Entity Framework Core | **7.0.20** (provider **SQLite**) |
| Outil `dotnet-ef` | 7.0.x (`dotnet tool install --global dotnet-ef --version 7.0.20`) |
| Base de données | **SQLite** (fichier local, aucune installation serveur) |

> **Note d'adaptation.** L'énoncé visait Windows + .NET 10 + SQL Server. Le projet a été
> réalisé sur **macOS avec .NET 7 et SQLite** (natif, zéro infrastructure, 100 % reproductible).
> L'architecture, les règles métier et les contrats HTTP sont identiques à l'énoncé.

---

## 2. Architecture

```
BiblioPlus/
├── Domain/          # Entités, enums, règles intrinsèques (aucune dépendance)
├── Application/     # IRepository, IUnitOfWork, contrats, CirculationService
├── Infrastructure/  # DbContext, EF Core, migrations, repositories, UnitOfWork
├── Web/             # ASP.NET Core MVC + vues Razor (référentiel CategorieLivre)
├── Api/             # API REST, DTO, OpenAPI, fichier .http
└── docs/            # conception, règles métier, diagramme
```

Sens des références : `Application → Domain` ; `Infrastructure → Application + Domain` ;
`Web`/`Api → Application + Domain + Infrastructure`.

Diagramme du modèle : `docs/base_de_donnees.png` (et `docs/conception.md`).

---

## 3. Connexion et base de données

Chaîne de connexion (identique dans `Api/appsettings.json` et `Web/appsettings.json`) :

```json
"ConnectionStrings": { "BiblioPlus": "Data Source=biblioplus.db" }
```

Le fichier `biblioplus.db` est créé **à la racine de la solution** et partagé par l'Api, le Web
et `dotnet ef` (via `SqlitePathHelper`). Il n'est **pas** versionné (voir `.gitignore`).

### Migrations

La base et le schéma sont créés automatiquement au démarrage (l'Api et le Web appellent
`Database.Migrate()` + un seed minimal). Pour le faire manuellement :

```bash
# (re)générer la migration initiale
dotnet ef migrations add InitialCreate \
  --project Infrastructure/Infrastructure.csproj \
  --startup-project Api/Api.csproj \
  --context BiblioPlusContext \
  --output-dir Data/Migrations

# appliquer / créer la base
dotnet ef database update \
  --project Infrastructure/Infrastructure.csproj \
  --startup-project Api/Api.csproj \
  --context BiblioPlusContext
```

Données de démonstration semées : 2 catégories (`ROM`, `BD`), 1 livre (« L'Étranger »),
2 exemplaires, 1 adhérent (`ADH-0001`).

---

## 4. Lancement

```bash
dotnet restore
dotnet build BiblioPlus.sln

# API — Swagger / OpenAPI sur /swagger   (http://localhost:5188)
dotnet run --project Api/Api.csproj --launch-profile http

# Web MVC — référentiel sur /CategoriesLivres   (http://localhost:5033)
dotnet run --project Web/Web.csproj --launch-profile http
```

---

## 5. Routes

### Référentiel `CategorieLivre`
```
GET    /api/categories-livres
GET    /api/categories-livres/{id}
POST   /api/categories-livres        -> 201 + Location
PUT    /api/categories-livres/{id}   -> 204
DELETE /api/categories-livres/{id}   -> 204 (suppression logique)
```

### Routes métier
```
GET   /api/livres/disponibles?recherche=...
POST  /api/emprunts                  -> 201
POST  /api/emprunts/{id}/retour      -> 200
PATCH /api/penalites/{id}/regler     -> 200
```

Codes HTTP : `200 / 201 / 204 / 400 / 404 / 409`. Les DTO sont utilisés partout ; aucune
entité EF Core n'est sérialisée. Fichier de requêtes reproductible : `Api/BiblioPlus.http`.

---

## 6. Règles métier (workflow `CirculationService`)

1. Adhérent **actif**, **aucune pénalité non réglée**, **< 3 emprunts actifs**.
2. Exemplaire **disponible** au moment de l'emprunt.
3. Échéance calculée par le **serveur** : `DateEmprunt + DureeMaxJours` de la catégorie.
4. Retour en retard → pénalité = **jours commencés × pénalité journalière** ; le retour
   **libère l'exemplaire** de façon **atomique** (une seule sauvegarde).

Détails et scénarios : `docs/Regles-metier.md`.

---

## 7. Scénario de démonstration (via `Api/BiblioPlus.http` ou Swagger)

1. `POST /api/categories-livres` (valide) → **201** + `Location`.
2. `POST` invalide → **400** ; `GET /{id}` inexistant → **404** ; code en doublon → **409**.
3. `POST /api/emprunts` `{adherentId:1, exemplaireId:1}` → **201**.
4. `POST /api/emprunts/{id}/retour` → **200** (exemplaire libéré).
5. Emprunter un exemplaire déjà pris → **409** (règle 2).
6. `DELETE` d'une catégorie utilisée → **409** ; d'une catégorie libre → **204**,
   puis `GET` → **404** (suppression logique : la donnée est masquée).

Toutes ces étapes ont été rejouées et vérifiées (voir le tableau de preuves dans `../new-readme.md`).

---

## 8. Limites connues

- **SQLite** au lieu de SQL Server (adaptation macOS). `RowVersion`/`[Timestamp]` n'est pas
  auto-géré par SQLite (colonne présente mais non incrémentée).
- La recherche `LIKE` de SQLite n'est **pas accent-insensible** : « etranger » ne matche pas
  « L'**É**tranger » (« camus », « tranger » fonctionnent).
- Périmètre volontairement limité au socle : pas de réservation, paiement, e-mail, PDF, etc.

Choix et difficultés détaillés : `docs/Notes-choix-difficultes.md`.

---

## 9. Points de soutenance

- Sens des `ProjectReference` et interdiction des références inverses.
- `AddScoped` / `AddDbContext` / `AddControllers` / `MapControllers`.
- Intérêt du **DTO** ; rôle du **repository**, du **Unit of Work** et du **service**.
- Validation de **forme** (DataAnnotations, 400) vs **règle métier** (409).
- Codes `201 / 204 / 400 / 404 / 409` ; fonctionnement de la **suppression logique**.
