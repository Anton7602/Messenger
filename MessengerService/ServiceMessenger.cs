using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MessengerService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceMessenger : IServiceMessenger
    {
        public List<User> userList = new List<User>();
        public event EventHandler<Message> ServerCallback;
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
            SendMessage(user.Name + " is Connected", 0);
            userList.Add(user);
            return user.ID;
        }

        public void Disconnect(int id)
        {
            var user = userList.FirstOrDefault(x => x.ID == id);
            if (user != null)
            {
                userList.Remove(user);
                SendMessage(user.Name + " is Disconnected", 0);
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
            ServerCallback?.Invoke(this, message);
            foreach(User reciever in userList)
            {
                reciever.UserOperationContext.GetCallbackChannel<IServerMessengerCallback>().MessageCallback(message.ToString());
            }
        }
    }
}
