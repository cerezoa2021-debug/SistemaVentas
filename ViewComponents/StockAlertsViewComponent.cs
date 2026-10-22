using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.ViewComponents;

public class StockAlertsViewComponent : ViewComponent
{
    private readonly AppDbContext _context;

    public StockAlertsViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var lowStock = _context.Productos
            .AsNoTracking()
            .Where(product => product.Stock <= product.StockMinimo);

        var model = new StockAlertsViewModel
        {
            Count = await lowStock.CountAsync(),
            Products = await lowStock
                .OrderBy(product => product.Stock)
                .ThenBy(product => product.Nombre)
                .Take(8)
                .ToListAsync()
        };

        return View(model);
    }
}
