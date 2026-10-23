namespace TP_Stationnement.Model;

// Petit aide-mémoire pour les heures.
// On enregistre toujours de l'UTC (DateTime.UtcNow), comme demandé.
public static class Horloge
{
    // MySQL ne conserve pas le "Kind" de .NET. En relisant, l'heure
    // revient Unspecified, même si la valeur est bien de l'UTC.
    // On remet le Kind à UTC sans décaler l'horloge.
    // ToUniversalTime() serait une erreur ici : il traiterait l'heure
    // comme locale et enlèverait 4 ou 5 heures.
    public static DateTime CommeUtc(DateTime heure)
    {
        if (heure.Kind == DateTimeKind.Utc)
            return heure;

        if (heure.Kind == DateTimeKind.Local)
            return heure.ToUniversalTime();

        return DateTime.SpecifyKind(heure, DateTimeKind.Utc);
    }
}
