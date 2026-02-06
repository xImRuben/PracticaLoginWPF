using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySql.Data.MySqlClient;

namespace PracticaLoginWPF
{
    public partial class GestionApelacionesWindow : Window
    {
        ConexionDB db = new ConexionDB();

        public GestionApelacionesWindow()
        {
            InitializeComponent();
            CargarApelaciones();
        }

        private void CargarApelaciones()
        {
            // JOIN para sacar el nombre del usuario junto con la apelación
            string query = "SELECT a.id, u.nombre as usuario, a.texto_apelacion, a.fecha, a.id_usuario " +
                           "FROM apelaciones a JOIN usuarios u ON a.id_usuario = u.id " +
                           "WHERE a.estado = 'pendiente'";

            DataTable dt = new DataTable();
            using (MySqlConnection conn = db.GetConnection())
            {
                try
                {
                    conn.Open();
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    da.Fill(dt);
                    GridApelaciones.ItemsSource = dt.DefaultView;

                    // Mostrar mensaje si está vacío
                    if (dt.Rows.Count == 0)
                    {
                        txtSinDatos.Visibility = Visibility.Visible;
                        GridApelaciones.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        txtSinDatos.Visibility = Visibility.Collapsed;
                        GridApelaciones.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar apelaciones: " + ex.Message);
                }
            }
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Seguro que quieres desbanear a este usuario?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int idUsuario = Convert.ToInt32((sender as Button).Tag);

                // 1. Quitar el ban
                db.BanearUsuario(idUsuario, false);

                // 2. Marcar apelación como aceptada
                db.RegistrarLog(Sesion.UsuarioActual.Nombre, "DESBANEAR (Apelación Aceptada)", idUsuario.ToString());
                ActualizarEstadoApelacion(idUsuario, "aceptada");

                MessageBox.Show("Usuario desbaneado y reintegrado a Nexus.");
                CargarApelaciones();
            }
        }

        private void BtnRechazar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Rechazar apelación? El usuario seguirá baneado.", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                int idApelacion = Convert.ToInt32((sender as Button).Tag);
                ActualizarEstadoApelacionPorId(idApelacion, "rechazada");

                db.RegistrarLog(Sesion.UsuarioActual.Nombre, "APELACIÓN RECHAZADA", "ID Apelación: " + idApelacion);

                MessageBox.Show("Apelación rechazada y archivada.");
                CargarApelaciones();
            }
        }

        private void ActualizarEstadoApelacion(int idUsuario, string estado)
        {
            using (MySqlConnection conn = db.GetConnection())
            {
                try { conn.Open(); new MySqlCommand($"UPDATE apelaciones SET estado='{estado}' WHERE id_usuario={idUsuario}", conn).ExecuteNonQuery(); } catch { }
            }
        }

        private void ActualizarEstadoApelacionPorId(int idApelacion, string estado)
        {
            using (MySqlConnection conn = db.GetConnection())
            {
                try { conn.Open(); new MySqlCommand($"UPDATE apelaciones SET estado='{estado}' WHERE id={idApelacion}", conn).ExecuteNonQuery(); } catch { }
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}