using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;
public class Cliente
{
    public int Id {get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 120 caracteres.")]
    [RegularExpression(Reglas.Letras, ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, apóstrofes y guiones.")]
    public string Nombre{get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [StringLength(254, ErrorMessage = "El correo no puede superar 254 caracteres.")]
    public string? Email{get; set; }

    [RegularExpression(Reglas.Celular, ErrorMessage = "El celular debe tener 7 u 8 dígitos.")]
    public string? Celular{get; set; }

    //cliente tiene muchas ventas
    public List<Venta> Ventas {get;  set; } = new();
}