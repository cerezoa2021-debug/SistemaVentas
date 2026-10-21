using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Ventas
    public async Task<IActionResult> Index()
    {
        var ventas = await _context.Ventas
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();

        return View(ventas);
    }

    // GET: /Ventas/BuscarClientes?q=texto
    [HttpGet]
    public async Task<IActionResult> BuscarClientes(string? q)
    {
        q = (q ?? "").Trim();
        var lista = await _context.Clientes
            .Where(c => q == "" || c.Nombre.Contains(q) || (c.Celular != null && c.Celular.Contains(q)))
            .OrderBy(c => c.Nombre)
            .Take(8)
            .Select(c => new { c.Id, c.Nombre, c.Celular })
            .ToListAsync();
        return Json(lista);
    }

    // POST: /Ventas/RegistrarCliente
    [HttpPost]
    public async Task<IActionResult> RegistrarCliente([FromBody] Cliente datos)
    {
        if (string.IsNullOrWhiteSpace(datos?.Nombre))
            return BadRequest(new { error = "El nombre es obligatorio." });

        var cliente = new Cliente
        {
            Nombre = datos.Nombre.Trim(),
            Celular = string.IsNullOrWhiteSpace(datos.Celular) ? null : datos.Celular.Trim(),
            Email = string.IsNullOrWhiteSpace(datos.Email) ? null : datos.Email.Trim()
        };

        if (!TryValidateModel(cliente))
        {
            var error = ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(value => value.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message));
            return BadRequest(new { error = error ?? "Los datos del cliente no son válidos." });
        }

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        return Json(new { cliente.Id, cliente.Nombre, cliente.Celular });
    }

    // GET: /Ventas/Create
    public async Task<IActionResult> Create()
    {
        await CargarClientes();
        await CargarProductos();
        return View(new Venta());
    }

    // POST: /Ventas/Create
    [HttpPost]
    public async Task<IActionResult> Create(Venta venta)
    {
        venta.Detalles ??= new List<DetalleVenta>();

        var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == venta.ClienteId);
        if (!clienteExiste)
            ModelState.AddModelError(nameof(venta.ClienteId), "Selecciona un cliente válido.");

        if (venta.Detalles.Count == 0)
            ModelState.AddModelError("", "La venta debe tener al menos un producto.");
        else if (venta.Detalles.Any(d => d.Cantidad <= 0))
            ModelState.AddModelError("", "La cantidad de cada producto debe ser mayor que cero.");
        else if (venta.Detalles.GroupBy(d => d.ProductoId).Any(g => g.Count() > 1))
            ModelState.AddModelError("", "No se debe repetir un producto en los detalles de la venta.");

        var productos = new Dictionary<int, Producto>();
        foreach (var detalle in venta.Detalles)
        {
            var producto = await _context.Productos.FindAsync(detalle.ProductoId);
            if (producto == null)
            {
                ModelState.AddModelError("", $"No existe el producto con Id {detalle.ProductoId}.");
                continue;
            }

            if (producto.Stock < detalle.Cantidad)
                ModelState.AddModelError("", $"Stock insuficiente para el producto {producto.Nombre}.");

            productos[producto.Id] = producto;
        }

        for (var i = 0; i < venta.Detalles.Count; i++)
        {
            var detalle = venta.Detalles[i];
            if (productos.TryGetValue(detalle.ProductoId, out var producto))
            {
                detalle.Precio = producto.Precio;
                detalle.CalcularSubtotal();
                ModelState.Remove($"Detalles[{i}].Precio");
                ModelState.Remove($"Detalles[{i}].Subtotal");
            }
        }

        venta.CalcularTotal();
        if (venta.Recibido < venta.Total)
            ModelState.AddModelError(nameof(venta.Recibido), "El dinero recibido no cubre el total de la venta.");

        if (!ModelState.IsValid)
        {
            await CargarClientes(venta.ClienteId);
            await CargarProductos();
            return View(venta);
        }

        venta.Fecha = DateTime.Now;
        venta.Anulada = false;
        venta.CalcularCambio();
        foreach (var detalle in venta.Detalles)
        {
            detalle.Venta = null;
            detalle.Producto = null;
            productos[detalle.ProductoId].Stock -= detalle.Cantidad;
        }

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Ventas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return NotFound();

        return View(venta);
    }

    // POST: /Ventas/Anular/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        if (!venta.Anulada)
        {
            foreach (var detalle in venta.Detalles)
            {
                var producto = await _context.Productos.FindAsync(detalle.ProductoId);
                if (producto != null)
                    producto.Stock += detalle.Cantidad;
            }

            venta.Anulada = true;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarClientes(int? clienteId = null)
    {
        ViewBag.ClienteId = new SelectList(
            await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync(),
            nameof(Cliente.Id),
            nameof(Cliente.Nombre),
            clienteId);
    }

    private async Task CargarProductos()
    {
        ViewBag.Productos = await _context.Productos
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }
}
