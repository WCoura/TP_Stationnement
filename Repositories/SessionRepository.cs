using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TP_Stationnement.Model;

namespace TP_Stationnement.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _contexte;

    public SessionRepository(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public Task<SessionStationnement?> ObtenirParIdAsync(int id)
    {
        return _contexte.Sessions
            .Include(s => s.Vehicule)
            .FirstOrDefaultAsync(s => s.SessionId == id);
    }

    public Task<bool> AUneSessionActiveAsync(int vehiculeId)
    {
        return _contexte.Sessions.AnyAsync(s => s.VehiculeId == vehiculeId && !s.EstTerminee);
    }

    public async Task<SessionStationnement> CreerEntreeAsync(int vehiculeId)
    {
        var session = new SessionStationnement
        {
            VehiculeId = vehiculeId,
            HeureEntree = DateTime.UtcNow,
            HeureSortie = null,
            MontantFacture = null,
            EstTerminee = false
        };

        _contexte.Sessions.Add(session);
        await _contexte.SaveChangesAsync();
        return session;
    }

    public async Task EnregistrerSortieAsync(SessionStationnement session, DateTime heureSortie, decimal montant)
    {
        session.HeureSortie = Horloge.CommeUtc(heureSortie);
        session.MontantFacture = montant;
        session.EstTerminee = true;
        await _contexte.SaveChangesAsync();
    }

    // Retourne false si rien n'a bougé : on n'écrit pas une ligne d'audit vide.
    public async Task<bool> CorrigerSortieAsync(
        SessionStationnement session,
        DateTime nouvelleSortie,
        decimal nouveauMontant,
        string employe)
    {
        bool changement = false;

        string ancienneHeure = FormaterHeure(session.HeureSortie);
        string nouvelleHeure = FormaterHeure(nouvelleSortie);
        if (ancienneHeure != nouvelleHeure)
        {
            EnregistrerAudit(session.SessionId, "HeureSortie", ancienneHeure, nouvelleHeure, employe);
            session.HeureSortie = Horloge.CommeUtc(nouvelleSortie);
            changement = true;
        }

        string ancienMontant = FormaterMontant(session.MontantFacture);
        string montantTexte = FormaterMontant(nouveauMontant);
        if (ancienMontant != montantTexte)
        {
            EnregistrerAudit(session.SessionId, "MontantFacture", ancienMontant, montantTexte, employe);
            session.MontantFacture = nouveauMontant;
            changement = true;
        }

        if (!changement)
            return false;

        await _contexte.SaveChangesAsync();
        return true;
    }

    public Task<List<SessionStationnement>> ObtenirActivesAsync()
    {
        // Les plus anciennes en haut : ce sont elles qui coûtent le plus cher.
        return _contexte.Sessions
            .Include(s => s.Vehicule)
            .Where(s => !s.EstTerminee)
            .OrderBy(s => s.HeureEntree)
            .ToListAsync();
    }

    public async Task<List<SessionStationnement>> ObtenirTermineesParPeriodeAsync(DateTime dateDebut, DateTime dateFin)
    {
        var debutJour = dateDebut.Date;
        var finJour = dateFin.Date;

        // Les heures en base sont en UTC. On élargit la requête d'une journée,
        // puis on garde celles dont la sortie tombe dans la journée locale.
        // Comme ça, une sortie à 21 h à Montréal compte encore pour cette journée.
        var debutLarge = DateTime.SpecifyKind(debutJour.AddDays(-1), DateTimeKind.Utc);
        var finLarge = DateTime.SpecifyKind(finJour.AddDays(2), DateTimeKind.Utc);

        var sessions = await _contexte.Sessions
            .Include(s => s.Vehicule)
            .Where(s => s.EstTerminee
                && s.HeureSortie != null
                && s.HeureSortie >= debutLarge
                && s.HeureSortie < finLarge)
            .ToListAsync();

        return sessions
            .Where(s =>
            {
                var jourLocal = Horloge.CommeUtc(s.HeureSortie!.Value).ToLocalTime().Date;
                return jourLocal >= debutJour && jourLocal <= finJour;
            })
            .OrderByDescending(s => s.HeureSortie)
            .ToList();
    }

    public Task<List<AuditModification>> ObtenirAuditAsync(int sessionId)
    {
        return _contexte.Audits
            .Where(a => a.SessionId == sessionId)
            .OrderBy(a => a.DateModification)
            .ThenBy(a => a.AuditId)
            .ToListAsync();
    }

    // Ajouté au contexte, mais sauvé avec le reste dans SaveChanges.
    // Si la sauvegarde échoue, la correction et le journal partent ensemble.
    private void EnregistrerAudit(int sessionId, string champ, string ancienneValeur, string nouvelleValeur, string employe)
    {
        _contexte.Audits.Add(new AuditModification
        {
            SessionId = sessionId,
            ChampModifie = champ,
            AncienneValeur = ancienneValeur,
            NouvelleValeur = nouvelleValeur,
            DateModification = DateTime.UtcNow,
            ModifiePar = employe
        });
    }

    private static string FormaterHeure(DateTime? heure)
    {
        if (!heure.HasValue)
            return "";

        return Horloge.CommeUtc(heure.Value).ToString("yyyy-MM-dd HH:mm:ss");
    }

    // Point décimal fixe, pour que deux montants se comparent pareil
    // peu importe la langue de Windows.
    private static string FormaterMontant(decimal? montant)
    {
        if (!montant.HasValue)
            return "";

        return montant.Value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}
