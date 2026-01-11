using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposVERIFACTU : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_presupuestos_series_facturacion_SerieNumeracionId",
            //    table: "presupuestos");

            //migrationBuilder.DropIndex(
            //    name: "IX_presupuestos_SerieNumeracionId",
            //    table: "presupuestos");

            //migrationBuilder.DropColumn(
            //    name: "SerieNumeracionId",
            //    table: "presupuestos");

            //migrationBuilder.AddColumn<decimal>(
            //    name: "base_imponible",
            //    table: "lineas_albaran",
            //    type: "numeric(18,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "importe_descuento",
            //    table: "lineas_albaran",
            //    type: "numeric(18,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "importe_iva",
            //    table: "lineas_albaran",
            //    type: "numeric(18,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "porcentaje_descuento",
            //    table: "lineas_albaran",
            //    type: "numeric(5,2)",
            //    nullable: false,
            //    defaultValue: 0m);

            //migrationBuilder.AddColumn<int>(
            //    name: "ejercicio",
            //    table: "facturas",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "numero_factura_rectificada",
            //    table: "facturas",
            //    type: "character varying(50)",
            //    maxLength: 50,
            //    nullable: true);

            //migrationBuilder.AddColumn<byte[]>(
            //    name: "qr_verifactu",
            //    table: "facturas",
            //    type: "bytea",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "tipo_factura_verifactu",
            //    table: "facturas",
            //    type: "character varying(2)",
            //    maxLength: 2,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "tipo_rectificacion",
            //    table: "facturas",
            //    type: "character varying(20)",
            //    maxLength: 20,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "url_verifactu",
            //    table: "facturas",
            //    type: "character varying(500)",
            //    maxLength: 500,
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "ejercicio",
            //    table: "albaranes",
            //    type: "integer",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "fecha_creacion",
            //    table: "albaranes",
            //    type: "timestamp with time zone",
            //    nullable: false,
            //    defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "fecha_modificacion",
            //    table: "albaranes",
            //    type: "timestamp with time zone",
            //    nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_serie_id",
                table: "presupuestos",
                column: "serie_id");

            migrationBuilder.AddForeignKey(
                name: "FK_presupuestos_series_facturacion_serie_id",
                table: "presupuestos",
                column: "serie_id",
                principalTable: "series_facturacion",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_presupuestos_series_facturacion_serie_id",
                table: "presupuestos");

            migrationBuilder.DropIndex(
                name: "IX_presupuestos_serie_id",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "base_imponible",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "importe_descuento",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "importe_iva",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "porcentaje_descuento",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "ejercicio",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "numero_factura_rectificada",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "qr_verifactu",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "tipo_factura_verifactu",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "tipo_rectificacion",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "url_verifactu",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "ejercicio",
                table: "albaranes");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "albaranes");

            migrationBuilder.DropColumn(
                name: "fecha_modificacion",
                table: "albaranes");

            migrationBuilder.AddColumn<int>(
                name: "SerieNumeracionId",
                table: "presupuestos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_SerieNumeracionId",
                table: "presupuestos",
                column: "SerieNumeracionId");

            migrationBuilder.AddForeignKey(
                name: "FK_presupuestos_series_facturacion_SerieNumeracionId",
                table: "presupuestos",
                column: "SerieNumeracionId",
                principalTable: "series_facturacion",
                principalColumn: "id");
        }
    }
}
