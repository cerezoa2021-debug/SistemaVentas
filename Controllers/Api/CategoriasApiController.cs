using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/categorias")]
public class CategoriasApiController: ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriasApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/categorias
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> GetAll()
    {
        return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
    }

    // GET api/categorias/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound();
        return categoria;
    }

    // POST api/categorias
    // Body: {"nombre": "Limpieza"}
    [HttpPost]
    public async Task<ActionResult<Categoria>> Create(Categoria categoria)
    {
        if (await _context.Categorias.AnyAsync(c => c.Nombre == categoria.Nombre))
            return Conflict("Ya existe una categoría con ese nombre.");

        _context.Categorias.Add(categoria);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo guardar la categoría. Puede que ya exista otra con ese nombre.");
        }

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    // PUT api/categorias/3
    // Body: {"id": 3, "nombre": "Bebidas gaseosas"}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Categoria categoria)
    {
        if (id != categoria.Id)
            return BadRequest("El Id de la URL no coincide con el del cuerpo");

        if (!await _context.Categorias.AnyAsync(c => c.Id == id))
            return NotFound();

        if (await _context.Categorias.AnyAsync(c => c.Id != id && c.Nombre == categoria.Nombre))
            return Conflict("Ya existe una categoría con ese nombre.");

        _context.Categorias.Update(categoria);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo guardar la categoría. Puede que ya exista otra con ese nombre.");
        }

        return NoContent();
    }

    // DELETE api/categorias/4
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
            return NotFound();

        if (await _context.Productos.AnyAsync(p => p.CategoriaId == id))
            return Conflict("No se puede eliminar: la categoria tiene productos");

        _context.Categorias.Remove(categoria);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict("No se pudo eliminar la categoría porque tiene productos asociados.");
        }

        return NoContent();
    }
}
