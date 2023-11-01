using MessengerServer.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;

namespace MessengerServer
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                this.Run(new MainWindowView());
            }
            catch (DllNotFoundException ex)
            {
                MessageBox.Show($"Error: Dll is missing \n\nPlease make sure all required DLLs are present in executable folder.", "DLL Missing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: Failed to launch application. \n\nCheck if all proper dlls are present in executable folder.", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
