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
using System.Windows.Threading; // Para el Timer del Carrusel

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

        // --- VARIABLES PARA EL CARRUSEL ---
        private DispatcherTimer bannerTimer;
        private int indiceBanner = 0;
        private List<Juego> juegosDestacados;

        public HomeWindow()
        {
            InitializeComponent();
            this.KeyDown += HomeWindow_KeyDown;

            if (Sesion.UsuarioActual != null)
            {
                userRol = Sesion.UsuarioActual.Rol;
                userId = Sesion.UsuarioActual.Id;

                // ---------------------------------------------------------
                // ¡NUEVO! CARGAMOS EL CARRITO DE LA BBDD AL ENTRAR
                // ---------------------------------------------------------
                carrito = db.ObtenerCarrito(userId);
                ActualizarContadorCarrito();
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

                // Iniciamos el carrusel tras cargar los juegos
                IniciarCarrusel();
            }
            catch (Exception ex)
            {
                NexusMessageBox.Show("Error al conectar con el servidor: " + ex.Message);
            }
        }

        // ===============================================
        // LÓGICA DEL CARRUSEL (BANNER ROTATIVO)
        // ===============================================
        private void IniciarCarrusel()
        {
            if (todosLosJuegos == null || todosLosJuegos.Count == 0) return;

            // Cogemos 5 juegos aleatorios
            juegosDestacados = todosLosJuegos.OrderBy(x => Guid.NewGuid()).Take(5).ToList();

            if (bannerTimer == null)
            {
                bannerTimer = new DispatcherTimer();
                bannerTimer.Interval = TimeSpan.FromSeconds(5);
                bannerTimer.Tick += BannerTimer_Tick;
                bannerTimer.Start();
            }
            ActualizarBannerVisualmente();
        }

        private void BannerTimer_Tick(object sender, EventArgs e)
        {
            if (juegosDestacados == null || juegosDestacados.Count == 0) return;

            // Fade Out
            DoubleAnimation fadeOut = new DoubleAnimation { From = 1.0, To = 0.0, Duration = TimeSpan.FromSeconds(0.5) };

            fadeOut.Completed += (s, ev) =>
            {
                indiceBanner++;
                if (indiceBanner >= juegosDestacados.Count) indiceBanner = 0;

                ActualizarBannerVisualmente();

                // Fade In
                DoubleAnimation fadeIn = new DoubleAnimation { From = 0.0, To = 1.0, Duration = TimeSpan.FromSeconds(0.5) };
                if (BorderBanner != null) BorderBanner.BeginAnimation(OpacityProperty, fadeIn);
            };

            if (BorderBanner != null) BorderBanner.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ActualizarBannerVisualmente()
        {
            if (juegosDestacados.Count == 0) return;
            var juegoActual = juegosDestacados[indiceBanner];

            if (TxtTituloBanner != null) TxtTituloBanner.Text = juegoActual.Titulo.ToUpper();
            if (TxtDescBanner != null) TxtDescBanner.Text = juegoActual.Genero + " | " + juegoActual.Precio + " €";

            if (ImgFondoBanner != null)
            {
                if (juegoActual.CaratulaImagen != null) ImgFondoBanner.ImageSource = juegoActual.CaratulaImagen;
                else try { ImgFondoBanner.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico")); } catch { }
            }
        }

        // ===============================================

        private void CargarBiblioteca()
        {
            try { misJuegos = db.ObtenerBiblioteca(userId); } catch { }
        }

        // ===============================================
        // CARRITO Y PAGOS (CON PERSISTENCIA)
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

            // Borrar de RAM
            carrito.Remove(j);

            // ¡NUEVO! BORRAR DE BBDD
            db.EliminarDelCarrito(userId, j.Id);

            CargarCarrito();
            ActualizarContadorCarrito();
        }

        private void BtnPagarAhora_Click(object sender, RoutedEventArgs e)
        {
            if (carrito.Count == 0) return;

            decimal subtotal = carrito.Sum(j => j.Precio);
            decimal totalAPagar = subtotal + (subtotal * 0.21m);

            if (Sesion.UsuarioActual.Saldo < totalAPagar)
            {
                NexusMessageBox.Show($"¡SALDO INSUFICIENTE!\nCartera: {Sesion.UsuarioActual.Saldo:0.00} €\nNecesitas: {totalAPagar:0.00} €");
                return;
            }

            if (MessageBox.Show($"¿Pagar {totalAPagar:0.00}€ de tu saldo?", "Confirmar Compra", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                bool error = false;
                foreach (var j in carrito)
                {
                    if (!db.RegistrarCompra(userId, j.Id)) error = true;
                    else misJuegos.Add(j);
                }

                if (!error)
                {
                    Sesion.UsuarioActual.Saldo -= totalAPagar;
                    db.ActualizarSaldo(userId, Sesion.UsuarioActual.Saldo);

                    NexusMessageBox.Show($"¡Compra realizada!\nTe quedan: {Sesion.UsuarioActual.Saldo:0.00} €");

                    // ¡NUEVO! LIMPIAR RAM Y BBDD
                    carrito.Clear();
                    db.VaciarCarrito(userId);

                    ActualizarContadorCarrito();
                    CargarCarrito();
                    ConstruirVistaBiblioteca();
                    CambiarPantalla(GridMyGames);
                }
                else
                {
                    NexusMessageBox.Show("Hubo un error al procesar algunos juegos.");
                }
            }
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
                txtGenero.Text = $"{juegoSeleccionado.Genero}  |  {juegoSeleccionado.EstrellasDisplay}";
                txtPrecio.Text = juegoSeleccionado.PrecioFormato;
                txtDesc.Text = juegoSeleccionado.Descripcion;

                if (juegoSeleccionado.CaratulaImagen != null) BrushDetalle.ImageSource = juegoSeleccionado.CaratulaImagen;
                else BrushDetalle.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));

                bool yaLoTengo = misJuegos.Any(j => j.Id == juegoSeleccionado.Id);
                // Comprobamos por ID para evitar duplicados de objetos
                bool enCarrito = carrito.Any(j => j.Id == juegoSeleccionado.Id);

                if (yaLoTengo)
                {
                    ConfigurarBotonJugar();
                }
                else if (enCarrito)
                {
                    btnAccion.Content = "✓ EN EL CARRITO";
                    btnAccion.IsEnabled = true;
                    btnAccion.Background = new SolidColorBrush(Color.FromRgb(69, 39, 160));
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
            if (btnAccion.Content.ToString().Contains("✓")) return;

            if (btnAccion.Content.ToString().Contains("JUGAR"))
            {
                GameLauncherWindow launcher = new GameLauncherWindow(juegoSeleccionado);
                launcher.ShowDialog();
                return;
            }

            // AÑADIR AL CARRITO
            if (!carrito.Any(j => j.Id == juegoSeleccionado.Id))
            {
                // 1. Añadir a RAM
                carrito.Add(juegoSeleccionado);

                // 2. ¡NUEVO! GUARDAR EN BBDD
                db.AgregarAlCarrito(userId, juegoSeleccionado.Id);

                ActualizarContadorCarrito();
                NexusMessageBox.Show($"¡{juegoSeleccionado.Titulo} añadido!");

                btnAccion.Content = "✓ EN EL CARRITO";
                btnAccion.IsEnabled = true;
                btnAccion.Background = new SolidColorBrush(Color.FromRgb(69, 39, 160));
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e) => CambiarPantalla(GridLibrary);


        // ===============================================
        // PERFIL
        // ===============================================
        private void CargarPerfil()
        {
            lblPerfilUser.Text = Sesion.UsuarioActual.Nombre;
            lblPerfilRol.Text = $"{Sesion.UsuarioActual.Rol.ToUpper()}  |  SALDO: {Sesion.UsuarioActual.Saldo:0.00} €";
            lblPerfilFecha.Text = "Miembro desde: " + Sesion.UsuarioActual.FechaRegistro;
            txtEmailPerfil.Text = Sesion.UsuarioActual.Email;
            txtPassActual.Text = "";
            txtPassNueva.Text = "";

            if (Sesion.UsuarioActual.AvatarImage != null)
                imgPerfilUser.ImageSource = Sesion.UsuarioActual.AvatarImage;
            else
                imgPerfilUser.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/logo.ico"));
        }

        private void BtnRecargar_Click_Simulado(object sender, RoutedEventArgs e)
        {
            Sesion.UsuarioActual.Saldo += 100;
            db.ActualizarSaldo(userId, Sesion.UsuarioActual.Saldo);
            CargarPerfil();
            NexusMessageBox.Show("¡Has recargado 100€ a tu cuenta!");
        }

        private void BtnSubirFotoPerfil_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.jpeg;*.png" };
            if (op.ShowDialog() == true)
            {
                try
                {
                    byte[] imgBytes = File.ReadAllBytes(op.FileName);
                    if (db.ActualizarAvatar(userId, imgBytes))
                    {
                        Sesion.UsuarioActual.Avatar = imgBytes;
                        imgPerfilUser.ImageSource = new BitmapImage(new Uri(op.FileName));
                        NexusMessageBox.Show("Avatar actualizado.");
                    }
                }
                catch { NexusMessageBox.Show("Error al subir la imagen."); }
            }
        }

        private void BtnGuardarPerfil_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassActual.Text))
            {
                if (txtPassActual.Text != Sesion.UsuarioActual.Password)
                {
                    NexusMessageBox.Show("Contraseña actual incorrecta.");
                    return;
                }
            }
            else if (!string.IsNullOrEmpty(txtPassNueva.Text))
            {
                NexusMessageBox.Show("Confirma tu contraseña actual para cambiarla.");
                return;
            }

            string passwordFinal = string.IsNullOrEmpty(txtPassNueva.Text) ? Sesion.UsuarioActual.Password : txtPassNueva.Text;
            Usuario u = new Usuario { Id = userId, Nombre = Sesion.UsuarioActual.Nombre, Email = txtEmailPerfil.Text, Password = passwordFinal };

            if (db.EditarPerfilUsuario(u))
            {
                Sesion.UsuarioActual.Email = u.Email;
                Sesion.UsuarioActual.Password = u.Password;
                NexusMessageBox.Show("Perfil guardado.");
                CargarPerfil();
                txtPassActual.Text = ""; txtPassNueva.Text = "";
            }
            else
            {
                NexusMessageBox.Show("Error al guardar.");
            }
        }

        private void BtnCerrarPerfil_Click(object sender, RoutedEventArgs e) => CambiarPantalla(GridLibrary);

        // ===============================================
        // COMUNIDAD Y NAVEGACIÓN
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

        private void BtnAyuda_Click(object sender, RoutedEventArgs e)
        {
            HelpWindow ayuda = new HelpWindow();
            ayuda.ShowDialog();
        }

        private void HomeWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
            {
                HelpWindow ayuda = new HelpWindow();
                ayuda.ShowDialog();
            }
        }
    }
}