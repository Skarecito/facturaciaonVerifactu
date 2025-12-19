using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPresupuestosYLineas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "total",
                table: "presupuestos",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,2)");

            migrationBuilder.AddColumn<int>(
                name: "ejercicio",
                table: "presupuestos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "presupuestos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_modificacion",
                table: "presupuestos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "base_imponible",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_descuento",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_iva",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_descuento",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ejercicio",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "fecha_modificacion",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "base_imponible",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "importe_descuento",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "importe_iva",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "porcentaje_descuento",
                table: "lineas_presupuesto");

            migrationBuilder.AlterColumn<decimal>(
                name: "total",
                table: "presupuestos",
                type: "numeric(28,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");
        }
    }
}
