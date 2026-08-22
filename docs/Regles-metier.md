# BiblioPlus — Règles métier et scénarios (Jalon 3)

Le workflow principal est porté par `CirculationService` (couche `Application`), qui reçoit
`IUnitOfWork` par injection et **ne dépend ni de MVC, ni de `ControllerBase`, ni des vues Razor**.
Chaque cas d'usage réussi appelle `SaveChangesAsync()` **une seule fois** (atomicité).

Convention d'intervalle des périodes : `[DateEmprunt, DateEcheance[`.

---

## Les 4 règles métier

| # | Règle | Où | En cas de violation |
|---|---|---|---|
| 1 | Adhérent **actif**, **aucune pénalité non réglée**, **< 3 emprunts actifs** | `EmprunterAsync` | `Conflit` (409) |
| 2 | Exemplaire **`Disponible`** au moment de l'emprunt | `EmprunterAsync` | `Conflit` (409) |
| 3 | Échéance calculée par le serveur : `DateEmprunt + DureeMaxJours` de la catégorie | `EmprunterAsync` | — (calcul) |
| 4 | Retour en retard → pénalité = `jours commencés × PenaliteParJour` ; le retour libère l'exemplaire atomiquement | `RetournerAsync` | crée une `Penalite` `ARegler` |

La `Result`/`Result<T>` applicative porte un `ErreurType`
(`Validation` → 400, `Introuvable` → 404, `Conflit` → 409) que l'Api traduira au Jalon 4.

---

## Scénario A — Emprunt réussi ✅

**Contexte (données de démonstration)** : adhérent `ADH-0001` (actif, 0 pénalité, 0 emprunt),
exemplaire `EX-0001` du livre « L'Étranger » (catégorie `ROM`, `DureeMaxJours = 21`), statut `Disponible`.

**Action** : `EmprunterAsync(adherentId = ADH-0001, exemplaireId = EX-0001)`.

**Déroulé** :
1. Règle 1 : adhérent actif ✔, aucune pénalité non réglée ✔, 0 < 3 emprunts actifs ✔.
2. Règle 2 : exemplaire `Disponible` ✔.
3. Règle 3 : `DateEcheance = aujourd'hui + 21 jours` (calculée serveur).
4. L'exemplaire passe à `Emprunte`, l'emprunt est créé, **un seul** `SaveChangesAsync()`.

**Résultat attendu** : `Result<EmpruntResultat>` en **succès**
`{ EmpruntId, DateEmprunt, DateEcheance }`. Côté Api (Jalon 4) : **201 Created**.

---

## Scénario B — Refus : exemplaire non disponible ❌ (règle 2)

**Contexte** : l'exemplaire `EX-0001` vient d'être emprunté (scénario A), il est donc `Emprunte`.

**Action** : `EmprunterAsync(adherentId = ADH-0001, exemplaireId = EX-0001)` à nouveau.

**Déroulé** :
1. Règle 1 : OK.
2. Règle 2 : `Statut != Disponible` → **arrêt**.

**Résultat attendu** : `Result` en échec, `TypeErreur = Conflit`,
message « L'exemplaire n'est pas disponible. ». **Aucune** écriture en base.
Côté Api : **409 Conflict**.

---

## Scénario C — Refus : adhérent avec pénalité non réglée ❌ (règle 1)

**Contexte** : un adhérent a rendu un exemplaire **en retard** ; `RetournerAsync` a donc créé
une `Penalite` à l'état `ARegler` (règle 4). La pénalité n'est pas encore réglée.

**Action** : ce même adhérent tente `EmprunterAsync(...)` sur un exemplaire disponible.

**Déroulé** :
1. Règle 1 : `AdherentADesPenalitesNonRegleesAsync` renvoie `true` → **arrêt**.

**Résultat attendu** : `Result` en échec, `TypeErreur = Conflit`,
message « L'adhérent a des pénalités non réglées. ». Côté Api : **409 Conflict**.

> Variante de refus (règle 1) : un adhérent possédant déjà **3 emprunts actifs** est également
> refusé avec « L'adhérent a déjà 3 emprunts actifs. ».

---

## Scénario D — Retour en retard et calcul de pénalité (règle 4)

**Contexte** : un emprunt de catégorie `ROM` (`PenaliteParJour = 0,50`) a une échéance
dépassée de **3 jours** au moment du retour.

**Action** : `RetournerAsync(empruntId)`.

**Déroulé** :
1. `DateRetour = aujourd'hui`.
2. `DateRetour > DateEcheance` → `joursRetard = 3`.
3. `Montant = 3 × 0,50 = 1,50` → création d'une `Penalite` `ARegler`.
4. L'exemplaire repasse à `Disponible` ; **un seul** `SaveChangesAsync()` (atomique).

**Résultat attendu** : `Result<RetourResultat>` en succès
`{ EnRetard = true, JoursRetard = 3, MontantPenalite = 1,50, PenaliteId }`.

*(Retour à l'heure : `JoursRetard = 0`, `MontantPenalite = 0`, aucune pénalité créée,
l'exemplaire est libéré.)*

---

Ces scénarios seront **rejouables** au Jalon 4 via les routes de l'Api et le fichier `.http`.
