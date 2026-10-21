
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/ventas")]
public class VentasApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public VentasApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/ventas
    [HttpGet]
    public async Task<ActionResult<List<Venta>>> GetAll()
    {
        return await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    // GET api/ventas/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Venta>> GetById(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
            return NotFound();

        return venta;
    }

    // POST api/ventas
    // Body:
    // {
    //   "clienteId": 1,
    //   "recibido": 100,
    //   "detalles": [
    //     {"productoId": 1, "cantidad": 2},
    //     {"productoId": 2, "cantidad": 1}
    //   ]
    // }
    [HttpPost]
    public async Task<ActionResult<Venta>> Create(Venta venta)
    {
        venta.Id = 0;
        venta.Cliente = null;
        if (venta.Detalles != null)
        {
            foreach (var detalle in venta.Detalles)
            {
                detalle.Id = 0;
                detalle.Venta = null;
                detalle.Producto = null;
            }
        }

        if (!await _context.Clientes.AnyAsync(c => c.Id == venta.ClienteId))
            return BadRequest("El cliente indicado no existe");

        if (venta.Detalles == null || venta.Detalles.Count == 0)
            return BadRequest("La venta debe tener al menos un detalle");

        if (venta.Detalles.Any(d => d.Cantidad <= 0))
            return BadRequest("La cantidad debe ser mayor que cero");

        // Evitar repetir el mismo producto en varios detalles.
        if (venta.Detalles.GroupBy(d => d.ProductoId).Any(g => g.Count() > 1))
            return BadRequest("No se debe repetir un producto en los detalles");

        foreach (var detalle in venta.Detalles)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == detalle.ProductoId);

            if (producto == null)
                return BadRequest($"No existe el producto con Id {detalle.ProductoId}");

            if (producto.Stock < detalle.Cantidad)
                return Conflict($"Stock insuficiente para el producto {producto.Nombre}");

            // El precio se obtiene del producto registrado.
            detalle.Precio = producto.Precio;
            detalle.CalcularSubtotal();

            producto.Stock -= detalle.Cantidad;
        }

        venta.Fecha = DateTime.Now;
        venta.Anulada = false;
        venta.CalcularTotal();

        if (venta.Recibido < venta.Total)
            return BadRequest("El dinero recibido no cubre el total de la venta");

        venta.CalcularCambio();

        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = venta.Id },
            venta
        );
    }

    // POST api/ventas/1/anular
    [HttpPost("{id}/anular")]
    public async Task<IActionResult> Anular(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null)
            return NotFound();

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

        return NoContent();
    }
}
