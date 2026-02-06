using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLoginWPF
{
    public partial class RegisterWindow : Window
    {
        ConexionDB db = new ConexionDB();

        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string u = txtRegUser.Text;
            string p = txtRegPass.Password;
            string p2 = txtRegPassConfirm.Password;

            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                lblError.Text = "Todos los campos son obligatorios.";
                return;
            }

            if (p != p2)
            {
                lblError.Text = "Las contraseñas no coinciden.";
                return;
            }

            bool registrado = db.RegistrarUsuario(u, p);

            if (registrado)
            {
                MessageBox.Show("¡Cuenta creada en Nexus! Ahora puedes iniciar sesión.", "Éxito");
                MainWindow login = new MainWindow();
                login.Show();
                this.Close();
            }
            else
            {
                lblError.Text = "Error: El usuario ya existe o falló la conexión.";
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}