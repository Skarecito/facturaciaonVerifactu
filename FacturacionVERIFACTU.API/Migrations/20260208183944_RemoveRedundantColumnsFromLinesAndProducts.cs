using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRedundantColumnsFromLinesAndProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "iva",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "iva",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "iva",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "iva",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_albaran");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "iva",
                table: "productos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_equivalencia",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva",
                table: "lineas_factura",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_equivalencia",
                table: "lineas_factura",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_factura",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva",
                table: "lineas_albaran",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_equivalencia",
                table: "lineas_albaran",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_albaran",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
