using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ServiceModel;

namespace ChiliTester
{
    public static class Connector
    {

        public static ChiliService.mainSoapClient SetupConnection(string url)
        {
            if (System.Net.ServicePointManager.SecurityProtocol == (SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls))
            {
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            }

            // Stop 417 exception
            System.Net.ServicePointManager.Expect100Continue = false;

            bool connectionGood = TestConnection(url);

            if (connectionGood)
            {
                EndpointAddress endpointAddress = new EndpointAddress(url);

                if (url.Contains("https"))
                {
                    BasicHttpsBinding basicHttpsBinding = new BasicHttpsBinding();
                    basicHttpsBinding.MaxReceivedMessageSize = int.MaxValue;
                    basicHttpsBinding.OpenTimeout = new TimeSpan(0, 20, 0);
                    basicHttpsBinding.CloseTimeout = new TimeSpan(0, 20, 0);
                    basicHttpsBinding.SendTimeout = new TimeSpan(0, 20, 0);
                    basicHttpsBinding.ReceiveTimeout = new TimeSpan(0, 20, 0);
                    return new ChiliService.mainSoapClient(basicHttpsBinding, endpointAddress);
                }
                else
                {
                    BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
                    basicHttpBinding.MaxReceivedMessageSize = int.MaxValue;
                    basicHttpBinding.OpenTimeout = new TimeSpan(0, 20, 0);
                    basicHttpBinding.CloseTimeout = new TimeSpan(0, 20, 0);
                    basicHttpBinding.SendTimeout = new TimeSpan(0, 20, 0);
                    basicHttpBinding.ReceiveTimeout = new TimeSpan(0, 20, 0);
                    return new ChiliService.mainSoapClient(basicHttpBinding, endpointAddress);
                }
            }

            return null;
        }

        private static bool TestConnection(string url)
        {
            return true;

            using (WebClient webClient = new WebClient())
            {
                try
                {
                    webClient.OpenRead(url);
                    return true;
                }
                catch (WebException e)
                {
                    return false;
                }
            }

        }
    }
}
