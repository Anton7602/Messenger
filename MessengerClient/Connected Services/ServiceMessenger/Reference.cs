﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MessengerClient.ServiceMessenger {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceMessenger.IServiceMessenger", CallbackContract=typeof(MessengerClient.ServiceMessenger.IServiceMessengerCallback))]
    public interface IServiceMessenger {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/Connect", ReplyAction="http://tempuri.org/IServiceMessenger/ConnectResponse")]
        int Connect(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/Connect", ReplyAction="http://tempuri.org/IServiceMessenger/ConnectResponse")]
        System.Threading.Tasks.Task<int> ConnectAsync(string name);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/Disconnect", ReplyAction="http://tempuri.org/IServiceMessenger/DisconnectResponse")]
        void Disconnect(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/Disconnect", ReplyAction="http://tempuri.org/IServiceMessenger/DisconnectResponse")]
        System.Threading.Tasks.Task DisconnectAsync(int id);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/GetUsersNamesList", ReplyAction="http://tempuri.org/IServiceMessenger/GetUsersNamesListResponse")]
        string[] GetUsersNamesList();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IServiceMessenger/GetUsersNamesList", ReplyAction="http://tempuri.org/IServiceMessenger/GetUsersNamesListResponse")]
        System.Threading.Tasks.Task<string[]> GetUsersNamesListAsync();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/SendMessage")]
        void SendMessage(string message, int id);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/SendMessage")]
        System.Threading.Tasks.Task SendMessageAsync(string message, int id);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceMessengerCallback {
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/ServerShutDownCallback")]
        void ServerShutDownCallback();
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/ChatMemberLeftCallback")]
        void ChatMemberLeftCallback(string chatMember);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/ChatMemberJoinedCallback")]
        void ChatMemberJoinedCallback(string chatMember);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/MessageCallback")]
        void MessageCallback(string message);
        
        [System.ServiceModel.OperationContractAttribute(IsOneWay=true, Action="http://tempuri.org/IServiceMessenger/PingCallback")]
        void PingCallback();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IServiceMessengerChannel : MessengerClient.ServiceMessenger.IServiceMessenger, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class ServiceMessengerClient : System.ServiceModel.DuplexClientBase<MessengerClient.ServiceMessenger.IServiceMessenger>, MessengerClient.ServiceMessenger.IServiceMessenger {
        
        public ServiceMessengerClient(System.ServiceModel.InstanceContext callbackInstance) : 
                base(callbackInstance) {
        }
        
        public ServiceMessengerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName) : 
                base(callbackInstance, endpointConfigurationName) {
        }
        
        public ServiceMessengerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceMessengerClient(System.ServiceModel.InstanceContext callbackInstance, string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, endpointConfigurationName, remoteAddress) {
        }
        
        public ServiceMessengerClient(System.ServiceModel.InstanceContext callbackInstance, System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(callbackInstance, binding, remoteAddress) {
        }
        
        public int Connect(string name) {
            return base.Channel.Connect(name);
        }
        
        public System.Threading.Tasks.Task<int> ConnectAsync(string name) {
            return base.Channel.ConnectAsync(name);
        }
        
        public void Disconnect(int id) {
            base.Channel.Disconnect(id);
        }
        
        public System.Threading.Tasks.Task DisconnectAsync(int id) {
            return base.Channel.DisconnectAsync(id);
        }
        
        public string[] GetUsersNamesList() {
            return base.Channel.GetUsersNamesList();
        }
        
        public System.Threading.Tasks.Task<string[]> GetUsersNamesListAsync() {
            return base.Channel.GetUsersNamesListAsync();
        }
        
        public void SendMessage(string message, int id) {
            base.Channel.SendMessage(message, id);
        }
        
        public System.Threading.Tasks.Task SendMessageAsync(string message, int id) {
            return base.Channel.SendMessageAsync(message, id);
        }
    }
}
