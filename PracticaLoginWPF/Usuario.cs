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
        public string FechaRegistro { get; set; }
        public string Rol { get; set; }
        public string Email { get; set; }
        public string Estado { get; set; } // "activo" o "baneado"
        public byte[] Avatar { get; set; }

        public ImageSource AvatarImage
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

        // --- NUEVO: Propiedad para el color del estado ---
        // Si está "baneado" devuelve Rojo suave (#FF5252).
        // Si no, devuelve Verde brillante (#00E676).
        public string ColorEstado
        {
            get
            {
                return (Estado == "baneado") ? "#FF5252" : "#00E676";
            }
        }
    }
}