using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;


public static class BiblioPlusSeeder
{
    // Identifiants de démonstration (à changer en production).
    public const string AdminEmail = "admin@biblioplus.local";
    public const string AdminMotDePasse = "Admin123!";
    public const string MembreEmail = "marie@biblioplus.local";
    public const string MembreMotDePasse = "Membre123!";

    public static async Task SeedAsync(BiblioPlusContext context, IPasswordHasher passwordHasher)
    {
        await context.Database.MigrateAsync();

        // Paramètres de circulation : ligne unique par défaut, garantie même sur une base existante.
        if (!await context.ParametresCirculation.AnyAsync())
        {
            context.ParametresCirculation.Add(new ParametresCirculation());
            await context.SaveChangesAsync();
        }

        // Compte administrateur : garanti même sur une base déjà existante.
        if (!await context.Utilisateurs.AnyAsync(u => u.Role == RoleUtilisateur.Admin))
        {
            context.Utilisateurs.Add(new Utilisateur
            {
                Email = AdminEmail,
                Nom = "Administrateur",
                Role = RoleUtilisateur.Admin,
                MotDePasseHash = passwordHasher.Hash(AdminMotDePasse),
                Actif = true
            });
            await context.SaveChangesAsync();
        }

        // Catalogue de démonstration : uniquement sur une base vierge.
        if (!await context.CategoriesLivres.AnyAsync())
        {
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
                Email = MembreEmail,
                Actif = true
            };

            context.CategoriesLivres.AddRange(roman, bd);
            context.Livres.Add(livre);
            context.Adherents.Add(adherent);
            await context.SaveChangesAsync();
        }

        // Compte adhérent de démonstration : garanti même sur une base déjà existante.
        // Rattaché à Marie Dupont (ADH-0001) si elle est présente.
        if (!await context.Utilisateurs.AnyAsync(u => u.Email == MembreEmail))
        {
            var marie = await context.Adherents.FirstOrDefaultAsync(a => a.Numero == "ADH-0001");
            if (marie is not null)
            {
                if (string.IsNullOrWhiteSpace(marie.Email))
                {
                    marie.Email = MembreEmail;
                }

                context.Utilisateurs.Add(new Utilisateur
                {
                    Email = MembreEmail,
                    Nom = marie.Nom,
                    Role = RoleUtilisateur.Adherent,
                    MotDePasseHash = passwordHasher.Hash(MembreMotDePasse),
                    AdherentId = marie.Id,
                    Actif = true
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
