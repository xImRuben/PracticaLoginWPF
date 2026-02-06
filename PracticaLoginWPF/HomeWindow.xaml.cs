using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;

namespace PracticaLoginWPF
{
    public partial class HomeWindow : Window
    {
        ConexionDB db = new ConexionDB();
        string userRol;
        int userId;
        Juego juegoSeleccionado;

        List<Juego> todosLosJuegos = new List<Juego>();
        List<Juego> misJuegos = new List<Juego>();
        List<Juego> carrito = new List<Juego>();

        public HomeWindow()
        {
            InitializeComponent();
            this.KeyDown += HomeWindow_KeyDown;

            if (Sesion.UsuarioActual != null)
            {
                userRol = Sesion.UsuarioActual.Rol;
                userId = Sesion.UsuarioActual.Id;
            }

            ConfigurarPermisos();
            CargarCatalogo();
            CargarBiblioteca();
        }

        private void ConfigurarPermisos()
        {
            if (userRol != "admin")
            {
                if (BtnAdmin != null) BtnAdmin.Visibility = Visibility.Collapsed;
            }
        }

        private void CargarCatalogo()
        {
            try
            {
                todosLosJuegos = db.ObtenerJuegos(false);
                ListaJuegos.ItemsSource = todosLosJuegos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al conectar con el servidor: " + ex.Message);
            }
        }

        private void CargarBiblioteca()
        {
            try { misJuegos = db.ObtenerBiblioteca(userId); } catch { }
        }

        // ===============================================
        // LÓGICA DEL PERFIL (NUEVO DISEÑO)
        // ===============================================
        private void CargarPerfil()
        {
            // Llenar los datos de la izquierda (Barra lateral)
            lblPerfilUser.Text = Sesion.UsuarioActual.Nombre;
            lblPerfilRol.Text = Sesion.UsuarioActual.Rol.ToUpper(); // EJ: ADMIN
            lblPerfilFecha.Text = "Miembro desde: " + Sesion.UsuarioActual.FechaRegistro;

            // Llenar formulario derecha
            txtEmailPerfil.Text = Sesion.UsuarioActual.Email;

            // Limpiar campos de contraseña por seguridad
            txtPassActual.Text = "";
            txtPassNueva.Text = "";

            // Cargar avatar
            if (Sesion.UsuarioActual.AvatarImage != null)
            {
                imgPerfilUser.ImageSource = Sesion.UsuarioActual.AvatarImage;
            }
            else
            {
                imgPerfilUser.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
            }
        }

        private void BtnSubirFotoPerfil_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Seleccionar avatar";
            op.Filter = "Imágenes|*.jpg;*.jpeg;*.png";
            if (op.ShowDialog() == true)
            {
                try
                {
                    byte[] imgBytes = File.ReadAllBytes(op.FileName);
                    if (db.ActualizarAvatar(userId, imgBytes))
                    {
                        Sesion.UsuarioActual.Avatar = imgBytes;
                        imgPerfilUser.ImageSource = new BitmapImage(new Uri(op.FileName));
                        MessageBox.Show("Avatar actualizado correctamente.");
                    }
                }
                catch { MessageBox.Show("Error al subir la imagen."); }
            }
        }

        private void BtnGuardarPerfil_Click(object sender, RoutedEventArgs e)
        {
            // 1. Validar contraseña actual si quiere cambiar algo sensible
            if (!string.IsNullOrEmpty(txtPassActual.Text))
            {
                if (txtPassActual.Text != Sesion.UsuarioActual.Password)
                {
                    MessageBox.Show("La contraseña actual no es correcta.", "Error de seguridad");
                    return;
                }
            }
            else
            {
                // Si no escribe la actual, le avisamos (opcional, pero recomendado)
                if (!string.IsNullOrEmpty(txtPassNueva.Text))
                {
                    MessageBox.Show("Para cambiar la contraseña, debes confirmar la actual primero.");
                    return;
                }
            }

            // 2. Determinar qué contraseña guardar (Nueva o la que ya tenía)
            string passwordFinal = string.IsNullOrEmpty(txtPassNueva.Text) ? Sesion.UsuarioActual.Password : txtPassNueva.Text;

            Usuario u = new Usuario
            {
                Id = userId,
                // El nombre lo dejamos igual para no romper logins, o podríamos permitir cambiarlo
                Nombre = Sesion.UsuarioActual.Nombre,
                Email = txtEmailPerfil.Text,
                Password = passwordFinal
            };

            if (db.EditarPerfilUsuario(u))
            {
                Sesion.UsuarioActual.Email = u.Email;
                Sesion.UsuarioActual.Password = u.Password;
                MessageBox.Show("Perfil actualizado con éxito.");
                txtPassActual.Text = "";
                txtPassNueva.Text = "";
            }
            else
            {
                MessageBox.Show("Error al guardar cambios.");
            }
        }

