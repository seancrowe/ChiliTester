using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ChiliTester
{
    class DataMaster
    {
        private Dictionary<string, string> xmlData = new Dictionary<string, string>();

        private readonly string filePath;

        public DataMaster()
        {
            filePath = Directory.GetCurrentDirectory() + "\\storage.xml";

            xmlSerializer = new XmlSerializer(typeof(Dictionary<string, string>));

            ReadUpdateCurrentData();
        }

        public string GetData(string key)
        {
            if (xmlData != null)
            {
                if (xmlData.ContainsKey(key))
                {
                    return xmlData[key];
                }
            }

            return "";
        }

        public void SetData(string key, string value)
        {
            if (xmlData != null)
            {
                if (xmlData.ContainsKey(key))
                {
                    xmlData.Add(key, value);
                }
                else
                {
                    xmlData[key] = value;
                }
            }
        }

        private XmlSerializer xmlSerializer;

        public void WriteUpdateCurrentData()
        {
            xmlSerializer.Serialize(File.Open(filePath, FileMode.OpenOrCreate), xmlData);


            /*XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(filePath);

            XmlNode xmlNode = xmlDocument.FirstChild;

            if (xmlNode != null)
            {
                XmlAttributeCollection attributeCollection = xmlNode.Attributes;

                foreach (KeyValuePair<string, string> valuePair in xmlData)
                {
                    attributeCollection.


                    if (xmlData.ContainsKey(xmlAttribute.Name))
                    {
                        xmlData[xmlAttribute.Name] = xmlAttribute.Value;
                    }
                    else
                    {
                        xmlData.Add(xmlAttribute.Name, xmlAttribute.Value);
                    }
                }
            }*/
        }

        public void ReadUpdateCurrentData()
        {
            if (File.Exists(filePath))
            {
               xmlData = xmlSerializer.Deserialize(File.Open(filePath, FileMode.Open)) as Dictionary<string, string>;
            }
            else
            {
                xmlData = new Dictionary<string, string>();
            }

            //XmlDocument xmlDocument = new XmlDocument();
            //xmlDocument.Load(filePath);

            //XmlNode xmlNode = xmlDocument.FirstChild;

            //if (xmlNode != null)
            //{
            //    XmlAttributeCollection attributeCollection = xmlNode.Attributes;

            //    foreach (XmlAttribute xmlAttribute in xmlNode.Attributes)
            //    {
            //        if (xmlData.ContainsKey(xmlAttribute.Name))
            //        {
            //            xmlData[xmlAttribute.Name] = xmlAttribute.Value;
            //        }
            //        else
            //        {
            //            xmlData.Add(xmlAttribute.Name, xmlAttribute.Value);
            //        }
            //    }
            //}
        }
    }
}
