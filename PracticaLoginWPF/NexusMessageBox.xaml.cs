using System.Windows;

namespace PracticaLoginWPF
{
    public partial class NexusMessageBox : Window
    {
        public NexusMessageBox(string mensaje)
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // Método estático para usarlo fácil: NexusMessageBox.Show("Hola");
        public static void Show(string mensaje)
        {
            NexusMessageBox msg = new NexusMessageBox(mensaje);
            msg.ShowDialog();
        }
    }
}