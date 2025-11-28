using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation; // Animaciones
using System.Threading.Tasks;         // Async/Await
using System;

namespace PracticaLoginWPF
{
    public partial class HomeWindow : Window
    {
        public HomeWindow()
        {
            InitializeComponent();
        }

        // ===============================================
        // 1. NAVEGACIÓN MENU SUPERIOR
        // ===============================================
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            string tag = btn.Tag.ToString();

            if (tag == "Lib") CambiarPantalla(GridLibrary);
            if (tag == "Store") CambiarPantalla(GridStore);
            if (tag == "Comm") CambiarPantalla(GridCommunity);
        }

        // Función para cambiar de pantalla con animación suave (Fade In)
        private void CambiarPantalla(UIElement nuevaPantalla)
        {
            // Ocultamos todas
            GridLibrary.Visibility = Visibility.Collapsed;
            GridStore.Visibility = Visibility.Collapsed;
            GridCommunity.Visibility = Visibility.Collapsed;
            GridDetails.Visibility = Visibility.Collapsed;

            // Preparamos la nueva
            nuevaPantalla.Opacity = 0;
            nuevaPantalla.Visibility = Visibility.Visible;

            // Animamos la opacidad de 0 a 1
            DoubleAnimation fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4));
            nuevaPantalla.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        // ===============================================
        // 2. DETALLES DEL JUEGO
        // ===============================================
        private void Juego_Click(object sender, MouseButtonEventArgs e)
        {
            Border b = sender as Border;
            string juego = b.Tag.ToString();

            if (juego == "Gears") SetDatos("GEARS 5", "Shooter / Acción", "39.99€", "/Assets/gears.jpg",
                "La guerra total desciende. Kait Diaz se separa para descubrir su conexión con el enemigo.");

            if (juego == "Halo") SetDatos("HALO INFINITE", "FPS / Sci-Fi", "GRATIS", "/Assets/halo.jpg",
                "Cuando toda esperanza se pierde, el Jefe Maestro está listo para enfrentarse al enemigo.");

            if (juego == "Forza") SetDatos("FORZA HORIZON 5", "Carreras", "59.99€", "/Assets/forza.jpg",
                "Explora los vibrantes paisajes del mundo abierto de México con una acción de conducción ilimitada.");

            if (juego == "Starfield") SetDatos("STARFIELD", "RPG / Espacio", "69.99€", "/Assets/starfield.jpg",
                "Crea el personaje que desees y explora con una libertad sin igual entre las estrellas.");

            CambiarPantalla(GridDetails);
        }

        private void SetDatos(string titulo, string genero, string precio, string rutaImg, string desc)
        {
            txtTitulo.Text = titulo;
            txtGenero.Text = genero;
            txtPrecio.Text = precio;
            txtDesc.Text = desc;

            // Reseteamos el botón de Acción
            btnAccion.Content = "INSTALAR";
            btnAccion.Background = (SolidColorBrush)Application.Current.Resources["AccentColor"];
            btnAccion.IsEnabled = true;
            PanelDescarga.Visibility = Visibility.Collapsed;

            // Carga de imagen segura
            try
            {
                string rutaCompleta = "pack://application:,,," + rutaImg;
                BrushDetalle.ImageSource = new BitmapImage(new Uri(rutaCompleta));
            }
            catch { }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            CambiarPantalla(GridLibrary);
        }

        // ===============================================
        // 3. SIMULACIÓN DE DESCARGA
        // ===============================================
        private async void BtnAccion_Click(object sender, RoutedEventArgs e)
        {
            if (btnAccion.Content.ToString() == "EJECUTANDO...") return;

            // Estado inicial: Preparando
            btnAccion.IsEnabled = false;
            btnAccion.Content = "PREPARANDO...";
            btnAccion.Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));

            PanelDescarga.Visibility = Visibility.Visible;
            BarraProgreso.Value = 0;

            // Bucle de carga (simulado)
            for (int i = 0; i <= 100; i++)
            {
                BarraProgreso.Value = i;
                txtPorcentaje.Text = i + "%";
                await Task.Delay(25); // Velocidad de la descarga
            }

            // Fin descarga
            PanelDescarga.Visibility = Visibility.Collapsed;

            // Estado Final: Ejecutando
            btnAccion.Background = new SolidColorBrush(Color.FromRgb(100, 221, 23));
            btnAccion.Content = "EJECUTANDO...";

            await Task.Delay(2000); // Simulamos arranque del juego

            // Reset final
            btnAccion.IsEnabled = true;
            btnAccion.Content = "JUGAR AHORA";
        }

        // ===============================================
        // 4. CONTROL DE VENTANA Y ADMIN
        // ===============================================
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2) BtnMaximizar_Click(sender, e);
                else if (this.WindowState == WindowState.Normal) DragMove();
            }
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.MaxHeight = SystemParameters.WorkArea.Height + 14;
                this.MaxWidth = SystemParameters.WorkArea.Width + 14;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.MaxHeight = double.PositiveInfinity;
                this.MaxWidth = double.PositiveInfinity;
                this.WindowState = WindowState.Normal;
            }
        }

        private void BtnCerrarApp_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        // ABRIR PANEL DE ADMIN
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow admin = new AdminWindow();
            admin.ShowDialog();
        }
    }
}