        private void BtnCerrarPerfil_Click(object sender, RoutedEventArgs e)
        {
            // Al cerrar, volvemos a la biblioteca o catálogo
            CambiarPantalla(GridLibrary);
        }

        // ===============================================
        // LÓGICA DEL CARRITO
        // ===============================================
        private void ActualizarContadorCarrito()
        {
            txtCountCarrito.Text = $"({carrito.Count})";
        }

        private void CargarCarrito()
        {
            ListaCarrito.ItemsSource = null;
            ListaCarrito.ItemsSource = carrito;

            decimal subtotal = carrito.Sum(j => j.Precio);
            decimal impuestos = subtotal * 0.21m;
            decimal total = subtotal + impuestos;

            txtSubtotal.Text = $"{subtotal:0.00}€";
            txtImpuestos.Text = $"{impuestos:0.00}€";
            txtTotal.Text = $"{total:0.00}€";
        }

        private void BtnEliminarCarrito_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            Juego j = btn.Tag as Juego;
            carrito.Remove(j);
            CargarCarrito();
            ActualizarContadorCarrito();
        }

        private void BtnPagarAhora_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0) return;

            if (MessageBox.Show($"¿Pagar un total de {txtTotal.Text}?", "Confirmar Pago", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                bool error = false;
                foreach (var j in carrito)
                {
                    if (!db.RegistrarCompra(userId, j.Id)) error = true;
                    else misJuegos.Add(j);
                }

                if (!error)
                {
                    MessageBox.Show("¡Compra realizada con éxito!", "Nexus Store");
                    carrito.Clear();
                    ActualizarContadorCarrito();
                    CargarCarrito();
                    ConstruirVistaBiblioteca();
                    CambiarPantalla(GridMyGames);
                }
                else
                {
                    MessageBox.Show("Hubo algún problema con uno o más juegos.");
                }
            }
        }

        // ===============================================
        // COMUNIDAD
        // ===============================================
        private void CargarComunidad()
        {
            var mensajes = db.ObtenerChat();
            ListaChat.ItemsSource = mensajes;
            if (ListaChat.Items.Count > 0) ListaChat.ScrollIntoView(ListaChat.Items[ListaChat.Items.Count - 1]);
            var top = db.ObtenerTopGamers();
            ListaTopGamers.ItemsSource = top;
        }

        private void BtnEnviarChat_Click(object sender, RoutedEventArgs e) => EnviarMensajeChat();
        private void TxtMensajeChat_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) EnviarMensajeChat(); }
        private void EnviarMensajeChat()
        {
            string texto = txtMensajeChat.Text.Trim();
            if (string.IsNullOrEmpty(texto)) return;
            if (db.EnviarMensaje(userId, texto)) { txtMensajeChat.Text = ""; CargarComunidad(); }
        }

        // ===============================================
        // NAVEGACIÓN
        // ===============================================
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as RadioButton;
            if (btn == null) return;

            string tag = btn.Tag.ToString();

            if (tag == "Lib") CambiarPantalla(GridLibrary);
            if (tag == "MyGames") { ConstruirVistaBiblioteca(); CambiarPantalla(GridMyGames); }
            if (tag == "Comm") { CargarComunidad(); CambiarPantalla(GridCommunity); }
            if (tag == "Cart") { CargarCarrito(); CambiarPantalla(GridCart); }
            if (tag == "Profile") { CargarPerfil(); CambiarPantalla(GridProfile); }
        }

        private void CambiarPantalla(UIElement nuevaPantalla)
        {
            GridLibrary.Visibility = Visibility.Collapsed;
            GridMyGames.Visibility = Visibility.Collapsed;
            GridCommunity.Visibility = Visibility.Collapsed;
            GridDetails.Visibility = Visibility.Collapsed;
            GridCart.Visibility = Visibility.Collapsed;
            GridProfile.Visibility = Visibility.Collapsed;

            nuevaPantalla.Opacity = 0;
            nuevaPantalla.Visibility = Visibility.Visible;

            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
            nuevaPantalla.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        // ===============================================
        // DETALLES Y AÑADIR AL CARRITO
        // ===============================================
        private void Juego_Click(object sender, MouseButtonEventArgs e)
        {
            var gridJuego = sender as Grid;
            juegoSeleccionado = gridJuego.DataContext as Juego;

            if (juegoSeleccionado != null)
            {
                txtTitulo.Text = juegoSeleccionado.Titulo;
                txtGenero.Text = juegoSeleccionado.Genero;
                txtPrecio.Text = juegoSeleccionado.PrecioFormato;
                txtDesc.Text = juegoSeleccionado.Descripcion;

                if (juegoSeleccionado.CaratulaImagen != null) BrushDetalle.ImageSource = juegoSeleccionado.CaratulaImagen;
                else BrushDetalle.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));

                bool yaLoTengo = misJuegos.Any(j => j.Id == juegoSeleccionado.Id);
                bool enCarrito = carrito.Any(j => j.Id == juegoSeleccionado.Id);

                if (yaLoTengo)
                {
                    ConfigurarBotonJugar();
                }
                else if (enCarrito)
                {
                    btnAccion.Content = "EN EL CARRITO";
                    btnAccion.IsEnabled = false;
                    btnAccion.Background = Brushes.Gray;
                }
                else
                {
                    btnAccion.Content = "AÑADIR AL CARRITO";
                    btnAccion.IsEnabled = true;
                    btnAccion.Background = (SolidColorBrush)Application.Current.Resources["AccentColor"];
                }

                CambiarPantalla(GridDetails);
            }
        }

        private void ConfigurarBotonJugar()
        {
            btnAccion.Content = "▶ JUGAR";
            btnAccion.IsEnabled = true;
            btnAccion.Background = new SolidColorBrush(Color.FromRgb(0, 200, 83));
        }

        private void BtnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (btnAccion.Content.ToString().Contains("JUGAR"))
            {
                MessageBox.Show($"Iniciando {juegoSeleccionado.Titulo}...", "NEXUS");
                return;
            }

            if (!carrito.Contains(juegoSeleccionado))
            {
                carrito.Add(juegoSeleccionado);
                ActualizarContadorCarrito();
                MessageBox.Show("Añadido al carrito");
                btnAccion.Content = "EN EL CARRITO";
                btnAccion.IsEnabled = false;
                btnAccion.Background = Brushes.Gray;
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => CambiarPantalla(GridLibrary);

        private void CmbFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (todosLosJuegos == null || cmbFiltro.SelectedItem == null || ListaJuegos == null) return;
            string filtro = (cmbFiltro.SelectedItem as ComboBoxItem).Content.ToString();
            if (filtro == "Todos") ListaJuegos.ItemsSource = todosLosJuegos;
            else ListaJuegos.ItemsSource = todosLosJuegos.Where(j => j.Genero != null && j.Genero.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        }

        private void ListaJuegos_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent.RaiseEvent(eventArg);
            }
        }

        private void ConstruirVistaBiblioteca()
        {
            GridMyGames.Children.Clear();
            if (misJuegos.Count == 0)
            {
                TextBlock vacio = new TextBlock { Text = "AÚN NO TIENES JUEGOS.", Foreground = Brushes.Gray, FontSize = 20, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                GridMyGames.Children.Add(vacio);
            }
            else
            {
                ListBox listaBiblio = new ListBox
                {
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    ItemsSource = misJuegos,
                    ItemContainerStyle = ListaJuegos.ItemContainerStyle,
                    ItemTemplate = ListaJuegos.ItemTemplate
                };
                ScrollViewer.SetHorizontalScrollBarVisibility(listaBiblio, ScrollBarVisibility.Disabled);
                listaBiblio.PreviewMouseWheel += ListaJuegos_PreviewMouseWheel;

                ScrollViewer scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Hidden, Margin = new Thickness(30) };
                StackPanel panel = new StackPanel();
                panel.Children.Add(new TextBlock { Text = "MI COLECCIÓN", Foreground = Brushes.White, FontSize = 22, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 20) });

                FrameworkElementFactory factory = new FrameworkElementFactory(typeof(WrapPanel));
                factory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
                listaBiblio.ItemsPanel = new ItemsPanelTemplate { VisualTree = factory };

                panel.Children.Add(listaBiblio);
                scroll.Content = panel;
                GridMyGames.Children.Add(scroll);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.ChangedButton == MouseButton.Left) { if (e.ClickCount == 2) BtnMaximizar_Click(sender, e); else if (this.WindowState == WindowState.Normal) DragMove(); } }
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e) { if (this.WindowState == WindowState.Normal) { this.MaxHeight = SystemParameters.WorkArea.Height + 14; this.MaxWidth = SystemParameters.WorkArea.Width + 14; this.WindowState = WindowState.Maximized; } else { this.MaxHeight = double.PositiveInfinity; this.MaxWidth = double.PositiveInfinity; this.WindowState = WindowState.Normal; } }
        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e) { Sesion.Cerrar(); new MainWindow().Show(); this.Close(); }
        private void BtnAdmin_Click(object sender, RoutedEventArgs e) { this.Hide(); new AdminWindow().ShowDialog(); this.Show(); CargarCatalogo(); }
        private void HomeWindow_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.F1) new HelpWindow().ShowDialog(); }
    }
}