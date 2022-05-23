using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Web.UI.WebControls;

namespace LITS.Infrastructure.Common
{
    public class XmlUtil
    {
        private XmlDataSource m_xmlData;

        #region XML

        public XmlDataSource GetXmlDataSource(string usDataFile, string xmlPath)
        {
            XmlDataSource dsData = new XmlDataSource();
            dsData.DataFile = usDataFile;
            dsData.XPath = xmlPath;
            return dsData;
        }

        public XmlDataSource GetEmailTemplateXmlData(string xmlPath)
        {
            return GetXmlDataSource("~/App_Data/MailTemplate.xml", xmlPath);
        }

        public XmlDataSource GetStatusXmlData(string xmlPath)
        {
            return GetXmlDataSource("~/App_Data/ProcessStatus.xml", xmlPath);
        }

        public string GetStatusItem(object status)
        {
            m_xmlData = GetStatusXmlData("Items/ProcessStatus/Status");

            XmlNode node = m_xmlData.GetXmlDocument().SelectSingleNode("Items/ProcessStatus/Status" + "[@Id='" + status.ToString() + "']");
            if (node != null)
                return node.Attributes["Value"].Value;
            else
                return "";
        }

        #endregion
    }

    public class XmlSerializeHelper
    {
        public static string Serialize<T>(T obj)
        {
            return Serialize<T>(obj, Encoding.UTF8);
        }

        /// <summary>
        /// Serialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize<T>(T obj, Encoding encoding)
        {
            try
            {

                if (obj == null)
                    throw new ArgumentNullException("obj");

                var ser = new XmlSerializer(obj.GetType());
                using (var ms = new MemoryStream())
                {
                    using (var writer = new XmlTextWriter(ms, encoding))
                    {
                        writer.Formatting = Formatting.Indented;
                        ser.Serialize(writer, obj);
                    }
                    var xml = encoding.GetString(ms.ToArray());
                    xml = xml.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
                    xml = xml.Replace("xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
                    return xml;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// DeSerialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string xml)
            where T : new()
        {
            return DeSerialize<T>(xml, Encoding.UTF8);
        }

        /// <summary>
        /// DeSerialize
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static T DeSerialize<T>(string xml, Encoding encoding)
            where T : new()
        {
            try
            {
                var mySerializer = new XmlSerializer(typeof(T));
                using (var ms = new MemoryStream(encoding.GetBytes(xml)))
                {
                    using (var sr = new StreamReader(ms, encoding))
                    {
                        return (T)mySerializer.Deserialize(sr);
                    }
                }
            }
            catch (Exception ex)
            {
                return default(T);
                throw ex;
            }
        }

        public static string GetXMLFromObject(object o)
        {
            StringWriter sw = new StringWriter();
            XmlTextWriter tw = null;

            try
            {
                XmlSerializer serializer = new XmlSerializer(o.GetType());
                tw = new XmlTextWriter(sw);
                serializer.Serialize(tw, o);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                sw.Close();
                if (tw != null)
                {
                    tw.Close();
                }
            }

            return sw.ToString();
        }

        public static Object ObjectToXML(string xml, Type objectType)
        {
            StringReader strReader = null;
            XmlSerializer serializer = null;
            XmlTextReader xmlReader = null;
            Object obj = null;
            try
            {
                strReader = new StringReader(xml);
                serializer = new XmlSerializer(objectType);
                xmlReader = new XmlTextReader(strReader);
                obj = serializer.Deserialize(xmlReader);
            }
            catch (Exception exp)
            {
                throw exp;
            }
            finally
            {
                if (xmlReader != null)
                {
                    xmlReader.Close();
                }
                if (strReader != null)
                {
                    strReader.Close();
                }
            }
            return obj;
        }

        public static XmlDocument JsonToXml(string json)
        {
            XmlNode newNode = null;
            XmlNode appendToNode = null;
            XmlDocument returnXmlDoc = new XmlDocument();
            returnXmlDoc.LoadXml("<Document />");
            XmlNode rootNode = returnXmlDoc.SelectSingleNode("Document");
            appendToNode = rootNode;

            string[] arrElementData;
            string[] arrElements = json.Split('\r');
            foreach (string element in arrElements)
            {
                string processElement = element.Replace("\r", "").Replace("\n", "").Replace("\t", "").Trim();
                if ((processElement.IndexOf("}") > -1 || processElement.IndexOf("]") > -1) && appendToNode != rootNode)
                {
                    appendToNode = appendToNode.ParentNode;
                }
                else if (processElement.IndexOf("[") > -1)
                {
                    processElement = processElement.Replace(":", "").Replace("[", "").Replace("\"", "").Trim();
                    newNode = returnXmlDoc.CreateElement(processElement);
                    appendToNode.AppendChild(newNode);
                    appendToNode = newNode;
                }
                else if (processElement.IndexOf("{") > -1 && processElement.IndexOf(":") > -1)
                {
                    processElement = processElement.Replace(":", "").Replace("{", "").Replace("\"", "").Trim();
                    newNode = returnXmlDoc.CreateElement(processElement);
                    appendToNode.AppendChild(newNode);
                    appendToNode = newNode;
                }
                else
                {
                    if (processElement.IndexOf(":") > -1)
                    {
                        arrElementData = processElement.Replace(": \"", ":").Replace("\",", "").Replace("\"", "").Split(':');
                        newNode = returnXmlDoc.CreateElement(arrElementData[0]);
                        for (int i = 1; i < arrElementData.Length; i++)
                        {
                            newNode.InnerText += arrElementData[i];
                        }

                        appendToNode.AppendChild(newNode);
                    }
                }
            }

            return returnXmlDoc;
        }
    }
}
