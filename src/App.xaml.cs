using Sql2Excel.Controllers;
using Sql2Excel.Model.Entities;
using Sql2Excel.Model.Extensions;
using Sql2Excel.Services;
using Sql2Excel.Utils;
using System.IO;
using System.Text.Json;
using System.Windows;

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

            var parameters = await GetExecutionParams();
            if (parameters is null)
            {
                Shutdown();
                return;
            }
            var main = new MainWindow(parameters, e);

            main.Show();
        }

        private async Task<ExecutionParameters?> GetExecutionParams()
        {
            if (!File.Exists(AppConsts.JSON_CONFIG_FILE))
            {
                NotificationUtil.ShowError("Config file not found");
                return null;
            }

            using FileStream fs = File.OpenRead(AppConsts.JSON_CONFIG_FILE);
            var context = await JsonSerializer.DeserializeAsync<ExecutionParameters>(fs);

            return context!;
        }
    }

}
