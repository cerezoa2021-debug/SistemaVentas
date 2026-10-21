using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaVentas.Migrations
{
    /// <inheritdoc />
    public partial class ValidacionesTresCapas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Cambio_Range",
                table: "Ventas",
                sql: "CAST(\"Cambio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Recibido_Range",
                table: "Ventas",
                sql: "CAST(\"Recibido\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ventas_Total_Range",
                table: "Ventas",
                sql: "CAST(\"Total\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Nombre_Length",
                table: "Productos",
                sql: "length(trim(\"Nombre\")) BETWEEN 1 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Precio_Range",
                table: "Productos",
                sql: "CAST(\"Precio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock_Range",
                table: "Productos",
                sql: "\"Stock\" BETWEEN 0 AND 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_StockMinimo_Range",
                table: "Productos",
                sql: "\"StockMinimo\" BETWEEN 0 AND 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetallesVenta_Cantidad_Range",
                table: "DetallesVenta",
                sql: "\"Cantidad\" BETWEEN 1 AND 1000000");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetallesVenta_Precio_Range",
                table: "DetallesVenta",
                sql: "CAST(\"Precio\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DetallesVenta_Subtotal_Range",
                table: "DetallesVenta",
                sql: "CAST(\"Subtotal\" AS NUMERIC) BETWEEN 0 AND 99999999.99");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes",
                sql: "\"Celular\" IS NULL OR length(\"Celular\") <= 20");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Email_Length",
                table: "Clientes",
                sql: "\"Email\" IS NULL OR length(\"Email\") <= 254");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes",
                sql: "length(trim(\"Nombre\")) BETWEEN 1 AND 120");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Categorias_Nombre_Length",
                table: "Categorias",
                sql: "length(trim(\"Nombre\")) BETWEEN 1 AND 80");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Cambio_Range",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Recibido_Range",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ventas_Total_Range",
                table: "Ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Nombre_Length",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Precio_Range",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock_Range",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_StockMinimo_Range",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetallesVenta_Cantidad_Range",
                table: "DetallesVenta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetallesVenta_Precio_Range",
                table: "DetallesVenta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DetallesVenta_Subtotal_Range",
                table: "DetallesVenta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Celular_Length",
                table: "Clientes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Email_Length",
                table: "Clientes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Clientes_Nombre_Length",
                table: "Clientes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Categorias_Nombre_Length",
                table: "Categorias");
        }
    }
}
