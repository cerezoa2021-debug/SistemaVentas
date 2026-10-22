using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;

public class ReponerProductoViewModel
{
    public int ProductoId { get; set; }
    public string NombreProducto { get; set; } = string.Empty;
    public int StockActual { get; set; }

    [Range(1, 1000000, ErrorMessage = "La cantidad a reponer debe ser mayor que 0.")]
    public int Cantidad { get; set; } = 1;
}
