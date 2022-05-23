using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml;
using System.Web;
using System.Runtime.Caching;

namespace Helpers.Functions
{
    public static class ConfigFunctions
    {
        public static void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);
                config.AppSettings.Settings[key].Value = value;
                config.Save();
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ConfigFunctions AddOrUpdateAppSettings", ex.Message, key, value);
            }
        }

        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key].ToString();
        }

        public static string GetConfig(string fileConfig, string key)
        {
            string result = "";
            try
            {
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(AppDomain.CurrentDomain.BaseDirectory + "Files\\Config\\" + fileConfig);
                XmlNode xnodes = xdoc.SelectSingleNode("/Configurations/" + key);
                result = xnodes.InnerText.ToString();
            }
            catch(Exception ex) {
                DataFunction.WriteLog("ConfigFunctions GetConfig", ex.Message, fileConfig, key);
            }
            return result;
        }
        public static string GetConfigByDomain(string key)
        {
           
            try
            {
                string fileConfig = HttpContext.Current.Request.Url.Host;
                return GetFromCache(fileConfig, key);
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ConfigFunctions GetConfigByDomain", ex.Message, "", key);
            }
            return "err in GetConfigByDomain "  + key;
        }
        private static string GetFromCache(string fileConfig, string key)
        {
            string result = "";
            fileConfig = fileConfig.ToLower();
            try
            {
                ObjectCache cache = MemoryCache.Default;
                XmlDocument model = cache.Get(fileConfig) as XmlDocument;
                
                if (model != null)
                {
                    XmlNode xnodes = model.SelectSingleNode("/Configurations/" + key);
                    result = xnodes.InnerText.ToString();
                }
                else
                {
                    CacheItemPolicy policy = new CacheItemPolicy
                    {
                        //policy.SlidingExpiration = new TimeSpan(0,5,0);
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
                    };
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(AppDomain.CurrentDomain.BaseDirectory + "Files\\Config\\" + fileConfig + ".sysconfig");
                    XmlNode xnodes = xdoc.SelectSingleNode("/Configurations/" + key);
                    result = xnodes.InnerText.ToString();
                    cache.Set(fileConfig, xdoc, policy);
                }
            }
            catch(Exception ex)
            {
                DataFunction.WriteLog("ConfigFunctions GetConfigByDomain GetFromCache", ex.Message, fileConfig, key);
                return "err in GetConfigByDomain GetFromCache " + fileConfig + key;
            }
            return result;
        }


        public static string AddorUpdateConfig(string fileConfig, string key, string value)
        {
            string result = "";
            try
            {
                string path = AppDomain.CurrentDomain.BaseDirectory + "Files\\Config\\" + fileConfig;
                XmlDocument xdoc = new XmlDocument();
                xdoc.Load(path);
                XmlNode xnodes = xdoc.SelectSingleNode("/Configurations/" + key);
                xnodes.InnerText = value;
                xdoc.Save(path);
                result = "OK";
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ConfigFunctions AddorUpdateConfig", ex.Message, fileConfig, key);
            }
            return result;
        }

        public static Dictionary<string, string> GetSettings(string path)
        {

            var document = System.Xml.Linq.XDocument.Load(path);

            var root = document.Root;
            var results =
              root
                .Elements()
                .ToDictionary(element => element.Name.ToString(), element => element.Value);

            return results;

        }
    }
}
