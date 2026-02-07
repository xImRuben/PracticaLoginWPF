using System;
using System.Threading.Tasks; // Necesario para la simulación
using System.Windows;
using System.Windows.Media.Animation;

namespace PracticaLoginWPF
{
    public partial class GameLauncherWindow : Window
    {
        public GameLauncherWindow(Juego juego)
        {
            InitializeComponent();
            txtTitulo.Text = juego.Titulo.ToUpper();
            if (juego.CaratulaImagen != null) imgFondo.Source = juego.CaratulaImagen;

            // Iniciar la carga falsa al abrir la ventana
            SimularCarga();
        }

        private async void SimularCarga()
        {
            // Esperamos un poco antes de empezar
            await Task.Delay(500);

            // Simulamos carga de 0 a 100
            for (int i = 0; i <= 100; i += 2)
            {
                // Actualizar ancho de la barra (Ancho total 400px * porcentaje)
                barraProgreso.Width = (i / 100.0) * 400;
                txtPorcentaje.Text = i + "%";

                // Velocidad variable para realismo
                if (i < 30) await Task.Delay(20);       // Rápido al principio
                else if (i < 70) await Task.Delay(50);  // Lento en medio
                else await Task.Delay(10);              // Rápido al final
            }

            await Task.Delay(500); // Pausa al 100%
            this.Close(); // Cerrar lanzador

            // Opcional: Mensaje final
            // NexusMessageBox.Show($"¡{txtTitulo.Text} se está ejecutando!");
        }
    }
}