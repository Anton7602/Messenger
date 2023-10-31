using MessengerServer.Models;
using MessengerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MessengerServer.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Fields and Properties
        private MessengerServiceHost host = new MessengerServiceHost();

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

        private string _statusTextBoxText = "Offline";
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

        public string CurrentURL => IpAddressTextBoxTextFirst+"."+ IpAddressTextBoxTextSecond+"."+ IpAddressTextBoxTextThird+"."+ IpAddressTextBoxTextForth + ":" + PortTextBoxText;

        public RelayCommand<object> connectionCommand { get; private set; }
        public RelayCommand<object> sendMessage { get; private set; }
        #endregion
        //---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


        public MainWindowViewModel()
        {
            using (var context = new MessageDbContext())
            {
                List<Message> databaseContent = context.Messages.ToList();
                foreach(Message message in databaseContent)
                {
                    MessagesList.Add(message.ToString());
                }
            }
            string relevantIP = GetPrivateIpAddress();
            string[] elementsOfIp = relevantIP.Split('.');
            if (elementsOfIp.Length == 4)
            {
                IpAddressTextBoxTextFirst = int.Parse(elementsOfIp[0]);
                IpAddressTextBoxTextSecond = int.Parse(elementsOfIp[1]);
                IpAddressTextBoxTextThird = int.Parse(elementsOfIp[2]);
                IpAddressTextBoxTextForth = int.Parse(elementsOfIp[3]);
            }
            host.Messenger.ServerUserAddedCallback += UserAddedCallbackHandler;
            host.Messenger.ServerUserRemovedCallback += UserRemovedCallbackHandler;
            host.Messenger.ServerMessageCallback += MessageCallbackHandler;
            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }

        private void HandleConnection(object url)
        {
            if (!IsOnline)
            {
                StatusTextBoxText = "Establishing connection";
                if (IsValidIP(url as string))
                {
                    try
                    {
                        host.startService(url as string);
                        StatusTextBoxText = "Online";
                        IsOnline = true;
                    }
                    catch (System.ServiceModel.AddressAlreadyInUseException)
                    {
                        StatusTextBoxText = "Provided port is occupied";
                        host.removeEndpoint();
                    }
                    catch (Exception ex)
                    {
                        StatusTextBoxText = ex.Message;
                        host.removeEndpoint();
                    }
                } else
                {
                    StatusTextBoxText = "Provided invalid IP/Port";
                }
            }
            else
            {
                StatusTextBoxText = "Disrupting connection";
                host.stopService();
                userList.Clear();
                StatusTextBoxText = "Offline";
                IsOnline = false;
            }
        }

        private void SendMessage(object message)
        {
            if (IsOnline)
            {
                String text = message as String;
                if (!text.Equals(String.Empty))
                {
                    host.Messenger.SendMessage(text, 0);
                    InputMessage = String.Empty;
                }
            }
        }

        private void MessageCallbackHandler(object sender, EventArgs e)
        {
            if (e!=null && e is Message)
            {
                MessagesList.Add(e.ToString());
                using (var context = new MessageDbContext())
                {
                    context.Messages.Add((e as Message));
                    context.SaveChanges();
                }
            }
        }

        private void UserAddedCallbackHandler(object sender, EventArgs e)
        {
            if (e!=null && e is User)
            {
                userList.Add((e as User).ToString());
            }
        }

        private void UserRemovedCallbackHandler(object sender, EventArgs e)
        {
            if (e != null && e is User)
            {
                userList.Remove((e as User).ToString());
            }
        }

        private static string GetPublicIpAddress()
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    string response = webClient.DownloadString("https://api.ipify.org");
                    return response.Trim();
                }
                catch
                {
                    return null;
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

        public static bool IsValidIP(string ipAddress)
        {
            //Checking that string pattern is valid for IP:port
            string pattern = @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d{1,5}$";
            if (!Regex.IsMatch(ipAddress, pattern)) {
                return false;
            }

            //Checking that : splits string in ip and port, if port is parsing to int and if it in range 1..65535 
            int port;
            string[] parts = ipAddress.Split(':');
            if (parts.Length != 2 || !int.TryParse(parts[1], out port) || port<1 || port >65535)
            {
                return false;
            }

            //Checking that . splits ip in 4 parts
            string[] octets = parts[0].Split('.');
            if (octets.Length!=4)
            {
                return false;
            }

            //Checking if every part of IP is int in range 0..255
            foreach (string ipPart in octets)
            {
                int parsedPart;
                if (!int.TryParse(ipPart, out parsedPart) || parsedPart < 0 || parsedPart>255)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
