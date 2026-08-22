using Api.Contracts;
using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/categories-livres")]
public class CategoriesLivresController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public CategoriesLivresController(IUnitOfWork uow) => _uow = uow;

    private static CategorieLivreResponse ToResponse(CategorieLivre c)
        => new(c.Id, c.Code, c.Libelle, c.DureeMaxJours, c.PenaliteParJour);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategorieLivreResponse>>> GetAll(CancellationToken ct)
    {
        var list = await _uow.CategoriesLivres.ListAsync(ct);
        return Ok(list.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategorieLivreResponse>> GetById(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();
        return Ok(ToResponse(categorie));
    }

    [HttpPost]
    public async Task<ActionResult<CategorieLivreResponse>> Create(
        CreateCategorieLivreRequest request, CancellationToken ct)
    {
        if (await _uow.CategoriesLivres.CodeExisteAsync(request.Code, null, ct))
            return Conflict(new { erreur = $"Le code « {request.Code} » existe déjà." });

        var categorie = new CategorieLivre
        {
            Code = request.Code,
            Libelle = request.Libelle,
            DureeMaxJours = request.DureeMaxJours,
            PenaliteParJour = request.PenaliteParJour
        };

        await _uow.CategoriesLivres.AddAsync(categorie, ct);
        await _uow.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = categorie.Id }, ToResponse(categorie));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategorieLivreRequest request, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();

        if (await _uow.CategoriesLivres.CodeExisteAsync(request.Code, id, ct))
            return Conflict(new { erreur = $"Le code « {request.Code} » existe déjà." });

        categorie.Code = request.Code;
        categorie.Libelle = request.Libelle;
        categorie.DureeMaxJours = request.DureeMaxJours;
        categorie.PenaliteParJour = request.PenaliteParJour;

        _uow.CategoriesLivres.Update(categorie);
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var categorie = await _uow.CategoriesLivres.GetByIdAsync(id, ct);
        if (categorie is null) return NotFound();

        if (await _uow.CategoriesLivres.ADesLivresActifsAsync(id, ct))
            return Conflict(new { erreur = "Catégorie utilisée par des livres actifs : suppression refusée." });

        _uow.CategoriesLivres.Remove(categorie); // suppression logique
        await _uow.SaveChangesAsync(ct);

        return NoContent();
    }
}
