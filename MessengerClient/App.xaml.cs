using MessengerClient.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MessengerClient
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                this.Run(new MessengerMainWindow());
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show($"Error: Dll is missing \n\nPlease make sure all required DLLs are present in executable folder.", "DLL Missing", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}. \n\nMake sure that MessengerClient.exe.config is present in executable folder", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}
