using System.Windows;
using System.Windows.Input;

namespace PracticaLoginWPF
{
    public partial class MainWindow : Window
    {
        // ==========================================
        // ESTO ES LO QUE TE FALTABA (VARIABLES GLOBALES)
        // ==========================================

        // 1. La conexión a la base de datos
        ConexionDB db = new ConexionDB();

        // 2. El contador de intentos fallidos
        private int intentos = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        // ==========================================
        // MÉTODOS Y EVENTOS
        // ==========================================

        // Mover ventana al arrastrar
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        // Minimizar
        private void BtnMinimizar_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Salir (Cerrar App)
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // BOTÓN LOGIN (Lógica Principal)
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Cogemos lo que ha escrito el usuario
            string u = txtUsuario.Text;
            string p = txtPassword.Password;

            // 1. Seguridad: Bloqueo tras 3 intentos
            if (intentos >= 3)
            {
                lblMensaje.Text = "⛔ SISTEMA BLOQUEADO: Demasiados intentos.";
                btnLogin.IsEnabled = false;
                return;
            }

            // 2. Validación: Campos vacíos
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                lblMensaje.Text = "⚠ Por favor, introduce usuario y clave.";
                return;
            }

            // 3. Consultamos a la Base de Datos (MySQL)
            if (db.ValidarUsuario(u, p))
            {
                // ¡Éxito!
                HomeWindow home = new HomeWindow();
                home.Show();
                this.Close();
            }
            else
            {
                // Fallo
                intentos++;
                lblMensaje.Text = $"⚠ Credenciales inválidas ({intentos}/3)";
                txtPassword.Clear();
                txtPassword.Focus();
            }
        }

        // BOTÓN IR AL REGISTRO
        private void BtnIrRegistro_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.Show();
            this.Close();
        }
    }
}