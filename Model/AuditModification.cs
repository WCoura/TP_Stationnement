using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TP_Stationnement.Model;

// Journal d'inspection. On ajoute des lignes, on ne les modifie pas
// et on ne les efface pas. Chaque correction de sortie laisse une trace.
[Table("audits_modification")]
public class AuditModification
{
    [Key]
    [Column("audit_id")]
    public int AuditId { get; set; }

    [Column("session_id")]
    public int SessionId { get; set; }

    [Column("champ_modifie")]
    [StringLength(50)]
    public string ChampModifie { get; set; } = string.Empty;

    [Column("ancienne_valeur")]
    [StringLength(100)]
    public string AncienneValeur { get; set; } = string.Empty;

    [Column("nouvelle_valeur")]
    [StringLength(100)]
    public string NouvelleValeur { get; set; } = string.Empty;

    [Column("date_modification")]
    public DateTime DateModification { get; set; }

    [Column("modifie_par")]
    [StringLength(80)]
    public string ModifiePar { get; set; } = string.Empty;
}
