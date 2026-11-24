using ProjektReklamelyd.View;
using System.Windows;

namespace ProjektReklamelyd
{
    /// <summary>
    /// Startvinduet. Herfra åbnes programmets funktioner.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Funktion 1: Administration og kategorisering af lydfiler
        private void KnapLydfiler_Click(object sender, RoutedEventArgs e)
        {
            LydfilWindow lydfilWindow = new LydfilWindow();
            lydfilWindow.Owner = this;
            lydfilWindow.ShowDialog();
        }
    }
}
