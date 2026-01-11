using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    codigo_postal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    provincia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_alta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cierres_ejercicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    fecha_cierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    hash_final = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    total_facturas = table.Column<int>(type: "integer", nullable: false),
                    total_importe = table.Column<decimal>(type: "numeric(28,2)", nullable: false),
                    enviado_verifactu = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_envio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cierres_ejercicios", x => x.id);
                    table.ForeignKey(
                        name: "FK_cierres_ejercicios_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nif = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    codigo_postal = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    provincia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_alta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                    table.ForeignKey(
                        name: "FK_clientes_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    precio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    unidad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_alta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.id);
                    table.ForeignKey(
                        name: "FK_productos_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "series_facturacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    proximo_numero = table.Column<int>(type: "integer", nullable: false),
                    ejercicio = table.Column<int>(type: "integer", nullable: false),
                    formato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_series_facturacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_series_facturacion_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_creaccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    serie_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    huella = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    huella_anterior = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    enviada_verifactu = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_envio_verifactu = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id);
                    table.ForeignKey(
                        name: "FK_facturas_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_facturas_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_facturas_series_facturacion_serie_id",
                        column: x => x.serie_id,
                        principalTable: "series_facturacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "presupuestos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    serie_id = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_validez = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    base_imponible = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(28,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SerieNumeracionId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuestos", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuestos_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuestos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_presupuestos_series_facturacion_SerieNumeracionId",
                        column: x => x.SerieNumeracionId,
                        principalTable: "series_facturacion",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Token = table.Column<string>(type: "text", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Revoked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lineas_factura",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    factura_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
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
                    table.ForeignKey(
                        name: "FK_lineas_factura_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "albaranes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tenant_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    serie_id = table.Column<int>(type: "integer", nullable: false),
                    presupuesto_id = table.Column<int>(type: "integer", nullable: true),
                    numero = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha_emision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_entrega = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    base_imponible = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total_iva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    direccion_entrega = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    facturado = table.Column<bool>(type: "boolean", nullable: false),
                    factura_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_albaranes", x => x.id);
                    table.ForeignKey(
                        name: "FK_albaranes_Tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "Tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_albaranes_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_albaranes_facturas_factura_id",
                        column: x => x.factura_id,
                        principalTable: "facturas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_albaranes_presupuestos_presupuesto_id",
                        column: x => x.presupuesto_id,
                        principalTable: "presupuestos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_albaranes_series_facturacion_serie_id",
                        column: x => x.serie_id,
                        principalTable: "series_facturacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lineas_presupuesto",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    presupuesto_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineas_presupuesto", x => x.id);
                    table.ForeignKey(
                        name: "FK_lineas_presupuesto_presupuestos_presupuesto_id",
                        column: x => x.presupuesto_id,
                        principalTable: "presupuestos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lineas_presupuesto_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "lineas_albaran",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    albaran_id = table.Column<int>(type: "integer", nullable: false),
                    producto_id = table.Column<int>(type: "integer", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cantidad = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    iva = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    importe = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lineas_albaran", x => x.id);
                    table.ForeignKey(
                        name: "FK_lineas_albaran_albaranes_albaran_id",
                        column: x => x.albaran_id,
                        principalTable: "albaranes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lineas_albaran_productos_producto_id",
                        column: x => x.producto_id,
                        principalTable: "productos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_albaranes_cliente_id",
                table: "albaranes",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_albaranes_factura_id",
                table: "albaranes",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "IX_albaranes_presupuesto_id",
                table: "albaranes",
                column: "presupuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_albaranes_serie_id",
                table: "albaranes",
                column: "serie_id");

            migrationBuilder.CreateIndex(
                name: "IX_albaranes_tenant_id_numero",
                table: "albaranes",
                columns: new[] { "tenant_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cierres_ejercicios_tenant_id",
                table: "cierres_ejercicios",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_tenant_id_nif",
                table: "clientes",
                columns: new[] { "tenant_id", "nif" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_cliente_id",
                table: "facturas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_serie_id",
                table: "facturas",
                column: "serie_id");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_tenant_id_numero",
                table: "facturas",
                columns: new[] { "tenant_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lineas_albaran_albaran_id",
                table: "lineas_albaran",
                column: "albaran_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_albaran_producto_id",
                table: "lineas_albaran",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_factura_factura_id",
                table: "lineas_factura",
                column: "factura_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_factura_producto_id",
                table: "lineas_factura",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_presupuesto_presupuesto_id",
                table: "lineas_presupuesto",
                column: "presupuesto_id");

            migrationBuilder.CreateIndex(
                name: "IX_lineas_presupuesto_producto_id",
                table: "lineas_presupuesto",
                column: "producto_id");

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_cliente_id",
                table: "presupuestos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_serie_id",
                table: "presupuestos",
                column: "serie_id");

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_tenant_id_numero",
                table: "presupuestos",
                columns: new[] { "tenant_id", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_productos_tenant_id_codigo",
                table: "productos",
                columns: new[] { "tenant_id", "codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UsuarioId",
                table: "RefreshTokens",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_series_facturacion_tenant_id_codigo_ejercicio",
                table: "series_facturacion",
                columns: new[] { "tenant_id", "codigo", "ejercicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_nif",
                table: "Tenants",
                column: "nif",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_tenant_id_email",
                table: "usuarios",
                columns: new[] { "tenant_id", "email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cierres_ejercicios");

            migrationBuilder.DropTable(
                name: "lineas_albaran");

            migrationBuilder.DropTable(
                name: "lineas_factura");

            migrationBuilder.DropTable(
                name: "lineas_presupuesto");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "albaranes");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "presupuestos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "series_facturacion");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
