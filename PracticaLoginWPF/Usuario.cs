using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PracticaLoginWPF
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public string Email { get; set; }
        public string FechaRegistro { get; set; }
        public string Estado { get; set; }

        // Datos de la imagen en bruto (Base de datos)
        public byte[] Avatar { get; set; }

        // Propiedad para que WPF entienda la imagen
        public ImageSource AvatarImage
        {
            get
            {
                if (Avatar == null || Avatar.Length == 0) return null;
                try
                {
                    BitmapImage bi = new BitmapImage();
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