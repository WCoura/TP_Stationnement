using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP_Stationnement.Model;

// Un véhicule peut revenir plusieurs fois. On ne l'efface jamais :
// ses sessions restent dans l'historique.
[Table("vehicules")]
public class Vehicule
{
    [Key]
    [Column("vehicule_id")]
    public int VehiculeId { get; set; }

    [Required(ErrorMessage = "La marque est obligatoire.")]
    [StringLength(50, ErrorMessage = "La marque doit avoir au plus 50 caractères.")]
    [Column("marque")]
    public string Marque { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le modèle est obligatoire.")]
    [StringLength(50, ErrorMessage = "Le modèle doit avoir au plus 50 caractères.")]
    [Column("modele")]
    public string Modele { get; set; } = string.Empty;

    [Required(ErrorMessage = "La couleur est obligatoire.")]
    [StringLength(30, ErrorMessage = "La couleur doit avoir au plus 30 caractères.")]
    [Column("couleur")]
    public string Couleur { get; set; } = string.Empty;

    // Optionnel. null si l'employé n'a rien écrit.
    [StringLength(200, ErrorMessage = "La description doit avoir au plus 200 caractères.")]
    [Column("description")]
    public string? Description { get; set; }
}
