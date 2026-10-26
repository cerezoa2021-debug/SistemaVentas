using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.Now;
        var today = now.Date;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var yearStart = new DateTime(now.Year, 1, 1);
        var weekStart = today.AddDays(-6);
        var activeSales = _context.Ventas.AsNoTracking().Where(venta => !venta.Anulada);

        var todaySales = activeSales.Where(venta => venta.Fecha >= today && venta.Fecha < today.AddDays(1));
        var monthSales = activeSales.Where(venta => venta.Fecha >= monthStart && venta.Fecha < monthStart.AddMonths(1));
        var yearSales = activeSales.Where(venta => venta.Fecha >= yearStart && venta.Fecha < yearStart.AddYears(1));

        var yearSalesRows = await yearSales
            .Select(venta => new { venta.Fecha, venta.Total })
            .ToListAsync();
        var monthlyTotals = yearSalesRows
            .GroupBy(venta => venta.Fecha.Month)
            .ToDictionary(group => group.Key, group => group.Sum(venta => venta.Total));
        var weeklySalesRows = await activeSales
            .Where(venta => venta.Fecha >= weekStart && venta.Fecha < today.AddDays(1))
            .Select(venta => new { venta.Fecha, venta.Total })
            .ToListAsync();
        var weeklySales = Enumerable.Range(0, 7)
            .Select(offset =>
            {
                var date = weekStart.AddDays(offset);
                return new WeeklySalesViewModel
                {
                    Date = date,
                    Amount = weeklySalesRows
                        .Where(sale => sale.Fecha.Date == date)
                        .Sum(sale => sale.Total)
                };
            })
            .ToList();

        var categorySalesRows = await _context.DetallesVenta
            .AsNoTracking()
            .Where(detail => detail.Venta != null && !detail.Venta.Anulada)
            .Select(detail => new
            {
                CategoryName = detail.Producto!.Categoria!.Nombre,
                detail.Subtotal
            })
            .ToListAsync();
        var salesByCategory = categorySalesRows
            .GroupBy(detail => detail.CategoryName)
            .Select(group => new CategorySalesViewModel
            {
                Name = group.Key,
                Amount = group.Sum(detail => detail.Subtotal)
            })
            .OrderByDescending(category => category.Amount)
            .ToList();

        var bestSellingProducts = await _context.DetallesVenta
            .AsNoTracking()
            .Where(detail => detail.Venta != null && !detail.Venta.Anulada)
            .GroupBy(detail => new { detail.ProductoId, detail.Producto!.Nombre })
            .Select(group => new TopProductViewModel
            {
                Name = group.Key.Nombre,
                Quantity = group.Sum(detail => detail.Cantidad)
            })
            .OrderByDescending(product => product.Quantity)
            .ThenBy(product => product.Name)
            .Take(5)
            .ToListAsync();
        var allActiveSaleTotals = await activeSales
            .Select(venta => venta.Total)
            .ToListAsync();
        var yearSalesTotal = yearSalesRows.Sum(venta => venta.Total);

        var viewModel = new DashboardViewModel
        {
            Today = now,
            SelectedYear = now.Year,
            TodaySalesCount = await todaySales.CountAsync(),
            TodaySalesAmount = yearSalesRows
                .Where(venta => venta.Fecha >= today && venta.Fecha < today.AddDays(1))
                .Sum(venta => venta.Total),
            TotalSalesCount = await activeSales.CountAsync(),
            TotalSalesAmount = allActiveSaleTotals.Sum(),
            MonthSalesCount = await monthSales.CountAsync(),
            MonthSalesAmount = yearSalesRows
                .Where(venta => venta.Fecha >= monthStart && venta.Fecha < monthStart.AddMonths(1))
                .Sum(venta => venta.Total),
            TotalProducts = await _context.Productos.CountAsync(),
            LowStockProductsCount = await _context.Productos.CountAsync(product => product.Stock <= product.StockMinimo),
            RegisteredClients = await _context.Clientes.CountAsync(),
            AverageMonthlySales = yearSalesTotal / now.Month,
            MonthlySales = Enumerable.Range(1, 12)
                .Select(month => new MonthlySalesViewModel
                {
                    Month = month,
                    Amount = monthlyTotals.GetValueOrDefault(month)
                })
                .ToList(),
            WeeklySales = weeklySales,
            SalesByCategory = salesByCategory,
            BestSellingProducts = bestSellingProducts,
            RecentSales = await _context.Ventas
                .AsNoTracking()
                .Include(venta => venta.Cliente)
                .OrderByDescending(venta => venta.Fecha)
                .Take(6)
                .ToListAsync(),
            LowStockProducts = await _context.Productos
                .AsNoTracking()
                .Include(product => product.Categoria)
                .Where(product => product.Stock <= product.StockMinimo)
                .OrderBy(product => product.Stock)
                .ThenBy(product => product.Nombre)
                .Take(6)
                .ToListAsync()
        };

        return View(viewModel);
    }

    public IActionResult Error()
    {
        return View();
    }
}
