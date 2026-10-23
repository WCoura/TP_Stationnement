using TP_Stationnement.Model;

namespace TP_Stationnement.Repositories;

// Pas de méthode Supprimer : le travail l'interdit.
public interface IVehiculeRepository
{
    Task<List<Vehicule>> ObtenirTousAsync();
    Task<Vehicule?> ObtenirParIdAsync(int id);
    Task<Vehicule> AjouterAsync(Vehicule vehicule);
    Task ModifierAsync(Vehicule vehicule);
}
