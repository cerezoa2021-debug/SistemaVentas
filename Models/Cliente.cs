namespace SistemaVentas.Models;
public class Cliente
{
    public int Id {get; set; }

    public string Nombre{get; set; } = string.Empty;

    public string? Email{get; set; }

    public string? Celular{get; set; }

    //cliente tiene muchas ventas
    public List<Venta> Ventas {get;  set; } = new();
}