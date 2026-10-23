namespace SistemaVentas.Models;

public class ReportPrintHeaderViewModel
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public DateTime Emitido { get; set; } = DateTime.Now;
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 25;
    public int TotalFilas { get; set; }
    public string FiltrosAplicados { get; set; } = string.Empty;
    public bool EsImpresion { get; set; }
    public int PrimeraFila => TotalFilas == 0 ? 0 : (Pagina - 1) * TamanoPagina + 1;
    public int UltimaFila => Math.Min(Pagina * TamanoPagina, TotalFilas);
    public int TotalPaginas => Math.Max(1, (int)Math.Ceiling(TotalFilas / (double)TamanoPagina));
}

public class ReporteVentasViewModel : ReportPrintHeaderViewModel
{
    public int? ClienteId { get; set; }
    public int? CategoriaId { get; set; }
    public string Estado { get; set; } = "Todas";
    public List<ClienteOpcionReporteViewModel> Clientes { get; set; } = new();
    public List<CategoriaOpcionReporteViewModel> Categorias { get; set; } = new();
    public List<VentaReporteFilaViewModel> Ventas { get; set; } = new();
    public decimal TotalVentas { get; set; }
    public decimal TotalRecibido { get; set; }
    public decimal TotalCambio { get; set; }
    public int CantidadVentas { get; set; }
    public string? Error { get; set; }
    public bool FiltraCategoria => CategoriaId.HasValue;
}

public class VentaReporteFilaViewModel
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public decimal SubtotalCategoria { get; set; }
    public decimal Recibido { get; set; }
    public decimal Cambio { get; set; }
    public bool Anulada { get; set; }
}

public class ReporteMasVendidosViewModel : ReportPrintHeaderViewModel
{
    public int? CategoriaId { get; set; }
    public string Mostrar { get; set; } = "10";
    public string Orden { get; set; } = "cantidad";
    public List<CategoriaOpcionReporteViewModel> Categorias { get; set; } = new();
    public List<ProductoVendidoReporteViewModel> Productos { get; set; } = new();
    public decimal TotalVendido { get; set; }
    public int TotalUnidades { get; set; }
    public string? Error { get; set; }
}

public class ProductoVendidoReporteViewModel
{
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal MontoVendido { get; set; }
}

public class ReporteStockViewModel : ReportPrintHeaderViewModel
{
    public int? CategoriaId { get; set; }
    public string Estado { get; set; } = "Todos";
    public string Busqueda { get; set; } = string.Empty;
    public List<CategoriaOpcionReporteViewModel> Categorias { get; set; } = new();
    public List<ProductoStockReporteViewModel> Productos { get; set; } = new();
    public int Agotados { get; set; }
    public int Bajos { get; set; }
    public int Disponibles { get; set; }
    public string? Error { get; set; }
}

public class ProductoStockReporteViewModel
{
    public string Producto { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class ReporteClientesViewModel : ReportPrintHeaderViewModel
{
    public string Busqueda { get; set; } = string.Empty;
    public string ComprasFiltro { get; set; } = "Todas";
    public string Orden { get; set; } = "nombre";
    public List<ClienteReporteFilaViewModel> Clientes { get; set; } = new();
    public string? Error { get; set; }
}

public class ClienteReporteFilaViewModel
{
    public string Nombre { get; set; } = string.Empty;
    public string Celular { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public int Compras { get; set; }
    public decimal MontoTotal { get; set; }
}

public class ClienteOpcionReporteViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class CategoriaOpcionReporteViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class ReportesIndexViewModel
{
    public List<ReporteMenuItemViewModel> Reportes { get; set; } = new();
}

public class ReporteMenuItemViewModel
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Icono { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
}
