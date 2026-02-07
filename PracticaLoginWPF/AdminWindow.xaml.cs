using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;

namespace PracticaLoginWPF
{
    public partial class AdminWindow : Window
    {
        ConexionDB db = new ConexionDB();
        byte[] caratulaActualBytes = null;
        byte[] avatarUserBytes = null;

        public AdminWindow()
        {
            InitializeComponent();
            CargarUsuarios();
            CargarJuegos();
            ActualizarStats();
        }

        private void ActualizarStats()
        {
            try
            {
                int[] datos = db.ObtenerEstadisticas();
                lblTotal.Text = datos[0].ToString();
                lblActivos.Text = datos[1].ToString();
                lblBaneados.Text = datos[2].ToString();
            }
            catch { }
        }

        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null) return;
            string tag = rb.Tag.ToString();

            if (tag == "Users")
            {
                ViewUsers.Visibility = Visibility.Visible;
                ViewGames.Visibility = Visibility.Collapsed;
            }
            else if (tag == "Games")
            {
                ViewUsers.Visibility = Visibility.Collapsed;
                ViewGames.Visibility = Visibility.Visible;
            }
        }

        // =======================================================
        // GESTIÓN DE USUARIOS
        // =======================================================
        private void CargarUsuarios()
        {
            try { ListaUsuarios.ItemsSource = db.ObtenerUsuarios(txtBuscar.Text); } catch { }
        }

        private void ListaUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var u = ListaUsuarios.SelectedItem as Usuario;
            if (u != null)
            {
                txtId.Text = u.Id.ToString();
                txtNombre.Text = u.Nombre;
                txtPass.Text = u.Password;
                txtEmail.Text = u.Email;
                foreach (ComboBoxItem item in cmbRol.Items) if (item.Content.ToString() == u.Rol) cmbRol.SelectedItem = item;
                foreach (ComboBoxItem item in cmbEstado.Items) if (item.Content.ToString() == u.Estado) cmbEstado.SelectedItem = item;
                txtMotivo.Text = (u.Estado == "baneado") ? db.ObtenerMotivoBan(u.Nombre) : "";

                imgAvatarPreview.Source = u.AvatarImage ?? new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
                avatarUserBytes = u.Avatar;
            }
        }

        private void BtnSubirFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.png" };
            if (op.ShowDialog() == true)
            {
                try
                {
                    avatarUserBytes = File.ReadAllBytes(op.FileName);
                    BitmapImage bi = new BitmapImage(); bi.BeginInit(); bi.StreamSource = new MemoryStream(avatarUserBytes); bi.EndInit();
                    imgAvatarPreview.Source = bi;
                }
                catch { MessageBox.Show("Error al cargar imagen."); }
            }
        }

        private void BtnBorrarFoto_Click(object sender, RoutedEventArgs e)
        {
            avatarUserBytes = null;
            imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            Usuario u = new Usuario
            {
                Id = int.Parse(txtId.Text),
                Nombre = txtNombre.Text,
                Password = txtPass.Text,
                Email = txtEmail.Text,
                Rol = (cmbRol.SelectedItem as ComboBoxItem).Content.ToString(),
                Estado = (cmbEstado.SelectedItem as ComboBoxItem).Content.ToString()
            };

            db.EditarUsuario(u);
            db.ActualizarAvatar(u.Id, avatarUserBytes);
            CargarUsuarios();
            ActualizarStats();
            MessageBox.Show("Usuario guardado.");
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtId.Text = ""; txtNombre.Text = ""; txtPass.Text = ""; txtEmail.Text = ""; txtMotivo.Text = "";
            ListaUsuarios.SelectedIndex = -1;
            imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            avatarUserBytes = null;
        }

        private void BtnBanear_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtId.Text))
            {
                if (db.BanearUsuario(int.Parse(txtId.Text), true, txtMotivo.Text))
                {
                    MessageBox.Show($"Usuario baneado correctamente.\nMotivo: {txtMotivo.Text}");
                    CargarUsuarios();
                    ActualizarStats();
                }
            }
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtId.Text))
            {
                if (db.BanearUsuario(int.Parse(txtId.Text), false))
                {
                    MessageBox.Show("Usuario desbaneado y activado nuevamente.");
                    CargarUsuarios();
                    ActualizarStats();
                }
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrEmpty(txtId.Text)) { db.EliminarUsuario(int.Parse(txtId.Text)); CargarUsuarios(); ActualizarStats(); BtnLimpiar_Click(null, null); } }
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) { CargarUsuarios(); }


        // =======================================================
        // GESTIÓN DE JUEGOS
        // =======================================================
        private void CargarJuegos()
        {
            try { ListaJuegosAdmin.ItemsSource = db.ObtenerJuegos(true); } catch { }
        }

        private void ListaJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var j = ListaJuegosAdmin.SelectedItem as Juego;
            if (j != null)
            {
                txtIdJuego.Text = j.Id.ToString();
                txtTituloJuego.Text = j.Titulo;
                foreach (ComboBoxItem item in cmbGeneroJuego.Items)
                    if (item.Content.ToString() == j.Genero) cmbGeneroJuego.SelectedItem = item;
                txtPrecioJuego.Text = j.Precio.ToString();
                txtDescJuego.Text = j.Descripcion;
                chkVisible.IsChecked = j.Visible;

                if (j.CaratulaImagen != null) imgCaratula.Source = j.CaratulaImagen;
                else imgCaratula.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));

                caratulaActualBytes = j.Caratula;
            }
        }

        private void BtnSubirCaratula_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.png;*.jpeg" };
            if (op.ShowDialog() == true)
            {
                try
                {
                    caratulaActualBytes = File.ReadAllBytes(op.FileName);
                    BitmapImage bi = new BitmapImage(); bi.BeginInit(); bi.StreamSource = new MemoryStream(caratulaActualBytes); bi.EndInit();
                    imgCaratula.Source = bi;
                }
                catch { MessageBox.Show("Error al cargar imagen."); }
            }
        }

        private void BtnGuardarJuego_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtTituloJuego.Text)) { MessageBox.Show("Título obligatorio."); return; }

                string precioTexto = txtPrecioJuego.Text.Replace(",", ".");
                decimal precioFinal = 0;
                decimal.TryParse(precioTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out precioFinal);

                string genero = (cmbGeneroJuego.SelectedItem as ComboBoxItem)?.Content.ToString();

                Juego j = new Juego
                {
                    Titulo = txtTituloJuego.Text,
                    Genero = genero,
                    Precio = precioFinal,
                    Descripcion = txtDescJuego.Text,
                    Visible = chkVisible.IsChecked == true,
                    Caratula = caratulaActualBytes
                };

                if (string.IsNullOrEmpty(txtIdJuego.Text))
                {
                    if (db.AgregarJuego(j))
                    {
                        db.RegistrarLog(Sesion.UsuarioActual.Nombre, "CREAR JUEGO", j.Titulo);
                        MessageBox.Show("Juego creado."); CargarJuegos(); BtnLimpiarJuego_Click(null, null);
                    }
                }
                else
                {
                    j.Id = int.Parse(txtIdJuego.Text);
                    if (db.ModificarJuego(j))
                    {
                        db.RegistrarLog(Sesion.UsuarioActual.Nombre, "EDITAR JUEGO", j.Titulo);
                        MessageBox.Show("Juego actualizado."); CargarJuegos();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error inesperado: " + ex.Message); }
        }

        private void BtnEliminarJuego_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtIdJuego.Text) && MessageBox.Show("¿Eliminar?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.EliminarJuego(int.Parse(txtIdJuego.Text));
                CargarJuegos(); BtnLimpiarJuego_Click(null, null);
            }
        }

        private void BtnLimpiarJuego_Click(object sender, RoutedEventArgs e)
        {
            txtIdJuego.Text = ""; txtTituloJuego.Text = ""; cmbGeneroJuego.SelectedIndex = -1;
            txtPrecioJuego.Text = ""; txtDescJuego.Text = "";
            imgCaratula.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            caratulaActualBytes = null;
            ListaJuegosAdmin.SelectedIndex = -1;
        }

        private void BtnHistorial_Click(object sender, RoutedEventArgs e) { LogsWindow logs = new LogsWindow(); logs.ShowDialog(); }
        private void BtnApelaciones_Click(object sender, RoutedEventArgs e) { GestionApelacionesWindow gest = new GestionApelacionesWindow(); gest.ShowDialog(); CargarUsuarios(); ActualizarStats(); }
        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}