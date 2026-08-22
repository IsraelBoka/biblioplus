using System.ComponentModel.DataAnnotations;

namespace Api.Contracts;

/// <summary>DTO de réponse : jamais l'entité EF Core directement.</summary>
public record CategorieLivreResponse(
    int Id,
    string Code,
    string Libelle,
    int DureeMaxJours,
    decimal PenaliteParJour);

/// <summary>DTO de création.</summary>
public class CreateCategorieLivreRequest
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Libelle { get; set; } = string.Empty;

    [Range(1, 365)]
    public int DureeMaxJours { get; set; }

    [Range(0, 10000)]
    public decimal PenaliteParJour { get; set; }
}

/// <summary>DTO de modification.</summary>
public class UpdateCategorieLivreRequest
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Libelle { get; set; } = string.Empty;

    [Range(1, 365)]
    public int DureeMaxJours { get; set; }

    [Range(0, 10000)]
    public decimal PenaliteParJour { get; set; }
}
