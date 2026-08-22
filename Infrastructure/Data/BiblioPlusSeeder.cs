using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// Données de démonstration minimales, insérées seulement si la base est vide.
/// </summary>
public static class BiblioPlusSeeder
{
    public static async Task SeedAsync(BiblioPlusContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.CategoriesLivres.AnyAsync())
        {
            return; // déjà initialisée
        }

        var roman = new CategorieLivre
        {
            Code = "ROM",
            Libelle = "Roman",
            DureeMaxJours = 21,
            PenaliteParJour = 0.50m
        };
        var bd = new CategorieLivre
        {
            Code = "BD",
            Libelle = "Bande dessinée",
            DureeMaxJours = 14,
            PenaliteParJour = 0.75m
        };

        var livre = new Livre
        {
            Isbn = "9782070368228",
            Titre = "L'Étranger",
            Auteur = "Albert Camus",
            CategorieLivre = roman,
            Exemplaires =
            {
                new Exemplaire { CodeBarres = "EX-0001", Etat = "Bon", Statut = StatutExemplaire.Disponible },
                new Exemplaire { CodeBarres = "EX-0002", Etat = "Bon", Statut = StatutExemplaire.Disponible }
            }
        };

        var adherent = new Adherent
        {
            Numero = "ADH-0001",
            Nom = "Marie Dupont",
            Telephone = "0600000000",
            Actif = true
        };

        context.CategoriesLivres.AddRange(roman, bd);
        context.Livres.Add(livre);
        context.Adherents.Add(adherent);

        await context.SaveChangesAsync();
    }
}
