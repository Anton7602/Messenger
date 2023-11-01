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
    /// <summary>
    /// MessengerService Host holder. Provides access to Service methods and Host parameters
    /// </summary>
    internal class MessengerServiceHost
    {
        private ServiceHost Host { get; set; }
        public ServiceMessenger Messenger = new ServiceMessenger();

        /// <summary>
        /// Opens Hosting of Messenger Service on a provided IP:Port
        /// </summary>
        /// <param name="url">IP:Port to host service. 192.168.0.0:7602 for example</param>
        public void startService(string url)
        {
            Uri hostAddress = new Uri($"net.tcp://{url}");
            Host = new ServiceHost(Messenger);
            Host.AddServiceEndpoint(typeof(MessengerService.IServiceMessenger), new NetTcpBinding(), hostAddress);
            Host.Open();
        }

        /// <summary>
        /// Closes Hosting of Messenger Service
        /// </summary>
        public void stopService()
        {
            if (Host!=null && Host.State == CommunicationState.Opened)
            {
                Host.Close();
                removeEndpoint();
            }
        }

        /// <summary>
        /// Removes endpoint from ServiceHost. Called when hosting is closed or failed to open
        /// </summary>
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
