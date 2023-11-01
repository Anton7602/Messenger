using MessengerClient.ServiceMessenger;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Windows.Input;

namespace MessengerClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IServiceMessengerCallback
    {
        #region Fields and Properties
        private int ID;
        ServiceMessenger.ServiceMessengerClient client;

        private ObservableCollection<string> _userList = new ObservableCollection<string>();
        public ObservableCollection<string> UserList
        {
            get { return _userList; }
            set
            {
                _userList = value;
                OnPropertyChanged(nameof(UserList));
            }
        }

        private ObservableCollection<String> _messagesList = new ObservableCollection<String>();
        public ObservableCollection<String> MessagesList
        {
            get { return _messagesList; }
            set
            {
                _messagesList = value;
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
        #endregion

        #region ViewModels Constructors

        public MainWindowViewModel()
        {
            //Attempt to prefill proper IP
            string relevantIP = GetPublicIpAddress();
            string[] elementsOfIp = relevantIP.Split('.');
            if (elementsOfIp.Length == 4)
            {
                IpAddressTextBoxTextFirst = int.Parse(elementsOfIp[0]);
                IpAddressTextBoxTextSecond = int.Parse(elementsOfIp[1]);
                IpAddressTextBoxTextThird = int.Parse(elementsOfIp[2]);
                IpAddressTextBoxTextForth = int.Parse(elementsOfIp[3]);
            }

            //Binding Commands to handler Methods
            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }
        #endregion

        #region MessengerMethods
        /// <summary>
        /// Establishes new ServiceMessengerClient and attempts to connect to a service on a provided URL. Switches UI elements to Online (or error) mode
        /// </summary>
        /// <param name="url">URL of Messenger Server</param>
        private void Connect(string url)
        {
            if (!IsOnline)
            {
                StatusTextBoxText = "Establishing connection";
                client = new ServiceMessengerClient(new System.ServiceModel.InstanceContext(this));
                Uri connectAddress = new Uri($"net.tcp://{url}");
                EndpointIdentity identity = EndpointIdentity.CreateUpnIdentity("DESKTOP-BTRBT9G\\Антон");
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(connectAddress, identity);
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
                            _userList.Add(chatMember);
                        }
                    }
                    catch (Exception ex)
                    {
                        StatusTextBoxText = "Connection failed " + ex.Message;
                    }
                } else
                {
                    StatusTextBoxText = "Provided invalid username";
                }
            }
        }

        /// <summary>
        /// Disrupts connection to a server and switches UI elements to Offline mode
        /// </summary>
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

        /// <summary>
        /// Handler for connection ToggleButton command
        /// </summary>
        /// <param name="url">URL of Messenger Server</param>
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

        /// <summary>
        /// Method to invoke MessengerService SendMessage Method
        /// </summary>
        /// <param name="message">Text of message to send</param>
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
        #endregion

        #region UIMethods
        /// <summary>
        /// Checks if provided username if valid
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>Bool - true if username is valid, false if isn't</returns>
        private bool IsUsernameVaild(string username)
        {
            if (username  == null || username.Length==0)
            {
                return false;
            }
            if (!username.All(c => char.IsLetterOrDigit(c)))
            {
                return false;
            }

            if (username.Length > 20)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Call internet API to get machine's public IP. Currently not used, but if hosted on static public IP might be used to show client connection string
        /// </summary>
        /// <returns>Public IP String. 5.15.122.150 For Example</returns>
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

        /// <summary>
        /// Method that tries to recieve local machine's IP address
        /// </summary>
        /// <returns>Private IP as String. 192.168.0.0 for example.</returns>
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
        #endregion

        #region Views Events Handlers
        /// <summary>
        /// Event handler for Messenger Window closing
        /// </summary>
        /// <param name="sender">Messenger Window</param>
        /// <param name="e">Empty EventArgs</param>
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            Disconnect();
        }

        /// <summary>
        /// Event handler for key pressed in Messenger Window
        /// </summary>
        /// <param name="sender">Messenger Window</param>
        /// <param name="e">Empty EventArgs</param>
        public void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage(InputMessage);
            }
        }
        #endregion

        #region IServiceMessengerCallback Implementation
        /// <summary>
        /// IServiceMessengerCallback that triggers when a new message reaches server
        /// </summary>
        /// <param name="message">Text of recieved message</param>
        public void MessageCallback(string message)
        {
            if (message!=null)
            {
                MessagesList.Add(message);
            }
        }

        /// <summary>
        /// IServiceMessengerCallback that triggers when some Active User properly Disconnets from servers
        /// </summary>
        /// <param name="chatMember">Name of client who disconnected</param>
        public void ChatMemberLeftCallback(string chatMember)
        {
            if (chatMember != null && UserList.Contains(chatMember))
            {
                UserList.Remove(chatMember);
            }
        }

        /// <summary>
        /// IServiceMessengerCallback that triggers when new user Connects to the server
        /// </summary>
        /// <param name="chatMember">Name of client who connected</param>
        public void ChatMemberJoinedCallback(string chatMember)
        {
            if (chatMember!=null)
            {
                UserList.Add(chatMember);
            }
        }

        /// <summary>
        /// IServiceMessengerCallback that triggers when server properly shuts off
        /// </summary>
        public void ServerShutDownCallback()
        {
            IsOnline = false;
            UserList.Clear();
            StatusTextBoxText = "Disconnected";
        }
        #endregion
    }
}
