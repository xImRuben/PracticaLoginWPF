using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;

namespace PracticaLoginWPF
{
    public class ConexionDB
    {
        // Cadena de conexión (Asegúrate de que tu base de datos se llame 'NexusDB')
        private string connectionString = "Server=localhost;Database=NexusDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // =============================================================
        // 1. GESTIÓN DE LOGIN Y USUARIOS
        // =============================================================

        public Usuario LoginUsuario(string nombre, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // IMPORTANTE: En un entorno real, usaríamos Hashing. Aquí usamos texto plano por requerimiento.
                    string query = "SELECT * FROM usuarios WHERE BINARY nombre = @u AND BINARY password = @p";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", nombre);
                    cmd.Parameters.AddWithValue("@p", password);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Usuario
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),
                                Password = reader.GetString("password"),
                                FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd"),
                                Rol = reader["rol"].ToString(),
                                Email = reader["email"].ToString(),
                                Estado = reader["estado"].ToString()
                            };
                        }
                    }
                }
                catch { }
            }
            return null;
        }

        public string ObtenerMotivoBan(string nombre)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT motivo_ban FROM usuarios WHERE nombre = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", nombre);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : "";
                }
                catch { return ""; }
            }
        }

        public bool ExisteUsuario(string nombre)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM usuarios WHERE BINARY nombre = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", nombre);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }
        }

        // =============================================================
        // 2. CRUD USUARIOS + AVATAR
        // =============================================================

        public List<Usuario> ObtenerUsuarios(string filtro = "")
        {
            List<Usuario> lista = new List<Usuario>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM usuarios";
                    if (!string.IsNullOrEmpty(filtro)) query += " WHERE nombre LIKE @f";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    if (!string.IsNullOrEmpty(filtro)) cmd.Parameters.AddWithValue("@f", "%" + filtro + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] avatarBytes = null;
                            if (reader["avatar"] != DBNull.Value) avatarBytes = (byte[])reader["avatar"];

                            lista.Add(new Usuario
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),
                                Password = reader.GetString("password"),
                                FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd"),
                                Rol = reader["rol"].ToString(),
                                Email = reader["email"].ToString(),
                                Estado = reader["estado"].ToString(),
                                Avatar = avatarBytes
                            });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        public bool CrearUsuarioAdmin(Usuario u)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    if (ExisteUsuario(u.Nombre)) return false;
                    string query = "INSERT INTO usuarios (nombre, password, rol, email, estado) VALUES (@n, @p, @r, @e, @s)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@n", u.Nombre);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@r", u.Rol);
                    cmd.Parameters.AddWithValue("@e", u.Email);
                    cmd.Parameters.AddWithValue("@s", u.Estado);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool EditarUsuario(Usuario u)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET nombre=@n, password=@p, email=@e, rol=@r, estado=@s WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@n", u.Nombre);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@e", u.Email);
                    cmd.Parameters.AddWithValue("@r", u.Rol);
                    cmd.Parameters.AddWithValue("@s", u.Estado);
                    cmd.Parameters.AddWithValue("@id", u.Id);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        // SUBIR FOTO
        public bool ActualizarAvatar(int idUsuario, byte[] imagenBytes)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET avatar = @img WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@img", imagenBytes);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        // BORRAR FOTO
        public bool EliminarAvatar(int idUsuario)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET avatar = NULL WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool EliminarUsuario(int id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM usuarios WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool BanearUsuario(int id, bool banear, string motivo = "")
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string estado = banear ? "baneado" : "activo";
                    string sqlMotivo = banear ? motivo : "";
                    string query = "UPDATE usuarios SET estado = @s, motivo_ban = @m WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@s", estado);
                    cmd.Parameters.AddWithValue("@m", sqlMotivo);
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool RegistrarUsuario(string usuario, string password)
        {
            Usuario u = new Usuario { Nombre = usuario, Password = password, Rol = "user", Email = "", Estado = "activo" };
            return CrearUsuarioAdmin(u);
        }

        // =============================================================
        // 3. LOGS Y ESTADÍSTICAS
        // =============================================================

        public void RegistrarLog(string adminName, string tipoAccion, string usuarioAfectado)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO historial (admin_responsable, accion, usuario_afectado) VALUES (@adm, @acc, @usu)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@adm", adminName);
                    cmd.Parameters.AddWithValue("@acc", tipoAccion);
                    cmd.Parameters.AddWithValue("@usu", usuarioAfectado);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public DataTable ObtenerLogs()
        {
            DataTable dt = new DataTable();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM historial ORDER BY fecha DESC";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                catch { }
            }
            return dt;
        }

        public int[] ObtenerEstadisticas()
        {
            int[] datos = new int[3];
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd1 = new MySqlCommand("SELECT COUNT(*) FROM usuarios", conn);
                    datos[0] = Convert.ToInt32(cmd1.ExecuteScalar());
                    MySqlCommand cmd2 = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE estado='activo'", conn);
                    datos[1] = Convert.ToInt32(cmd2.ExecuteScalar());
                    MySqlCommand cmd3 = new MySqlCommand("SELECT COUNT(*) FROM usuarios WHERE estado='baneado'", conn);
                    datos[2] = Convert.ToInt32(cmd3.ExecuteScalar());
                }
                catch { }
            }
            return datos;
        }

        // =============================================================
        // 4. FACTOR X: APELACIONES (NUEVO)
        // =============================================================

        public bool EnviarApelacion(string usuario, string texto)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // 1. Obtener ID del usuario por nombre
                    string queryId = "SELECT id FROM usuarios WHERE nombre = @u";
                    MySqlCommand cmdId = new MySqlCommand(queryId, conn);
                    cmdId.Parameters.AddWithValue("@u", usuario);
                    object result = cmdId.ExecuteScalar();

                    if (result != null)
                    {
                        int uid = Convert.ToInt32(result);
                        // 2. Insertar la apelación
                        string query = "INSERT INTO apelaciones (id_usuario, texto_apelacion) VALUES (@uid, @txt)";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.AddWithValue("@txt", texto);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        public bool ExisteApelacionPendiente(string usuario)
        {
            // Verifica si este usuario ya tiene una apelación en estado 'pendiente'
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM apelaciones a JOIN usuarios u ON a.id_usuario = u.id WHERE u.nombre = @u AND a.estado = 'pendiente'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }
        }
    }
}