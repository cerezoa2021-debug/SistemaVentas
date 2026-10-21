using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    [RegularExpression(Reglas.Producto, ErrorMessage = "El nombre debe incluir letras y solo puede contener letras, números y los caracteres . , % ( ) / -.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0, 99999999.99, ErrorMessage = "El precio debe estar entre 0 y 99,999,999.99.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock debe estar entre 0 y 1,000,000.")]
    public int Stock { get; set; }

    [Required(ErrorMessage = "El stock mínimo es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock mínimo debe estar entre 0 y 1,000,000.")]
    public int StockMinimo { get; set; }

    [Required(ErrorMessage = "Selecciona una categoría.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public bool TieneStock()
    {
        return Stock > 0;
    }
}