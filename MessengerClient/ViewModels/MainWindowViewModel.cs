using DevExpress.Data;
using MessengerClient.ServiceMessenger;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Policy;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Data;
using MessengerClient.Models;

namespace MessengerClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IServiceMessengerCallback
    {
        private int ID;
        ServiceMessenger.ServiceMessengerClient client;

        private ObservableCollection<string> userList = new ObservableCollection<string>();
        public ObservableCollection<string> UserList
        {
            get { return userList; }
            set
            {
                userList = value;
                OnPropertyChanged(nameof(userList));
            }
        }

        private ObservableCollection<String> messagesList = new ObservableCollection<String>();
        public ObservableCollection<String> MessagesList
        {
            get { return messagesList; }
            set
            {
                messagesList = value;
                OnPropertyChanged(nameof(MessagesList));
            }
        }

        private int _ipAddressTextBoxTextFirst = 192;
        public int IpAddressTextBoxTextFirst
        {
            get { return _ipAddressTextBoxTextFirst; }
            set
            {
                _ipAddressTextBoxTextFirst = value;
                OnPropertyChanged(nameof(IpAddressTextBoxTextFirst));
            }
        }

        private int _ipAddressTextBoxTextSecond = 168;
        public int IpAddressTextBoxTextSecond
        {
            get { return _ipAddressTextBoxTextSecond; }
            set
            {
                _ipAddressTextBoxTextSecond = value;
                OnPropertyChanged(nameof(IpAddressTextBoxTextSecond));
            }
        }

        private int _ipAddressTextBoxTextThird = 0;
        public int IpAddressTextBoxTextThird
        {
            get { return _ipAddressTextBoxTextThird; }
            set
            {
                _ipAddressTextBoxTextThird = value;
                OnPropertyChanged(nameof(IpAddressTextBoxTextThird));
            }
        }

        private int _ipAddressTextBoxTextForth = 0;
        public int IpAddressTextBoxTextForth
        {
            get { return _ipAddressTextBoxTextForth; }
            set
            {
                _ipAddressTextBoxTextForth = value;
                OnPropertyChanged(nameof(IpAddressTextBoxTextForth));
            }
        }

        private int _portTextBoxText = 7602;
        public int PortTextBoxText
        {
            get { return _portTextBoxText; }
            set
            {
                _portTextBoxText = value;
                OnPropertyChanged(nameof(PortTextBoxText));
            }
        }

        private string _userNameTextBoxText = String.Empty;
        public string UserNameTextBoxText
        {
            get { return _userNameTextBoxText; }
            set
            {
                _userNameTextBoxText = value;
                OnPropertyChanged(nameof(UserNameTextBoxText));
            }
        }

        private bool _isOnline = false;
        public bool IsOnline
        {
            get { return _isOnline; }
            set
            {
                _isOnline = value;
                OnPropertyChanged(nameof(IsOnline));
            }
        }

        private string _statusTextBoxText = "Disconnected";
        public string StatusTextBoxText
        {
            get { return _statusTextBoxText; }
            set
            {
                _statusTextBoxText = value;
                OnPropertyChanged(nameof(StatusTextBoxText));
            }
        }

        private string _inputMessage = "";
        public string InputMessage
        {
            get { return _inputMessage; }
            set
            {
                _inputMessage = value;
                OnPropertyChanged(nameof(InputMessage));
            }
        }

        public string CurrentURL => IpAddressTextBoxTextFirst + "." + IpAddressTextBoxTextSecond + "." + IpAddressTextBoxTextThird + "." + IpAddressTextBoxTextForth + ":" + PortTextBoxText;

        public RelayCommand<object> connectionCommand { get; private set; }
        public RelayCommand<object> sendMessage { get; private set; }

        public MainWindowViewModel()
        {
            string relevantIP = GetPrivateIpAddress();
            string[] elementsOfIp = relevantIP.Split('.');
            if (elementsOfIp.Length == 4)
            {
                IpAddressTextBoxTextFirst = int.Parse(elementsOfIp[0]);
                IpAddressTextBoxTextSecond = int.Parse(elementsOfIp[1]);
                IpAddressTextBoxTextThird = int.Parse(elementsOfIp[2]);
                IpAddressTextBoxTextForth = int.Parse(elementsOfIp[3]);
            }

            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }

        private void Connect(string url)
        {
            if (!IsOnline)
            {
                StatusTextBoxText = "Establishing connection";
                client = new ServiceMessengerClient(new System.ServiceModel.InstanceContext(this));
                Uri connectAddress = new Uri($"net.tcp://{url}");
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(connectAddress);
                if (IsUsernameVaild(UserNameTextBoxText))
                {
                    try
                    {
                        ID = client.Connect(UserNameTextBoxText);
                        IsOnline = true;
                        StatusTextBoxText = "Connected";
                        UserList.Clear();
                        foreach (var chatMember in client.GetUsersList())
                        {
                            userList.Add(chatMember);
                        }
                    }
                    catch
                    {
                        StatusTextBoxText = "Connection failed";
                    }
                } else
                {
                    StatusTextBoxText = "Provided invalid username";
                }
            }
        }

        private void Disconnect()
        {
            if (IsOnline)
            {
                StatusTextBoxText = "Disrupting connection";
                client.Disconnect(ID);
                IsOnline = false;
                UserList.Clear();
                StatusTextBoxText = "Disconnected";
            }
        }

        private void HandleConnection(object url)
        {
            if (!IsOnline)
            {
                Connect(url.ToString());

            } else
            {
                Disconnect();
            }
        }

        private bool IsUsernameVaild(string username)
        {
            if (username  == null || username.Length==0)
            {
                return false;
            }
            return true;
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage(InputMessage);
            }
        }

        private void SendMessage(object message)
        {
            if (IsOnline)
            {
                string text = message.ToString();
                if (!text.Equals(String.Empty))
                {
                    client.SendMessage(text, ID);
                    InputMessage = String.Empty;
                }
            }
        }

        private static string GetPrivateIpAddress()
        {
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface networkInterface in networkInterfaces)
            {
                if (networkInterface.OperationalStatus == OperationalStatus.Up && networkInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation ipAddress in ipProperties.UnicastAddresses)
                    {
                        if (ipAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            return ipAddress.Address.ToString();
                        }
                    }
                }
            }
            return null;
        }

        public void MessageCallback(string message)
        {
            if (message!=null)
            {
                MessagesList.Add(message);
            }
        }

        public void ChatMemberLeftCallback(string chatMember)
        {
            if (chatMember != null && UserList.Contains(chatMember))
            {
                UserList.Remove(chatMember);
            }
        }

        public void ChatMemberJoinedCallback(string chatMember)
        {
            if (chatMember!=null)
            {
                UserList.Add(chatMember);
            }
        }

        public void ServerShutDownCallback()
        {
            IsOnline = false;
            UserList.Clear();
            StatusTextBoxText = "Disconnected";
        }
    }
}
