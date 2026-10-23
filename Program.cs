using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using TP_Stationnement.Model;
using TP_Stationnement.Repositories;
using TP_Stationnement.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new ConvertisseurDateUtc());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Le message automatique de [ApiController] est en anglais.
        // On le remplace pour rester cohérent avec le reste.
        options.InvalidModelStateResponseFactory = contexte =>
        {
            var details = new ValidationProblemDetails(contexte.ModelState)
            {
                Title = "Certains champs sont invalides.",
                Status = StatusCodes.Status400BadRequest
            };

            return new BadRequestObjectResult(details);
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Stationnement",
        Version = "v1",
        Description = "Entrées, sorties et facturation. Le guichet web est à la racine du site."
    });

    var fichierXml = Path.Combine(AppContext.BaseDirectory, "TP_Stationnement.xml");
    if (File.Exists(fichierXml))
        options.IncludeXmlComments(fichierXml);
});

// Les trois tarifs viennent de appsettings.json.
builder.Services.AddScoped<CalculateurTarif>();
builder.Services.AddScoped<IVehiculeRepository, VehiculeRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

// Demandé pour le développement : n'importe quelle origine peut appeler l'API.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", politique =>
    {
        politique.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

string connexion = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("La chaîne DefaultConnection est manquante dans appsettings.json.");

// Version fixée pour que l'application démarre même si MySQL est arrêté.
// XAMPP avec MariaDB : remplacer par new Version(10, 4, 32) si la migration rouspète.
var versionMySql = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connexion, versionMySql));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();
