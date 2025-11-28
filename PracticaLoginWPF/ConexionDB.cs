using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;

namespace PracticaLoginWPF
{
    public class ConexionDB
    {
        // AJUSTA TU CONTRASEÑA SI ES NECESARIO (Pwd=...)
        private string connectionString = "Server=localhost;Database=NexusDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // ==============================================================
        // 1. MÉTODO NUEVO: VERIFICAR SOLO SI EXISTE EL NOMBRE
        // ==============================================================
        public bool ExisteUsuario(string usuario)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // BINARY para distinguir mayúsculas de minúsculas
                    string query = "SELECT COUNT(*) FROM usuarios WHERE BINARY nombre = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);

                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }
        }

        // 2. VALIDAR LOGIN COMPLETO (Usuario + Pass)
        public bool ValidarUsuario(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM usuarios WHERE BINARY nombre = @u AND BINARY password = @p";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@p", password);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
                catch { return false; }
            }
        }

        // 3. REGISTRAR USUARIO
        public bool RegistrarUsuario(string usuario, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // Usamos el método ExisteUsuario para no repetir código
                    if (ExisteUsuario(usuario)) return false;

                    string query = "INSERT INTO usuarios (nombre, password) VALUES (@u, @p)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", usuario);
                    cmd.Parameters.AddWithValue("@p", password);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        // 4. LISTAR USUARIOS (Admin)
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
                            u.Id = reader.GetInt32("id");
                            u.Nombre = reader.GetString("nombre");
                            u.Password = reader.GetString("password");
                            u.FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd HH:mm");
                            lista.Add(u);
                        }
                    }
                }
                catch { }
            }
            return lista;
        }
    }
}