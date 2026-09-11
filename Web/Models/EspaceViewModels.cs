using Domain.Enums;

namespace Web.Models;

/// <summary>Tableau de bord personnel de l'adhérent connecté.</summary>
public class EspaceDashboardViewModel
{
    public string Nom { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public int EmpruntsActifs { get; set; }
    public int EmpruntsEnRetard { get; set; }
    public int PenalitesARegler { get; set; }
    public decimal MontantDu { get; set; }
    public List<EspaceEmpruntLigne> DerniersEmprunts { get; set; } = new();
}

public class EspaceEmpruntLigne
{
    public int Id { get; set; }
    public string LivreTitre { get; set; } = "—";
    public DateTime DateEmprunt { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DateRetour { get; set; }
    public bool EstActif => DateRetour is null;
    public bool EnRetard { get; set; }
}

public class EspaceCatalogueLigne
{
    public string Titre { get; set; } = string.Empty;
    public string Auteur { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public string Categorie { get; set; } = "—";
    public int ExemplairesDisponibles { get; set; }
}

public class EspacePenaliteLigne
{
    public int Id { get; set; }
    public string Motif { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public EtatPenalite Etat { get; set; }
    public DateTimeOffset Date { get; set; }
}
