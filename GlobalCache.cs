using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Scb.Framework;
using System.Xml;

namespace ResponseMonitorService
{
    public class GlobalCache
    {
        #region Singleton Implement

        private static GlobalCache _instance = null;

        internal static GlobalCache GetInstance()
        {
            if (null == _instance)
            {
                _instance = new GlobalCache();
            }
            return _instance;
        }

        #endregion

        #region Constructor

        private GlobalCache()
        {
            LoadParameters();
        }
        #endregion

        internal int ReminderInterval = 60;//default: 60 second
        internal string SmtpHost = string.Empty;
        internal int SmtpPort = -1;
        internal string SystemEmail = string.Empty;
        internal string SmtpPassword = string.Empty;
        internal string SmtpUserName = string.Empty;
        internal bool SmtpUseSsl = false;
        internal string ConnectionString = string.Empty;
        internal int WaitForStoppingProcessor = 5;
        internal int CommandTimeout = 300;
        internal string MailSubject = "";

        private void LoadParameters()
        {
            try
            {
                ReminderInterval = int.Parse(ConfigurationManager.AppSettings["ReminderInterval"]);
                SmtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                SmtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
                SystemEmail = ConfigurationManager.AppSettings["SystemEmail"];
                SmtpPassword = ConfigurationManager.AppSettings["SmtpPassord"];
                SmtpUserName = ConfigurationManager.AppSettings["SmtpUserName"];
                SmtpUseSsl = Convert.ToBoolean(ConfigurationManager.AppSettings["SmtpUseSsl"]);
                MailSubject = ConfigurationManager.AppSettings["Subject"];

            }
            catch (Exception ex)
            {
                Logger.Error("LoadParameters got an error", ex);
            }
        }

        public string LoadEmailTemplate(string processName)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(AppDomain.CurrentDomain.BaseDirectory + "MailTemplate.xml");
                XmlNode node = doc.SelectSingleNode("Data/DataItem[@key='" + processName + "']");
                if (node != null)
                    return node.Attributes["value"].Value;

                return "";
            }
            catch (Exception ex)
            {
                Logger.Error("LoadXmlTemplate", ex);
                return "";
            }
        }
    }

}
