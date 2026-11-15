using CrudClientesCSharp.Data;
using CrudClientesCSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrudClientesCSharp.Controllers;

public class ClientesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ClientesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Clientes.AsNoTracking()
            .Include(c => c.Provincia).Include(c => c.Canton)
            .OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres).ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        var cliente = await BuscarCliente(id);
        return cliente == null ? NotFound() : View(cliente);
    }

    public async Task<IActionResult> Create()
    {
        await CargarUbicaciones();
        return View(new Cliente());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Nombres,Apellidos,Cedula,Correo,Telefono,Direccion,FechaNacimiento,Ocupacion,ProvinciaId,CantonId")] Cliente cliente)
    {
        await ValidarUbicacion(cliente);
        if (ModelState.IsValid)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Cliente registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }
        await CargarUbicaciones(cliente.ProvinciaId, cliente.CantonId);
        return View(cliente);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        await CargarUbicaciones(cliente.ProvinciaId, cliente.CantonId);
        return View(cliente);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id,
        [Bind("Id,Nombres,Apellidos,Cedula,Correo,Telefono,Direccion,FechaNacimiento,Ocupacion,ProvinciaId,CantonId")] Cliente cliente)
    {
        if (id != cliente.Id || !await _context.Clientes.AnyAsync(c => c.Id == id))
            return NotFound();

        await ValidarUbicacion(cliente);
        if (ModelState.IsValid)
        {
            try
            {
                _context.Clientes.Update(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Clientes.AnyAsync(c => c.Id == id))
                    return NotFound();
                throw;
            }
            TempData["Mensaje"] = "Datos del cliente actualizados.";
            return RedirectToAction(nameof(Index));
        }
        await CargarUbicaciones(cliente.ProvinciaId, cliente.CantonId);
        return View(cliente);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        var cliente = await BuscarCliente(id);
        return cliente == null ? NotFound() : View(cliente);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
            return NotFound();

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();
        TempData["Mensaje"] = "Cliente eliminado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetCantones(int provinciaId)
    {
        var cantones = await _context.Cantones.AsNoTracking()
            .Where(c => c.ProvinciaId == provinciaId).OrderBy(c => c.Nombre)
            .Select(c => new { c.Id, c.Nombre }).ToListAsync();
        return Json(cantones);
    }

    private async Task<Cliente?> BuscarCliente(int? id)
    {
        return await _context.Clientes.AsNoTracking()
            .Include(c => c.Provincia).Include(c => c.Canton)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    private async Task CargarUbicaciones(int provinciaId = 0, int cantonId = 0)
    {
        ViewBag.Provincias = new SelectList(await _context.Provincias.AsNoTracking()
            .OrderBy(p => p.Nombre).ToListAsync(), "Id", "Nombre", provinciaId);
        ViewBag.Cantones = new SelectList(await _context.Cantones.AsNoTracking()
            .Where(c => c.ProvinciaId == provinciaId).OrderBy(c => c.Nombre).ToListAsync(),
            "Id", "Nombre", cantonId);
    }

    private async Task ValidarUbicacion(Cliente cliente)
    {
        if (cliente.ProvinciaId > 0 && !await _context.Provincias.AnyAsync(p => p.Id == cliente.ProvinciaId))
            ModelState.AddModelError(nameof(Cliente.ProvinciaId), "La provincia seleccionada no existe.");

        if (cliente.CantonId > 0 && !await _context.Cantones
            .AnyAsync(c => c.Id == cliente.CantonId && c.ProvinciaId == cliente.ProvinciaId))
            ModelState.AddModelError(nameof(Cliente.CantonId), "Seleccione un cantón que pertenezca a la provincia elegida.");

        if (cliente.FechaNacimiento?.Date > DateTime.Today)
            ModelState.AddModelError(nameof(Cliente.FechaNacimiento), "La fecha de nacimiento no puede ser futura.");
    }
}
