using MessengerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
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
            Uri hostAddress = new Uri($"net.tcp://{url}/ServiceMessenger");
            Host = new ServiceHost(Messenger, hostAddress);
            Host.AddServiceEndpoint(typeof(MessengerService.IServiceMessenger), new NetTcpBinding(), "");
            try
            {
                Host.Open();
            } catch (Exception ex)
            {
            }
        }

        public void stopService()
        {
            Host.Close();
        }
    }
}
