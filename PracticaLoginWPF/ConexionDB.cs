using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic; // <--- IMPRESCINDIBLE PARA USAR LISTAS

namespace PracticaLoginWPF
{
    public class ConexionDB
    {
        // -------------------------------------------------------------
        // CONFIGURACIÓN DE CONEXIÓN
        // -------------------------------------------------------------
        // Si usas XAMPP por defecto: Uid=root; Pwd=;
        private string connectionString = "Server=localhost;Database=NexusDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // -------------------------------------------------------------
        // 1. LOGIN ESTRICTO (Case Sensitive)
        // -------------------------------------------------------------
        public bool ValidarUsuario(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // 'BINARY' obliga a diferenciar mayúsculas de minúsculas
                    string query = "SELECT COUNT(*) FROM usuarios WHERE BINARY nombre = @u AND BINARY password = @p";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@p", password);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                catch (Exception) { return false; }
            }
        }

        // -------------------------------------------------------------
        // 2. REGISTRO DE NUEVO USUARIO
        // -------------------------------------------------------------
        public bool RegistrarUsuario(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();

                    // Verificar si ya existe
                    string checkQuery = "SELECT COUNT(*) FROM usuarios WHERE BINARY nombre = @u";
                    MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@u", usuario);

                    if (Convert.ToInt32(checkCmd.ExecuteScalar()) > 0) return false;

                    // Insertar nuevo
                    string query = "INSERT INTO usuarios (nombre, password) VALUES (@u, @p)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@p", password);

                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception) { return false; }
            }
        }

        // -------------------------------------------------------------
        // 3. OBTENER TODOS LOS USUARIOS (Para el Panel Admin)
        // -------------------------------------------------------------
        public List<Usuario> ObtenerUsuarios()
        {
            List<Usuario> lista = new List<Usuario>();

            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM usuarios";
                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Usuario u = new Usuario();

                            // Leemos los datos de la columna de MySQL
                            u.Id = reader.GetInt32("id");
                            u.Nombre = reader.GetString("nombre");
                            u.Password = reader.GetString("password");

                            // Formateamos la fecha para que se vea bonita
                            u.FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd HH:mm");

                            lista.Add(u);
                        }
                    }
                }
                catch (Exception)
                {
                    // Si hay error, devolverá la lista vacía o lo que haya conseguido cargar
                }
            }
            return lista;
        }
    }
}