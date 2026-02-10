using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PracticaLoginWPF
{
    // 1. CLASE USUARIO
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; }
        public string Email { get; set; }
        public string FechaRegistro { get; set; }
        public string Estado { get; set; } // 'activo' o 'baneado'
        public decimal Saldo { get; set; }
        public byte[] Avatar { get; set; }

        public BitmapImage AvatarImage
        {
            get { return ConexionDB.ConvertirImagen(Avatar); }
        }

        // AQUÍ ESTÁ EL CAMBIO: Se llama 'ColorEstado' para coincidir con TU diseño
        public string ColorEstado
        {
            get { return Estado == "activo" ? "#00E676" : "#FF5252"; } // Verde o Rojo
        }
    }

    // 2. CLASE AMIGO
    public class Amigo
    {
        public int Id { get; set; }
        public int IdSolicitud { get; set; }
        public string Nombre { get; set; }
        public bool IsOnline { get; set; }
        public byte[] Avatar { get; set; }

        public BitmapImage AvatarImagen
        {
            get { return ConexionDB.ConvertirImagen(Avatar); }
        }

        public string ColorEstado
        {
            get { return IsOnline ? "#00E676" : "#666666"; }
        }

        public string TextoEstado
        {
            get { return IsOnline ? "Online" : "Offline"; }
        }
    }
}