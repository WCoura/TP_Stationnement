using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TP_Stationnement.Model;

// Une session commence à l'entrée et se ferme à la sortie.
// On ne la supprime pas : elle devient de l'historique.
[Table("sessions_stationnement")]
public class SessionStationnement
{
    [Key]
    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("vehicule_id")]
    public int VehiculeId { get; set; }

    // Remplie par le serveur avec DateTime.UtcNow. Le client ne l'envoie pas.
    [Column("heure_entree")]
    public DateTime HeureEntree { get; set; }

    // Null tant que la voiture est encore dans le stationnement.
    [Column("heure_sortie")]
    public DateTime? HeureSortie { get; set; }

    // Calculé seulement à la sortie. Null pendant la session.
    [Column("montant_facture")]
    [Precision(10, 2)]
    public decimal? MontantFacture { get; set; }

    // false = en cours, true = terminée. On ne termine qu'une fois.
    [Column("est_terminee")]
    public bool EstTerminee { get; set; }

    // Pas une colonne SQL. EF s'en sert pour joindre le véhicule.
    public Vehicule? Vehicule { get; set; }
}
