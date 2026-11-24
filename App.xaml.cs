using System.Windows;
using System.Windows.Threading;

namespace ProjektReklamelyd
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Hvis der sker en fejl, som vi ikke har fanget, viser vi den i stedet for at programmet lukker
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Der skete en uventet fejl:\n\n" + e.Exception.Message, "Fejl",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
