using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.IO; // <--- NECESARIO PARA GUARDAR EN FICHERO

namespace PracticaLoginWPF
{
    public partial class MainWindow : Window
    {
        // 1. VARIABLES GLOBALES
        ConexionDB db = new ConexionDB();
        private int intentos = 0;
        private string rutaFichero = "usuario_guardado.txt"; // Nombre del archivo donde guardaremos el dato

        public MainWindow()
        {
            InitializeComponent();
            CargarUsuario(); // Leemos el fichero al arrancar
        }

        // ==============================================================
        // 2. SISTEMA NUEVO: RECORDAR USUARIO (FICHERO DE TEXTO)
        // ==============================================================
        private void CargarUsuario()
        {
            try
            {
                // Si existe el archivo, leemos lo que hay dentro
                if (File.Exists(rutaFichero))
                {
                    string usuarioGuardado = File.ReadAllText(rutaFichero);

                    if (!string.IsNullOrWhiteSpace(usuarioGuardado))
                    {
                        txtUsuario.Text = usuarioGuardado;
                        chkRecordar.IsChecked = true;
                        txtPassword.Focus(); // Ponemos el foco en la contraseña
                    }
                }
            }
            catch { /* Si falla al leer, no hacemos nada */ }
        }

        // ==============================================================
        // 3. LÓGICA DEL BOTÓN ENTRAR (Tus 4 Reglas)
        // ==============================================================
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string u = txtUsuario.Text;
            string p = txtPassword.Password;

            // REGLA 1: BLOQUEO TRAS 3 FALLOS
            if (intentos >= 3)
            {
                MostrarError("⛔ Se bloquea temporalmente el usuario");
                btnLogin.IsEnabled = false;
                return;
            }

            // REGLA 2: CAMPOS VACÍOS
            if (string.IsNullOrWhiteSpace(u) || string.IsNullOrWhiteSpace(p))
            {
                MostrarError("⚠ Introduzca datos");
                return;
            }

            // REGLA 3: EL USUARIO NO EXISTE
            // (Requiere que ConexionDB tenga el método ExisteUsuario)
            if (!db.ExisteUsuario(u))
            {
                MostrarError("⚠ El usuario introducido no existe");
                return;
            }

            // REGLA 4: VALIDACIÓN DE CONTRASEÑA
            if (db.ValidarUsuario(u, p))
            {
                // --- ¡LOGIN CORRECTO! ---

                // GUARDAR O BORRAR EL FICHERO SEGÚN EL CHECKBOX
                try
                {
                    if (chkRecordar.IsChecked == true)
                    {
                        File.WriteAllText(rutaFichero, u); // Creamos/Sobrescribimos el archivo con el nombre
                    }
                    else
                    {
                        if (File.Exists(rutaFichero)) File.Delete(rutaFichero); // Lo borramos si no quiere recordar
                    }
                }
                catch { /* Ignoramos errores de disco */ }

                // Abrimos la Home
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

        // Método auxiliar para mensajes
        private void MostrarError(string mensaje)
        {
            lblMensaje.Text = mensaje;
            lblMensaje.Foreground = new SolidColorBrush(Color.FromRgb(255, 82, 82)); // Rojo
        }

        // ==============================================================
        // 4. OTROS EVENTOS
        // ==============================================================

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