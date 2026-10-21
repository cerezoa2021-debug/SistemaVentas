using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;
public class DetalleVenta
{
    public int Id{get; set; }

    [Range(1, 1000000, ErrorMessage = "La cantidad debe estar entre 1 y 1,000,000.")]
    public int Cantidad{get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El precio debe estar entre 0 y 99,999,999.99.")]
    public decimal Precio{get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El subtotal debe estar entre 0 y 99,999,999.99.")]
    public decimal Subtotal{get; set;}

    //relaciones
    public int VentaId{get; set; }

    public Venta? Venta{get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un producto válido.")]
    public int ProductoId{get; set; }

    public Producto? Producto{get; set; }

    //metodos

    public void CalcularSubtotal()
    {
        Subtotal = Precio * Cantidad;
    }
}