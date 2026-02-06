using System.Windows;
using System.Windows.Input;

namespace PracticaLoginWPF
{
    public partial class MainWindow : Window
    {
        ConexionDB db = new ConexionDB();

        public MainWindow()
        {
            InitializeComponent();
            CargarUsuarioRecordado();
        }

        // Recuperar usuario guardado al abrir la app
        private void CargarUsuarioRecordado()
        {
            try
            {
                if (!string.IsNullOrEmpty(PracticaLoginWPF.Properties.Settings.Default.NombreUsuario))
                {
                    txtUser.Text = PracticaLoginWPF.Properties.Settings.Default.NombreUsuario;
                    chkRecordar.IsChecked = true;
                    txtPass.Focus();
                }
            }
            catch { }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUser.Text;
            string pass = txtPass.Password;

            // Ocultamos el error por si acaso estaba visible de antes
            PanelError.Visibility = Visibility.Collapsed;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MostrarError("Introduce tu ID de usuario y clave.");
                return;
            }

            Usuario u = db.LoginUsuario(user, pass);

            if (u != null)
            {
                // =========================================================
                // 🛑 LÓGICA DE BANEO Y APELACIÓN (FACTOR X)
                // =========================================================
                if (u.Estado == "baneado")
                {
                    string motivo = db.ObtenerMotivoBan(u.Nombre);
                    if (string.IsNullOrEmpty(motivo)) motivo = "Incumplimiento de normas.";

                    // USAMOS LA NUEVA ALERTA PERSONALIZADA (NexusAlert)
                    NexusAlert alerta = new NexusAlert($"Tu cuenta ha sido suspendida.\nMotivo: {motivo}");
                    alerta.ShowDialog(); // Espera a que el usuario pulse un botón

                    // Si el usuario pulsó "SÍ, APELAR" (Resultado = true)
                    if (alerta.Resultado == true)
                    {
                        ApelacionWindow apelacion = new ApelacionWindow(u.Nombre);
                        apelacion.ShowDialog();
                    }

                    return; // Cortamos aquí para que no entre al sistema
                }

                // =========================================================
                // ✅ LOGIN CORRECTO
                // =========================================================

                // 1. Iniciamos la sesión global
                Sesion.Iniciar(u);

                // 2. Guardar o borrar "Recordar usuario"
                if (chkRecordar.IsChecked == true)
                {
                    PracticaLoginWPF.Properties.Settings.Default.NombreUsuario = txtUser.Text;
                }
                else
                {
                    PracticaLoginWPF.Properties.Settings.Default.NombreUsuario = "";
                }
                PracticaLoginWPF.Properties.Settings.Default.Save();

                // 3. REDIRECCIÓN: TODOS AL HOME
                // (El Home ya se encarga de mostrar u ocultar el botón Admin según el rol)
                HomeWindow home = new HomeWindow();
                home.Show();

                this.Close();
            }
            else
            {
                MostrarError("Credenciales incorrectas.");
            }
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            PanelError.Visibility = Visibility.Visible;
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow reg = new RegisterWindow();
            reg.Show();
            this.Close();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}