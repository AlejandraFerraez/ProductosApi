using ProductosApi.Models;

namespace ProductosApi.Repositories;


public interface IProductoRepository
{
    List<Producto> ObtenerTodos();
    Producto? ObtenerPorId(int id);
    int Insertar(Producto producto);
    bool Actualizar(Producto producto);
    bool Eliminar(int id);

    // Extra para cumplir el punto "Buscar" de la actividad.
    List<Producto> Buscar(string texto);
}