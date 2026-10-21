using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;
public class Categoria
{
    public int Id {get; set;}

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, MinimumLength = 1, ErrorMessage = "El nombre no puede superar 80 caracteres.")]
    [RegularExpression(Reglas.Letras, ErrorMessage = "El nombre solo puede contener letras, espacios, puntos, apóstrofes y guiones.")]
    public string Nombre { get; set; } = string.Empty;
    
    //relacion: categoria y producto
    [JsonIgnore]
    public List<Producto> Productos{get; set; } = new();

}