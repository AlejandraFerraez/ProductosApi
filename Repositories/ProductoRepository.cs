using Microsoft.Data.SqlClient;
using ProductosApi.Models;

namespace ProductosApi.Repositories;

// Esta clase implementa la interfaz: cumple el contrato IProductoRepository.
// Aquí vive todo el acceso a datos con ADO.NET.
public class ProductoRepository : IProductoRepository
{
    // Cadena de conexión: dirección, base de datos y credenciales de SQL Server.
    private readonly string _connectionString;

    // El constructor recibe la configuración (appsettings.json)
    // y lee la cadena "DefaultConnection".
    public ProductoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión.");
    }

    // CONNECTION: crea una conexión nueva. Cada método la abre dentro de "using"
    // para que se cierre sola, aunque ocurra un error.
    private SqlConnection CrearConexion()
    {
        return new SqlConnection(_connectionString);
    }

    // CONSULTA: devuelve todos los productos (Command + DataReader).
    public List<Producto> ObtenerTodos()
    {
        var lista = new List<Producto>();

        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            // COMMAND: la instrucción SQL que se ejecuta.
            string sql = "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos";
            using var comando = new SqlCommand(sql, conexion);

            // DATAREADER: lee las filas una por una, solo hacia adelante.
            using var lector = comando.ExecuteReader();
            while (lector.Read())
            {
                lista.Add(LeerProducto(lector));
            }
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al obtener los productos: " + ex.Message, ex);
        }

        return lista;
    }

    // CONSULTA POR ID: devuelve un producto o null si no existe.
    public Producto? ObtenerPorId(int id)
    {
        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            // PARÁMETRO SQL: @Id evita la inyección SQL.
            string sql = "SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos WHERE Id = @Id";
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Id", id);

            using var lector = comando.ExecuteReader();
            if (lector.Read())
            {
                return LeerProducto(lector);
            }
            return null;
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al obtener el producto: " + ex.Message, ex);
        }
    }

    // INSERT: agrega un producto y devuelve el Id que generó SQL Server.
    public int Insertar(Producto producto)
    {
        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            // SCOPE_IDENTITY() devuelve el Id recién generado (IDENTITY).
            string sql = @"INSERT INTO Productos (Nombre, Descripcion, Precio, Stock)
                           VALUES (@Nombre, @Descripcion, @Precio, @Stock);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
            // Si la descripción es null, se manda DBNull para guardar NULL en la BD.
            comando.Parameters.AddWithValue("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value);
            comando.Parameters.AddWithValue("@Precio", producto.Precio);
            comando.Parameters.AddWithValue("@Stock", producto.Stock);

            // ExecuteScalar devuelve un solo valor: el nuevo Id.
            return (int)comando.ExecuteScalar()!;
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al insertar el producto: " + ex.Message, ex);
        }
    }

    // UPDATE: modifica un producto. Devuelve true si se actualizó alguna fila.
    public bool Actualizar(Producto producto)
    {
        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            string sql = @"UPDATE Productos
                           SET Nombre = @Nombre, Descripcion = @Descripcion,
                               Precio = @Precio, Stock = @Stock
                           WHERE Id = @Id";
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Id", producto.Id);
            comando.Parameters.AddWithValue("@Nombre", producto.Nombre);
            comando.Parameters.AddWithValue("@Descripcion", (object?)producto.Descripcion ?? DBNull.Value);
            comando.Parameters.AddWithValue("@Precio", producto.Precio);
            comando.Parameters.AddWithValue("@Stock", producto.Stock);

            // ExecuteNonQuery devuelve cuántas filas se afectaron.
            return comando.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al actualizar el producto: " + ex.Message, ex);
        }
    }


    public bool Eliminar(int id)
    {
        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            string sql = "DELETE FROM Productos WHERE Id = @Id";
            using var comando = new SqlCommand(sql, conexion);
            comando.Parameters.AddWithValue("@Id", id);

            return comando.ExecuteNonQuery() > 0;
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al eliminar el producto: " + ex.Message, ex);
        }
    }


    public List<Producto> Buscar(string texto)
    {
        var lista = new List<Producto>();

        try
        {
            using var conexion = CrearConexion();
            conexion.Open();

            string sql = @"SELECT Id, Nombre, Descripcion, Precio, Stock FROM Productos
                           WHERE Nombre LIKE @Texto OR Descripcion LIKE @Texto";
            using var comando = new SqlCommand(sql, conexion);

            comando.Parameters.AddWithValue("@Texto", "%" + texto + "%");

            using var lector = comando.ExecuteReader();
            while (lector.Read())
            {
                lista.Add(LeerProducto(lector));
            }
        }
        catch (SqlException ex)
        {
            throw new Exception("Error al buscar productos: " + ex.Message, ex);
        }

        return lista;
    }

    private static Producto LeerProducto(SqlDataReader lector)
    {
        return new Producto
        {
            Id = lector.GetInt32(0),
            Nombre = lector.GetString(1),
      
            Descripcion = lector.IsDBNull(2) ? null : lector.GetString(2),
            Precio = lector.GetDecimal(3),
            Stock = lector.GetInt32(4)
        };
    }
}