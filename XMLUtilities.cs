using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Scb.Framework;
using System.Xml;

namespace WBODocsTracking.Web.Common
{
  public class XMLUtilities
  {
    public static XmlDataSource m_xmlData;
    private static string xmlFileProcessCore = "~/App_Data/ProcessCoreRole.xml";

    #region ProcessCore

    public static int ProcessId()
    {
      string strProcessId = GetXmlTemplateItemInner(xmlFileProcessCore, "ProcessCore/ProcessId");
      if (string.IsNullOrEmpty(strProcessId))
        return -1;

      return int.Parse(strProcessId);
    }

    #region GetValueInXML

    private static int GetRoleValueInXML(string role)
    {
      string strRole = GetXmlTemplateItem(xmlFileProcessCore, "ProcessCore/TaskList/Task", role);
      if (strRole != null)
        return int.Parse(strRole);

      return -1;
    }

    private static int GetPermissionValueInXML(string role)
    {
      string strPermission = GetXmlTemplateItem(xmlFileProcessCore, "ProcessCore/UserPermission/Permission", role);
      if (strPermission != null)
        return int.Parse(strPermission);

      return -1;
    }

    #endregion

    #region GetIDRole

    public static int RoleMakerCMO()
    {
      return GetRoleValueInXML("CMO-Maker");
    }

    public static int RoleCheckerCMO()
    {
      return GetRoleValueInXML("CMO-Checker");
    }

    public static int RoleMakerTRADE()
    {
      return GetRoleValueInXML("TRADE-Maker");
    }

    public static int RoleCheckerTRADE()
    {
      return GetRoleValueInXML("TRADE-Checker");
    }

    public static int RoleMakerFMO()
    {
      return GetRoleValueInXML("FMO-Maker");
    }

    public static int RoleCheckerFMO()
    {
      return GetRoleValueInXML("FMO-Checker");
    }

    public static int RoleViewerCSG()
    {
      return GetPermissionValueInXML("Viewer-CSG");
    }

    public static int RoleViewerSME()
    {
      return GetPermissionValueInXML("Viewer-SME");
    }

    public static int RoleViewerCB()
    {
      return GetPermissionValueInXML("Viewer-CB");
    }

    public static int RoleAdmin()
    {
      return GetPermissionValueInXML("Admin");
    }

    public static int RoleAdminCMO()
    {
      return GetPermissionValueInXML("Admin-CMO");
    }

    public static int RoleAdminFMO()
    {
      return GetPermissionValueInXML("Admin-FMO");
    }

    public static int RoleAdminTRADE()
    {
      return GetPermissionValueInXML("Admin-TRADE");
    }

    #endregion

    #endregion

    #region XML

    public static XmlDataSource GetXmlDataSource(string usDataFile, string xmlPath)
    {
      XmlDataSource dsData = new XmlDataSource();
      dsData.DataFile = usDataFile;
      dsData.XPath = xmlPath;
      return dsData;
    }

    public static string GetXmlTemplateItem(string xmlFile, string xmlPath, string strRole)
    {
      m_xmlData = GetXmlDataSource(xmlFile, xmlPath);

      XmlNode node = m_xmlData.GetXmlDocument().SelectSingleNode(xmlPath + "[@Role='" + strRole + "']");
      if (node != null)
        return node.Attributes["Value"].Value;

      return null;
    }

    public static string GetXmlTemplateItemInner(string xmlFile, string xmlPathInner)
    {
      m_xmlData = GetXmlDataSource(xmlFile, xmlPathInner);
      return m_xmlData.GetXmlDocument().InnerText;
    }

    #endregion
  }
}
