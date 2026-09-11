# BiblioPlus — Preuves d'exécution (rejouables via `Api/BiblioPlus.http`)

Base fraîche, API `http://localhost:5188`. Chaque ligne montre le code HTTP obtenu.

```http
# 1) Création valide
POST /api/categories-livres            -> 201  (201 attendu, + en-tête Location)
# 2) Donnée invalide
POST /api/categories-livres (invalide) -> 400  (400 attendu)
# 3) Id inexistant
GET  /api/categories-livres/9999       -> 404  (404 attendu)
# 4) Unicité violée
POST /api/categories-livres (ROM)      -> 409  (409 attendu)
# 5) Workflow réussi
GET  /api/livres/disponibles?rech=camus-> 200  (200 attendu)
POST /api/emprunts {adh1,ex1}          -> 201  (emprunt #1 créé)
POST /api/emprunts/1/retour           -> 200  (200 attendu, exemplaire libéré)
# 6) Workflow refusé (exemplaire déjà emprunté)
POST /api/emprunts {adh1,ex2}          -> 201  (ex2 emprunté)
POST /api/emprunts {adh1,ex2} (repris) -> 409  (409 attendu)
# 7) Suppression logique
DELETE /api/categories-livres/1 (ROM)  -> 409  (409 attendu : utilisée)
DELETE /api/categories-livres/2 (BD)   -> 204  (204 attendu)
GET    /api/categories-livres/2        -> 404  (404 attendu : masquée)
```
