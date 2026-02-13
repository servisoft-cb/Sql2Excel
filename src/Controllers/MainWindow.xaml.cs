using DocumentFormat.OpenXml.Spreadsheet;
using Sql2Excel.Model.Entities;
using Sql2Excel.Model.Extensions;
using Sql2Excel.Services;
using Sql2Excel.Utils;
using System.Windows;

namespace Sql2Excel.Controllers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly ExecutionParameters _parameters;
        private readonly string _sql;
        private readonly string? _filename;

        public MainWindow(ExecutionParameters parameters, StartupEventArgs e)
        {
            _parameters = parameters;
            _sql = e.Args[0];

            if(e.Args.Length > 1)
            {
                _filename = e.Args[1];
            }

            InitializeComponent();

            Loaded += MainWindowLoaded;
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var data = await GetData(_sql, _parameters);

            if (!data.Any())
            {
                NotificationUtil.ShowError("No data found");
                Application.Current.Shutdown();
            }


            await Task.Run((async () =>
            {
                using var file = WorkbookService.GenerateXlsx(data, _parameters.GetWorkbookTheme());
                WorkbookService.Persist(file, _parameters.DestinationPath, _filename);
            }));

            Application.Current.Shutdown();
        }

        private async Task<IEnumerable<dynamic>> GetData(string sql, ExecutionParameters parameters)
        {
            var dbConnection = DatabaseService.GetDbConnection(parameters);
            if (dbConnection is null)
            {
                NotificationUtil.ShowError("Connection not established");
                Application.Current.Shutdown();
                return [];
            }

            if (!sql.IsValidSqlQuery())
            {
                NotificationUtil.ShowError("Invalid SQL Statement");
                Application.Current.Shutdown();
                return [];
            }


            var resultSQL = await DatabaseService.QueryData(dbConnection, sql);

            return resultSQL.ToList();
        }
    }
}