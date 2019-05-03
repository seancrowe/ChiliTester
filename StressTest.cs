using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ChiliTester
{
    class StressTest
    {
        private Dictionary<string, string> documentsToTest = new Dictionary<string, string>();

        ChiliService.mainSoapClient soapClient;

        public bool downloadPdf;
        public bool includeUrls;
        public bool showErrors;
        public bool showCopyId;
        public bool finalErrorReport;
        public string currentDirectory;

        public StressTest(ChiliService.mainSoapClient soapClient, List<GenerationTask> generationTasks, Dictionary<string, string> documentsToTest = null, int batchAmount = 1, bool includeUrls = false, bool downloadPdf = true, bool showErrors = false, bool finalErrorReport = true, string testHeader = "")
        {
            this.soapClient = soapClient;
            this.key = GetKey();
            this.downloadPdf = downloadPdf;
            this.includeUrls = includeUrls;
            this.showErrors = showErrors;
            this.finalErrorReport = finalErrorReport;

            if (documentsToTest != null)
            {
                this.documentsToTest = documentsToTest;
            }

            currentDirectory = $"{Directory.GetCurrentDirectory()}\\tests\\{DateTime.Now.Month}-{DateTime.Now.Day} at {DateTime.Now.Hour}_{DateTime.Now.Minute}";

            if (testHeader != "")
            {
                currentDirectory += " " + testHeader + "\\";
            }


            Directory.CreateDirectory(currentDirectory);
            Directory.CreateDirectory(currentDirectory + "\\pdfs\\");

            foreach (KeyValuePair<string, string> documentIdEnv in documentsToTest)
            {
                string environment = documentIdEnv.Value;
                string id = documentIdEnv.Key;


                foreach (GenerationTask generationTask in generationTasks)
                {

                    RunTest(id, environment, batchAmount, generationTask);

                }

            }


        }

        private void RunTest(string documentId, string environment, int batchAmount, GenerationTask generationTask)
        {
            string apiKey = GetKey();

            soapClient.SetWorkingEnvironment(key, environment);

            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(soapClient.ResourceItemGetDefinitionXML(apiKey, "Documents", documentId));

            string name = xmlDocument.FirstChild.Attributes["name"].Value.ToString();

            //Chili version
            xmlDocument.LoadXml(soapClient.ServerGetSettings(apiKey));
            string version = xmlDocument.FirstChild.Attributes["version"].Value;

            WriteTestLog("CHILI Version: " + version, name);


            WriteTestLog(Environment.NewLine + "Testing: " + name, name);
            WriteTestLog("ID: " + documentId, name);
            WriteTestLog("Environment: " + environment, name);

            //Get PDF Settings Name
            xmlDocument.LoadXml(generationTask.pdfExportSettings);
            string pdfName = xmlDocument.FirstChild.Attributes["name"].Value;
            WriteTestLog("", name);
            WriteTestLog("---------------------------", name);
            WriteTestLog("PDF Settings: " + pdfName, name);
            WriteTestLog("Is Copy: " + generationTask.copyDocument, name);
            WriteTestLog("SIE False: " + (generationTask.savedInEdtiorFalse || generationTask.allowServerRendering), name);
            WriteTestLog("Rendering: " + generationTask.allowServerRendering, name);
            WriteTestLog("", name);
            WriteTestLog("---------------------------", name);
            WriteTestLog("Batch Amount: " + batchAmount, name);

            Console.WriteLine("Testing " + name + " using " + pdfName);

            double startTime = DateTime.Now.ToOADate();

            List<string> taskIds = new List<string>();

            // Queue the tasks
            while (batchAmount > 0)
            {
                batchAmount--;

                string tempDocId = documentId;

                if (generationTask.copyDocument == true)
                {
                    string copyPath = $"Chili-STCopiesQuad/{DateTime.Now.Month}-{DateTime.Now.Day}/{tempDocId.Substring(0, 6)}";
                    xmlDocument.LoadXml(soapClient.ResourceItemCopy(apiKey, "Documents", tempDocId, name, copyPath));
                    tempDocId = xmlDocument.FirstChild.Attributes["id"].Value;

                    if (showCopyId == true)
                    {
                        WriteTestLog("Copy ID: " + tempDocId, name);
                    }
                }

                string variablesXml = generationTask.GetVariablesXml();

                if (variablesXml != "")
                {
                    try {
                        soapClient.DocumentSetVariableValues(apiKey, tempDocId, variablesXml);

                    }
                    catch (Exception e)
                    {
                        WriteTestLog("SOAP ERROR: " + e.Message, name);
                    }
                }


                //Temp
                //soapClient.DocumentSetDataSource(key, tempDocId, File.ReadAllText(Directory.GetCurrentDirectory() + "\\create_datasets.xml"));

                // Process server side first
                /*
                string response = soapClient.DocumentProcessServerSide(key, documentId, null);
                xmlDocument.LoadXml(response);
                string repId = xmlDocument.FirstChild.Attributes["id"].Value;

                
                bool done = false;
                while (done)
                {
                    xmlDocument.LoadXml(soapClient.TaskGetStatus(apiKey, repId));

                    XmlAttributeCollection taskAttributes = xmlDocument.FirstChild.Attributes;
                    if (taskAttributes["finished"].Value.ToString() == "True")
                    {
                        done = true;
                    }
                }
                Console.WriteLine(response);

                 */

                xmlDocument.LoadXml(soapClient.DocumentCreatePDF(apiKey, tempDocId, generationTask.pdfExportSettings, 5));

                //xmlDocument.LoadXml(soapClient.DocumentCreateImages(apiKey, tempDocId, generationTask.pdfExportSettings, "2b3679cf-4a38-4c2f-bee7-e223695f5207", 7));

                taskIds.Add(xmlDocument.FirstChild.Attributes["id"].Value);
                Console.WriteLine("Task ID: " + xmlDocument.FirstChild.Attributes["id"].Value);
            }

            List<string> completedTasks = new List<string>();
            List<bool> success = new List<bool>();
            List<string> urls = new List<string>();
            List<string> errors = new List<string>();

            while (taskIds.Count > 0)
            {
                for (int i = taskIds.Count - 1; i > -1; i--)
                {
                    string taskId = taskIds[i];

                    string taskXML = soapClient.TaskGetStatus(apiKey, taskId);

                    //Console.WriteLine(taskXML);

                    xmlDocument.LoadXml(taskXML);

                    XmlAttributeCollection taskAttributes = xmlDocument.FirstChild.Attributes;

                    if (taskAttributes["found"] == null || taskAttributes["found"].Value != "false")
                    {
                        if (taskAttributes["finished"].Value.ToString() == "True")
                        {
                            
                            completedTasks.Add(taskId);

                            if (taskAttributes["succeeded"].Value.ToString() == "True")
                            {
                                success.Add(true);

                                Logger.WriteLog(taskXML, "CompleteTasks");

                                xmlDocument.LoadXml(taskAttributes["result"].Value);

                                string url = xmlDocument.FirstChild.Attributes["url"].Value;
                                string itemId = taskAttributes["itemID"].Value;

                                if (downloadPdf == true)
                                {
                                    DownloadPdf(url, name + "-" + taskId, pdfName);
                                }

                                urls.Add(url);
                            }
                            else
                            {
                                success.Add(false);

                                if (showErrors == true)
                                {
                                    //Console.WriteLine(xmlDocument.OuterXml);

                                    string error = xmlDocument.FirstChild.Attributes["errorMessage"].Value;

                                    if (errors.Contains(error) != true)
                                    {
                                        errors.Add(error);
                                    }
                                }



                            }

                            taskIds.RemoveAt(i);
                        }
                    }
                    else
                    {
                        WriteTestLog("Task no found: " + taskAttributes["id"].Value, name);
                        Console.WriteLine("Task no found: " + taskAttributes["id"].Value);
                        taskIds.RemoveAt(i);
                    }
                }
            }

            TimeSpan timeSpan = DateTime.Now.Subtract(DateTime.FromOADate(startTime));

            WriteTestLog("Time In Seconds: " + timeSpan.TotalSeconds.ToString(), name);
            WriteTestLog("Time In Minutes: " + timeSpan.TotalMinutes.ToString(), name);

            //Success?
            if (success.Contains(false))
            {
                WriteTestLog("All Tasks Succeeded: False", name);

                WriteTestLog("Number Of Failures: " + success.Count((x) => x == false), name);

                Console.Write("Failure - ");

                if (errors.Count > 0)
                {
                    WriteTestLog("All Tasks Succeeded: False", name);

                    if (showErrors == true)
                    {
                        WriteTestLog("", name);
                        WriteTestLog("Errors", name);
                        WriteTestLog("---------------------------", name);
                        foreach (string error in errors)
                        {
                            WriteTestLog(error, name);
                        }
                        WriteTestLog("", name);
                    }
                }
            }
            else
            {
                WriteTestLog("All Tasks Succeeded: True", name);
            }

            if (includeUrls == true)
            {
                WriteTestLog("", name);
                WriteTestLog("PDF URLs ----------------------------", name);

                foreach (string url in urls)
                {
                    WriteTestLog(url, name);
                }
            }

            if (finalErrorReport == true)
            {
                WriteTestLog("Name: " + name, "A_FinalErrorReport");
                WriteTestLog("Success: " + success.Count((b) => { return (b == true); }), "A_FinalErrorReport");
                WriteTestLog("Errors: " + success.Count((b) => { return (b == false); }), "A_FinalErrorReport");
            }

            Console.WriteLine("Done testing " + name);
        }

        private string key;

        private string GetKey()
        {
            return KeyMaster.key;
        }

        private void DownloadPdf(string url, string documentName, string testName)
        {
            DirectoryInfo directory = Directory.CreateDirectory(currentDirectory + "\\pdfs\\" + testName);

            try
            {
                WebClient wc = new WebClient();
                wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.33 Safari/537.36");
                wc.DownloadFile(url, directory.FullName +"\\" + documentName +".zip");
            }
            catch (WebException e)
            {
                
            }
        }


        private List<string> headers = new List<string>();

        private void WriteTestLog(string message, string testName = "log")
        {

            try
            {
                FileStream fs = File.OpenWrite(currentDirectory + "\\" + testName + ".txt");
                fs.Position = fs.Length;

                if (!headers.Contains(testName))
                {
                    headers.Add(testName);

                    message = DateTime.Now.Day + "/" + DateTime.Now.Month + "/" + DateTime.Now.Year + " @ " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second + Environment.NewLine + message;
                    message = "New Test-------------- " + message;

                    if (fs.Length > 0)
                    {
                        message = Environment.NewLine + Environment.NewLine + message;
                    }

                }

                message = Environment.NewLine + message;

                byte[] messageBytes = Encoding.UTF8.GetBytes(message);

                fs.Write(messageBytes, 0, messageBytes.Length);
                fs.Close();
            }
            catch (Exception e)
            {

            }
        }



    }
}