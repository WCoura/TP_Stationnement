-- Tables du stationnement.
-- Lancer d'abord Scripts/00_creer_base.sql, puis ce fichier.
USE stationnement_db;

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    CREATE TABLE `vehicules` (
        `vehicule_id` int NOT NULL AUTO_INCREMENT,
        `marque` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `modele` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `couleur` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `description` varchar(200) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_vehicules` PRIMARY KEY (`vehicule_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    CREATE TABLE `sessions_stationnement` (
        `session_id` int NOT NULL AUTO_INCREMENT,
        `vehicule_id` int NOT NULL,
        `heure_entree` datetime(6) NOT NULL,
        `heure_sortie` datetime(6) NULL,
        `montant_facture` decimal(10,2) NULL,
        `est_terminee` tinyint(1) NOT NULL DEFAULT FALSE,
        CONSTRAINT `PK_sessions_stationnement` PRIMARY KEY (`session_id`),
        CONSTRAINT `FK_sessions_stationnement_vehicules_vehicule_id` FOREIGN KEY (`vehicule_id`) REFERENCES `vehicules` (`vehicule_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    CREATE TABLE `audits_modification` (
        `audit_id` int NOT NULL AUTO_INCREMENT,
        `session_id` int NOT NULL,
        `champ_modifie` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `ancienne_valeur` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `nouvelle_valeur` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `date_modification` datetime(6) NOT NULL,
        `modifie_par` varchar(80) CHARACTER SET utf8mb4 NOT NULL,
        CONSTRAINT `PK_audits_modification` PRIMARY KEY (`audit_id`),
        CONSTRAINT `FK_audits_modification_sessions_stationnement_session_id` FOREIGN KEY (`session_id`) REFERENCES `sessions_stationnement` (`session_id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    CREATE INDEX `IX_audits_modification_session_id` ON `audits_modification` (`session_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    CREATE INDEX `IX_sessions_stationnement_vehicule_id` ON `sessions_stationnement` (`vehicule_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261023130713_CreationInitiale') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261023130713_CreationInitiale', '8.0.11');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

