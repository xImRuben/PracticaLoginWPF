using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;

namespace PracticaLoginWPF
{
    public class ConexionDB
    {
        private string connectionString = "Server=localhost;Database=NexusDB;Uid=root;Pwd=;";

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        // Helper estático para imágenes
        public static BitmapImage ConvertirImagen(byte[] imagenBytes)
        {
            if (imagenBytes == null || imagenBytes.Length == 0) return null;
            try
            {
                using (var stream = new MemoryStream(imagenBytes))
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    return image;
                }
            }
            catch { return null; }
        }

        // =============================================================
        // 1. GESTIÓN DE LOGIN, REGISTRO Y SALDO
        // =============================================================

        public Usuario LoginUsuario(string nombre, string password)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM usuarios WHERE BINARY nombre = @u AND BINARY password = @p";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", nombre);
                    cmd.Parameters.AddWithValue("@p", password);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            byte[] avatarBytes = null;
                            if (reader["avatar"] != DBNull.Value) avatarBytes = (byte[])reader["avatar"];
                            decimal saldo = 0;
                            if (reader["saldo"] != DBNull.Value) saldo = reader.GetDecimal("saldo");

                            return new Usuario
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),
                                Password = reader.GetString("password"),
                                FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd"),
                                Rol = reader["rol"].ToString(),
                                Email = reader["email"].ToString(),
                                Estado = reader["estado"].ToString(),
                                Avatar = avatarBytes,
                                Saldo = saldo
                            };
                        }
                    }
                }
                catch { }
            }
            return null;
        }

        public bool ActualizarSaldo(int idUsuario, decimal nuevoSaldo)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET saldo = @s WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@s", nuevoSaldo);
                    cmd.Parameters.AddWithValue("@id", idUsuario);
                    return cmd.ExecuteNonQuery() > 0;
                }
                catch { return false; }
            }
        }

        public bool RegistrarUsuario(string usuario, string password)
        {
            Usuario u = new Usuario { Nombre = usuario, Password = password, Rol = "user", Email = "", Estado = "activo", Saldo = 100 };
            return CrearUsuarioAdmin(u);
        }

        // =============================================================
        // 2. CRUD USUARIOS (ADMIN)
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
                            decimal saldo = 0;
                            if (reader["saldo"] != DBNull.Value) saldo = reader.GetDecimal("saldo");

                            lista.Add(new Usuario
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre"),
                                Password = reader.GetString("password"),
                                FechaRegistro = reader.GetDateTime("fecha_registro").ToString("yyyy-MM-dd"),
                                Rol = reader["rol"].ToString(),
                                Email = reader["email"].ToString(),
                                Estado = reader["estado"].ToString(),
                                Avatar = avatarBytes,
                                Saldo = saldo
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
                    string query = "INSERT INTO usuarios (nombre, password, rol, email, estado, saldo) VALUES (@n, @p, @r, @e, @s, @dinero)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@n", u.Nombre);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@r", u.Rol);
                    cmd.Parameters.AddWithValue("@e", u.Email);
                    cmd.Parameters.AddWithValue("@s", u.Estado);
                    cmd.Parameters.AddWithValue("@dinero", u.Saldo);
                    cmd.ExecuteNonQuery();
                    return true;
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
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

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
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error Avatar: " + ex.Message);
                    return false;
                }
            }
        }

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
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        public bool EditarPerfilUsuario(Usuario u)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET password=@p, email=@e WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@p", u.Password);
                    cmd.Parameters.AddWithValue("@e", u.Email);
                    cmd.Parameters.AddWithValue("@id", u.Id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
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
                    cmd.ExecuteNonQuery();
                    return true;
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
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        // =============================================================
        // 3. JUEGOS Y CATALOGO
        // =============================================================

        public List<Juego> ObtenerJuegos(bool esAdmin)
        {
            List<Juego> lista = new List<Juego>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT * FROM juegos";
                    if (!esAdmin) query += " WHERE visible = TRUE";

                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] imgBytes = null;
                            if (reader["caratula"] != DBNull.Value) imgBytes = (byte[])reader["caratula"];
                            int val = 5;
                            if (reader["valoracion"] != DBNull.Value) val = reader.GetInt32("valoracion");

                            lista.Add(new Juego
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Genero = reader.GetString("genero"),
                                Precio = reader.GetDecimal("precio"),
                                Descripcion = reader.GetString("descripcion"),
                                Visible = reader.GetBoolean("visible"),
                                Caratula = imgBytes,
                                Valoracion = val
                            });
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error Cargar Juegos: " + ex.Message); }
            }
            return lista;
        }

        public bool AgregarJuego(Juego j)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO juegos (titulo, genero, precio, descripcion, visible, caratula, valoracion) VALUES (@t, @g, @p, @d, @v, @img, 5)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@t", j.Titulo);
                    cmd.Parameters.AddWithValue("@g", j.Genero);
                    cmd.Parameters.AddWithValue("@p", j.Precio);
                    cmd.Parameters.AddWithValue("@d", j.Descripcion);
                    cmd.Parameters.AddWithValue("@v", j.Visible);
                    if (j.Caratula != null && j.Caratula.Length > 0) cmd.Parameters.AddWithValue("@img", j.Caratula);
                    else cmd.Parameters.AddWithValue("@img", DBNull.Value);

                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { MessageBox.Show("Error Agregar Juego: " + ex.Message); return false; }
            }
        }

        public bool ModificarJuego(Juego j)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE juegos SET titulo=@t, genero=@g, precio=@p, descripcion=@d, visible=@v";
                    if (j.Caratula != null) query += ", caratula=@img";
                    query += " WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@t", j.Titulo);
                    cmd.Parameters.AddWithValue("@g", j.Genero);
                    cmd.Parameters.AddWithValue("@p", j.Precio);
                    cmd.Parameters.AddWithValue("@d", j.Descripcion);
                    cmd.Parameters.AddWithValue("@v", j.Visible);
                    cmd.Parameters.AddWithValue("@id", j.Id);
                    if (j.Caratula != null) cmd.Parameters.AddWithValue("@img", j.Caratula);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { MessageBox.Show("Error Modificar Juego: " + ex.Message); return false; }
            }
        }

        public bool EliminarJuego(int id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM juegos WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        public bool RegistrarCompra(int idUsuario, int idJuego)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string check = "SELECT COUNT(*) FROM biblioteca WHERE id_usuario=@u AND id_juego=@j";
                    MySqlCommand cmdCheck = new MySqlCommand(check, conn);
                    cmdCheck.Parameters.AddWithValue("@u", idUsuario);
                    cmdCheck.Parameters.AddWithValue("@j", idJuego);
                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0) return false;

                    string query = "INSERT INTO biblioteca (id_usuario, id_juego) VALUES (@u, @j)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    cmd.Parameters.AddWithValue("@j", idJuego);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex) { MessageBox.Show("Error SQL Compra: " + ex.Message); return false; }
            }
        }

        public List<Juego> ObtenerBiblioteca(int idUsuario)
        {
            List<Juego> misJuegos = new List<Juego>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT j.* FROM juegos j JOIN biblioteca b ON j.id = b.id_juego WHERE b.id_usuario = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] imgBytes = null;
                            if (reader["caratula"] != DBNull.Value) imgBytes = (byte[])reader["caratula"];
                            int val = 5;
                            if (reader["valoracion"] != DBNull.Value) val = reader.GetInt32("valoracion");

                            misJuegos.Add(new Juego
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Genero = reader.GetString("genero"),
                                Precio = reader.GetDecimal("precio"),
                                Descripcion = reader.GetString("descripcion"),
                                Visible = reader.GetBoolean("visible"),
                                Caratula = imgBytes,
                                Valoracion = val
                            });
                        }
                    }
                }
                catch { }
            }
            return misJuegos;
        }

        // =============================================================
        // 4. LOGS, CHAT, ESTADISTICAS
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

        public bool EnviarApelacion(string usuario, string texto)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string queryId = "SELECT id FROM usuarios WHERE nombre = @u";
                    MySqlCommand cmdId = new MySqlCommand(queryId, conn);
                    cmdId.Parameters.AddWithValue("@u", usuario);
                    object result = cmdId.ExecuteScalar();
                    if (result != null)
                    {
                        int uid = Convert.ToInt32(result);
                        string query = "INSERT INTO apelaciones (id_usuario, texto_apelacion) VALUES (@uid, @txt)";
                        MySqlCommand cmd = new MySqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@uid", uid);
                        cmd.Parameters.AddWithValue("@txt", texto);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    return false;
                }
                catch { return false; }
            }
        }

        public bool ExisteApelacionPendiente(string usuario)
        {
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

        public bool EnviarMensaje(int idUsuario, string texto)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO chat (id_usuario, mensaje) VALUES (@u, @m)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    cmd.Parameters.AddWithValue("@m", texto);
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch { return false; }
            }
        }

        public List<Mensaje> ObtenerChat()
        {
            List<Mensaje> lista = new List<Mensaje>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT u.nombre, u.rol, c.mensaje, c.fecha FROM chat c JOIN usuarios u ON c.id_usuario = u.id ORDER BY c.fecha ASC LIMIT 50";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string rol = reader["rol"].ToString();
                            string color = (rol == "admin") ? "#651FFF" : "White";
                            lista.Add(new Mensaje { Usuario = reader.GetString("nombre"), Texto = reader.GetString("mensaje"), Fecha = reader.GetDateTime("fecha").ToString("HH:mm"), ColorNombre = color });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        public List<TopGamer> ObtenerTopGamers()
        {
            List<TopGamer> lista = new List<TopGamer>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT u.nombre, u.avatar, COUNT(b.id_juego) as total_juegos FROM usuarios u LEFT JOIN biblioteca b ON u.id = b.id_usuario GROUP BY u.id ORDER BY total_juegos DESC LIMIT 5";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        int rank = 1;
                        while (reader.Read())
                        {
                            byte[] ava = null;
                            if (reader["avatar"] != DBNull.Value) ava = (byte[])reader["avatar"];
                            lista.Add(new TopGamer { Rank = rank++, Nombre = reader.GetString("nombre"), Nivel = Convert.ToInt32(reader["total_juegos"]) * 10, Avatar = ava });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        // =============================================================
        // 5. CARRITO Y AMIGOS
        // =============================================================

        public List<Juego> ObtenerCarrito(int idUsuario)
        {
            List<Juego> lista = new List<Juego>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "SELECT j.* FROM juegos j INNER JOIN carrito c ON j.id = c.id_juego WHERE c.id_usuario = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] imgBytes = null;
                            if (reader["caratula"] != DBNull.Value) imgBytes = (byte[])reader["caratula"];
                            int val = 5;
                            if (reader["valoracion"] != DBNull.Value) val = reader.GetInt32("valoracion");
                            lista.Add(new Juego
                            {
                                Id = reader.GetInt32("id"),
                                Titulo = reader.GetString("titulo"),
                                Genero = reader.GetString("genero"),
                                Precio = reader.GetDecimal("precio"),
                                Descripcion = reader.GetString("descripcion"),
                                Visible = reader.GetBoolean("visible"),
                                Caratula = imgBytes,
                                Valoracion = val
                            });
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error al recuperar carrito: " + ex.Message); }
            }
            return lista;
        }

        public void AgregarAlCarrito(int idUsuario, int idJuego)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "INSERT INTO carrito (id_usuario, id_juego) VALUES (@u, @j)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    cmd.Parameters.AddWithValue("@j", idJuego);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public void EliminarDelCarrito(int idUsuario, int idJuego)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM carrito WHERE id_usuario = @u AND id_juego = @j LIMIT 1";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    cmd.Parameters.AddWithValue("@j", idJuego);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public void VaciarCarrito(int idUsuario)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM carrito WHERE id_usuario = @u";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u", idUsuario);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public void SetOnlineStatus(int userId, bool online)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE usuarios SET is_online = @s WHERE id = @id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@s", online);
                    cmd.Parameters.AddWithValue("@id", userId);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        public string EnviarSolicitudAmistad(int idEmisor, string nombreReceptor)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string queryId = "SELECT id FROM usuarios WHERE nombre = @n";
                    MySqlCommand cmdId = new MySqlCommand(queryId, conn);
                    cmdId.Parameters.AddWithValue("@n", nombreReceptor);
                    object res = cmdId.ExecuteScalar();
                    if (res == null) return "Usuario no encontrado.";
                    int idReceptor = Convert.ToInt32(res);
                    if (idReceptor == idEmisor) return "No puedes añadirte a ti mismo.";
                    string check = "SELECT COUNT(*) FROM amigos WHERE (id_usuario1=@u1 AND id_usuario2=@u2) OR (id_usuario1=@u2 AND id_usuario2=@u1)";
                    MySqlCommand cmdCheck = new MySqlCommand(check, conn);
                    cmdCheck.Parameters.AddWithValue("@u1", idEmisor);
                    cmdCheck.Parameters.AddWithValue("@u2", idReceptor);
                    if (Convert.ToInt32(cmdCheck.ExecuteScalar()) > 0) return "Ya existe una solicitud o amistad.";
                    string insert = "INSERT INTO amigos (id_usuario1, id_usuario2, estado) VALUES (@u1, @u2, 'pendiente')";
                    MySqlCommand cmdInsert = new MySqlCommand(insert, conn);
                    cmdInsert.Parameters.AddWithValue("@u1", idEmisor);
                    cmdInsert.Parameters.AddWithValue("@u2", idReceptor);
                    cmdInsert.ExecuteNonQuery();
                    return "Solicitud enviada a " + nombreReceptor;
                }
                catch (Exception ex) { return "Error: " + ex.Message; }
            }
        }

        public List<Amigo> ObtenerListaAmigos(int myId)
        {
            List<Amigo> lista = new List<Amigo>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT u.id, u.nombre, u.avatar, u.is_online FROM usuarios u JOIN amigos a ON (u.id = a.id_usuario1 OR u.id = a.id_usuario2) WHERE (a.id_usuario1 = @myId OR a.id_usuario2 = @myId) AND u.id != @myId AND a.estado = 'aceptado'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@myId", myId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] ava = null;
                            if (reader["avatar"] != DBNull.Value) ava = (byte[])reader["avatar"];
                            bool online = false;
                            if (reader["is_online"] != DBNull.Value) online = reader.GetBoolean("is_online");
                            lista.Add(new Amigo { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre"), IsOnline = online, Avatar = ava });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        public List<Amigo> ObtenerSolicitudesPendientes(int myId)
        {
            List<Amigo> lista = new List<Amigo>();
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT u.id, u.nombre, u.avatar, a.id as id_solicitud FROM usuarios u JOIN amigos a ON u.id = a.id_usuario1 WHERE a.id_usuario2 = @myId AND a.estado = 'pendiente'";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@myId", myId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] ava = null;
                            if (reader["avatar"] != DBNull.Value) ava = (byte[])reader["avatar"];
                            lista.Add(new Amigo { Id = reader.GetInt32("id"), IdSolicitud = reader.GetInt32("id_solicitud"), Nombre = reader.GetString("nombre"), Avatar = ava });
                        }
                    }
                }
                catch { }
            }
            return lista;
        }

        public void AceptarSolicitud(int idSolicitud)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE amigos SET estado='aceptado' WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idSolicitud);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        // Método para RECHAZAR solicitud (Borrarla)
        public void RechazarSolicitud(int idSolicitud)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM amigos WHERE id=@id";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", idSolicitud);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }

        // Método para ELIMINAR A UN AMIGO (Ya confirmado)
        public void EliminarAmigo(int miId, int idAmigo)
        {
            using (MySqlConnection conn = GetConnection())
            {
                try
                {
                    conn.Open();
                    // Borramos la relación en ambas direcciones por si acaso
                    string query = "DELETE FROM amigos WHERE (id_usuario1=@u1 AND id_usuario2=@u2) OR (id_usuario1=@u2 AND id_usuario2=@u1)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@u1", miId);
                    cmd.Parameters.AddWithValue("@u2", idAmigo);
                    cmd.ExecuteNonQuery();
                }
                catch { }
            }
        }
    }
}