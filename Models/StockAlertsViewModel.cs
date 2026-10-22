namespace SistemaVentas.Models;

public class StockAlertsViewModel
{
    public int Count { get; set; }
    public List<Producto> Products { get; set; } = new();
}
