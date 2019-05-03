using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ChiliTester.ChiliService;

namespace ChiliTester
{
    class GenerationTask
    {
        public string pdfSettingsId;
        public string environment;

        public bool savedInEdtiorFalse = false;
        public bool makeSureSavedInEditorIsTrue = false;
        public bool allowServerRendering = false;
        public bool copyDocument = false;
        public Dictionary<string, string> variables = new Dictionary<string, string>();

        public string pdfExportSettings;

        private mainSoapClient soapClient;

        public GenerationTask(string pdfSettingsId, string environment, mainSoapClient mainSoap)
        {
            this.pdfSettingsId = pdfSettingsId;
            this.environment = environment;
            this.soapClient = mainSoap;

            GetKey();
            soapClient.SetWorkingEnvironment(key, environment);

            pdfExportSettings = soapClient.ResourceItemGetXML(GetKey(), "PDFExportSettings", pdfSettingsId);


        }

        public string GetVariablesXml()
        {

            if (savedInEdtiorFalse == true || allowServerRendering == true || variables.Count > 0)
            {
                XmlDocument variablesXml = new XmlDocument();

                variablesXml.LoadXml("<variables></variables>");

                if (savedInEdtiorFalse == true || allowServerRendering == true)
                {
                    XmlAttribute xmlAttribute = variablesXml.CreateAttribute("savedInEditor");
                    xmlAttribute.Value = "false";

                    variablesXml.FirstChild.Attributes.Append(xmlAttribute);


                }

                if (allowServerRendering == true)
                {
                    XmlAttribute xmlAttribute = variablesXml.CreateAttribute("allowServerRendering");
                    xmlAttribute.Value = "true";

                    variablesXml.FirstChild.Attributes.Append(xmlAttribute);
                }

                if (makeSureSavedInEditorIsTrue == true)
                {
                    XmlAttribute xmlAttribute = null;
                    
                    if (variablesXml.Attributes["savedInEditor"] != null)
                    {
                        xmlAttribute = variablesXml.Attributes["savedInEditor"];
                    }
                    else
                    {
                        xmlAttribute = variablesXml.CreateAttribute("savedInEditor");
                        variablesXml.FirstChild.Attributes.Append(xmlAttribute);
                    }

                    xmlAttribute.Value = "false";
                }

                if (variables.Count > 0)

                //<item name="Cllr1_Fname" displayName="Firstname" type="" value="SeanIsAwesome" displayValue="SeanIsAwesome" /></variables>
                {
                    foreach (KeyValuePair<string, string> variable in variables)
                    {
                        XmlElement variableTag = variablesXml.CreateElement("item");
                        variableTag.SetAttribute("name", variable.Key);
                        variableTag.SetAttribute("displayName", variable.Key);
                        variableTag.SetAttribute("value", variable.Value);
                        variableTag.SetAttribute("displayValue", variable.Value);
                        variablesXml.FirstChild.AppendChild(variableTag);
                    }
                }

                return variablesXml.OuterXml;
            }

            return "";
        }

        private string key;

        private string GetKey()
        {
            key = KeyMaster.key;
            return key;
        }

    }
}
