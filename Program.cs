using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace ChiliTester
{
    class Program
    {
        public static DataMaster dataMaster;

        public static string appDirectory;

        static void Main(string[] args)
        {
            string url = "http://www.crowe.chili/5.4/main.asmx";

            ChiliService.mainSoapClient soapClient = Connector.SetupConnection(url);

            string environment = "";
            string testName = environment;

            KeyMaster.GetKey(soapClient, "admin", "admin", environment);

            Dictionary<string, string> documentsToTest = new Dictionary<string, string>()
            {
                

            };

            GenerationTask render = new GenerationTask("", environment, soapClient);
            //render.copyDocument = true;
            //render.savedInEdtiorFalse = true;
            //render.allowServerRendering = true;

            List<GenerationTask> generationTasks = new List<GenerationTask>()
            {
               render
            };


            StressTest stressTest = new StressTest(soapClient, generationTasks, documentsToTest, 1, true, true, true, false, testName);
            
            Console.ReadKey();

        }


        public static List<GenerationTask> ThreeRendering(string id, string environment, ChiliService.mainSoapClient soapClient)
        {


            GenerationTask gen = new GenerationTask(id, environment, soapClient);
            gen.copyDocument = true;

            GenerationTask genS = new GenerationTask(id, environment, soapClient);
            genS.copyDocument = true;
            genS.savedInEdtiorFalse = true;

            GenerationTask genR = new GenerationTask(id, environment, soapClient);
            genR.copyDocument = true;
            genR.allowServerRendering = true;

            return new List<GenerationTask>()
            {
                gen, genS, genR
            };
        }

        public static List<string> GetAllDocumentsInFolder(ChiliService.mainSoapClient soapClient, string parentFolder, int levels = 1)
        {
            List<string> documents = new List<string>();

            string resourceGetTree = soapClient.ResourceGetTreeLevel(KeyMaster.key, "Documents", parentFolder, levels);


            if (resourceGetTree != null)
            {

                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(resourceGetTree);

                GetDocumentsInFolder(xmlDocument.FirstChild, ref documents, 0);
            }

            return documents;
        }

        private static void GetDocumentsInFolder(XmlNode folderNode, ref List<string> documentList, int level)
        {
            foreach (XmlNode node in folderNode.ChildNodes)
            {
                if (node.Attributes["isFolder"] != null && node.Attributes["isFolder"].Value == "false")
                {

                    string id = node.Attributes["id"].Value;

                    documentList.Add(id);

                }
                else
                {
                    GetDocumentsInFolder(node, ref documentList, level + 1);
                }
            }
        }

    
    }
}
