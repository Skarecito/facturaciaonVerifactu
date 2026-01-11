using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaToTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cierres_ejercicios_Tenants_tenant_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_cierres_ejercicios_tenant_id",
                table: "cierres_ejercicios");

            migrationBuilder.AddColumn<string>(
                name: "schema",
                table: "Tenants",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.AddColumn<decimal>(
                name: "cuota_retencion",
                table: "presupuestos",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_retencion",
                table: "presupuestos",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_recargo",
                table: "presupuestos",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "recargo_equivalencia",
                table: "lineas_presupuesto",
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
                name: "recargo_equivalencia",
                table: "lineas_albaran",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "actualizado_en",
                table: "facturas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "bloqueada",
                table: "facturas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "cuota_recargo",
                table: "facturas",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "cuota_retencion",
                table: "facturas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_retencion",
                table: "facturas",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_recargo",
                table: "facturas",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notas_fiscales",
                table: "clientes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "porcentaje_retencion_defecto",
                table: "clientes",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "regimen_recargo_equivalencia",
                table: "clientes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "tipo_cliente",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_importe",
                table: "cierres_ejercicios",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(28,2)");

            migrationBuilder.AddColumn<DateTime>(
                name: "actualizado_en",
                table: "cierres_ejercicios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "creado_en",
                table: "cierres_ejercicios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "esta_abierto",
                table: "cierres_ejercicios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_reapertura",
                table: "cierres_ejercicios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_reapertura",
                table: "cierres_ejercicios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ruta_libro_facturas",
                table: "cierres_ejercicios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ruta_resumen_iva",
                table: "cierres_ejercicios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_base_imponible",
                table: "cierres_ejercicios",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_iva",
                table: "cierres_ejercicios",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_recargo",
                table: "cierres_ejercicios",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "total_retencion",
                table: "cierres_ejercicios",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "usuario_id",
                table: "cierres_ejercicios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "usuario_reapertura_id",
                table: "cierres_ejercicios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_recargo",
                table: "albaranes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_cierres_ejercicios_tenant_id_ejercicio_esta_abierto",
                table: "cierres_ejercicios",
                columns: new[] { "tenant_id", "ejercicio", "esta_abierto" },
                unique: true,
                filter: "esta_abierto = false");

            migrationBuilder.CreateIndex(
                name: "IX_cierres_ejercicios_usuario_id",
                table: "cierres_ejercicios",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_cierres_ejercicios_usuario_reapertura_id",
                table: "cierres_ejercicios",
                column: "usuario_reapertura_id");

            migrationBuilder.AddForeignKey(
                name: "FK_cierres_ejercicios_Tenants_tenant_id",
                table: "cierres_ejercicios",
                column: "tenant_id",
                principalTable: "Tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cierres_ejercicios_usuarios_usuario_id",
                table: "cierres_ejercicios",
                column: "usuario_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cierres_ejercicios_usuarios_usuario_reapertura_id",
                table: "cierres_ejercicios",
                column: "usuario_reapertura_id",
                principalTable: "usuarios",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cierres_ejercicios_Tenants_tenant_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropForeignKey(
                name: "FK_cierres_ejercicios_usuarios_usuario_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropForeignKey(
                name: "FK_cierres_ejercicios_usuarios_usuario_reapertura_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_cierres_ejercicios_tenant_id_ejercicio_esta_abierto",
                table: "cierres_ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_cierres_ejercicios_usuario_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropIndex(
                name: "IX_cierres_ejercicios_usuario_reapertura_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "schema",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "iva_defecto",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia_defecto",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "tipo_iva",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "cuota_retencion",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "porcentaje_retencion",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "total_recargo",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_presupuesto");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_factura");

            migrationBuilder.DropColumn(
                name: "recargo_equivalencia",
                table: "lineas_albaran");

            migrationBuilder.DropColumn(
                name: "actualizado_en",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "bloqueada",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "cuota_recargo",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "cuota_retencion",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "porcentaje_retencion",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "total_recargo",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "notas_fiscales",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "porcentaje_retencion_defecto",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "regimen_recargo_equivalencia",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "tipo_cliente",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "actualizado_en",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "creado_en",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "esta_abierto",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "fecha_reapertura",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "motivo_reapertura",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "ruta_libro_facturas",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "ruta_resumen_iva",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "total_base_imponible",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "total_iva",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "total_recargo",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "total_retencion",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "usuario_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "usuario_reapertura_id",
                table: "cierres_ejercicios");

            migrationBuilder.DropColumn(
                name: "total_recargo",
                table: "albaranes");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_importe",
                table: "cierres_ejercicios",
                type: "numeric(28,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_cierres_ejercicios_tenant_id",
                table: "cierres_ejercicios",
                column: "tenant_id");

            migrationBuilder.AddForeignKey(
                name: "FK_cierres_ejercicios_Tenants_tenant_id",
                table: "cierres_ejercicios",
                column: "tenant_id",
                principalTable: "Tenants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
