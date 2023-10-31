using MessengerClient.ServiceMessenger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

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

        private int _ipAddressTextBoxTextForth = 140;
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

        public string CurrentURL => IpAddressTextBoxTextFirst + "." + IpAddressTextBoxTextSecond + "." + IpAddressTextBoxTextThird + "." + IpAddressTextBoxTextForth + ":" + PortTextBoxText;

        public RelayCommand<object> connectionCommand { get; private set; }
        public RelayCommand<object> sendMessage { get; private set; }

        public MainWindowViewModel()
        {
            connectionCommand = new RelayCommand<object>(HandleConnection);
            sendMessage = new RelayCommand<object>(SendMessage);
        }

        private void HandleConnection(object url)
        {
            if (!IsOnline)
            {
                client = new ServiceMessengerClient(new System.ServiceModel.InstanceContext(this));
                Uri connectAddress = new Uri($"net.tcp://{url}");
                client.Endpoint.Address = new System.ServiceModel.EndpointAddress(connectAddress);
                ID = client.Connect(UserNameTextBoxText);
                IsOnline = true;
                StatusTextBoxText = "Online";
                UserList.Clear();
                foreach(var chatMember in client.GetUsersList())
                {
                    userList.Add(chatMember);
                }
            } else
            {
                client.Disconnect(ID);
                IsOnline = false;
                UserList.Clear();
                StatusTextBoxText = "Offline";
            }

        }

        private void SendMessage(object message)
        {
            if (IsOnline)
            {
                String text = message as String;
                if (!text.Equals(String.Empty))
                {
                    client.SendMessage(text, ID);
                    InputMessage = String.Empty;
                }
            }
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
    }
}
