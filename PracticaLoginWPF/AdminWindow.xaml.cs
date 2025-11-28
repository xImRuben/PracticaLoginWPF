using System.Windows;
using System.Windows.Input;

namespace PracticaLoginWPF
{
    public partial class AdminWindow : Window
    {
        ConexionDB db = new ConexionDB();

        public AdminWindow()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            GridUsuarios.ItemsSource = db.ObtenerUsuarios();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}