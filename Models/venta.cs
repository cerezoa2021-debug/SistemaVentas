namespace SistemaVentas.Models;
public class Venta
{
    public int Id{get; set; }

    public DateTime Fecha {get; set; } = DateTime.Now;

    public decimal Total {get; set; }

    public decimal Recibido {get; set; }

    public decimal Cambio{get; set; }

    public bool Anulada { get; set; }

    //relaciones

    public int ClienteId {get; set; }

    public Cliente? Cliente{get; set; }

    public List<DetalleVenta> Detalles{get; set; } = new();

    //metodos
    public void CalcularTotal()
    {
        Total = Detalles.Sum(d=>d.Subtotal);
    }

    public void CalcularCambio()
    {
        Cambio = Recibido - Total;
    }

    public void ActualizarStock()
    {
        foreach (var d in Detalles)
        {
            if(d.Producto != null)
                d.Producto.Stock -= d.Cantidad;
        }
    }

}