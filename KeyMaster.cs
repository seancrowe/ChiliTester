using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChiliTester
{

    class KeyMaster
    {
        private DataMaster dataMaster;

        private ChiliService.mainSoapClient soapClient;

        public KeyMaster(ChiliService.mainSoapClient soapClient)
        {
            //this.dataMaster = dataMaster;
            //this.soapClient = soapClient;

            
        }

        public static string key;

        public static string GetKey(ChiliService.mainSoapClient soapClient, string username, string password)
        {

            if (key == null || key == "")
            {
                string keyXml = soapClient.GenerateApiKey("Admin", username, password);

                XmlDocument xmlDocument = new XmlDocument();

                xmlDocument.LoadXml(keyXml);

                if (xmlDocument.FirstChild.Attributes["key"] != null)
                {
                    key = xmlDocument.FirstChild.Attributes["key"].Value.ToString();
                }
            }

            return key;
        }

        //public GetKey()
        //{
        //    dataMaster.GetData("");
        //}

        /*public bool IsKeyGood(bool serverCheck = false)
        {
            string apiKey = Program.dataMaster.GetData("apiKey");
            string validTill = Program.dataMaster.GetData("keyValidDate");

            if (apiKey != "")
            {

                if (keyCreationDate != "")
                {
                    long.TryParse(keyCreationDate, out long creationDate);

                    int hoursSince = DateTime.Now.Subtract(DateTime.FromFileTime(creationDate)).Hours;

                    if (hoursSince > 2)
                    {
                        serverCheck = true;
                    }
                }

                if (serverCheck == true)
                {
                    soapClient.ApiKeyGetCurrentSettings(apiKey);
                }
            }*/

            

        

    }
}
