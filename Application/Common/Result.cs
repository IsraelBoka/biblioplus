namespace Application.Common;

/// <summary>Nature d'un échec métier, utilisée pour choisir le code HTTP côté Api.</summary>
public enum ErreurType
{
    Aucune = 0,
    Validation = 1,  // -> 400
    Introuvable = 2, // -> 404
    Conflit = 3      // -> 409 (règle métier, unicité, dépendance)
}

/// <summary>Résultat d'un cas d'usage : succès, ou échec métier explicite.</summary>
public class Result
{
    public bool EstSucces { get; }
    public string? Erreur { get; }
    public ErreurType TypeErreur { get; }

    protected Result(bool succes, string? erreur, ErreurType type)
    {
        EstSucces = succes;
        Erreur = erreur;
        TypeErreur = type;
    }

    public static Result Succes() => new(true, null, ErreurType.Aucune);
    public static Result Echec(string erreur, ErreurType type) => new(false, erreur, type);

    public static Result Introuvable(string erreur) => Echec(erreur, ErreurType.Introuvable);
    public static Result Conflit(string erreur) => Echec(erreur, ErreurType.Conflit);
    public static Result Invalide(string erreur) => Echec(erreur, ErreurType.Validation);
}

/// <summary>Résultat porteur d'une valeur en cas de succès.</summary>
public class Result<T> : Result
{
    public T? Valeur { get; }

    private Result(bool succes, T? valeur, string? erreur, ErreurType type)
        : base(succes, erreur, type)
    {
        Valeur = valeur;
    }

    public static Result<T> Succes(T valeur) => new(true, valeur, null, ErreurType.Aucune);

    public static new Result<T> Introuvable(string erreur) => new(false, default, erreur, ErreurType.Introuvable);
    public static new Result<T> Conflit(string erreur) => new(false, default, erreur, ErreurType.Conflit);
    public static new Result<T> Invalide(string erreur) => new(false, default, erreur, ErreurType.Validation);
}
