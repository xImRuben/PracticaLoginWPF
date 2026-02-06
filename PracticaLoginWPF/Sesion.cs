using System;

namespace PracticaLoginWPF
{
    // Clase estática para acceder al usuario actual desde CUALQUIER ventana
    public static class Sesion
    {
        public static Usuario UsuarioActual { get; set; }

        public static void Iniciar(Usuario u)
        {
            UsuarioActual = u;
        }

        public static void Cerrar()
        {
            UsuarioActual = null;
        }

        public static bool EstaLogueado()
        {
            return UsuarioActual != null;
        }
    }
}