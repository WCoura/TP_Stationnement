using TP_Stationnement.Model;

namespace TP_Stationnement.Repositories;

// Le journal d'audit n'a pas son propre repository.
// Il s'écrit ici, en même temps que la correction, juste avant SaveChanges.
// Personne ne peut le modifier ni le supprimer par l'API.
public interface ISessionRepository
{
    Task<SessionStationnement?> ObtenirParIdAsync(int id);
    Task<bool> AUneSessionActiveAsync(int vehiculeId);
    Task<SessionStationnement> CreerEntreeAsync(int vehiculeId);
    Task EnregistrerSortieAsync(SessionStationnement session, DateTime heureSortie, decimal montant);
    Task<bool> CorrigerSortieAsync(SessionStationnement session, DateTime nouvelleSortie, decimal nouveauMontant, string employe);
    Task<List<SessionStationnement>> ObtenirActivesAsync();
    Task<List<SessionStationnement>> ObtenirTermineesParPeriodeAsync(DateTime dateDebut, DateTime dateFin);
    Task<List<AuditModification>> ObtenirAuditAsync(int sessionId);
}
