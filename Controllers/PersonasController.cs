using Microsoft.AspNetCore.Mvc;
using Npgsql;
using MiWebService.Models;

namespace MiWebService.Controllers
{
    [ApiController]
    [Route("MiWebService")]
    public class PersonasController : ControllerBase
    {
        private readonly string _connectionString;

        public PersonasController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("ConexionServidor");
        }

        [HttpPost]
        [Route("GetPersonas")]
        public List<Persona> GetPersonas(DateTime? dtFechaModificacion)
        {
            NpgsqlConnection connection = null;
            var personas = new List<Persona>();

            try
            {
                connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                string sql = @"SELECT int_id, var_nombre, var_apaterno, var_amaterno, var_telefono, var_correo, 
                                    var_nametag, dt_fecha_registro, dt_fecha_modificacion, dt_fecha_nacimiento, int_empresa_id, bol_enuso 
                                FROM usuarios
                                WHERE ";

                if (dtFechaModificacion == null)
                    sql += "bol_enuso = true";
                else
                    sql += "dt_fecha_modificacion >= @dtFechaModificacion AND bol_enuso = true";

                sql += " ORDER BY dt_fecha_modificacion DESC, var_nombre";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    if (dtFechaModificacion != null && dtFechaModificacion != DateTime.MinValue)
                        command.Parameters.AddWithValue("@dtFechaModificacion", dtFechaModificacion.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var persona = new Persona
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                Nombre = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                APaterno = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                AMaterno = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Telefono = reader.IsDBNull(4) ? 0 : reader.GetInt64(4),
                                Correo = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                            };
                            personas.Add(persona);
                        }
                    }
                }

                return personas;
            }
            catch (Exception)
            {
                return new List<Persona>();
            }
            finally
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }


        [HttpPost]
        [Route("CreatePersona")]
        public int CreatePersona([FromBody] Persona persona)
        {
            NpgsqlConnection connection = null;

            try
            {
                if (persona == null)
                    return -1;

                connection = new NpgsqlConnection(_connectionString);
                connection.Open();

                using (var setTimezoneCommand = new NpgsqlCommand("SET TIME ZONE 'UTC'", connection))
                {
                    setTimezoneCommand.ExecuteNonQuery();
                }

                DateTime fechaCreacion = DateTime.UtcNow;
                DateTime fechaModificacion = DateTime.UtcNow;

                string sql = @"INSERT INTO usuarios 
                        (var_nombre, var_apaterno, var_amaterno, var_telefono, var_correo, var_nametag, 
                        dt_fecha_nacimiento, dt_fecha_registro, dt_fecha_modificacion, int_empresa_id, bol_enuso)
                        VALUES (@nombre, @apaterno, @amaterno, @telefono, @correo, @nametag, 
                                @fechaNacimiento, date_trunc('milliseconds', @fechaRegistro), date_trunc('milliseconds', @fechaModificacion), @empresaId, true)
                        RETURNING int_id;";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre",
                        string.IsNullOrEmpty(persona.Nombre) ? string.Empty : persona.Nombre);
                    command.Parameters.AddWithValue("@apaterno",
                        string.IsNullOrEmpty(persona.APaterno) ? string.Empty : persona.APaterno);
                    command.Parameters.AddWithValue("@amaterno",
                        string.IsNullOrEmpty(persona.AMaterno) ? string.Empty : persona.AMaterno);
                    command.Parameters.AddWithValue("@telefono",
                        persona.Telefono);
                    command.Parameters.AddWithValue("@correo",
                        string.IsNullOrEmpty(persona.Correo) ? string.Empty : persona.Correo);
                    command.Parameters.AddWithValue("@fechaRegistro", fechaCreacion);
                    command.Parameters.AddWithValue("@fechaModificacion", fechaModificacion);
                    DateTime fechaNacimiento = DateTime.Parse(persona.FechaNacimiento);
                    command.Parameters.AddWithValue("@fechaNacimiento", fechaNacimiento.Date);
                    var result = command.ExecuteScalar();
                    if (result == null)
                        return 0;
                    int idCreado = (int)result;
                    return idCreado;
                }
            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                if (connection?.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }

        [HttpPost]
        [Route("GenerarToken")]
        public string GenerarToken()
        {
            return "TokenGenerado";
        }

        [HttpPost]
        [Route("GenerarToken1")]
        public string GenerarToken1()
        {
            return "TokenGenerado1";
        }
    }
}
