using System.ComponentModel.DataAnnotations;

namespace Web.Models;

/// <summary>Ligne d'affichage d'un emprunt (liste / détails).</summary>
public class EmpruntViewModel
{
    public int Id { get; set; }

    [Display(Name = "Adhérent")]
    public string AdherentNom { get; set; } = string.Empty;

    [Display(Name = "Exemplaire")]
    public string ExemplaireCodeBarres { get; set; } = string.Empty;

    [Display(Name = "Livre")]
    public string LivreTitre { get; set; } = string.Empty;

    [Display(Name = "Date d'emprunt")]
    public DateTime DateEmprunt { get; set; }

    [Display(Name = "Échéance")]
    public DateTime DateEcheance { get; set; }

    [Display(Name = "Date de retour")]
    public DateTime? DateRetour { get; set; }

    public bool EstActif => DateRetour is null;

    /// <summary>Actif et échéance dépassée.</summary>
    public bool EnRetard => EstActif && DateTime.UtcNow.Date > DateEcheance.Date;
}

/// <summary>Formulaire de création d'un emprunt (choix adhérent + exemplaire).</summary>
public class EmpruntCreateViewModel
{
    [Display(Name = "Adhérent")]
    [Range(1, int.MaxValue, ErrorMessage = "L'adhérent est obligatoire.")]
    public int AdherentId { get; set; }

    [Display(Name = "Exemplaire disponible")]
    [Range(1, int.MaxValue, ErrorMessage = "L'exemplaire est obligatoire.")]
    public int ExemplaireId { get; set; }
}
