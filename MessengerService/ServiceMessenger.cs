using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;

namespace MessengerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceMessenger : IServiceMessenger
    {
        private enum MessengerEvent { MessageSent, ChatMemberJoined, ChatMemberLeft, ServerShutDown }
        private int currentID = 1;
        private List<User> userList = new List<User>();
        public event EventHandler<Message> ServerMessageCallback;
        public event EventHandler<User> ServerUserAddedCallback;
        public event EventHandler<User> ServerUserRemovedCallback;

        /// <summary>
        /// Handles connection of new user with provided Name
        /// </summary>
        /// <param name="name">Username</param>
        /// <returns>Generated user ID</returns>
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

        public void pingUsers(List<User> usersToPing)
        {
            List<User> unresponsiveUsers = new List<User>();
            foreach(User client in usersToPing)
            {
                try
                {
                    client.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().PingCallback();
                }
                catch
                {
                    unresponsiveUsers.Add(client);
                }
            }
            DisconnectUnresponsiveUsers(unresponsiveUsers);
        }

        /// <summary>
        /// Handles disconnection of user with provided userID
        /// </summary>
        /// <param name="id">User's ID</param>
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

        /// <summary>
        /// Goes through the userList and force disconnects all clients from server
        /// </summary>
        public void DisconnectAllClients()
        {
            List<User> tempUsers = new List<User>(userList);
            foreach (var user in tempUsers)
            {
                Disconnect(user.ID);
            }
        }

        /// <summary>
        /// Goes through provided list of users and force disconnects all of them
        /// </summary>
        /// <param name="unresponsiveUsers"></param>
        private void DisconnectUnresponsiveUsers(List<User> unresponsiveUsers)
        {
            foreach (User disconnectedUser in unresponsiveUsers)
            {
                Disconnect(disconnectedUser.ID);
                SendMessage("Lost connection with " + disconnectedUser.Name, 0);
                ServerUserRemovedCallback?.Invoke(this, disconnectedUser);
            }
        }

        /// <summary>
        /// Handles sending message from user with provided ID
        /// </summary>
        /// <param name="messageText">Message's text</param>
        /// <param name="id">Sender ID</param>
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

        /// <summary>
        /// Handles server shutdown Notifies all clients about it
        /// </summary>
        public void ServiceWrapUp()
        {
            NotifyClientsAboutEvent(MessengerEvent.ServerShutDown, String.Empty);
            DisconnectAllClients();
        }

        /// <summary>
        /// Goes through Users List and triggers proper callback depending on event
        /// </summary>
        /// <param name="notificationEvent">Messanger Event</param>
        /// <param name="parameter">Message or Joined/Left User string</param>
        private void NotifyClientsAboutEvent(MessengerEvent notificationEvent, Object parameter) {
            List<User> unresponsiveUsers = new List<User>();
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
                        case (MessengerEvent.ServerShutDown):
                            client.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().ServerShutDownCallback();
                            break;
                    }
                }
                catch
                {
                    unresponsiveUsers.Add(client);
                }
            }
            DisconnectUnresponsiveUsers(unresponsiveUsers);
        }

        /// <summary>
        /// Return users list
        /// </summary>
        /// <returns>List of users names</returns>
        public List<string> GetUsersNamesList()
        {
            List<string> list = new List<string>();
            foreach (User user in userList)
            {
                list.Add(user.ToString());
            }
            return list;
        }

        /// <summary>
        /// Return users list
        /// </summary>
        /// <returns>List of User objects</returns>
        public List<User> GetUsersList()
        {
            return userList;
        }
    }
}
