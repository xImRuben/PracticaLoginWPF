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

        public AdminWindow()
        {
            InitializeComponent();
            CargarUsuarios();
            ActualizarStats();
        }

        private void CargarUsuarios()
        {
            List<Usuario> usuarios = db.ObtenerUsuarios(txtBuscar.Text);
            ListaUsuarios.ItemsSource = usuarios;
        }

        private void ActualizarStats()
        {
            int[] datos = db.ObtenerEstadisticas();
            lblTotal.Text = datos[0].ToString();
            lblActivos.Text = datos[1].ToString();
            lblBaneados.Text = datos[2].ToString();
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

                foreach (ComboBoxItem item in cmbRol.Items)
                    if (item.Content.ToString() == u.Rol) cmbRol.SelectedItem = item;

                foreach (ComboBoxItem item in cmbEstado.Items)
                    if (item.Content.ToString() == u.Estado) cmbEstado.SelectedItem = item;

                txtMotivo.Text = (u.Estado == "baneado") ? db.ObtenerMotivoBan(u.Nombre) : "";

                if (u.AvatarImage != null) imgAvatarPreview.Source = u.AvatarImage;
                else imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            }
        }

        private void BtnSubirFoto_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) { MessageBox.Show("Selecciona un usuario."); return; }
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Selecciona imagen";
            op.Filter = "Imágenes|*.jpg;*.jpeg;*.png";

            if (op.ShowDialog() == true)
            {
                try
                {
                    byte[] imageBytes = File.ReadAllBytes(op.FileName);
                    int id = int.Parse(txtId.Text);
                    if (db.ActualizarAvatar(id, imageBytes))
                    {
                        CargarUsuarios();
                        BitmapImage bi = new BitmapImage(); bi.BeginInit(); bi.StreamSource = new MemoryStream(imageBytes); bi.EndInit();
                        imgAvatarPreview.Source = bi;

                        // Log
                        db.RegistrarLog(Sesion.UsuarioActual.Nombre, "Subir Foto", txtNombre.Text);
                        MessageBox.Show("Foto actualizada.");
                    }
                }
                catch { MessageBox.Show("Error al subir imagen."); }
            }
        }

        private void BtnBorrarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            if (MessageBox.Show("¿Borrar foto?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (db.EliminarAvatar(int.Parse(txtId.Text)))
                {
                    CargarUsuarios();
                    imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));

                    // Log
                    db.RegistrarLog(Sesion.UsuarioActual.Nombre, "Borrar Foto", txtNombre.Text);
                    MessageBox.Show("Foto eliminada.");
                }
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e) { CargarUsuarios(); }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) { MessageBox.Show("Selecciona un usuario."); return; }
            Usuario u = new Usuario
            {
                Id = int.Parse(txtId.Text),
                Nombre = txtNombre.Text,
                Password = txtPass.Text,
                Email = txtEmail.Text,
                Rol = (cmbRol.SelectedItem as ComboBoxItem).Content.ToString(),
                Estado = (cmbEstado.SelectedItem as ComboBoxItem).Content.ToString()
            };
            if (db.EditarUsuario(u))
            {
                db.RegistrarLog(Sesion.UsuarioActual.Nombre, "Editar Usuario", u.Nombre);
                MessageBox.Show("Guardado.");
                CargarUsuarios();
                ActualizarStats();
            }
        }

        private void BtnLimpiar_Click(object sender, RoutedEventArgs e)
        {
            txtId.Text = ""; txtNombre.Text = ""; txtPass.Text = ""; txtEmail.Text = ""; txtMotivo.Text = "";
            cmbRol.SelectedIndex = -1; cmbEstado.SelectedIndex = -1; ListaUsuarios.SelectedIndex = -1;
            imgAvatarPreview.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
        }

        private void BtnBanear_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text) || string.IsNullOrEmpty(txtMotivo.Text)) { MessageBox.Show("Faltan datos."); return; }
            if (db.BanearUsuario(int.Parse(txtId.Text), true, txtMotivo.Text))
            {
                db.RegistrarLog(Sesion.UsuarioActual.Nombre, "BANEAR", txtNombre.Text);
                CargarUsuarios();
                ActualizarStats();
            }
        }

        private void BtnActivar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            if (db.BanearUsuario(int.Parse(txtId.Text), false))
            {
                db.RegistrarLog(Sesion.UsuarioActual.Nombre, "REACTIVAR", txtNombre.Text);
                CargarUsuarios();
                ActualizarStats();
            }
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtId.Text)) return;
            if (MessageBox.Show("¿Eliminar usuario?", "PELIGRO", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                if (db.EliminarUsuario(int.Parse(txtId.Text)))
                {
                    db.RegistrarLog(Sesion.UsuarioActual.Nombre, "ELIMINAR USUARIO", txtNombre.Text);
                    BtnLimpiar_Click(null, null);
                    CargarUsuarios();
                    ActualizarStats();
                }
        }

        // --- BOTONES NUEVOS DE GESTIÓN ---
        private void BtnHistorial_Click(object sender, RoutedEventArgs e)
        {
            LogsWindow logs = new LogsWindow();
            logs.ShowDialog();
        }

        private void BtnApelaciones_Click(object sender, RoutedEventArgs e)
        {
            GestionApelacionesWindow gest = new GestionApelacionesWindow();
            gest.ShowDialog();

            // Al volver, recargamos la lista por si se desbloqueó a alguien
            CargarUsuarios();
            ActualizarStats();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}