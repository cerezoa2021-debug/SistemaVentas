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
        var activeSales = _context.Ventas.AsNoTracking().Where(venta => !venta.Anulada);

        var todaySales = activeSales.Where(venta => venta.Fecha >= today && venta.Fecha < today.AddDays(1));
        var monthSales = activeSales.Where(venta => venta.Fecha >= monthStart && venta.Fecha < monthStart.AddMonths(1));
        var yearSales = activeSales.Where(venta => venta.Fecha >= yearStart && venta.Fecha < yearStart.AddYears(1));

        var monthlyTotals = await yearSales
            .GroupBy(venta => venta.Fecha.Month)
            .Select(group => new { Month = group.Key, Amount = group.Sum(venta => venta.Total) })
            .ToListAsync();

        var salesByCategory = await _context.DetallesVenta
            .AsNoTracking()
            .Where(detail => detail.Venta != null && !detail.Venta.Anulada)
            .GroupBy(detail => detail.Producto!.Categoria!.Nombre)
            .Select(group => new CategorySalesViewModel
            {
                Name = group.Key,
                Amount = group.Sum(detail => detail.Subtotal)
            })
            .OrderByDescending(category => category.Amount)
            .ToListAsync();

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

        var viewModel = new DashboardViewModel
        {
            Today = now,
            SelectedYear = now.Year,
            TodaySalesCount = await todaySales.CountAsync(),
            TodaySalesAmount = await todaySales.SumAsync(venta => (decimal?)venta.Total) ?? 0,
            TotalSalesCount = await activeSales.CountAsync(),
            TotalSalesAmount = await activeSales.SumAsync(venta => (decimal?)venta.Total) ?? 0,
            MonthSalesCount = await monthSales.CountAsync(),
            MonthSalesAmount = await monthSales.SumAsync(venta => (decimal?)venta.Total) ?? 0,
            RegisteredClients = await _context.Clientes.CountAsync(),
            AverageMonthlySales = (await yearSales.SumAsync(venta => (decimal?)venta.Total) ?? 0) / now.Month,
            MonthlySales = Enumerable.Range(1, 12)
                .Select(month => new MonthlySalesViewModel
                {
                    Month = month,
                    Amount = monthlyTotals.FirstOrDefault(total => total.Month == month)?.Amount ?? 0
                })
                .ToList(),
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
