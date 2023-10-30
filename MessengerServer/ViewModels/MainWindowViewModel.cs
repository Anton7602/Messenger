using MessengerServer.Models;
using MessengerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;

namespace MessengerServer.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {
        #region Fields and Properties
        private MessengerServiceHost host = new MessengerServiceHost();

        private List<User> userList = new List<User>();
        public List<User> UserList 
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

        private int _ipAddressTextBoxTextFirst = 127;
        public int IpAddressTextBoxTextFirst
        {
            get { return _ipAddressTextBoxTextFirst; }
            set
            {
                _ipAddressTextBoxTextFirst = value;
                OnPropertyChanged(nameof(IpAddressTextBoxTextFirst));
            }
        }

        private int _ipAddressTextBoxTextSecond = 0;
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

        private int _ipAddressTextBoxTextForth = 1;
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
            string publicIP = GetPublicIpAddress();
            string[] elementsOfIp = publicIP.Split('.');
            if (elementsOfIp.Length == 4)
            {
                IpAddressTextBoxTextFirst = int.Parse(elementsOfIp[0]);
                IpAddressTextBoxTextSecond = int.Parse(elementsOfIp[1]);
                IpAddressTextBoxTextThird = int.Parse(elementsOfIp[2]);
                IpAddressTextBoxTextForth = int.Parse(elementsOfIp[3]);
            }
            host.Messenger.ServerCallback += MessageCallbackHandler;
            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }

        private void HandleConnection(object url)
        {
            if (!IsOnline)
            {
                StatusTextBoxText = "Establishing connection";
                host.startService(url as string);
                StatusTextBoxText = "Online";
                IsOnline = true;
            }
            else
            {
                StatusTextBoxText = "Disrupting connection";
                host.stopService();
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

        private static string GetPublicIpAddress()
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    string response = webClient.DownloadString("https://api.ipify.org");
                    return response.Trim();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving public IP address: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
