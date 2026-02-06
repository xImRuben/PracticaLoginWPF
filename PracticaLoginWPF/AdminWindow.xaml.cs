using System;
using System.Collections.Generic;
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

        public AdminWindow()
        {
            InitializeComponent();
            CargarUsuarios();
            CargarJuegos();
            ActualizarStats();
        }

        private void ActualizarStats()
        {
            int[] datos = db.ObtenerEstadisticas();
            lblTotal.Text = datos[0].ToString();
            lblActivos.Text = datos[1].ToString();
            lblBaneados.Text = datos[2].ToString();
        }

        // =======================================================
        // NAVEGACIÓN
        // =======================================================
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
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
            ListaUsuarios.ItemsSource = db.ObtenerUsuarios(txtBuscar.Text);
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
            }
        }

        private void BtnSubirFoto_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            OpenFileDialog op = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.png" };
            if (op.ShowDialog() == true)
            {
                byte[] img = File.ReadAllBytes(op.FileName);
                if (db.ActualizarAvatar(int.Parse(txtId.Text), img))
                {
                    CargarUsuarios();
                    imgAvatarPreview.Source = new BitmapImage(new Uri(op.FileName));
                }
            }
        }

        private void BtnBorrarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtId.Text) && db.EliminarAvatar(int.Parse(txtId.Text)))
            {
                CargarUsuarios();
                imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            }
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
            if (db.EditarUsuario(u)) { CargarUsuarios(); ActualizarStats(); MessageBox.Show("Guardado."); }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtId.Text = ""; txtNombre.Text = ""; txtPass.Text = ""; txtEmail.Text = ""; txtMotivo.Text = "";
            ListaUsuarios.SelectedIndex = -1;
        }

        private void BtnBanear_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrEmpty(txtId.Text)) { db.BanearUsuario(int.Parse(txtId.Text), true, txtMotivo.Text); CargarUsuarios(); ActualizarStats(); } }
        private void BtnActivar_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrEmpty(txtId.Text)) { db.BanearUsuario(int.Parse(txtId.Text), false); CargarUsuarios(); ActualizarStats(); } }
        private void BtnEliminar_Click(object sender, RoutedEventArgs e) { if (!string.IsNullOrEmpty(txtId.Text)) { db.EliminarUsuario(int.Parse(txtId.Text)); CargarUsuarios(); ActualizarStats(); BtnLimpiar_Click(null, null); } }
        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) { CargarUsuarios(); }

        // =======================================================
        // GESTIÓN DE JUEGOS
        // =======================================================
        private void CargarJuegos()
        {
            ListaJuegosAdmin.ItemsSource = db.ObtenerJuegos(true);
        }

        private void ListaJuegos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var j = ListaJuegosAdmin.SelectedItem as Juego;
            if (j != null)
            {
                txtIdJuego.Text = j.Id.ToString();
                txtTituloJuego.Text = j.Titulo;

                // SELECCIÓN DEL GÉNERO EN EL COMBOBOX
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
                caratulaActualBytes = File.ReadAllBytes(op.FileName);
                BitmapImage bi = new BitmapImage(); bi.BeginInit(); bi.StreamSource = new MemoryStream(caratulaActualBytes); bi.EndInit();
                imgCaratula.Source = bi;
            }
        }

        private void BtnGuardarJuego_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // GUARDADO DEL GÉNERO DESDE COMBOBOX
                string generoSeleccionado = (cmbGeneroJuego.SelectedItem as ComboBoxItem)?.Content.ToString();

                Juego j = new Juego
                {
                    Titulo = txtTituloJuego.Text,
                    Genero = generoSeleccionado,
                    Precio = decimal.Parse(txtPrecioJuego.Text),
                    Descripcion = txtDescJuego.Text,
                    Visible = chkVisible.IsChecked == true,
                    Caratula = caratulaActualBytes
                };

                if (string.IsNullOrEmpty(txtIdJuego.Text))
                {
                    if (db.AgregarJuego(j))
                    {
                        db.RegistrarLog(Sesion.UsuarioActual.Nombre, "CREAR JUEGO", j.Titulo);
                        MessageBox.Show("Juego creado.");
                        CargarJuegos();
                        BtnLimpiarJuego_Click(null, null);
                    }
                }
                else
                {
                    j.Id = int.Parse(txtIdJuego.Text);
                    if (db.ModificarJuego(j))
                    {
                        db.RegistrarLog(Sesion.UsuarioActual.Nombre, "EDITAR JUEGO", j.Titulo);
                        MessageBox.Show("Juego actualizado.");
                        CargarJuegos();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error en los datos: " + ex.Message); }
        }

        private void BtnEliminarJuego_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtIdJuego.Text))
            {
                if (MessageBox.Show("¿Eliminar juego?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.EliminarJuego(int.Parse(txtIdJuego.Text));
                    db.RegistrarLog(Sesion.UsuarioActual.Nombre, "ELIMINAR JUEGO", txtTituloJuego.Text);
                    CargarJuegos();
                    BtnLimpiarJuego_Click(null, null);
                }
            }
        }

        private void BtnLimpiarJuego_Click(object sender, RoutedEventArgs e)
        {
            txtIdJuego.Text = ""; txtTituloJuego.Text = "";
            cmbGeneroJuego.SelectedIndex = -1; // Resetear combo
            txtPrecioJuego.Text = ""; txtDescJuego.Text = "";
            imgCaratula.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            caratulaActualBytes = null;
            ListaJuegosAdmin.SelectedIndex = -1;
        }

        private void BtnHistorial_Click(object sender, RoutedEventArgs e)
        {
            LogsWindow logs = new LogsWindow();
            logs.ShowDialog();
        }

        private void BtnApelaciones_Click(object sender, RoutedEventArgs e)
        {
            GestionApelacionesWindow gest = new GestionApelacionesWindow();
            gest.ShowDialog();
            CargarUsuarios();
            ActualizarStats();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}