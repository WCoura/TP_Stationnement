using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TP_Stationnement.Migrations
{
    /// <inheritdoc />
    public partial class CreationInitiale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vehicules",
                columns: table => new
                {
                    vehicule_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    marque = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modele = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    couleur = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicules", x => x.vehicule_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sessions_stationnement",
                columns: table => new
                {
                    session_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    vehicule_id = table.Column<int>(type: "int", nullable: false),
                    heure_entree = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    heure_sortie = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    montant_facture = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    est_terminee = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions_stationnement", x => x.session_id);
                    table.ForeignKey(
                        name: "FK_sessions_stationnement_vehicules_vehicule_id",
                        column: x => x.vehicule_id,
                        principalTable: "vehicules",
                        principalColumn: "vehicule_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audits_modification",
                columns: table => new
                {
                    audit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    session_id = table.Column<int>(type: "int", nullable: false),
                    champ_modifie = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ancienne_valeur = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nouvelle_valeur = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    date_modification = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    modifie_par = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audits_modification", x => x.audit_id);
                    table.ForeignKey(
                        name: "FK_audits_modification_sessions_stationnement_session_id",
                        column: x => x.session_id,
                        principalTable: "sessions_stationnement",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_audits_modification_session_id",
                table: "audits_modification",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_stationnement_vehicule_id",
                table: "sessions_stationnement",
                column: "vehicule_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audits_modification");

            migrationBuilder.DropTable(
                name: "sessions_stationnement");

            migrationBuilder.DropTable(
                name: "vehicules");
        }
    }
}
