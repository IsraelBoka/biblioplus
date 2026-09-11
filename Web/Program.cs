using Application.Abstractions;
using Infrastructure;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

// Charge le fichier .env (SMTP Gmail, etc.) en variables d'environnement avant la configuration.
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Reprend les variables d'environnement (dont celles issues du .env) dans la configuration.
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddControllersWithViews();

builder.Services.AddBiblioPlusPersistence(builder.Configuration);

// Authentification par cookie : espace admin et espace adhérent partagent le même schéma,
// la distinction se fait par le rôle (claim Role) porté par chaque compte.
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Compte/Connexion";
        options.LogoutPath = "/Compte/Deconnexion";
        options.AccessDeniedPath = "/Compte/AccesRefuse";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BiblioPlusContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await BiblioPlusSeeder.SeedAsync(context, hasher);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
