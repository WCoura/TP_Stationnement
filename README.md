
# Stationnement — TP POO2

Institut Grasset · automne 2026 · 420.B0

API pour enregistrer l'entrée et la sortie des véhicules, consulter les sessions et calculer le montant à payer. Il n'y a pas de suppression : ni les véhicules, ni les sessions, ni le journal.

## Équipe

Wallace Coura et Adryen Moser.

| Partie | Responsable |
| --- | --- |
| Modèles (`Vehicule`, `SessionStationnement`, `AuditModification`), `AppDbContext` et lien entre les tables | Wallace Coura |
| Migrations Entity Framework, scripts SQL et connexion MySQL dans `appsettings.json` | Wallace Coura |
| Pattern Repository, injection de dépendances et CORS dans `Program.cs` | Wallace Coura |
| Calcul du tarif (`CalculateurTarif`) : tranches de 15 min, heure et plafond de la journée | Adryen Moser |
| Contrôleurs `Vehicules` et `Sessions` (entrée, sortie, correction, listes, audit) et validation | Adryen Moser |
| Swagger et guichet web (`wwwroot/index.html`) | Adryen Moser |

Les deux ont relu l'ensemble avant la remise, surtout le calcul du montant et le journal d'audit.

## Lancer le projet

Il te faut le SDK .NET 8 et MySQL 8 (ou MariaDB avec XAMPP).

1. Crée la base. Dans MySQL Workbench ou phpMyAdmin :

```sql
CREATE DATABASE stationnement_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
```

Le fichier `Scripts/00_creer_base.sql` contient la même chose.

2. Ouvre `appsettings.json` et mets ton mot de passe après `password=`. Avec XAMPP le mot de passe de `root` est souvent vide, donc la chaîne peut rester telle quelle. Évite le point-virgule dans le mot de passe.

```text
server=127.0.0.1;port=3306;database=stationnement_db;user=root;password=;CharSet=utf8mb4;
```

`127.0.0.1` plutôt que `localhost`, sinon Windows essaie parfois l'IPv6 et MySQL ne répond pas.

3. Applique les tables, dans le dossier du projet :

```text
dotnet tool restore
dotnet ef database update
```

Si `dotnet ef` n'est pas reconnu, la commande `dotnet tool restore` installe l'outil local décrit dans `dotnet-tools.json`.

Autre option : exécuter `Scripts/creation_tables.sql` après avoir sélectionné la base `stationnement_db`.

Dans Visual Studio, ouvre `TP_Stationnement.slnx`.

4. Démarre l'API :

```text
dotnet run
```

- Guichet : http://localhost:5184/
- Swagger : http://localhost:5184/swagger

Le profil HTTPS est dans `Properties/launchSettings.json` si tu lances depuis Visual Studio.

## Tarifs

Les trois valeurs sont dans `appsettings.json` et lues avec `IConfiguration`.

| Clé | Valeur | Rôle |
| --- | --- | --- |
| tarif15min | 1,50 $ | une tranche de 15 minutes entamée |
| tarifHeure | 5,00 $ | une heure complète (moins cher que 4 × 1,50 $) |
| tarifJour | 40,00 $ | plafond de chaque bloc de 24 h |

Exemples, les mêmes que dans `Services/CalculateurTarif.cs` :

| Durée | Détail | Montant |
| --- | --- | --- |
| 10 min | 1 tranche | 1,50 $ |
| 50 min | 4 tranches | 6,00 $ |
| 1 h 00 | 1 heure | 5,00 $ |
| 1 h 37 | 1 heure + 3 tranches | 9,50 $ |
| 3 h 00 | 3 heures | 15,00 $ |
| 10 h | 50 $ plafonnés à la journée | 40,00 $ |
| 24 h | 1 journée | 40,00 $ |
| 25 h 10 | 1 journée + 1 heure + 1 tranche | 46,50 $ |

50 minutes coûtent plus cher qu'une heure pile. L'heure n'est pas un plafond : elle s'applique seulement quand elle est complétée. Le plafond, c'est la journée, et il recommence après chaque 24 h.

Pour le vérifier sans attendre :

- Une sortie faite tout de suite après l'entrée dure quelques secondes, donc une tranche : 1,50 $.
- Pour 1 h 37, on ferme la session, puis on corrige l'heure de sortie (guichet ou `PUT /sessions/{id}/modifier`) à l'entrée plus 1 h 37. Le montant passe à 9,50 $ et le journal reçoit la correction.

L'heure d'entrée est toujours `DateTime.UtcNow` côté serveur. Le client ne peut pas l'envoyer.

## Routes

Véhicules

- `GET /vehicules`
- `GET /vehicules/{id}`
- `POST /vehicules`
- `PUT /vehicules/{id}`

Sessions

- `POST /sessions/entree` avec `{ "vehiculeId": 1 }`
- `PUT /sessions/{id}/sortie`
- `PUT /sessions/{id}/modifier` avec `{ "heureSortie": "2026-10-23T18:30:00Z", "modifiePar": "guichet" }`
- `GET /sessions/actives`
- `GET /sessions/periode?dateDebut=2026-10-01&dateFin=2026-10-31`
- `GET /sessions/{id}/audit`

`dateFin` est incluse. Une session déjà terminée ne peut pas être fermée une deuxième fois. Une correction n'est acceptée que sur une session terminée, et elle écrit dans `audits_modification` avant le `SaveChanges`.

## Guichet

La page dans `wwwroot/index.html` parle à la même API :

- formulaire d'entrée (nouveau véhicule, ou un véhicule déjà connu)
- bouton de sortie avec le montant
- sessions en cours, rafraîchies toutes les 20 secondes
- recherche par période
- correction d'une heure de sortie et lecture du journal

CORS `AllowAll` est activé seulement en développement, comme demandé. La page du guichet est servie par l'API, donc elle n'en a pas besoin.

## Si MariaDB (XAMPP) refuse la migration

Dans `Program.cs`, la version est fixée à MySQL 8.0.36. Pour un MariaDB 10.4, remplace par `new Version(10, 4, 32)`, efface le dossier `Migrations` s'il a été généré pour MySQL 8, puis relance `dotnet ef migrations add CreationInitiale` et `dotnet ef database update`.
