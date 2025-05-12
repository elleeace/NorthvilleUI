using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NorthvilleUI
{
    internal static class FunctionalityErrorMessage
    {
        public static void DisplayFunctionLimitMessage()
        {
            MessageBox.Show(
    "You do not have permission to access this functionality.\n\nPlease contact the Library Administrator for access.",
    "Access Denied",
    MessageBoxButton.OK,
    MessageBoxImage.Warning
);

        }
    }
}
