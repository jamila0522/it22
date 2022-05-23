using BussinessObjects.Xml;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Helpers.Functions
{
    public class ReaderTools
    {
        public static List<FilterParam> GetListFilterParam(string filePath = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlPath, XmlReadMode.InferSchema);

                List<FilterParam> parms = new List<FilterParam>();

                if (dataSet.Tables[0].Columns.Count == 4 /* Basic param */)
                {
                    parms = (from rows in dataSet.Tables[0].AsEnumerable()
                             select new FilterParam
                             {
                                 name = rows[0].ToString(),
                                 display = rows[1].ToString(),
                                 type = rows[2].ToString(),
                                 databinding = rows[3].ToString(),
                             }).ToList();
                }
                else // Combobox param
                {
                    parms = (from rows in dataSet.Tables[0].AsEnumerable()
                             select new FilterParam
                             {
                                 name = rows.Table.Columns.IndexOf("name") > -1 ? rows["name"].ToString() : "",
                                 display = rows.Table.Columns.IndexOf("display") > -1 ? rows["display"].ToString() : "",
                                 type = rows.Table.Columns.IndexOf("type") > -1 ? rows["type"].ToString() : "",
                                 databinding = rows.Table.Columns.IndexOf("databinding") > -1 ? rows["databinding"].ToString() : "",
                                 tablebinding = rows.Table.Columns.IndexOf("tablebinding") > -1 ? rows["tablebinding"].ToString() : "",
                                 fnamebinding = rows.Table.Columns.IndexOf("fnamebinding") > -1 ? rows["fnamebinding"].ToString() : "",
                                 fvaluebinding = rows.Table.Columns.IndexOf("fvaluebinding") > -1 ? rows["fvaluebinding"].ToString() : "",
                             }).ToList();
                }
                return parms;
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetListFilterParam", ex.Message);
                return new List<FilterParam>();
            }
        }

        public static List<SubMenu> GetSubMenu(string filePath = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlPath, XmlReadMode.InferSchema);

                List<SubMenu> subMenus = new List<SubMenu>();
                subMenus = (from rows in dataSet.Tables[0].AsEnumerable()
                            select new SubMenu
                            {
                                index = Convert.ToInt32(rows[0].ToString()),
                                display = rows[1].ToString(),
                                link = rows[2].ToString(),
                                active = Convert.ToInt32(rows[3].ToString())
                            }).ToList();
                return subMenus;
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetSubMenu", ex.Message);
                return new List<SubMenu>();
            }
        }

        /*
            <name>ParamId</name>
            <display>Tên thuộc tính</display>
            <type>text</type>
            <action_link>text</action_link>
            <action_style>info</action_style>
            <action_type>popup</action_type>
        */
        //public static List<ListField> GetDataFieldForList(string filePath = "")
        //{
        //    try
        //    {
        //        string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
        //        DataSet ds = new DataSet();//Using dataset to read xml file  
        //        ds.ReadXml(xmlPath);
        //        var parms = new List<ListField>();
        //        parms = (from rows in ds.Tables[0].AsEnumerable()
        //                 select new ListField
        //                 {
        //                     name = rows[0].ToString(),
        //                     display = rows[1].ToString(),
        //                     type = rows[2].ToString()
        //                 }).ToList();
        //        return parms;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<ListField>();
        //    }
        //}

        public static List<ListField> GetDataFieldForList2(string filePath)
        {
            try
            {
                List<ListField> fields = new List<ListField>();
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                //Get and display the last item node.
                XmlElement root = doc.DocumentElement;
                XmlNodeList nodeList = root.GetElementsByTagName("datafield");
                if (nodeList != null && nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        XmlNodeList childs = node.ChildNodes;
                        ListField field = new ListField();
                        if (childs != null && childs.Count > 0)
                        {
                            foreach (XmlNode child in childs)
                            {
                                if (child.Name == "index")
                                {
                                    field.index = Convert.ToInt32(child.InnerText.Trim());
                                }
                                if (child.Name == "name")
                                {
                                    field.name = child.InnerText;
                                }
                                if (child.Name == "display")
                                {
                                    field.display = child.InnerText;
                                }
                                if (child.Name == "type")
                                {
                                    field.type = child.InnerText;
                                }
                                if (child.Name == "details")
                                {
                                    if (child.ChildNodes != null && child.ChildNodes.Count > 0)
                                    {
                                        field.Details = new List<FieldDetail>();

                                        foreach (XmlNode detail in child.ChildNodes)
                                        {
                                            FieldDetail fieldDetail = new FieldDetail();
                                            XmlNodeList details = detail.ChildNodes;
                                            if (details != null && details.Count > 0)
                                            {
                                                foreach (XmlNode childDetail in details)
                                                {
                                                    if (childDetail.Name == "dataBind")
                                                    {
                                                        fieldDetail.dataBind = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "displayBind")
                                                    {
                                                        fieldDetail.displayBind = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "style")
                                                    {
                                                        fieldDetail.style = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "type")
                                                    {
                                                        fieldDetail.type = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "link")
                                                    {
                                                        fieldDetail.link = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "icon")
                                                    {
                                                        fieldDetail.icon = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "message")
                                                    {
                                                        fieldDetail.message = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "jsfunction")
                                                    {
                                                        fieldDetail.jsfunction = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "conditionTarget")
                                                    {
                                                        fieldDetail.conditionTarget = childDetail.InnerText;
                                                    }
                                                    if (childDetail.Name == "conditionValue")
                                                    {
                                                        fieldDetail.conditionValue = childDetail.InnerText;
                                                    }
                                                }
                                                field.Details.Add(fieldDetail);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        fields.Add(field);
                    }
                    return fields;
                }
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetDataFieldForList2", ex.Message);
            }
            return new List<ListField>();
        }


        /*
            public string name { get; set; }
            public string display { get; set; }
            public string type { get; set; }
            public string link { get; set; }
            public string style { get; set; }
            public string icon { get; set; }
            public string message { get; set; } 
        */
        public static List<ActionLinkModel> GetActionLinkForList(string filePath = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet ds = new DataSet();//Using dataset to read xml file  
                ds.ReadXml(xmlPath);
                List<ActionLinkModel> parms = new List<ActionLinkModel>();
                parms = (from rows in ds.Tables[0].AsEnumerable()
                         select new ActionLinkModel
                         {
                             name = rows[0].ToString(),
                             display = rows[1].ToString(),
                             type = rows[2].ToString(),
                             link = rows[3].ToString(),
                             style = rows[4].ToString(),
                             icon = rows[5].ToString(),
                             message = rows[6].ToString(),
                             target = rows[7].ToString()
                         }).ToList();
                return parms;
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetActionLinkForList", ex.Message);
                return new List<ActionLinkModel>();
            }
        }

        public static List<ListTab> GetDataTabForList(string filePath = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet ds = new DataSet();//Using dataset to read xml file  
                ds.ReadXml(xmlPath);
                List<ListTab> tab = new List<ListTab>();
                tab = (from rows in ds.Tables[0].AsEnumerable()
                       select new ListTab
                       {
                           index = rows[0].ToString(),
                           display = rows[1].ToString(),
                           status = rows[2].ToString()
                       }).ToList();
                return tab;
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetDataTabForList", ex.Message);
                return new List<ListTab>();
            }
        }

        public static List<ListTab> GetDataTabForList2(string filePath = "")
        {
            try
            {
                List<ListTab> tabs = new List<ListTab>();
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                if (doc != null)
                {
                    //Get and display the last item node.
                    XmlElement root = doc.DocumentElement;
                    XmlNodeList nodeList = root.GetElementsByTagName("tab");
                    if (nodeList != null && nodeList.Count > 0)
                    {
                        foreach (XmlNode node in nodeList)
                        {
                            XmlNodeList childs = node.ChildNodes;
                            ListTab tab = new ListTab();
                            if (childs != null && childs.Count > 0)
                            {
                                foreach (XmlNode child in childs)
                                {
                                    if (child.Name == "index")
                                    {
                                        tab.index = child.InnerText.Trim();
                                    }
                                    if (child.Name == "display")
                                    {
                                        tab.display = child.InnerText;
                                    }
                                    if (child.Name == "status")
                                    {
                                        tab.status = child.InnerText;
                                    }
                                    if (child.Name == "active")
                                    {
                                        tab.active = Convert.ToInt32(child.InnerText);
                                    }
                                }
                            }
                            tabs.Add(tab);
                        }
                        return tabs;
                    }
                }
                return new List<ListTab>();
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GetDataTabForList2", ex.Message);
                return new List<ListTab>();
            }
        }

        #region 'Service'
        public static List<XmlMethod> ReadMethodInfo(string filePath = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlPath, XmlReadMode.InferSchema);
                List<XmlMethod> method = new List<XmlMethod>();
                method = (from rows in dataSet.Tables[0].AsEnumerable()
                          select new XmlMethod
                          {
                              method = rows[0].ToString(),
                              param = rows[1].ToString(),
                              datasource = rows[2].ToString()
                          }).ToList();
                return method;
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("ReadMethodInfo", ex.Message);
                return new List<XmlMethod>();
            }
        }

        public static ServiceMethod ReadServiceMethod(string filePath = "", string key = "")
        {
            try
            {
                string xmlPath = HttpContext.Current.Server.MapPath(filePath); //Path of the xml script 
                DataSet dataSet = new DataSet();
                dataSet.ReadXml(xmlPath, XmlReadMode.InferSchema);
                List<ServiceMethod> methods = new List<ServiceMethod>();
                methods = (from rows in dataSet.Tables[0].AsEnumerable()
                           select new ServiceMethod
                           {
                               index = Convert.ToInt32(rows[0].ToString()),
                               key = rows[1].ToString(),
                               datasource = rows[2].ToString()
                           }).ToList();
                if (methods != null && methods.Count() > 0)
                {
                    return methods.Where(m => m.key == key).FirstOrDefault();
                }
            }
            catch (Exception ex)
            { Helpers.Functions.DataFunction.WriteLog("ReadServiceMethod", ex.Message); }
            return new ServiceMethod();
        }

        public static T DeserializeXMLFileToObject<T>(string XmlFilename)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(XmlFilename))
            {
                return default(T);
            }

            try
            {
                StreamReader xmlStream = new StreamReader(XmlFilename);
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                returnObject = (T)serializer.Deserialize(xmlStream);
                xmlStream.Close();
            }
            catch (Exception ex)
            { Helpers.Functions.DataFunction.WriteLog("DeserializeXMLFileToObject", ex.Message); }
            return returnObject;
        }
        public static T DeserializeJsonFileToObject<T>(string fileName)
        {
            T returnObject = default(T);
            if (string.IsNullOrEmpty(fileName))
            {
                return default(T);
            }
            try
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    returnObject = JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            { Helpers.Functions.DataFunction.WriteLog("DeserializeJsonFileToObject", ex.Message); }
            return returnObject;
        }
        #endregion
        public static bool SerializationObjetTolXML<T>(T DataObject, string XmlFilename, string Folder)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));

            string xml = "";
            ////serialization
            using (StringWriter sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, DataObject);
                    xml = sww.ToString(); // Your XML
                }
            }
            ////Write to file
            return DataFunction.WriteFiles(xml, XmlFilename, Folder);
            
        }
        public static bool SerializationObjetTolJson<T>(T DataObject, string fileName, string Folder)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(DataObject);
                ////Write to file
                return DataFunction.WriteFiles(jsonData, fileName, Folder);
            }
            catch (Exception ex)
            { 
                Helpers.Functions.DataFunction.WriteLog("SerializationObjetTolJson", ex.Message);
                return false;
            }
            return true;
        }


        #region new code here
        public static List<FilterParam> GetCategoriesFormInfo(string filePath = "")
        {
            try
            {
                string fileName = HttpContext.Current.Server.MapPath(filePath);
                DataSet set = new DataSet();
                set.ReadXml(fileName, XmlReadMode.InferSchema);
                List<FilterParam> list = new List<FilterParam>();
                if (set.Tables[0].Columns.Count == 4)
                {
                    list = set.Tables[0].AsEnumerable().Select<DataRow, FilterParam>(rows => new FilterParam
                    {
                        name = rows[0].ToString(),
                        display = rows[1].ToString(),
                        type = rows[2].ToString(),
                        databinding = rows[3].ToString()
                    }).ToList<FilterParam>();
                }
                else
                {
                    list = set.Tables[0].AsEnumerable().Select<DataRow, FilterParam>(rows => new FilterParam
                    {
                        name = (rows.Table.Columns.IndexOf("name") > -1) ? rows["name"].ToString() : "",
                        display = (rows.Table.Columns.IndexOf("display") > -1) ? rows["display"].ToString() : "",
                        type = (rows.Table.Columns.IndexOf("type") > -1) ? rows["type"].ToString() : "",
                        databinding = (rows.Table.Columns.IndexOf("databinding") > -1) ? rows["databinding"].ToString() : "",
                        tablebinding = (rows.Table.Columns.IndexOf("tablebinding") > -1) ? rows["tablebinding"].ToString() : "",
                        fnamebinding = (rows.Table.Columns.IndexOf("fnamebinding") > -1) ? rows["fnamebinding"].ToString() : "",
                        fvaluebinding = (rows.Table.Columns.IndexOf("fvaluebinding") > -1) ? rows["fvaluebinding"].ToString() : ""
                    }).ToList<FilterParam>();
                }
                return list;
            }
            catch (Exception)
            {
                return new List<FilterParam>();
            }
        }
        //public static List<FilterParam> GetListFilterParam(string filePath = "")
        //{
        //    try
        //    {

        //        string fileName = HttpContext.Current.Server.MapPath(filePath);
        //        DataSet set = new DataSet();
        //        set.ReadXml(fileName, XmlReadMode.InferSchema);
        //        List<FilterParam> list = new List<FilterParam>();
        //        if (set.Tables[0].Columns.Count == 4)
        //        {
        //            list = set.Tables[0].AsEnumerable().Select<DataRow, FilterParam>(rows => new FilterParam
        //            {
        //                name = rows[0].ToString(),
        //                display = rows[1].ToString(),
        //                type = rows[2].ToString(),
        //                databinding = rows[3].ToString()
        //            }).ToList<FilterParam>();
        //        }
        //        else
        //        {
        //            list = set.Tables[0].AsEnumerable().Select<DataRow, FilterParam>(rows => new FilterParam
        //            {
        //                name = (rows.Table.Columns.IndexOf("name") > -1) ? rows["name"].ToString() : "",
        //                display = (rows.Table.Columns.IndexOf("display") > -1) ? rows["display"].ToString() : "",
        //                type = (rows.Table.Columns.IndexOf("type") > -1) ? rows["type"].ToString() : "",
        //                databinding = (rows.Table.Columns.IndexOf("databinding") > -1) ? rows["databinding"].ToString() : "",
        //                tablebinding = (rows.Table.Columns.IndexOf("tablebinding") > -1) ? rows["tablebinding"].ToString() : "",
        //                fnamebinding = (rows.Table.Columns.IndexOf("fnamebinding") > -1) ? rows["fnamebinding"].ToString() : "",
        //                fvaluebinding = (rows.Table.Columns.IndexOf("fvaluebinding") > -1) ? rows["fvaluebinding"].ToString() : ""
        //            }).ToList<FilterParam>();
        //        }
        //        return list;
        //    }
        //    catch (Exception)
        //    {
        //        return new List<FilterParam>();
        //    }
        //}

        //public static List<XmlMethod> ReadMethodInfo(string filePath = "")
        //{
        //    try
        //    {

        //        string fileName = HttpContext.Current.Server.MapPath(filePath);
        //        DataSet set = new DataSet();
        //        set.ReadXml(fileName, XmlReadMode.InferSchema);
        //        List<XmlMethod> list = new List<XmlMethod>();
        //        return set.Tables[0].AsEnumerable().Select<DataRow, XmlMethod>(rows => new XmlMethod
        //        {
        //            method = rows[0].ToString(),
        //            param = rows[1].ToString(),
        //            datasource = rows[2].ToString()
        //        }).ToList<XmlMethod>();
        //    }
        //    catch (Exception)
        //    {
        //        return new List<XmlMethod>();
        //    }
        //}


        //public static List<XmlMethod> ReadMethodInfo(string filePath = "")
        //{
        //    try
        //    {

        //        string fileName = HttpContext.Current.Server.MapPath(filePath);
        //        DataSet set = new DataSet();
        //        set.ReadXml(fileName, XmlReadMode.InferSchema);
        //        List<XmlMethod> list = new List<XmlMethod>();
        //        return set.Tables[0].AsEnumerable().Select<DataRow, XmlMethod>(rows => new XmlMethod
        //        {
        //            method = rows[0].ToString(),
        //            param = rows[1].ToString(),
        //            datasource = rows[2].ToString()
        //        }).ToList<XmlMethod>();
        //    }
        //    catch (Exception)
        //    {
        //        return new List<XmlMethod>();
        //    }
        //}

        //public static ServiceMethod ReadServiceMethod(string filePath = "", string key = "")
        //{
        //    try
        //    {

        //        string fileName = HttpContext.Current.Server.MapPath(filePath);
        //        DataSet set = new DataSet();
        //        set.ReadXml(fileName, XmlReadMode.InferSchema);
        //        List<ServiceMethod> source = new List<ServiceMethod>();
        //        source = set.Tables[0].AsEnumerable().Select<DataRow, ServiceMethod>(rows => new ServiceMethod
        //        {
        //            index = Convert.ToInt32(rows[0].ToString()),
        //            key = rows[1].ToString(),
        //            datasource = rows[2].ToString()
        //        }).ToList<ServiceMethod>();
        //        if ((source != null) && (source.Count<ServiceMethod>() > 0))
        //        {
        //            return (from m in source
        //                    where m.key == key
        //                    select m).FirstOrDefault<ServiceMethod>();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return new ServiceMethod();
        //}


        //public static ServiceMethod ReadServiceMethod(string filePath = "", string key = "")
        //{
        //    try
        //    {
        //        string fileName = HttpContext.Current.Server.MapPath(filePath);
        //        DataSet set = new DataSet();
        //        set.ReadXml(fileName, XmlReadMode.InferSchema);
        //        List<ServiceMethod> source = new List<ServiceMethod>();
        //        source = set.Tables[0].AsEnumerable().Select<DataRow, ServiceMethod>(rows => new ServiceMethod
        //        {
        //            index = Convert.ToInt32(rows[0].ToString()),
        //            key = rows[1].ToString(),
        //            datasource = rows[2].ToString()
        //        }).ToList<ServiceMethod>();
        //        if ((source != null) && (source.Count<ServiceMethod>() > 0))
        //        {
        //            return (from m in source
        //                    where m.key == key
        //                    select m).FirstOrDefault<ServiceMethod>();
        //        }
        //    }
        //    catch (Exception)
        //    {
        //    }
        //    return new ServiceMethod();
        //}


        #endregion


    }
}
