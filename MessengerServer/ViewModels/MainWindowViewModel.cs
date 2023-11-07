using MessengerServer.Models;
using MessengerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace MessengerServer.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Fields and Properties
        private readonly MessengerServiceHost Host = new MessengerServiceHost();
        private BackgroundWorker backgroundWorker;
        private int durationBetweenPings = 1; // Minutes
        private Timer pingTimer;
        private bool isConnecting = false;

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

        private string _ipAddressTextBoxTextSecond = "168";
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
                _portTextBoxText = ValidatePortInput(value, _portTextBoxText);
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

        private string _inputMessage = String.Empty;
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

        #region ViewModels Constructor
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
                IpAddressTextBoxTextFirst = elementsOfIp[0];
                IpAddressTextBoxTextSecond = elementsOfIp[1];
                IpAddressTextBoxTextThird = elementsOfIp[2];
                IpAddressTextBoxTextForth = elementsOfIp[3];
            }
            Host.Messenger.ServerUserAddedCallback += UserAddedCallbackHandler;
            Host.Messenger.ServerUserRemovedCallback += UserRemovedCallbackHandler;
            Host.Messenger.ServerMessageCallback += MessageCallbackHandler;
            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }
        #endregion

        #region MessengerService
        /// <summary>
        /// Checks if url is valid and if is - tries to host MessengerService on it. Updates UI to online state
        /// </summary>
        /// <param name="url">IP:Port to host Service</param>
        public void Connect(string url)
        {
            if (!IsOnline)
            {
                StatusTextBoxText = "Starting server";
                if (IsValidIP(url as string))
                {
                    try
                    {
                        Host.startService(url as string);
                        pingTimer = new Timer(pingTimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(durationBetweenPings));
                        StatusTextBoxText = "Online";
                        IsOnline = true;
                    }
                    catch (System.ServiceModel.AddressAlreadyInUseException)
                    {
                        StatusTextBoxText = "Provided port is occupied";
                        Host.removeEndpoint();
                    }
                    catch (Exception ex)
                    {
                        StatusTextBoxText = ex.Message;
                        Host.removeEndpoint();
                    }
                }
                else
                {
                    StatusTextBoxText = "Provided invalid IP/Port";
                }
                isConnecting = false;
            }
        }

        /// <summary>
        /// Call Service shut down and updates UI to offline state
        /// </summary>
        public void Disconnect()
        {
            StatusTextBoxText = "Closing server";
            if (pingTimer!=null)
            {
                pingTimer.Dispose();
            }
            Host.Messenger.ServiceWrapUp();
            backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_Disconnect;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_DisconnectionCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorker_Disconnect(object sender, DoWorkEventArgs e)
        {
            Host.stopService();
        }

        private void BackgroundWorker_DisconnectionCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _userList.Clear();
            StatusTextBoxText = "Offline";
            IsOnline = false;
            isConnecting = false;
        }

        /// <summary>
        /// Method triggers by pingTimer and calls ping callback from all users to check if everyone still connected
        /// </summary>
        /// <param name="state"></param>
        private void pingTimerCallback(object state)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Host.Messenger.pingUsers(Host.Messenger.GetUsersList());
            });
        }

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
                    Host.Messenger.SendMessage(text, 0);
                    InputMessage = String.Empty;
                }
            }
        }
        #endregion

        #region Service Events Handlers
        /// <summary>
        /// Handles event when new message reaches the server. Saves recieved message to SQL Database
        /// </summary>
        /// <param name="sender">MessengerService</param>
        /// <param name="e">Message object</param>
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

        /// <summary>
        /// Handles event whan new client connects to the server
        /// </summary>
        /// <param name="sender">MessengerService</param>
        /// <param name="e">User object</param>
        private void UserAddedCallbackHandler(object sender, EventArgs e)
        {
            if (e!=null && e is User)
            {
                _userList.Add((e as User).ToString());
            }
        }

        /// <summary>
        /// Handles event when client disconnects (properly) from the server
        /// </summary>
        /// <param name="sender">MessengerService</param>
        /// <param name="e">User object</param>
        private void UserRemovedCallbackHandler(object sender, EventArgs e)
        {
            if (e != null && e is User)
            {
                _userList.Remove((e as User).ToString());
            }
        }
        #endregion

        #region Views Events Handlers
        /// <summary>
        /// Event handler for Messenger Window closing
        /// </summary>
        /// <param name="sender">Messenger Window</param>
        /// <param name="e">Empty EventArgs</param>
        public void OnWindowClosing(object sender, EventArgs e)
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

        #region IP Helpers Methods
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
        /// Tries to get machine's private IP to prefill IP TextBoxes
        /// </summary>
        /// <returns>Private IP. 192.168.0.1 for example</returns>
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
        /// Validates provided string to be a proper IP:Port
        /// </summary>
        /// <param name="ipAddress">IP:Port</param>
        /// <returns>Bool - true if string fit IP:port criteria, false if not.</returns>
        private static bool IsValidIP(string ipAddress)
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

        /// <summary>
        /// Method used to valided inputs on IP's textboxes. Prevents Ip values to go anywhere outside of range 1..255
        /// </summary>
        /// <returns>Valid IP octet value in range 0..255</returns>
        private static string ValidateIPOctetInput(string textInput, string currentValue)
        {
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
                if (currentValue.Equals("0") && textInput.Length==2)
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
    }
}
