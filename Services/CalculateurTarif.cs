using System.Globalization;
using TP_Stationnement.Model;

namespace TP_Stationnement.Services;

// La grille du travail :
// - on facture par tranches de 15 minutes
// - une tranche commencée est due au complet
// - chaque heure complète a son propre tarif, moins cher que 4 tranches
// - après chaque heure, les tranches de 15 min recommencent à zéro
// - le tarif de la journée (24 h) est un plafond pour ce bloc de 24 h
//
// Avec les chiffres de appsettings.json (1,50 $ / 5,00 $ / 40,00 $) :
//   10 min          -> 1 tranche                         =  1,50 $
//   50 min          -> 4 tranches                        =  6,00 $
//   1 h 00          -> 1 heure (moins cher que 4 tranches) =  5,00 $
//   1 h 37          -> 1 heure + 3 tranches              =  9,50 $
//   3 h 00          -> 3 heures                          = 15,00 $
//   10 h            -> le calcul donne 50 $, plafonné    = 40,00 $
//   24 h            -> 1 journée                         = 40,00 $
//   25 h 10         -> 1 journée + 1 heure + 1 tranche   = 46,50 $
//
// On ne remplace pas 4 tranches par une heure tant que l'heure
// n'est pas complétée. C'est pour ça que 50 min (6 $) coûte plus cher
// qu'une heure pile (5 $). Le seul plafond du travail, c'est la journée.
public class CalculateurTarif
{
    private readonly decimal _tarif15;
    private readonly decimal _tarifHeure;
    private readonly decimal _tarifJour;

    public CalculateurTarif(IConfiguration configuration)
    {
        _tarif15 = LireTarif(configuration, "tarif15min");
        _tarifHeure = LireTarif(configuration, "tarifHeure");
        _tarifJour = LireTarif(configuration, "tarifJour");
    }

    public decimal CalculerMontant(DateTime heureEntree, DateTime heureSortie)
    {
        var entree = Horloge.CommeUtc(heureEntree);
        var sortie = Horloge.CommeUtc(heureSortie);

        if (sortie < entree)
            throw new ArgumentException("L'heure de sortie est avant l'heure d'entrée.");

        long ticks = (sortie - entree).Ticks;
        if (ticks <= 0)
            return 0m;

        // Une minute commencée compte. On reste en ticks pour ne pas
        // se faire jouer par les arrondis d'un double.
        int minutesTotales = (int)((ticks + TimeSpan.TicksPerMinute - 1) / TimeSpan.TicksPerMinute);

        const int minutesParJour = 24 * 60;
        int joursComplets = minutesTotales / minutesParJour;
        int minutesApresJours = minutesTotales % minutesParJour;

        int heuresCompletes = minutesApresJours / 60;
        int minutesApresHeures = minutesApresJours % 60;

        // 0 minute restante = 0 tranche. 1 à 15 min = 1 tranche, etc.
        int tranches = 0;
        if (minutesApresHeures > 0)
            tranches = (int)Math.Ceiling(minutesApresHeures / 15.0);

        decimal montantJours = joursComplets * _tarifJour;
        decimal montantReste = (heuresCompletes * _tarifHeure) + (tranches * _tarif15);

        // Plafond : pour la portion de moins de 24 h, on ne dépasse pas
        // le tarif journée. Le jour suivant repart à neuf.
        if (montantReste > _tarifJour)
            montantReste = _tarifJour;

        // On garde les cents, comme sur une facture.
        return decimal.Round(montantJours + montantReste, 2, MidpointRounding.AwayFromZero);
    }

    // Windows ici est souvent en français : la virgule est le séparateur.
    // Dans appsettings.json le point est utilisé, alors on lit en invariant.
    private static decimal LireTarif(IConfiguration configuration, string cle)
    {
        string? texte = configuration[cle];

        if (string.IsNullOrWhiteSpace(texte)
            || !decimal.TryParse(texte, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal valeur)
            || valeur <= 0)
        {
            throw new InvalidOperationException(
                "Le tarif \"" + cle + "\" est manquant ou invalide dans appsettings.json.");
        }

        return valeur;
    }
}
