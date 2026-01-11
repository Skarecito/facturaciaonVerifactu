using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLogoAndRegistroMercantilToTenants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "folio",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "hoja",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inscripcion",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "libro",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "logo",
                table: "Tenants",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "registro_mercantil",
                table: "Tenants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "seccion",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tomo",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tenants_schema",
                table: "Tenants",
                column: "schema",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tenants_schema",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "folio",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "hoja",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "inscripcion",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "libro",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "logo",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "registro_mercantil",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "seccion",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "tomo",
                table: "Tenants");
        }
    }
}
