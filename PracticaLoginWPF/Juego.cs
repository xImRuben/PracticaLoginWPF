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

        // Propiedad para formatear el precio en la interfaz (ej: "39.99 €")
        public string PrecioFormato => Precio == 0 ? "GRATIS" : $"{Precio} €";

        // IMAGEN (Base de datos -> Byte[])
        public byte[] Caratula { get; set; }

        // IMAGEN (Logica Inteligente: DB > Archivo Local > Null)
        public ImageSource CaratulaImagen
        {
            get
            {
                // 1. PRIORIDAD: Si hay imagen en la Base de Datos (BLOB), úsala.
                if (Caratula != null && Caratula.Length > 0)
                {
                    try
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.StreamSource = new MemoryStream(Caratula);
                        bi.CacheOption = BitmapCacheOption.OnLoad;
                        bi.EndInit();
                        return bi;
                    }
                    catch { }
                }

                // 2. RESPALDO: Si la DB está vacía, buscamos en Assets según el nombre del juego.
                // Esto es temporal para que los juegos iniciales se vean bonitos.
                string rutaLocal = null;

                if (Titulo != null)
                {
                    if (Titulo.Contains("Gears")) rutaLocal = "/Assets/gears.jpg";
                    else if (Titulo.Contains("Halo")) rutaLocal = "/Assets/halo.jpg";
                    else if (Titulo.Contains("Forza")) rutaLocal = "/Assets/forza.jpg";
                    else if (Titulo.Contains("Starfield")) rutaLocal = "/Assets/starfield.jpg";
                }

                if (rutaLocal != null)
                {
                    try
                    {
                        return new BitmapImage(new Uri("pack://application:,,," + rutaLocal));
                    }
                    catch { }
                }

                // 3. SI NO HAY NADA: Devolvemos null (el XAML pondrá el logo.ico por defecto)
                return null;
            }
        }
    }
}