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