using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new ReportesIndexViewModel
        {
            Reportes =
            [
                new() { Titulo = "Ventas por fechas", Descripcion = "Consulta ventas, clientes y totales en un periodo.", Icono = "bi-receipt", Accion = nameof(Ventas) },
                new() { Titulo = "Productos más vendidos", Descripcion = "Compara unidades y monto vendido por producto.", Icono = "bi-graph-up-arrow", Accion = nameof(MasVendidos) },
                new() { Titulo = "Stock actual", Descripcion = "Revisa existencias, mínimos y estado del inventario.", Icono = "bi-box-seam", Accion = nameof(Stock) },
                new() { Titulo = "Clientes", Descripcion = "Consulta datos de contacto y compras vigentes.", Icono = "bi-people", Accion = nameof(Clientes) }
            ]
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Ventas(
        DateTime? desde, DateTime? hasta, int? clienteId, int? categoriaId,
        string estado = "Todas", int pagina = 1, int tamanoPagina = 25, bool imprimir = false)
    {
        return View(await CrearReporteVentasAsync(
            desde, hasta, clienteId, categoriaId, estado, pagina, tamanoPagina, incluirTodos: imprimir));
    }

    [HttpGet]
    public async Task<IActionResult> MasVendidos(
        DateTime? desde, DateTime? hasta, int? categoriaId,
        string mostrar = "10", string orden = "cantidad", int pagina = 1, int tamanoPagina = 25,
        bool imprimir = false)
    {
        return View(await CrearReporteMasVendidosAsync(
            desde, hasta, categoriaId, mostrar, orden, pagina, tamanoPagina, incluirTodos: imprimir));
    }

    [HttpGet]
    public async Task<IActionResult> Stock(
        int? categoriaId, string estado = "Todos", string busqueda = "",
        int pagina = 1, int tamanoPagina = 25, bool imprimir = false)
    {
        return View(await CrearReporteStockAsync(
            categoriaId, estado, busqueda, pagina, tamanoPagina, incluirTodos: imprimir));
    }

    [HttpGet]
    public async Task<IActionResult> Clientes(
        string busqueda = "", string comprasFiltro = "Todas",
        DateTime? desde = null, DateTime? hasta = null, string orden = "nombre",
        int pagina = 1, int tamanoPagina = 25, bool imprimir = false)
    {
        return View(await CrearReporteClientesAsync(
            busqueda, comprasFiltro, desde, hasta, orden, pagina, tamanoPagina, incluirTodos: imprimir));
    }

    [HttpGet]
    public async Task<IActionResult> ExportarVentasCsv(
        DateTime? desde, DateTime? hasta, int? clienteId, int? categoriaId, string estado = "Todas")
    {
        var model = await CrearReporteVentasAsync(
            desde, hasta, clienteId, categoriaId, estado, 1, 25, incluirTodos: true);
        if (model.Error != null)
            return BadRequest(model.Error);

        var rows = new List<string[]>
        {
            new[] { "Filtros aplicados", model.FiltrosAplicados },
            model.FiltraCategoria
                ? new[] { "Fecha", "Cliente", "Total", "Subtotal categoría", "Recibido", "Cambio", "Estado" }
                : new[] { "Fecha", "Cliente", "Total", "Recibido", "Cambio", "Estado" }
        };
        rows.AddRange(model.Ventas.Select(venta => model.FiltraCategoria
            ? new[]
            {
                venta.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                venta.Cliente,
                FormatearNumero(venta.Total),
                FormatearNumero(venta.SubtotalCategoria),
                FormatearNumero(venta.Recibido),
                FormatearNumero(venta.Cambio),
                venta.Anulada ? "Anulada" : "Vigente"
            }
            : new[]
            {
                venta.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
                venta.Cliente,
                FormatearNumero(venta.Total),
                FormatearNumero(venta.Recibido),
                FormatearNumero(venta.Cambio),
                venta.Anulada ? "Anulada" : "Vigente"
            }));
        rows.Add(model.FiltraCategoria
            ? new[]
            {
                "Totales vigentes", "", "", FormatearNumero(model.TotalVentas),
                FormatearNumero(model.TotalRecibido), FormatearNumero(model.TotalCambio),
                model.CantidadVentas.ToString(CultureInfo.InvariantCulture)
            }
            : new[]
            {
                "Totales vigentes", "", FormatearNumero(model.TotalVentas),
                FormatearNumero(model.TotalRecibido), FormatearNumero(model.TotalCambio),
                model.CantidadVentas.ToString(CultureInfo.InvariantCulture)
            });

        return Csv(rows, $"ventas_{model.Desde:yyyy-MM-dd}_a_{model.Hasta:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarMasVendidosCsv(
        DateTime? desde, DateTime? hasta, int? categoriaId, string mostrar = "10", string orden = "cantidad")
    {
        var model = await CrearReporteMasVendidosAsync(
            desde, hasta, categoriaId, mostrar, orden, 1, 25, incluirTodos: true);
        if (model.Error != null)
            return BadRequest(model.Error);

        var rows = new List<string[]>
        {
            new[] { "Filtros aplicados", model.FiltrosAplicados },
            new[] { "Producto", "Categoría", "Cantidad", "Monto vendido" }
        };
        rows.AddRange(model.Productos.Select(producto => new[]
        {
            producto.Producto,
            producto.Categoria,
            producto.Cantidad.ToString(CultureInfo.InvariantCulture),
            FormatearNumero(producto.MontoVendido)
        }));
        rows.Add(new[] { "Totales", "", model.TotalUnidades.ToString(CultureInfo.InvariantCulture), FormatearNumero(model.TotalVendido) });

        return Csv(rows, $"productos_mas_vendidos_{model.Desde:yyyy-MM-dd}_a_{model.Hasta:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarStockCsv(int? categoriaId, string estado = "Todos", string busqueda = "")
    {
        var model = await CrearReporteStockAsync(categoriaId, estado, busqueda, 1, 25, incluirTodos: true);
        if (model.Error != null)
            return BadRequest(model.Error);
        var rows = new List<string[]>
        {
            new[] { "Filtros aplicados", model.FiltrosAplicados },
            new[] { "Producto", "Categoría", "Precio", "Stock", "Stock mínimo", "Estado" }
        };
        rows.AddRange(model.Productos.Select(producto => new[]
        {
            producto.Producto,
            producto.Categoria,
            FormatearNumero(producto.Precio),
            producto.Stock.ToString(CultureInfo.InvariantCulture),
            producto.StockMinimo.ToString(CultureInfo.InvariantCulture),
            producto.Estado
        }));
        return Csv(rows, $"stock_actual_{DateTime.Today:yyyy-MM-dd}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> ExportarClientesCsv(
        string busqueda = "", string comprasFiltro = "Todas",
        DateTime? desde = null, DateTime? hasta = null, string orden = "nombre")
    {
        var model = await CrearReporteClientesAsync(
            busqueda, comprasFiltro, desde, hasta, orden, 1, 25, incluirTodos: true);
        if (model.Error != null)
            return BadRequest(model.Error);
        var rows = new List<string[]>
        {
            new[] { "Filtros aplicados", model.FiltrosAplicados },
            new[] { "Nombre", "Celular", "Correo", "Cantidad de compras", "Monto total" }
        };
        rows.AddRange(model.Clientes.Select(cliente => new[]
        {
            cliente.Nombre,
            cliente.Celular,
            cliente.Correo,
            cliente.Compras.ToString(CultureInfo.InvariantCulture),
            FormatearNumero(cliente.MontoTotal)
        }));
        return Csv(rows, "clientes.csv");
    }

    private async Task<ReporteVentasViewModel> CrearReporteVentasAsync(
        DateTime? desde, DateTime? hasta, int? clienteId, int? categoriaId, string estado,
        int pagina, int tamanoPagina, bool incluirTodos = false)
    {
        var rango = ValidarRango(desde, hasta);
        var model = new ReporteVentasViewModel
        {
            Desde = rango.Desde,
            Hasta = rango.Hasta,
            ClienteId = clienteId,
            CategoriaId = categoriaId,
            Estado = estado,
            Emitido = DateTime.Now,
            EsImpresion = incluirTodos,
            Error = rango.Error ?? ValidarPaginacion(pagina, tamanoPagina),
            Clientes = await ObtenerOpcionesClientesAsync(),
            Categorias = await ObtenerOpcionesCategoriasAsync()
        };
        if (model.Error != null)
            return model;
        if (!await CategoriaExisteAsync(categoriaId))
        {
            model.Error = "Selecciona una categoría válida.";
            return model;
        }
        if (clienteId.HasValue && !model.Clientes.Any(cliente => cliente.Id == clienteId.Value))
        {
            model.Error = "Selecciona un cliente válido.";
            return model;
        }
        if (estado is not ("Todas" or "Vigentes" or "Anuladas"))
        {
            model.Error = "Selecciona un estado válido.";
            return model;
        }

        var inicio = rango.Desde!.Value.Date;
        var finExclusivo = rango.Hasta!.Value.Date.AddDays(1);
        var query = _context.Ventas.AsNoTracking()
            .Where(venta => venta.Fecha >= inicio && venta.Fecha < finExclusivo);
        if (clienteId.HasValue)
            query = query.Where(venta => venta.ClienteId == clienteId.Value);
        if (categoriaId.HasValue)
            query = query.Where(venta => venta.Detalles.Any(detalle => detalle.Producto!.CategoriaId == categoriaId.Value));
        if (estado == "Vigentes")
            query = query.Where(venta => !venta.Anulada);
        else if (estado == "Anuladas")
            query = query.Where(venta => venta.Anulada);

        model.FiltrosAplicados = $"Desde: {inicio:dd/MM/yyyy} · Hasta: {rango.Hasta:dd/MM/yyyy} · Categoría: {NombreCategoria(model.Categorias, categoriaId)} · Cliente: {NombreCliente(model.Clientes, clienteId)} · Estado: {estado}";
        var vigentes = query.Where(venta => !venta.Anulada);
        model.CantidadVentas = await vigentes.CountAsync();
        model.TotalVentas = await vigentes.SumAsync(venta => categoriaId.HasValue
            ? venta.Detalles.Where(detalle => detalle.Producto!.CategoriaId == categoriaId.Value).Sum(detalle => (decimal?)detalle.Subtotal) ?? 0
            : venta.Total);
        model.TotalRecibido = await vigentes.SumAsync(venta => (decimal?)venta.Recibido) ?? 0;
        model.TotalCambio = await vigentes.SumAsync(venta => (decimal?)venta.Cambio) ?? 0;

        var filas = query.OrderByDescending(venta => venta.Fecha).ThenByDescending(venta => venta.Id)
            .Select(venta => new VentaReporteFilaViewModel
            {
                Id = venta.Id,
                Fecha = venta.Fecha,
                Cliente = venta.Cliente!.Nombre,
                Total = venta.Total,
                SubtotalCategoria = categoriaId.HasValue
                    ? venta.Detalles.Where(detalle => detalle.Producto!.CategoriaId == categoriaId.Value)
                        .Sum(detalle => (decimal?)detalle.Subtotal) ?? 0
                    : venta.Total,
                Recibido = venta.Recibido,
                Cambio = venta.Cambio,
                Anulada = venta.Anulada
            });
        await CargarPaginaAsync(model, filas, pagina, tamanoPagina, incluirTodos, rows => model.Ventas = rows);
        return model;
    }

    private async Task<ReporteMasVendidosViewModel> CrearReporteMasVendidosAsync(
        DateTime? desde, DateTime? hasta, int? categoriaId, string mostrar, string orden,
        int pagina, int tamanoPagina, bool incluirTodos = false)
    {
        var rango = ValidarRango(desde, hasta);
        var model = new ReporteMasVendidosViewModel
        {
            Desde = rango.Desde,
            Hasta = rango.Hasta,
            CategoriaId = categoriaId,
            Mostrar = mostrar,
            Orden = orden,
            Emitido = DateTime.Now,
            EsImpresion = incluirTodos,
            Error = rango.Error ?? ValidarPaginacion(pagina, tamanoPagina),
            Categorias = await ObtenerOpcionesCategoriasAsync()
        };
        if (model.Error != null)
            return model;
        if (!await CategoriaExisteAsync(categoriaId))
        {
            model.Error = "Selecciona una categoría válida.";
            return model;
        }
        if (mostrar is not ("10" or "20" or "50" or "Todos"))
        {
            model.Error = "Selecciona una cantidad válida para Mostrar.";
            return model;
        }
        if (orden is not ("cantidad" or "monto"))
        {
            model.Error = "Selecciona un orden válido.";
            return model;
        }

        var inicio = rango.Desde!.Value.Date;
        var finExclusivo = rango.Hasta!.Value.Date.AddDays(1);
        var detalles = _context.DetallesVenta.AsNoTracking()
            .Where(detalle => !detalle.Venta!.Anulada
                && detalle.Venta.Fecha >= inicio && detalle.Venta.Fecha < finExclusivo);
        if (categoriaId.HasValue)
            detalles = detalles.Where(detalle => detalle.Producto!.CategoriaId == categoriaId.Value);

        var agrupados = detalles.GroupBy(detalle => new
            {
                detalle.ProductoId,
                Producto = detalle.Producto!.Nombre,
                Categoria = detalle.Producto.Categoria!.Nombre
            })
            .Select(grupo => new ProductoVendidoReporteViewModel
            {
                Producto = grupo.Key.Producto,
                Categoria = grupo.Key.Categoria,
                Cantidad = grupo.Sum(detalle => detalle.Cantidad),
                MontoVendido = grupo.Sum(detalle => detalle.Subtotal)
            });
        IOrderedQueryable<ProductoVendidoReporteViewModel> ordenados = orden == "monto"
            ? agrupados.OrderByDescending(producto => producto.MontoVendido).ThenBy(producto => producto.Producto)
            : agrupados.OrderByDescending(producto => producto.Cantidad).ThenBy(producto => producto.Producto);
        var limit = mostrar == "Todos" ? (int?)null : int.Parse(mostrar, CultureInfo.InvariantCulture);
        var seleccionados = limit.HasValue ? ordenados.Take(limit.Value) : ordenados;
        model.TotalUnidades = await seleccionados.SumAsync(producto => (int?)producto.Cantidad) ?? 0;
        model.TotalVendido = await seleccionados.SumAsync(producto => (decimal?)producto.MontoVendido) ?? 0;
        var mostrarEtiqueta = mostrar == "Todos" ? "Todos" : $"Top {mostrar}";
        model.FiltrosAplicados = $"Desde: {inicio:dd/MM/yyyy} · Hasta: {rango.Hasta:dd/MM/yyyy} · Categoría: {NombreCategoria(model.Categorias, categoriaId)} · Mostrar: {mostrarEtiqueta} · Orden: {(orden == "monto" ? "Monto vendido" : "Cantidad vendida")}";
        await CargarPaginaAsync(model, seleccionados, pagina, tamanoPagina, incluirTodos, rows => model.Productos = rows);
        return model;
    }

    private async Task<ReporteStockViewModel> CrearReporteStockAsync(
        int? categoriaId, string estado, string busqueda, int pagina, int tamanoPagina, bool incluirTodos = false)
    {
        var model = new ReporteStockViewModel
        {
            CategoriaId = categoriaId,
            Estado = estado,
            Busqueda = busqueda.Trim(),
            Emitido = DateTime.Now,
            EsImpresion = incluirTodos,
            Error = ValidarPaginacion(pagina, tamanoPagina),
            Categorias = await ObtenerOpcionesCategoriasAsync()
        };
        if (model.Error != null)
            return model;
        if (!await CategoriaExisteAsync(categoriaId))
        {
            model.Error = "Selecciona una categoría válida.";
            return model;
        }
        if (estado is not ("Todos" or "Disponible" or "Bajo" or "Agotado"))
        {
            model.Error = "Selecciona un estado válido.";
            return model;
        }
        if (model.Busqueda.Length > 100)
        {
            model.Error = "La búsqueda no puede superar 100 caracteres.";
            return model;
        }

        var productos = _context.Productos.AsNoTracking();
        if (categoriaId.HasValue)
            productos = productos.Where(producto => producto.CategoriaId == categoriaId.Value);
        if (model.Busqueda.Length > 0)
        {
            var termino = model.Busqueda.ToLower();
            productos = productos.Where(producto => producto.Nombre.ToLower().Contains(termino));
        }

        model.Agotados = await productos.CountAsync(producto => producto.Stock == 0);
        model.Bajos = await productos.CountAsync(producto => producto.Stock > 0 && producto.Stock <= producto.StockMinimo);
        model.Disponibles = await productos.CountAsync(producto => producto.Stock > producto.StockMinimo);
        if (estado == "Disponible")
            productos = productos.Where(producto => producto.Stock > producto.StockMinimo);
        else if (estado == "Bajo")
            productos = productos.Where(producto => producto.Stock > 0 && producto.Stock <= producto.StockMinimo);
        else if (estado == "Agotado")
            productos = productos.Where(producto => producto.Stock == 0);

        var filas = productos.OrderBy(producto => producto.Stock == 0 ? 0 : producto.Stock <= producto.StockMinimo ? 1 : 2)
            .ThenBy(producto => producto.Categoria!.Nombre)
            .ThenBy(producto => producto.Nombre)
            .Select(producto => new ProductoStockReporteViewModel
            {
                Producto = producto.Nombre,
                Categoria = producto.Categoria!.Nombre,
                Precio = producto.Precio,
                Stock = producto.Stock,
                StockMinimo = producto.StockMinimo,
                Estado = producto.Stock == 0 ? "Agotado" : producto.Stock <= producto.StockMinimo ? "Bajo" : "Disponible"
            });
        model.FiltrosAplicados = $"Categoría: {NombreCategoria(model.Categorias, categoriaId)} · Estado: {estado} · Búsqueda: {(string.IsNullOrEmpty(model.Busqueda) ? "Todas" : model.Busqueda)}";
        await CargarPaginaAsync(model, filas, pagina, tamanoPagina, incluirTodos, rows => model.Productos = rows);
        return model;
    }

    private async Task<ReporteClientesViewModel> CrearReporteClientesAsync(
        string busqueda, string comprasFiltro, DateTime? desde, DateTime? hasta, string orden,
        int pagina, int tamanoPagina, bool incluirTodos = false)
    {
        var model = new ReporteClientesViewModel
        {
            Desde = desde?.Date,
            Hasta = hasta?.Date,
            Busqueda = busqueda.Trim(),
            ComprasFiltro = comprasFiltro,
            Orden = orden,
            Emitido = DateTime.Now,
            EsImpresion = incluirTodos,
            Error = ValidarPaginacion(pagina, tamanoPagina)
        };
        if (model.Error != null)
            return model;
        if (ModelState.IsValid == false)
        {
            model.Error = "Ingresa fechas válidas con formato día, mes y año.";
            return model;
        }
        if ((desde.HasValue != hasta.HasValue) || (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            || (hasta.HasValue && hasta.Value.Date >= DateTime.MaxValue.Date))
        {
            model.Error = "Ingresa un rango de fechas válido; Desde no puede ser posterior a Hasta.";
            return model;
        }
        if (comprasFiltro is not ("Todas" or "Con compras" or "Sin compras"))
        {
            model.Error = "Selecciona un filtro de compras válido.";
            return model;
        }
        if (orden is not ("nombre" or "compras" or "monto"))
        {
            model.Error = "Selecciona un orden válido.";
            return model;
        }
        if (model.Busqueda.Length > 120)
        {
            model.Error = "La búsqueda no puede superar 120 caracteres.";
            return model;
        }

        var inicio = desde?.Date;
        var finExclusivo = hasta.HasValue ? hasta.Value.Date.AddDays(1) : (DateTime?)null;
        var clientes = _context.Clientes.AsNoTracking().AsQueryable();
        if (model.Busqueda.Length > 0)
        {
            var termino = model.Busqueda.ToLower();
            clientes = clientes.Where(cliente =>
                cliente.Nombre.ToLower().Contains(termino)
                || (cliente.Celular != null && cliente.Celular.ToLower().Contains(termino))
                || (cliente.Email != null && cliente.Email.ToLower().Contains(termino)));
        }

        var query = clientes.Select(cliente => new ClienteReporteFilaViewModel
        {
            Nombre = cliente.Nombre,
            Celular = cliente.Celular ?? string.Empty,
            Correo = cliente.Email ?? string.Empty,
            Compras = cliente.Ventas.Count(venta => !venta.Anulada
                && (!inicio.HasValue || venta.Fecha >= inicio.Value)
                && (!finExclusivo.HasValue || venta.Fecha < finExclusivo.Value)),
            MontoTotal = cliente.Ventas.Where(venta => !venta.Anulada
                    && (!inicio.HasValue || venta.Fecha >= inicio.Value)
                    && (!finExclusivo.HasValue || venta.Fecha < finExclusivo.Value))
                .Sum(venta => (decimal?)venta.Total) ?? 0
        });
        if (comprasFiltro == "Con compras")
            query = query.Where(cliente => cliente.Compras > 0);
        else if (comprasFiltro == "Sin compras")
            query = query.Where(cliente => cliente.Compras == 0);
        var ordenados = orden switch
        {
            "compras" => query.OrderByDescending(cliente => cliente.Compras).ThenBy(cliente => cliente.Nombre),
            "monto" => query.OrderByDescending(cliente => cliente.MontoTotal).ThenBy(cliente => cliente.Nombre),
            _ => query.OrderBy(cliente => cliente.Nombre)
        };
        var ordenEtiqueta = orden switch
        {
            "compras" => "Más compras",
            "monto" => "Mayor monto comprado",
            _ => "Nombre (A-Z)"
        };
        model.FiltrosAplicados = $"Búsqueda: {(string.IsNullOrEmpty(model.Busqueda) ? "Todas" : model.Busqueda)} · Compras: {comprasFiltro} · Desde: {(inicio.HasValue ? inicio.Value.ToString("dd/MM/yyyy") : "Todas")} · Hasta: {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "Todas")} · Orden: {ordenEtiqueta}";
        await CargarPaginaAsync(model, ordenados, pagina, tamanoPagina, incluirTodos, rows => model.Clientes = rows);
        return model;
    }

    private async Task CargarPaginaAsync<T>(
        ReportPrintHeaderViewModel model, IQueryable<T> query,
        int pagina, int tamanoPagina, bool incluirTodos, Action<List<T>> asignarFilas)
    {
        model.TamanoPagina = tamanoPagina;
        model.TotalFilas = await query.CountAsync();
        model.Pagina = Math.Clamp(pagina, 1, model.TotalPaginas);
        var filas = incluirTodos
            ? await query.ToListAsync()
            : await query.Skip((model.Pagina - 1) * tamanoPagina).Take(tamanoPagina).ToListAsync();
        asignarFilas(filas);
    }

    private string? ValidarPaginacion(int pagina, int tamanoPagina)
    {
        if (pagina < 1)
            return "La página solicitada no es válida.";
        if (tamanoPagina is not (25 or 50 or 100))
            return "Selecciona un tamaño de página válido (25, 50 o 100).";
        if (!ModelState.IsValid)
            return "Uno o más filtros no tienen un formato válido.";
        return null;
    }

    private (DateTime? Desde, DateTime? Hasta, string? Error) ValidarRango(DateTime? desde, DateTime? hasta)
    {
        if (!ModelState.IsValid)
            return (desde?.Date, hasta?.Date, "Ingresa fechas válidas con formato día, mes y año.");
        if (desde == null && hasta == null)
        {
            var hoy = DateTime.Today;
            return (new DateTime(hoy.Year, hoy.Month, 1), hoy, null);
        }
        if (desde == null || hasta == null)
            return (desde?.Date, hasta?.Date, "Completa las fechas Desde y Hasta.");
        if (desde.Value.Date > hasta.Value.Date)
            return (desde.Value.Date, hasta.Value.Date, "La fecha Desde no puede ser posterior a Hasta.");
        if (hasta.Value.Date >= DateTime.MaxValue.Date)
            return (desde.Value.Date, hasta.Value.Date, "La fecha Hasta está fuera del rango permitido.");
        return (desde.Value.Date, hasta.Value.Date, null);
    }

    private Task<List<ClienteOpcionReporteViewModel>> ObtenerOpcionesClientesAsync() =>
        _context.Clientes.AsNoTracking().OrderBy(cliente => cliente.Nombre)
            .Select(cliente => new ClienteOpcionReporteViewModel { Id = cliente.Id, Nombre = cliente.Nombre })
            .ToListAsync();

    private Task<List<CategoriaOpcionReporteViewModel>> ObtenerOpcionesCategoriasAsync() =>
        _context.Categorias.AsNoTracking().OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new CategoriaOpcionReporteViewModel { Id = categoria.Id, Nombre = categoria.Nombre })
            .ToListAsync();

    private Task<bool> CategoriaExisteAsync(int? categoriaId) =>
        !categoriaId.HasValue
            ? Task.FromResult(true)
            : _context.Categorias.AsNoTracking().AnyAsync(categoria => categoria.Id == categoriaId.Value);

    private static string NombreCategoria(List<CategoriaOpcionReporteViewModel> categorias, int? categoriaId) =>
        categoriaId.HasValue
            ? categorias.FirstOrDefault(categoria => categoria.Id == categoriaId.Value)?.Nombre ?? "Desconocida"
            : "Todas las categorías";

    private static string NombreCliente(List<ClienteOpcionReporteViewModel> clientes, int? clienteId) =>
        clienteId.HasValue
            ? clientes.FirstOrDefault(cliente => cliente.Id == clienteId.Value)?.Nombre ?? "Desconocido"
            : "Todos";

    private IActionResult Csv(IEnumerable<string[]> rows, string fileName)
    {
        var csv = string.Join("\r\n", rows.Select(row => string.Join(";", row.Select(EscaparCampoCsv)))) + "\r\n";
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return File(encoding.GetPreamble().Concat(encoding.GetBytes(csv)).ToArray(), "text/csv; charset=utf-8", fileName);
    }

    private static string EscaparCampoCsv(string value)
    {
        var field = value ?? string.Empty;
        var trimmed = field.TrimStart();
        if (trimmed.Length > 0 && trimmed[0] is '=' or '+' or '-' or '@')
            field = "'" + field;
        return "\"" + field.Replace("\"", "\"\"") + "\"";
    }

    private static string FormatearNumero(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}
