using Microsoft.EntityFrameworkCore;
using TP_Stationnement.Model;

namespace TP_Stationnement.Repositories;

public class VehiculeRepository : IVehiculeRepository
{
    private readonly AppDbContext _contexte;

    public VehiculeRepository(AppDbContext contexte)
    {
        _contexte = contexte;
    }

    public Task<List<Vehicule>> ObtenirTousAsync()
    {
        return _contexte.Vehicules
            .OrderBy(v => v.Marque)
            .ThenBy(v => v.Modele)
            .ToListAsync();
    }

    public Task<Vehicule?> ObtenirParIdAsync(int id)
    {
        return _contexte.Vehicules.FirstOrDefaultAsync(v => v.VehiculeId == id);
    }

    public async Task<Vehicule> AjouterAsync(Vehicule vehicule)
    {
        _contexte.Vehicules.Add(vehicule);
        await _contexte.SaveChangesAsync();
        return vehicule;
    }

    public async Task ModifierAsync(Vehicule vehicule)
    {
        _contexte.Vehicules.Update(vehicule);
        await _contexte.SaveChangesAsync();
    }
}
