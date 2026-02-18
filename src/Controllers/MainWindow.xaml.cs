using Sql2Excel.Model.Entities;
using Sql2Excel.Model.Extensions;
using Sql2Excel.Services;
using Sql2Excel.Utils;
using System.IO;
using System.Windows;

namespace Sql2Excel.Controllers
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private readonly ExecutionParameters _parameters;

        public MainWindow(ExecutionParameters parameters, StartupEventArgs e)
        {
            _parameters = parameters;

            InitializeComponent();

            Loaded += MainWindowLoaded;
        }

        private async void MainWindowLoaded(object sender, RoutedEventArgs e)
        {
            var sql = await ReadSqlFromFile(_parameters.SqlFilePath);

            var data = await GetData(sql, _parameters);

            if (!data.Any())
            {
                NotificationUtil.ShowError("No data found");
                Application.Current.Shutdown();
            }


            await Task.Run((async () =>
            {
                using var file = WorkbookService.GenerateXlsx(data, _parameters.GetWorkbookTheme());
                WorkbookService.Persist(file, _parameters.DestinationPath, _parameters.XlsFilename);
            }));

            NotificationUtil.ShowInfo("Operação concluida");
            Application.Current.Shutdown();
        }

        private async Task<List<string>> ReadSqlFromFile(string sqlPath)
        {
            var completeName = $"{sqlPath}\\sqlRelatorio.txt";

            if (!File.Exists(completeName))
            {
                NotificationUtil.ShowError("Arquivo com sql não encontrado");
                Application.Current.Shutdown();
            }

            var sqlString = await File.ReadAllTextAsync(completeName);

            return sqlString.CorrigirSqlParaFirebird().ToList();
        }

        private async Task<IEnumerable<dynamic>> GetData(List<string> sqlQueries, ExecutionParameters parameters)
        {
            var dbConnection = DatabaseService.GetDbConnection(parameters);
            if (dbConnection is null)
            {
                NotificationUtil.ShowError("Connection not established");
                Application.Current.Shutdown();
                return [];
            }

            // Valida todas as queries
            foreach (var sql in sqlQueries)
            {
                if (!sql.IsValidSqlQuery())
                {
                    NotificationUtil.ShowError("Invalid SQL Statement");
                    Application.Current.Shutdown();
                    return [];
                }
            }

            // Executa todas as queries e combina os resultados
            var allResults = new List<dynamic>();
            foreach (var sql in sqlQueries)
            {
                var resultSQL = await DatabaseService.QueryData(dbConnection, sql);
                allResults.AddRange(resultSQL);
            }

            return allResults;
        }
    }
}