using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class ProductosController : Controller
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Productos
    public async Task<IActionResult> Index()
    {
        var productos = await _context.Productos
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        return View(productos);
    }

    // GET: /Productos/Create
    public async Task<IActionResult> Create()
    {
        await CargarCategorias();
        return View(new Producto());
    }

    // POST: /Productos/Create
    [HttpPost]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
            ModelState.AddModelError(nameof(producto.CategoriaId), "Selecciona una categoría válida.");

        if (!ModelState.IsValid)
        {
            await CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        await CargarCategorias(producto.CategoriaId);
        return View(producto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();

        if (!await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
            ModelState.AddModelError(nameof(producto.CategoriaId), "Selecciona una categoría válida.");

        if (!ModelState.IsValid)
        {
            await CargarCategorias(producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            var tieneVentas = await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id);
            if (tieneVentas)
            {
                ModelState.AddModelError("", "No se puede eliminar: el producto tiene detalles de venta registrados.");
                await _context.Entry(producto).Reference(p => p.Categoria).LoadAsync();
                return View("Delete", producto);
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCategorias(int? categoriaId = null)
    {
        ViewBag.CategoriaId = new SelectList(
            await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
            nameof(Categoria.Id),
            nameof(Categoria.Nombre),
            categoriaId);
    }
}
