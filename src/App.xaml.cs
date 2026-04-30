using MailTool.Config;
using Sql2Excel.Controllers;
using Sql2Excel.Utils;
using System.Windows;
using System.Windows.Threading;

namespace Sql2Excel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (e.Args.Length == 0)
            {
                NotificationUtil.ShowError("No arguments provided");
                return;
            }

            try
            {
                var config = new ConfigLoader();
                var parameters = config.GetExecutionParams();
                parameters.DestinationPath = e.Args[0];
                parameters.SqlFilePath = e.Args[1];

                if (e.Args.Length == 3)
                {
                    parameters.XlsFilename = e.Args[2];
                }

                var main = new MainWindow(parameters, e);
                main.Show();
            }
            catch (Exception ex)
            {
                NotificationUtil.ShowError(ex.Message);
                Shutdown();
            }
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            NotificationUtil.ShowError($"Erro inesperado: {e.Exception.Message}");
            e.Handled = true;
            Shutdown();
        }
    }

}
