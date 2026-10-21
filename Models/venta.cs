using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;
public class Venta
{
    public int Id{get; set; }

    public DateTime Fecha {get; set; } = DateTime.Now;

    [Range(0, 99999999.99, ErrorMessage = "El total debe estar entre 0 y 99,999,999.99.")]
    public decimal Total {get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El monto recibido debe estar entre 0 y 99,999,999.99.")]
    public decimal Recibido {get; set; }

    [Range(0, 99999999.99, ErrorMessage = "El cambio no puede ser negativo ni superar 99,999,999.99.")]
    public decimal Cambio{get; set; }

    public bool Anulada { get; set; }

    //relaciones

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente válido.")]
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