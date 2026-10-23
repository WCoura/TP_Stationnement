using System.ComponentModel.DataAnnotations;

namespace TP_Stationnement.Model;

// Ce que le client a le droit d'envoyer pour une entrée.
// L'heure n'est pas ici : c'est le serveur qui la prend.
public class DemandeEntree
{
    [Range(1, int.MaxValue, ErrorMessage = "Il faut un numéro de véhicule valide.")]
    public int VehiculeId { get; set; }
}

// Correction manuelle d'une sortie déjà enregistrée.
public class DemandeModification
{
    public DateTime HeureSortie { get; set; }

    [Required(ErrorMessage = "Il faut indiquer qui fait la correction.")]
    [StringLength(80, ErrorMessage = "Le nom de l'employé doit avoir au plus 80 caractères.")]
    public string ModifiePar { get; set; } = string.Empty;
}
