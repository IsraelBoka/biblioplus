namespace Web.Models;

/// <summary>Indicateurs affichés sur le tableau de bord d'accueil.</summary>
public class DashboardViewModel
{
    public int NombreLivres { get; set; }
    public int NombreExemplaires { get; set; }
    public int ExemplairesDisponibles { get; set; }
    public int AdherentsActifs { get; set; }
    public int EmpruntsActifs { get; set; }
    public int EmpruntsEnRetard { get; set; }
    public int PenalitesARegler { get; set; }
    public decimal MontantPenalitesARegler { get; set; }

    /// <summary>Derniers emprunts (actifs ou rendus), les plus récents d'abord.</summary>
    public List<DashboardEmpruntLigne> DerniersEmprunts { get; set; } = new();
}

/// <summary>Ligne compacte d'emprunt pour le tableau de bord.</summary>
public class DashboardEmpruntLigne
{
    public int Id { get; set; }
    public string AdherentNom { get; set; } = string.Empty;
    public string LivreTitre { get; set; } = string.Empty;
    public DateTime DateEmprunt { get; set; }
    public DateTime DateEcheance { get; set; }
    public bool EstActif { get; set; }
    public bool EnRetard { get; set; }
}
