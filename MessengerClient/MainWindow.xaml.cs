using MessengerClient.ServiceMessenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MessengerClient
{
    public partial class MainWindow : Window, IServiceMessengerCallback
    {
        private bool isConnected = false;
        private int ID;
        ServiceMessenger.ServiceMessengerClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        void ConnectUser()
        {
            if (!isConnected)
            {
                client = new ServiceMessengerClient(new System.ServiceModel.InstanceContext(this));
                Uri connectAddress = new Uri($"net.tcp://{tbIPadress.Text}:{tbPort.Text}/ServiceMessenger");
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(connectAddress);
                ID = client.Connect("Name");
                btnConnect.Content = "Disconnect";
                isConnected = true;
            }
        }

        void disconnectUser()
        {
            if (isConnected)
            {
                client.Disconnect(ID);
                btnConnect.Content = "Connect";
                isConnected = false;
            }
        }

        private void btnOpenHost_Click(object sender, RoutedEventArgs e)
        {
            if (isConnected)
            {
                disconnectUser();
            } else
            {
                ConnectUser();
            }
        }

        public void MessageCallback(string message)
        {
            throw new NotImplementedException();
        }
    }
}
