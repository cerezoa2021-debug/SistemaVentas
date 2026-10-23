namespace SistemaVentas.Models;

public class DashboardViewModel
{
    public DateTime Today { get; set; }
    public int SelectedYear { get; set; }
    public int TodaySalesCount { get; set; }
    public decimal TodaySalesAmount { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int MonthSalesCount { get; set; }
    public decimal MonthSalesAmount { get; set; }
    public int TotalProducts { get; set; }
    public int LowStockProductsCount { get; set; }
    public int RegisteredClients { get; set; }
    public decimal AverageMonthlySales { get; set; }
    public List<MonthlySalesViewModel> MonthlySales { get; set; } = new();
    public List<CategorySalesViewModel> SalesByCategory { get; set; } = new();
    public List<TopProductViewModel> BestSellingProducts { get; set; } = new();
    public List<WeeklySalesViewModel> WeeklySales { get; set; } = new();
    public List<Venta> RecentSales { get; set; } = new();
    public List<Producto> LowStockProducts { get; set; } = new();
}

public class WeeklySalesViewModel
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
}

public class MonthlySalesViewModel
{
    public int Month { get; set; }
    public decimal Amount { get; set; }
}

public class CategorySalesViewModel
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class TopProductViewModel
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
