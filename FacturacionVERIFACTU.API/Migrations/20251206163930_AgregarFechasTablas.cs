using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FacturacionVERIFACTU.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechasTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fecha_alta",
                table: "productos",
                newName: "fecha_modificacion");

            migrationBuilder.RenameColumn(
                name: "fecha_alta",
                table: "clientes",
                newName: "fecha_modificaion");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "productos",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "ciudad",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                table: "clientes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "pais",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "pais",
                table: "clientes");

            migrationBuilder.RenameColumn(
                name: "fecha_modificacion",
                table: "productos",
                newName: "fecha_alta");

            migrationBuilder.RenameColumn(
                name: "fecha_modificaion",
                table: "clientes",
                newName: "fecha_alta");

            migrationBuilder.AlterColumn<string>(
                name: "ciudad",
                table: "clientes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
