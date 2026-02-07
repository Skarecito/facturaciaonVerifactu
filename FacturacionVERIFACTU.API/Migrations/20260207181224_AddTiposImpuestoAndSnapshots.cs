using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposImpuestoAndSnapshots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_series_facturacion_tenant_id_codigo_ejercicio",
                table: "series_facturacion");

            migrationBuilder.DropColumn(
                name: "iva_defecto",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia_defecto",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "tipo_iva",
                table: "productos");

            migrationBuilder.AddColumn<int>(
                name: "tipo_impuesto_id",
                table: "productos",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "importe_recargo",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)");

            migrationBuilder.AddColumn<decimal>(
                name: "iva_percent_snapshot",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "re_percent_snapshot",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_impuesto_id",
                table: "lineas_presupuesto",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_presupuesto",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "base_imponible",
                table: "lineas_factura",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_iva",
                table: "lineas_factura",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_recargo",
                table: "lineas_factura",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva_percent_snapshot",
                table: "lineas_factura",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "re_percent_snapshot",
                table: "lineas_factura",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_impuesto_id",
                table: "lineas_factura",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_factura",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "importe_recargo",
                table: "lineas_albaran",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "iva_percent_snapshot",
                table: "lineas_albaran",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "re_percent_snapshot",
                table: "lineas_albaran",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "tipo_impuesto_id",
                table: "lineas_albaran",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_linea",
                table: "lineas_albaran",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "tipos_impuesto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    porcentaje_iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    porcentaje_recargo = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: true),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_impuesto", x => x.id);
                    table.ForeignKey(
                        name: "FK_tipos_impuesto_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_series_facturacion_tenant_id_codigo_ejercicio_tipo_documento",
                table: "series_facturacion",
                columns: new[] { "tenant_id", "codigo", "ejercicio", "tipo_documento" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_tipo_impuesto_id",
                table: "productos",
                column: "tipo_impuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_presupuesto_tipo_impuesto_id",
                table: "lineas_presupuesto",
                column: "tipo_impuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_factura_tipo_impuesto_id",
                table: "lineas_factura",
                column: "tipo_impuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_albaran_tipo_impuesto_id",
                table: "lineas_albaran",
                column: "tipo_impuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_tipos_impuesto_tenant_id_nombre",
                table: "tipos_impuesto",
                columns: new[] { "tenant_id", "nombre" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_lineas_albaran_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_albaran",
                column: "tipo_impuesto_id",
                principalTable: "tipos_impuesto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lineas_factura_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_factura",
                column: "tipo_impuesto_id",
                principalTable: "tipos_impuesto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_lineas_presupuesto_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_presupuesto",
                column: "tipo_impuesto_id",
                principalTable: "tipos_impuesto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_productos_tipos_impuesto_tipo_impuesto_id",
                table: "productos",
                column: "tipo_impuesto_id",
                principalTable: "tipos_impuesto",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_lineas_albaran_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_albaran");

            migrationBuilder.DropForeignKey(
                name: "FK_lineas_factura_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_factura");

            migrationBuilder.DropForeignKey(
                name: "FK_lineas_presupuesto_tipos_impuesto_tipo_impuesto_id",
                table: "lineas_presupuesto");

            migrationBuilder.DropForeignKey(
                name: "FK_productos_tipos_impuesto_tipo_impuesto_id",
                table: "productos");

            migrationBuilder.DropTable(
                name: "tipos_impuesto");

            migrationBuilder.DropIndex(
                name: "IX_series_facturacion_tenant_id_codigo_ejercicio_tipo_documento",
                table: "series_facturacion");

            migrationBuilder.DropIndex(
                name: "IX_productos_tipo_impuesto_id",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_lineas_presupuesto_tipo_impuesto_id",
                table: "lineas_presupuesto");

            migrationBuilder.DropIndex(
                name: "IX_lineas_factura_tipo_impuesto_id",
                table: "lineas_factura");

            migrationBuilder.DropIndex(
                name: "IX_lineas_albaran_tipo_impuesto_id",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "tipo_impuesto_id",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "iva_percent_snapshot",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "re_percent_snapshot",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "tipo_impuesto_id",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "base_imponible",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "importe_iva",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "importe_recargo",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "iva_percent_snapshot",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "re_percent_snapshot",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "tipo_impuesto_id",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "importe_recargo",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "iva_percent_snapshot",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "re_percent_snapshot",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "tipo_impuesto_id",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "total_linea",
                table: "lineas_albaran");

            migrationBuilder.AddColumn<decimal>(
                name: "iva_defecto",
                table: "productos",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_equivalencia_defecto",
                table: "productos",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_iva",
                table: "productos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "importe_recargo",
                table: "lineas_presupuesto",
                type: "numeric(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_series_facturacion_tenant_id_codigo_ejercicio",
                table: "series_facturacion",
                columns: new[] { "tenant_id", "codigo", "ejercicio" },
                unique: true);
        }
    }
}
