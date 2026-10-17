namespace SistemaVentas.Models;
public class DetalleVenta
{
    public int Id{get; set; }

    public int Cantidad{get; set; }

    public decimal Precio{get; set; }

    public decimal Subtotal{get; set;}

    //relaciones
    public int VentaId{get; set; }

    public Venta? Venta{get; set; }

    public int ProductoId{get; set; }

    public Producto? Producto{get; set; }

    //metodos

    public void CalcularSubtotal()
    {
        Subtotal = Precio * Cantidad;
    }
}