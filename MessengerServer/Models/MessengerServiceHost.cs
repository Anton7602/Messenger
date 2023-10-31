using MessengerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace MessengerServer
{
    internal class MessengerServiceHost
    {
        private ServiceHost Host { get; set; }
        public ServiceMessenger Messenger = new ServiceMessenger();

        public void startService(string url)
        {
            Uri hostAddress = new Uri($"net.tcp://{url}");
            Host = new ServiceHost(Messenger);
            Host.AddServiceEndpoint(typeof(MessengerService.IServiceMessenger), new NetTcpBinding(), hostAddress);
            Host.Open();
        }

        public void stopService()
        {
            Host.Close();
        }

        public void removeEndpoint()
        {
            ServiceEndpoint endpointToRemove = Host.Description.Endpoints.FirstOrDefault();
            if (endpointToRemove!= null)
            {
                Host.Description.Endpoints.Remove(endpointToRemove);
            }
        }
    }
}
