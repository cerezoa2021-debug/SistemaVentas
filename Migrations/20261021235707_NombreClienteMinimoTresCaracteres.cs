using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentas.Migrations
{
    /// <inheritdoc />
    public partial class NombreClienteMinimoTresCaracteres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes",
                sql: "length(trim(\"Nombre\")) BETWEEN 3 AND 120");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes",
                sql: "length(trim(\"Nombre\")) BETWEEN 1 AND 120");
        }
    }
}
