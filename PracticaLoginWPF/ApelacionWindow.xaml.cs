using System.Windows;
using System.Windows.Input;

namespace PracticaLoginWPF
{
    public partial class ApelacionWindow : Window
    {
        ConexionDB db = new ConexionDB();
        string usuarioBaneado;

        public ApelacionWindow(string usuario)
        {
            InitializeComponent();
            usuarioBaneado = usuario;
            txtUsuario.Text = usuarioBaneado;
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMensaje.Text))
            {
                MessageBox.Show("Por favor, escribe un motivo.");
                return;
            }

            if (db.ExisteApelacionPendiente(usuarioBaneado))
            {
                MessageBox.Show("Ya tienes una apelación pendiente de revisión.", "Espera");
                return;
            }

            if (db.EnviarApelacion(usuarioBaneado, txtMensaje.Text))
            {
                MessageBox.Show("Apelación enviada al Tribunal Nexus. La revisaremos pronto.", "Enviada");
                this.Close();
            }
            else
            {
                MessageBox.Show("Error al enviar la apelación.");
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}