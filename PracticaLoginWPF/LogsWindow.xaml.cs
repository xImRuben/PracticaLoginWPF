using System;
using System.Data;
using System.IO; // Para manejar archivos
using System.Text; // Para manejar texto
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32; // Para el cuadro de diálogo "Guardar Como"

namespace PracticaLoginWPF
{
    public partial class LogsWindow : Window
    {
        ConexionDB db = new ConexionDB();
        private DataTable dtLogs; // Guardamos los datos en memoria para poder exportarlos

        public LogsWindow()
        {
            InitializeComponent();
            CargarLogs();
        }

        private void CargarLogs()
        {
            // Obtenemos los datos y los guardamos en la variable global dtLogs
            dtLogs = db.ObtenerLogs();
            GridLogs.ItemsSource = dtLogs.DefaultView;
        }

        // --- EXPORTAR A CSV (EXCEL) ---
        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            if (dtLogs == null || dtLogs.Rows.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.");
                return;
            }

            // Abrimos cuadro de diálogo para preguntar dónde guardar
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Archivo CSV (*.csv)|*.csv";
            saveFileDialog.FileName = "Reporte_Auditoria_" + DateTime.Now.ToString("yyyyMMdd");

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    // 1. CABECERAS
                    sb.AppendLine("ID;FECHA;ADMIN;ACCION;USUARIO_AFECTADO");

                    // 2. DATOS (Recorremos la tabla)
                    foreach (DataRow row in dtLogs.Rows)
                    {
                        string fecha = row["fecha"].ToString();
                        string admin = row["admin_responsable"].ToString();
                        string accion = row["accion"].ToString();
                        string usuario = row["usuario_afectado"].ToString();
                        string id = row["id"].ToString();

                        // Formato CSV separado por punto y coma (Estándar Excel en Europa)
                        sb.AppendLine($"{id};{fecha};{admin};{accion};{usuario}");
                    }

                    // 3. GUARDAR EN DISCO
                    File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                    MessageBox.Show("¡Informe generado con éxito! Puedes abrirlo en Excel.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al exportar: " + ex.Message);
                }
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}