using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCampoSerieBloqueado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "bloqueada",
                table: "series_facturacion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "factura_id",
                table: "presupuestos",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bloqueada",
                table: "series_facturacion");

            migrationBuilder.DropColumn(
                name: "factura_id",
                table: "presupuestos");
        }
    }
}
