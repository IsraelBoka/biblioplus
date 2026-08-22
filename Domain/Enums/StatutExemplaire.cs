namespace Domain.Enums;

/// <summary>Cycle de vie d'un exemplaire physique.</summary>
public enum StatutExemplaire
{
    Disponible = 0,
    Emprunte = 1,
    Perdu = 2,
    Endommage = 3
}
