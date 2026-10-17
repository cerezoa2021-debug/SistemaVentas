
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/detallesventa")]
public class DetallesVentaApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public DetallesVentaApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/detallesventa
    [HttpGet]
    public async Task<ActionResult<List<DetalleVenta>>> GetAll()
    {
        return await _context.DetallesVenta
            .Include(d => d.Venta)
            .Include(d => d.Producto)
            .OrderBy(d => d.Id)
            .ToListAsync();
    }

    // GET api/detallesventa/1
    [HttpGet("{id}")]
    public async Task<ActionResult<DetalleVenta>> GetById(int id)
    {
        var detalle = await _context.DetallesVenta
            .Include(d => d.Venta)
            .Include(d => d.Producto)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (detalle == null)
            return NotFound();

        return detalle;
    }

}
