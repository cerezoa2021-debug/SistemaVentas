using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentas.Migrations
{
    /// <inheritdoc />
    public partial class ValidacionesFormato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Clientes\" SET \"Celular\" = NULL WHERE \"Celular\" IS NOT NULL AND (\"Celular\" GLOB '*[^0-9]*' OR length(\"Celular\") NOT BETWEEN 7 AND 8);");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes",
                sql: "\"Celular\" IS NULL OR (\"Celular\" NOT GLOB '*[^0-9]*' AND length(\"Celular\") BETWEEN 7 AND 8)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Nombre_Format",
                table: "Clientes",
                sql: "\"Nombre\" NOT GLOB '*[0-9]*'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Categorias_Nombre_Format",
                table: "Categorias",
                sql: "\"Nombre\" NOT GLOB '*[0-9]*'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Nombre_Format",
                table: "Clientes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Categorias_Nombre_Format",
                table: "Categorias");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes",
                sql: "\"Celular\" IS NULL OR length(\"Celular\") <= 20");
        }
    }
}
