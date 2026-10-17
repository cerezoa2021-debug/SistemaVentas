namespace SistemaVentas.Models;using System.Data.Common;
public class Producto
{
    //atributos
    public int Id {get; set; }

    public string Nombre {get; set; } = string.Empty;

    public decimal Precio {get; set; }

    public int Stock{get; set; }

    public int StockMinimo{get; set; }

    //relacion cada producto pertenece a  una categoria

    public int CategoriaId {get; set; }

    public Categoria? Categoria{get; set; }

    //metodos
    public bool TieneStock()
    {
        return Stock > 0;
    }
}