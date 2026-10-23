using Microsoft.AspNetCore.Mvc;
using TP_Stationnement.Model;
using TP_Stationnement.Repositories;
using TP_Stationnement.Services;

namespace TP_Stationnement.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController : ControllerBase
{
    private readonly ISessionRepository _sessions;
    private readonly IVehiculeRepository _vehicules;
    private readonly CalculateurTarif _tarif;

    public SessionsController(
        ISessionRepository sessions,
        IVehiculeRepository vehicules,
        CalculateurTarif tarif)
    {
        _sessions = sessions;
        _vehicules = vehicules;
        _tarif = tarif;
    }

    /// <summary>Ouvre une session. L'heure d'entrée est prise sur le serveur.</summary>
    [HttpPost("entree")]
    public async Task<IActionResult> Entree([FromBody] DemandeEntree demande)
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var vehicule = await _vehicules.ObtenirParIdAsync(demande.VehiculeId);
        if (vehicule == null)
            return NotFound(new { message = "Véhicule introuvable." });

        // Une voiture déjà sur le terrain ne peut pas entrer une deuxième fois.
        if (await _sessions.AUneSessionActiveAsync(demande.VehiculeId))
            return Conflict(new { message = "Ce véhicule a déjà une session en cours." });

        var session = await _sessions.CreerEntreeAsync(demande.VehiculeId);
        var complete = await _sessions.ObtenirParIdAsync(session.SessionId);

        return StatusCode(StatusCodes.Status201Created, complete);
    }

    /// <summary>Ferme la session et calcule le montant à payer.</summary>
    [HttpPut("{id}/sortie")]
    public async Task<IActionResult> Sortie(int id)
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var session = await _sessions.ObtenirParIdAsync(id);
        if (session == null)
            return NotFound(new { message = "Session introuvable." });

        if (session.EstTerminee)
            return Conflict(new { message = "Cette session est déjà terminée." });

        var heureSortie = DateTime.UtcNow;
        var montant = _tarif.CalculerMontant(session.HeureEntree, heureSortie);

        await _sessions.EnregistrerSortieAsync(session, heureSortie, montant);
        return Ok(session);
    }

    /// <summary>Corrige l'heure de sortie d'une session terminée et laisse un audit.</summary>
    [HttpPut("{id}/modifier")]
    public async Task<IActionResult> Modifier(int id, [FromBody] DemandeModification demande)
    {
        // DateTime a toujours une valeur, alors [Required] ne sert à rien ici.
        if (demande.HeureSortie == default)
            ModelState.AddModelError(nameof(demande.HeureSortie), "L'heure de sortie est obligatoire.");

        if (!ModelState.IsValid)
            return ProblemeValidation();

        var session = await _sessions.ObtenirParIdAsync(id);
        if (session == null)
            return NotFound(new { message = "Session introuvable." });

        if (!session.EstTerminee)
            return Conflict(new { message = "On corrige seulement une session déjà terminée." });

        var nouvelleSortie = Horloge.CommeUtc(demande.HeureSortie);

        if (nouvelleSortie <= Horloge.CommeUtc(session.HeureEntree))
            return BadRequest(new { message = "L'heure de sortie doit être après l'heure d'entrée." });

        // L'entrée est figée par le serveur, alors la correction est le seul
        // endroit où on peut indiquer l'heure réelle de départ — y compris
        // pour revérifier un cas comme 1 h 37.
        var montant = _tarif.CalculerMontant(session.HeureEntree, nouvelleSortie);
        bool change = await _sessions.CorrigerSortieAsync(
            session,
            nouvelleSortie,
            montant,
            demande.ModifiePar.Trim());

        return Ok(new
        {
            message = change
                ? "Correction enregistrée."
                : "Aucun changement à enregistrer.",
            session
        });
    }

    /// <summary>Liste les sessions encore ouvertes.</summary>
    [HttpGet("actives")]
    public async Task<IActionResult> Actives()
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var liste = await _sessions.ObtenirActivesAsync();
        return Ok(liste);
    }

    /// <summary>Liste les sessions terminées entre deux dates, inclusivement.</summary>
    [HttpGet("periode")]
    public async Task<IActionResult> Periode([FromQuery] DateTime dateDebut, [FromQuery] DateTime dateFin)
    {
        if (dateDebut == default || dateFin == default)
            ModelState.AddModelError("periode", "Il faut une date de début et une date de fin.");

        if (!ModelState.IsValid)
            return ProblemeValidation();

        if (dateFin.Date < dateDebut.Date)
            return BadRequest(new { message = "La date de fin est avant la date de début." });

        var liste = await _sessions.ObtenirTermineesParPeriodeAsync(dateDebut, dateFin);
        return Ok(liste);
    }

    /// <summary>Historique des corrections d'une session. Lecture seule.</summary>
    [HttpGet("{id}/audit")]
    public async Task<IActionResult> Audit(int id)
    {
        if (!ModelState.IsValid)
            return ProblemeValidation();

        var session = await _sessions.ObtenirParIdAsync(id);
        if (session == null)
            return NotFound(new { message = "Session introuvable." });

        var journal = await _sessions.ObtenirAuditAsync(id);
        return Ok(journal);
    }

    private IActionResult ProblemeValidation()
    {
        return ValidationProblem(
            title: "Certains champs sont invalides.",
            modelStateDictionary: ModelState);
    }
}
