using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PracticaLoginWPF
{
    public partial class MainWindow : Window
    {
        ConexionDB db = new ConexionDB();
        private int intentos = 0;

        public MainWindow()
        {
            InitializeComponent();
            CargarUsuario();
        }

        private void CargarUsuario()
        {
            string guardado = Properties.Settings.Default.UsuarioGuardado;
            if (!string.IsNullOrEmpty(guardado))
            {
                txtUsuario.Text = guardado;
                chkRecordar.IsChecked = true;
                txtPassword.Focus();
            }
        }

        // ==============================================================
        // LÓGICA DE LOGIN CON VALIDACIONES ESPECÍFICAS
        // ==============================================================
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string u = txtUsuario.Text;
            string p = txtPassword.Password;

            // 1. REGLA BLOQUEO: Si ya ha fallado 3 veces antes
            if (intentos >= 3)
            {
                MostrarError("⛔ Se bloquea temporalmente el usuario");
                btnLogin.IsEnabled = false; // Bloqueo visual del botón
                return;
            }

            // 2. REGLA VACÍOS: Login o pass en blanco
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                MostrarError("⚠ Introduzca datos");
                return;
            }

            // 3. REGLA USUARIO NO EXISTE
            // Comprobamos si el nombre existe en la base de datos
            if (!db.ExisteUsuario(u))
            {
                MostrarError("⚠ El usuario introducido no existe");
                return;
            }

            // 4. REGLA CONTRASEÑA INCORRECTA (El usuario sí existe)
            if (db.ValidarUsuario(u, p))
            {
                // --- LOGIN CORRECTO ---

                // Guardar usuario si aplica
                if (chkRecordar.IsChecked == true) Properties.Settings.Default.UsuarioGuardado = u;
                else Properties.Settings.Default.UsuarioGuardado = "";
                Properties.Settings.Default.Save();

                // Abrir Home y cerrar Login
                HomeWindow home = new HomeWindow();
                home.Show();
                this.Close();
            }
            else
            {
                // --- CONTRASEÑA MAL ---
                intentos++;

                if (intentos >= 3)
                {
                    MostrarError("⛔ Se bloquea temporalmente el usuario");
                    btnLogin.IsEnabled = false;
                }
                else
                {
                    MostrarError($"⚠ La contraseña no es correcta ({intentos}/3)");
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
        }

        // Método auxiliar para limpiar el código
        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Foreground = new SolidColorBrush(Color.FromRgb(255, 82, 82)); // Rojo
        }

        private void BtnIrRegistro_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.Show();
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnMinimizar_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void BtnSalir_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}