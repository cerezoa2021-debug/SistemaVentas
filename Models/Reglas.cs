namespace SistemaVentas.Models;

public static class Reglas
{
    public const string Letras = @"^[A-Za-zÁÉÍÓÚÜÑáéíóúüñ][A-Za-zÁÉÍÓÚÜÑáéíóúüñ .'\-]*$";
    public const string Producto = @"^(?=.*[A-Za-zÁÉÍÓÚÜÑáéíóúüñ])[A-Za-zÁÉÍÓÚÜÑáéíóúüñ0-9.,%()/-]+$";
    public const string Celular = @"^[0-9]{7,8}$";
}
