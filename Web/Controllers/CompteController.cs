using System.Security.Claims;
using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

/// <summary>
/// Connexion / inscription / déconnexion. Un seul schéma de cookie ;
/// le rôle (Admin ou Adherent) détermine l'espace accessible.
/// </summary>
[AllowAnonymous]
public class CompteController : Controller
{
    public const string ClaimAdherentId = "AdherentId";

    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _email;
    private readonly IWebHostEnvironment _env;

    public CompteController(
        IUnitOfWork uow, IPasswordHasher passwordHasher, IEmailSender email, IWebHostEnvironment env)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _email = email;
        _env = env;
    }

    // GET: /Compte/Connexion
    [HttpGet]
    public IActionResult Connexion(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectVersEspace();
        }
        return View(new ConnexionViewModel { ReturnUrl = returnUrl });
    }

    // POST: /Compte/Connexion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Connexion(ConnexionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var utilisateur = await _uow.Utilisateurs.GetByEmailAsync(model.Email.Trim(), ct);
        if (utilisateur is null || !utilisateur.Actif
            || !_passwordHasher.Verify(model.MotDePasse, utilisateur.MotDePasseHash))
        {
            ModelState.AddModelError(string.Empty, "E-mail ou mot de passe incorrect.");
            return View(model);
        }

        await SignInAsync(utilisateur);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        return RedirectVersEspace(utilisateur.Role);
    }

    // POST: /Compte/ConnexionRapide  (aide au test — DÉVELOPPEMENT UNIQUEMENT)
    // Connecte en un clic l'un des comptes de démonstration, sans saisie.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConnexionRapide(string role, CancellationToken ct)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound(); // désactivé hors développement
        }

        var email = role == RoleUtilisateur.Admin.ToString()
            ? BiblioPlusSeeder.AdminEmail
            : BiblioPlusSeeder.MembreEmail;

        var utilisateur = await _uow.Utilisateurs.GetByEmailAsync(email, ct);
        if (utilisateur is null)
        {
            TempData["Erreur"] = "Compte de démonstration introuvable.";
            return RedirectToAction(nameof(Connexion));
        }

        await SignInAsync(utilisateur);
        return RedirectVersEspace(utilisateur.Role);
    }

    // GET: /Compte/Inscription  (auto-inscription d'un adhérent)
    [HttpGet]
    public IActionResult Inscription() => View(new InscriptionViewModel());

    // POST: /Compte/Inscription
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Inscription(InscriptionViewModel model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim();
        if (await _uow.Utilisateurs.EmailExisteAsync(email, null, ct))
        {
            ModelState.AddModelError(nameof(model.Email), "Un compte utilise déjà cet e-mail.");
            return View(model);
        }

        // Crée l'adhérent puis son compte de connexion (une transaction).
        var adherent = new Adherent
        {
            Numero = await GenererNumeroAdherentAsync(ct),
            Nom = model.Nom.Trim(),
            Telephone = model.Telephone,
            Email = email,
            DateAdhesion = DateTime.UtcNow.Date,
            Actif = true
        };
        await _uow.Adherents.AddAsync(adherent, ct);
        await _uow.SaveChangesAsync(ct);

        var utilisateur = new Utilisateur
        {
            Email = email,
            Nom = model.Nom.Trim(),
            Role = RoleUtilisateur.Adherent,
            MotDePasseHash = _passwordHasher.Hash(model.MotDePasse),
            AdherentId = adherent.Id,
            Actif = true
        };
        await _uow.Utilisateurs.AddAsync(utilisateur, ct);
        await _uow.SaveChangesAsync(ct);

        // E-mail de bienvenue (best-effort).
        try
        {
            await _email.SendAsync(
                email,
                "BiblioPlus — Bienvenue",
                $"<p>Bonjour {adherent.Nom},</p>" +
                $"<p>Votre compte adhérent a été créé (numéro <strong>{adherent.Numero}</strong>).</p>" +
                "<p>Vous pouvez dès maintenant consulter le catalogue et suivre vos emprunts.</p>" +
                "<p>— L'équipe BiblioPlus</p>",
                ct);
        }
        catch { /* l'inscription réussit même si l'e-mail échoue */ }

        await SignInAsync(utilisateur);
        TempData["Message"] = "Bienvenue ! Votre compte a été créé.";
        return RedirectToAction("Index", "Espace");
    }

    // POST: /Compte/Deconnexion
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deconnexion()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Connexion));
    }

    // GET: /Compte/AccesRefuse
    [HttpGet]
    public IActionResult AccesRefuse() => View();

    private async Task SignInAsync(Utilisateur utilisateur)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
            new(ClaimTypes.Name, utilisateur.Nom),
            new(ClaimTypes.Email, utilisateur.Email),
            new(ClaimTypes.Role, utilisateur.Role.ToString())
        };
        if (utilisateur.AdherentId is int adherentId)
        {
            claims.Add(new Claim(ClaimAdherentId, adherentId.ToString()));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectVersEspace(RoleUtilisateur? role = null)
    {
        role ??= User.IsInRole(RoleUtilisateur.Admin.ToString())
            ? RoleUtilisateur.Admin
            : RoleUtilisateur.Adherent;

        return role == RoleUtilisateur.Admin
            ? RedirectToAction("Index", "Home")
            : RedirectToAction("Index", "Espace");
    }

    private async Task<string> GenererNumeroAdherentAsync(CancellationToken ct)
    {
        // Numéro simple, unique : ADH-#### basé sur le nombre d'adhérents existants.
        var existants = await _uow.Adherents.ListAsync(ct);
        var prochain = existants.Count + 1;
        string numero;
        do
        {
            numero = $"ADH-{prochain:D4}";
            prochain++;
        }
        while (await _uow.Adherents.NumeroExisteAsync(numero, null, ct));
        return numero;
    }
}
