using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PracticaLoginWPF
{
    // Clase para el Chat
    public class Mensaje
    {
        public string Usuario { get; set; }
        public string Texto { get; set; }
        public string Fecha { get; set; }
        public string ColorNombre { get; set; } // Ejemplo: "#651FFF" (Morado)
    }

    // Clase para el Ranking
    public class TopGamer
    {
        public int Rank { get; set; }
        public string Nombre { get; set; }
        public int Nivel { get; set; }
        public byte[] Avatar { get; set; }

        // Convierte los bytes de la BBDD a imagen visible
        public ImageSource AvatarImagen
        {
            get
            {
                if (Avatar == null || Avatar.Length == 0) return null;
                try
                {
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.StreamSource = new MemoryStream(Avatar);
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.EndInit();
                    return bi;
                }
                catch { return null; }
            }
        }
    }
}