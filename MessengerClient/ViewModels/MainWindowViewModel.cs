using MessengerClient.ServiceMessenger;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MessengerClient.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase, IServiceMessengerCallback
    {
        #region Fields and Properties
        private BackgroundWorker backgroundWorker;
        private int durationBetweenPings = 2; //minutes
        private Timer serverPingTimer;
        private bool isConnecting = false;
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

        private string _ipAddressTextBoxTextFirst = "192";
        public string IpAddressTextBoxTextFirst
        {
            get { return _ipAddressTextBoxTextFirst; }
            set
            {
                _ipAddressTextBoxTextFirst = ValidateIPOctetInput(value, _ipAddressTextBoxTextFirst);
                OnPropertyChanged(nameof(IpAddressTextBoxTextFirst));
            }
        }

        private string _ipAddressTextBoxTextSecond = "192";
        public string IpAddressTextBoxTextSecond
        {
            get { return _ipAddressTextBoxTextSecond; }
            set
            {
                _ipAddressTextBoxTextSecond = ValidateIPOctetInput(value, _ipAddressTextBoxTextSecond);
                OnPropertyChanged(nameof(IpAddressTextBoxTextSecond));
            }
        }

        private string _ipAddressTextBoxTextThird = "0";
        public string IpAddressTextBoxTextThird
        {
            get { return _ipAddressTextBoxTextThird; }
            set
            {
                _ipAddressTextBoxTextThird = ValidateIPOctetInput(value, _ipAddressTextBoxTextThird);
                OnPropertyChanged(nameof(IpAddressTextBoxTextThird));
            }
        }

        private string _ipAddressTextBoxTextForth = "0";
        public string IpAddressTextBoxTextForth
        {
            get { return _ipAddressTextBoxTextForth; }
            set
            {
                _ipAddressTextBoxTextForth = ValidateIPOctetInput(value, _ipAddressTextBoxTextForth);
                OnPropertyChanged(nameof(IpAddressTextBoxTextForth));
            }
        }

        private string _portTextBoxText = "7602";
        public string PortTextBoxText
        {
            get { return _portTextBoxText; }
            set
            {
                _portTextBoxText = ValidatePortInput(value,_portTextBoxText);
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
        private string connectionURL;

        public RelayCommand<object> connectionCommand { get; private set; }
        public RelayCommand<object> sendMessage { get; private set; }
        #endregion

        #region ViewModels Constructors

        public MainWindowViewModel()
        {
            //Attempt to prefill proper IP
            string relevantIP = GetPrivateIpAddress();
            string[] elementsOfIp = relevantIP.Split('.');
            if (elementsOfIp.Length == 4)
            {
                IpAddressTextBoxTextFirst = elementsOfIp[0];
                IpAddressTextBoxTextSecond = elementsOfIp[1];
                IpAddressTextBoxTextThird = elementsOfIp[2];
                IpAddressTextBoxTextForth = elementsOfIp[3];
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
                if (IsUsernameVaild(UserNameTextBoxText))
                {
                    isConnecting = true;
                    StatusTextBoxText = "Establishing connection";
                    client = new ServiceMessengerClient(new System.ServiceModel.InstanceContext(this));
                    Uri connectAddress = new Uri($"net.tcp://{url}");
                    EndpointIdentity identity = EndpointIdentity.CreateUpnIdentity("DESKTOP-BTRBT9G\\Антон");
                    client.Endpoint.Address = new System.ServiceModel.EndpointAddress(connectAddress, identity);
                    try
                    {
                        backgroundWorker = new BackgroundWorker();
                        backgroundWorker.DoWork += backgroundWorker_Connect;
                        backgroundWorker.RunWorkerCompleted += backgroundWorker_ConnectionCompleted;
                        backgroundWorker.RunWorkerAsync();
                    }
                    catch (Exception ex)
                    {
                        StatusTextBoxText = "Connection failed " + ex.Message;
                        isConnecting = false;
                    }
                } else
                {
                    StatusTextBoxText = "Provided invalid username";
                    isConnecting = false;
                }
            }
        }

        private void backgroundWorker_Connect(object sender, DoWorkEventArgs e)
        {
            ID = client.Connect(UserNameTextBoxText);
        }

        private void backgroundWorker_ConnectionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ID > 0)
            {
                serverPingTimer = new Timer(serverPingCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(durationBetweenPings));
                IsOnline = true;
                StatusTextBoxText = "Connected";
                UserList.Clear();
                foreach (var chatMember in client.GetUsersNamesList())
                {
                    _userList.Add(chatMember);
                }
            }
            else
            {
                StatusTextBoxText = "Username already used on server";
            }
            isConnecting = false;
        }

        /// <summary>
        /// Disrupts connection to a server and switches UI elements to Offline mode
        /// </summary>
        private void Disconnect()
        {
            if (IsOnline)
            {
                StatusTextBoxText = "Disrupting connection";
                if (serverPingTimer != null)
                {
                    serverPingTimer.Dispose();
                }
                try
                {
                    client.Disconnect(ID);
                    StatusTextBoxText = "Disconnected";
                }
                catch 
                {
                    StatusTextBoxText = "Disconnected with errors";
                }
                UserList.Clear();
                IsOnline = false;
                isConnecting = false;
            }
        }

        /// <summary>
        /// Method triggers by serverPingTimer and calls the server to check if connection is still online
        /// </summary>
        private void serverPingCallback(object status)
        {
            try
            {
                client.GetUsersNamesList();
            }
            catch
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Disconnect();
                    StatusTextBoxText = "Server stopped responding";
                });
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
                    try
                    {
                        client.SendMessage(text, ID);
                        InputMessage = String.Empty;

                    }
                    catch
                    {
                        Disconnect();
                        StatusTextBoxText = "Failed to send message";
                    }
                }
            }
        }
        #endregion

        #region UIMethods
        /// <summary>
        /// Handler for connection ToggleButton command
        /// </summary>
        /// <param name="url">URL of Messenger Server</param>
        private void HandleConnection(object url)
        {
            if (!isConnecting)
            {
                isConnecting = true;
                if (!IsOnline)
                {
                    Connect(url.ToString());
                }
                else
                {
                    Disconnect();
                }
            }
        }

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

        /// <summary>
        /// Method used to valided inputs on IP's textboxes. Prevents Ip values to go anywhere outside of range 1..255
        /// </summary>
        /// <returns>Valid IP octet value in range 0..255</returns>
        private static string ValidateIPOctetInput(string textInput, string currentValue)
        {
            if (textInput.Length>3)
            {
                return currentValue;
            }
            if (int.TryParse(textInput, out int numInput))
            {
                if (numInput > 255)
                {
                    return "255";
                }
                if (numInput < 0)
                {
                    return "";
                }
                if (currentValue.Equals("0") && textInput.Length == 2)
                {
                    return textInput.Remove(textInput.IndexOf('0'), 1);
                }
                return textInput;
            }
            return currentValue;
        }

        /// <summary>
        /// Method used to valided inputs on port textbox. Prevents port value to go anywhere outside of range 1..65535
        /// </summary>
        /// <returns>Valid port value in range 0..65535</returns>
        private static string ValidatePortInput(string textInput, string currentValue)
        {
            if (int.TryParse(textInput, out int numInput))
            {
                if (numInput > 65535)
                {
                    return "65535";
                }
                if (numInput < 0)
                {
                    return "";
                }
                if (currentValue.Equals("0") && textInput.Length == 2)
                {
                    return textInput.Remove(textInput.IndexOf('0'), 1);
                }
                return textInput;
            }
            return currentValue;
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
            Disconnect();
            StatusTextBoxText = "Server went offline";
        }

        /// <summary>
        /// IServiceMessengerCallback that triggers when server pings this client
        /// </summary>
        public void PingCallback()
        {
        }
        #endregion
    }
}
