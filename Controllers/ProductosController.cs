using Microsoft.AspNetCore.Mvc;
using ProductosApi.Models;
using ProductosApi.Repositories;

namespace ProductosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{

    private readonly IProductoRepository _repo;


    public ProductosController(IProductoRepository repo)
    {
        _repo = repo;
    }


    [HttpGet]
    public ActionResult<List<Producto>> ObtenerTodos()
    {
        return Ok(_repo.ObtenerTodos());
    }


    [HttpGet("buscar")]
    public ActionResult<List<Producto>> Buscar([FromQuery] string texto)
    {
        return Ok(_repo.Buscar(texto));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Producto> ObtenerPorId(int id)
    {
        var producto = _repo.ObtenerPorId(id);
        if (producto == null)
            return NotFound($"No existe el producto con Id {id}.");
        return Ok(producto);
    }

    [HttpPost]
    public ActionResult Insertar([FromBody] Producto producto)
    {
        if (string.IsNullOrWhiteSpace(producto.Nombre))
            return BadRequest("El nombre es obligatorio.");

        int nuevoId = _repo.Insertar(producto);
        producto.Id = nuevoId;
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nuevoId }, producto);
    }


    [HttpPut("{id:int}")]
    public ActionResult Actualizar(int id, [FromBody] Producto producto)
    {
        producto.Id = id;
        if (!_repo.Actualizar(producto))
            return NotFound($"No existe el producto con Id {id}.");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public ActionResult Eliminar(int id)
    {
        if (!_repo.Eliminar(id))
            return NotFound($"No existe el producto con Id {id}.");
        return NoContent();
    }
}