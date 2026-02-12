using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Sql2Excel.Utils;

public static class NotificationUtil
{
    public static void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
