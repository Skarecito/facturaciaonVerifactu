using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class CreateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "text", nullable: true),
                    codigo_postal = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    municipio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    provincia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    empresa_id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero_factura = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    serie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_expedicion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_operacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    base_imponible = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    tipo_iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    cuota_iva = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    cliente_nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cliente_nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    hash_verifactu = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: true),
                    fecha_envio_verifactu = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    estado_verifactu = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id);
                    table.ForeignKey(
                        name: "FK_facturas_empresas_empresa_id",
                        column: x => x.empresa_id,
                        principalTable: "empresas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lineas_factura",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    factura_id = table.Column<Guid>(type: "uuid", nullable: false),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(10,3)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    tipo_iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineas_factura", x => x.id);
                    table.ForeignKey(
                        name: "FK_lineas_factura_facturas_factura_id",
                        column: x => x.factura_id,
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_empresas_nif",
                table: "empresas",
                column: "nif",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_empresa_id_serie_numero_factura",
                table: "facturas",
                columns: new[] { "empresa_id", "serie", "numero_factura" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_estado_verifactu",
                table: "facturas",
                column: "estado_verifactu");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_fecha_expedicion",
                table: "facturas",
                column: "fecha_expedicion");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_factura_factura_id",
                table: "lineas_factura",
                column: "factura_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lineas_factura");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "empresas");
        }
    }
}
