
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers.Api;

[ApiController]
[Route("api/clientes")]
public class ClientesApiController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesApiController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/clientes
    [HttpGet]
    public async Task<ActionResult<List<Cliente>>> GetAll()
    {
        return await _context.Clientes
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    // GET api/clientes/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound();

        return cliente;
    }

    // POST api/clientes
    // Body: {"nombre": "Juan Perez", "email": "juan@gmail.com", "celular": "70000000"}
    [HttpPost]
    public async Task<ActionResult<Cliente>> Create(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = cliente.Id },
            cliente
        );
    }

    // PUT api/clientes/1
    // Body: {"id": 1, "nombre": "Juan Perez", "email": "juan@gmail.com", "celular": "70000000"}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Cliente cliente)
    {
        if (id != cliente.Id)
            return BadRequest("El Id de la URL no coincide con el del cuerpo");

        if (!await _context.Clientes.AnyAsync(c => c.Id == id))
            return NotFound();

        _context.Entry(cliente).State = EntityState.Modified;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/clientes/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente == null)
            return NotFound();

        if (await _context.Ventas.AnyAsync(v => v.ClienteId == id))
            return Conflict("No se puede eliminar: el cliente tiene ventas registradas");

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
