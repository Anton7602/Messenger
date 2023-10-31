using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace MessengerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceMessenger : IServiceMessenger
    {
        private enum MessengerEvent { MessageSent, ChatMemberJoined, ChatMemberLeft }
        public List<User> userList = new List<User>();
        public event EventHandler<Message> ServerMessageCallback;
        public event EventHandler<User> ServerUserAddedCallback;
        public event EventHandler<User> ServerUserRemovedCallback;
        int currentID = 1;

        public int Connect(string name)
        {
            User user = new User()
            {
                ID = currentID,
                Name = name,
                UserOperationContext = OperationContext.Current
            };
            currentID++;
            SendMessage(user.Name + " has Connected", 0);
            ServerUserAddedCallback?.Invoke(this, user);
            NotifyClientsAboutEvent(MessengerEvent.ChatMemberJoined, user);
            userList.Add(user);
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = userList.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                userList.Remove(user);
                ServerUserRemovedCallback?.Invoke(this, user);
                SendMessage(user.Name + " has Disconnected", 0);
                NotifyClientsAboutEvent(MessengerEvent.ChatMemberLeft, user);
            }
        }

        public void SendMessage(string messageText, int id)
        {
            var sender = userList.FirstOrDefault(x => x.ID == id);
            string senderName;
            if (sender!=null)
            {
                senderName = sender.Name;
            } else
            {
                senderName = "Server";
            }
            Message message = new Message(senderName, DateTime.Now, messageText);
            ServerMessageCallback?.Invoke(this, message);
            NotifyClientsAboutEvent(MessengerEvent.MessageSent, message);
        }

        public List<string> GetUsersList()
        {
            List<string> list = new List<string>();
            foreach(User user in userList)
            {
                list.Add(user.ToString());
            }
            return list;
        }

        private void DisconnectUnresponsiveUsers(List<User> unresponsiveUsers)
        {
            foreach (User disconnectedUser in unresponsiveUsers)
            {
                Disconnect(disconnectedUser.ID);
                SendMessage("Lost connection with " + disconnectedUser.Name, 0);
                ServerUserRemovedCallback?.Invoke(this, disconnectedUser);
            }
        }

        private void NotifyClientsAboutEvent(MessengerEvent notificationEvent, Object parameter) {
            //List<User> unresponsiveUsers = new List<User>();
            foreach (User client in userList)
            {
                try
                {
                    switch (notificationEvent)
                    {
                        case (MessengerEvent.MessageSent):
                            client.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().MessageCallback(parameter.ToString());
                            break;
                        case (MessengerEvent.ChatMemberJoined):
                            client.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().ChatMemberJoinedCallback(parameter.ToString());
                            break;
                        case (MessengerEvent.ChatMemberLeft):
                            client.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().ChatMemberLeftCallback(parameter.ToString());
                            break;
                    }
                }
                catch
                {
                    //unresponsiveUsers.Add(client);
                }
            }
            //DisconnectUnresponsiveUsers(unresponsiveUsers);
        }
    }
}
