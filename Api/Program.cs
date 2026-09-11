using Application.Abstractions;
using Infrastructure;
using Infrastructure.Data;

// Charge le fichier .env (SMTP Gmail, etc.) avant la configuration.
DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Couche d'infrastructure (DbContext SQLite partagé). UoW + services : Jalon 3.
builder.Services.AddBiblioPlusPersistence(builder.Configuration);

var app = builder.Build();

// Applique les migrations et insère les données de démonstration au démarrage.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<BiblioPlusContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await BiblioPlusSeeder.SeedAsync(context, hasher);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
