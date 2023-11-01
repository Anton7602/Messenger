using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace MessengerService
{
    [ServiceContract(CallbackContract =typeof(IServerMessengerCallback))]
    public interface IServiceMessenger
    {
        [OperationContract]
        int Connect(string name);

        [OperationContract]
        void Disconnect(int id);
        [OperationContract]
        List<string> GetUsersNamesList();

        [OperationContract(IsOneWay = true)]
        void SendMessage(string message, int id);
    }

    public interface IServerMessengerCallback
    {
        [OperationContract(IsOneWay = true)]
        void ServerShutDownCallback();
        [OperationContract(IsOneWay = true)]
        void ChatMemberLeftCallback(string chatMember);

        [OperationContract(IsOneWay = true)]
        void ChatMemberJoinedCallback(string chatMember);

        [OperationContract(IsOneWay = true)]
        void MessageCallback(string message);

        [OperationContract(IsOneWay = true)]
        void PingCallback();
    }
}
