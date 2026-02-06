using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation; // Para animaciones suaves

namespace PracticaLoginWPF
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
            StartLoading(); // Iniciamos la carga nada más abrirse
        }

        private async void StartLoading()
        {
            // FASE 1: Iniciando
            txtLoading.Text = "Inicializando núcleo del sistema...";
            await AnimateBar(0, 30, 800); // Anima del 0% al 30% en 0.8 segundos

            // FASE 2: Base de Datos
            txtLoading.Text = "Estableciendo conexión segura con NexusDB...";
            await AnimateBar(30, 70, 1200); // Del 30% al 70% en 1.2 segundos

            // FASE 3: Interfaz
            txtLoading.Text = "Cargando interfaz gráfica...";
            await AnimateBar(70, 100, 800); // Del 70% al 100% en 0.8 segundos

            // FASE 4: Finalizar
            txtLoading.Text = "¡Bienvenido!";
            await Task.Delay(500); // Pequeña pausa final

            // ABRIR EL LOGIN Y CERRAR ESTA VENTANA
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }

        // Método auxiliar para animar la barra suavemente
        private async Task AnimateBar(double fromPercent, double toPercent, int durationMs)
        {
            double totalWidth = 400; // El ancho que pusimos en el XAML (Grid Width="400")

            DoubleAnimation animation = new DoubleAnimation();
            animation.From = (fromPercent / 100) * totalWidth;
            animation.To = (toPercent / 100) * totalWidth;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(durationMs));

            ProgressBarFill.BeginAnimation(System.Windows.Controls.Border.WidthProperty, animation);

            await Task.Delay(durationMs);
        }
    }
}