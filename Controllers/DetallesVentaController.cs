using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers;

public class DetallesVentaController : Controller
{
    private readonly AppDbContext _context;

    public DetallesVentaController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /DetallesVenta
    public async Task<IActionResult> Index()
    {
        var detalles = await _context.DetallesVenta
            .Include(d => d.Venta)
            .Include(d => d.Producto)
            .OrderBy(d => d.Id)
            .ToListAsync();

        return View(detalles);
    }

}
