using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PracticaLoginWPF
{
    public class Juego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Genero { get; set; }
        public decimal Precio { get; set; }
        public string Descripcion { get; set; }
        public bool Visible { get; set; }
        public byte[] Caratula { get; set; }

        // --- NUEVO: VALORACIÓN Y ESTRELLAS ---
        public int Valoracion { get; set; }

        // Convierte el número (ej: 4) en texto (★★★★☆)
        public string EstrellasDisplay
        {
            get
            {
                string s = "";
                // Aseguramos que esté entre 0 y 5
                int val = Math.Max(0, Math.Min(5, Valoracion));

                for (int i = 0; i < val; i++) s += "★"; // Estrella llena
                for (int i = val; i < 5; i++) s += "☆"; // Estrella vacía
                return s;
            }
        }

        public string PrecioFormato { get { return Precio.ToString("0.00") + " €"; } }

        public ImageSource CaratulaImagen
        {
            get
            {
                if (Caratula == null || Caratula.Length == 0) return null;
                try
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(Caratula);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return bi;
                }
                catch { return null; }
            }
        }
    }
}