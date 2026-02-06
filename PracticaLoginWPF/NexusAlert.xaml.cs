using System.Windows;

namespace PracticaLoginWPF
{
    public partial class NexusAlert : Window
    {
        public bool Resultado { get; private set; } = false;

        public NexusAlert(string mensaje)
        {
            InitializeComponent();
            txtMensaje.Text = mensaje;
        }

        private void BtnSi_Click(object sender, RoutedEventArgs e)
        {
            Resultado = true;
            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            Resultado = false;
            this.Close();
        }
    }
}