using System.ComponentModel.DataAnnotations;
using Domain.Common;

namespace Domain.Entities;

public class ParametresCirculation : BaseEntity
{
    [Range(1, 100)]
    public int QuotaEmpruntsActifs { get; set; } = 3;

    [Range(0, 365)]
    public int DelaiGraceJours { get; set; } = 0;

    [Range(0, 100000)]
    public decimal PlafondPenalite { get; set; } = 0m;

    public bool BloquerSiPenalitesImpayees { get; set; } = true;
}
