using Microsoft.AspNetCore.Mvc;
using TP_Stationnement.Model;
using TP_Stationnement.Repositories;

namespace TP_Stationnement.Controllers;

[ApiController]
[Route("vehicules")]
public class VehiculesController : ControllerBase
{
    private readonly IVehiculeRepository _vehicules;

    public VehiculesController(IVehiculeRepository vehicules)
    {
        _vehicules = vehicules;
    }

    /// <summary>Retourne tous les véhicules enregistrés.</summary>
    [HttpGet]
    public async Task<IActionResult> ObtenirTous()
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var liste = await _vehicules.ObtenirTousAsync();
        return Ok(liste);
    }

    /// <summary>Retourne un véhicule par son identifiant.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenirParId(int id)
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var vehicule = await _vehicules.ObtenirParIdAsync(id);
        if (vehicule == null)
            return NotFound(new { message = "Véhicule introuvable." });

        return Ok(vehicule);
    }

    /// <summary>Crée un nouveau véhicule. La clé est générée par MySQL.</summary>
    [HttpPost]
    public async Task<IActionResult> Creer([FromBody] Vehicule vehicule)
    {
        Nettoyer(vehicule);

        if (!ModelState.IsValid)
            return ProblemeValidation();

        // On ignore un id envoyé par le client.
        vehicule.VehiculeId = 0;

        var cree = await _vehicules.AjouterAsync(vehicule);
        return CreatedAtAction(nameof(ObtenirParId), new { id = cree.VehiculeId }, cree);
    }

    /// <summary>Met à jour la fiche. L'identifiant vient de l'adresse, pas du corps.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Modifier(int id, [FromBody] Vehicule vehicule)
    {
        Nettoyer(vehicule);

        if (!ModelState.IsValid)
            return ProblemeValidation();

        var existant = await _vehicules.ObtenirParIdAsync(id);
        if (existant == null)
            return NotFound(new { message = "Véhicule introuvable." });

        existant.Marque = vehicule.Marque;
        existant.Modele = vehicule.Modele;
        existant.Couleur = vehicule.Couleur;
        existant.Description = vehicule.Description;

        await _vehicules.ModifierAsync(existant);
        return Ok(existant);
    }

    // [Required] laisse passer une chaîne de seulement des espaces.
    // On coupe les espaces, puis on revérifie.
    private void Nettoyer(Vehicule vehicule)
    {
        vehicule.Marque = (vehicule.Marque ?? "").Trim();
        vehicule.Modele = (vehicule.Modele ?? "").Trim();
        vehicule.Couleur = (vehicule.Couleur ?? "").Trim();
        vehicule.Description = string.IsNullOrWhiteSpace(vehicule.Description)
            ? null
            : vehicule.Description.Trim();

        if (vehicule.Marque.Length == 0)
            ModelState.AddModelError(nameof(vehicule.Marque), "La marque est obligatoire.");

        if (vehicule.Modele.Length == 0)
            ModelState.AddModelError(nameof(vehicule.Modele), "Le modèle est obligatoire.");

        if (vehicule.Couleur.Length == 0)
            ModelState.AddModelError(nameof(vehicule.Couleur), "La couleur est obligatoire.");
    }

    private IActionResult ProblemeValidation()
    {
        return ValidationProblem(
            title: "Certains champs sont invalides.",
            modelStateDictionary: ModelState);
    }
}
