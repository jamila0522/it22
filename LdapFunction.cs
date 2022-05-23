using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Helper.Functions
{
    public class LdapFunction
    {
        static string ldapPath = Helpers.Functions.ConfigFunctions.GetConfigByDomain("ADService");
        static string ldapDomain = Helpers.Functions.ConfigFunctions.GetConfigByDomain("LDAPDomain");
        //System.Web.Configuration.WebConfigurationManager.ConnectionStrings["LDAPDomain"].ConnectionString;
        static string adminitrator = Helpers.Functions.ConfigFunctions.GetConfigByDomain("ADAdministrator");
        static string ldapIP = Helpers.Functions.ConfigFunctions.GetConfigByDomain("LDAPIP");
        //System.Web.Configuration.WebConfigurationManager.ConnectionStrings["LDAPIP"].ConnectionString;
        static string password = Helpers.Functions.ConfigFunctions.GetConfigByDomain("ADPassword");

        public LdapFunction()
        {}

        public static void SetExpiryDate(string username, DateTime? date)
        {

            DirectoryEntry user = GetUser(username);

            if (date.HasValue)
            {
                user.Properties["accountExpires"].Value = date.Value.ToFileTime().ToString();
            }
            else
            {
                user.Properties["accountExpires"].Value = 0;
            }
            user.CommitChanges();
        }
        #region Group

        public static List<MemberOfGroupActiveDirectory> GetAllGroupOfUser(string username)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, username))
            {
                List<MemberOfGroupActiveDirectory> l = new List<MemberOfGroupActiveDirectory>();
                List<String> ListGroupName = new List<string>();
                UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                foreach (var group in user.GetGroups())
                {
                    ListGroupName.Add(group.Name);
                }
                foreach (SearchResult item in GetAllGroups())
                {
                    DirectoryEntry group = item.GetDirectoryEntry();
                    MemberOfGroupActiveDirectory m = new MemberOfGroupActiveDirectory();
                    m.GroupName = group.Name;
                    if (ListGroupName.Contains(group.Name))
                    {
                        m.IsMemberOfGroup = true;
                    }
                    else
                    {
                        m.IsMemberOfGroup = false;
                    }
                }
                return l;
            }
        }
        private static SearchResultCollection GetAllGroups()
        {
            // Enumerate groups 
            try
            {
                DirectoryEntry objADAM = default(DirectoryEntry);
                DirectorySearcher objSearchADAM = default(DirectorySearcher);
                SearchResultCollection objSearchResults = default(SearchResultCollection);

                objADAM = new DirectoryEntry("LDAP://" + ldapIP, adminitrator, password);
                objADAM.RefreshCache();
                objSearchADAM = new DirectorySearcher(objADAM);
                objSearchADAM.Filter = "(&(objectClass=group))";
                objSearchADAM.SearchScope = SearchScope.Subtree;
                objSearchResults = objSearchADAM.FindAll();
                return objSearchResults;

            }

            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        #endregion
        public static bool IsExists(string username)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, ldapIP, ldapDomain, adminitrator, password))
            {
                using (var foundUser = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username))
                {
                    return foundUser != null;
                }
            }
        }

        public static string CreateUserAccountLDAP(string userName, string displayName, string userPassword, string email)
        {
            if (!IsExists(userName))
            {
                try
                {
                    string oGUID = string.Empty;

                    DirectoryEntry dirEntry = new DirectoryEntry("LDAP://" + ldapIP, adminitrator, password);
                    DirectoryEntry newUser = dirEntry.Children.Add("CN=" + userName + ",OU=FRT", "user");
                    newUser.Properties["samAccountName"].Value = userName;
                    newUser.Properties["displayName"].Value = displayName;
                    if (ValidateUtil.IsValidEmail(email))
                    {
                        newUser.Properties["mail"].Value = email;
                        newUser.Properties["sN"].Value = email.Replace("@fpt.com.vn", "");//Vanlk thêm 29/03/2018 lastname
                    }
                    newUser.CommitChanges();
                    oGUID = newUser.Guid.ToString();

                    newUser.Invoke("SetPassword", new object[] { userPassword });
                    newUser.CommitChanges();
                    dirEntry.Close();
                    newUser.Close();

                    Enable(userName);
                    return oGUID;
                }
                catch (Exception ex)
                {
                    //log.Error(ex.Message);
                    throw new Exception(ex.Message);
                }
            }
            return String.Empty;

        }
        public static void SetEmail(string username, string email)
        {
            try
            {
                DirectoryEntry user = GetUser(username);
                if (ValidateUtil.IsValidEmail(email))
                {
                    user.Properties["mail"].Value = email;
                    user.Properties["sN"].Value = email.Replace("@fpt.com.vn", "");
                }
                user.CommitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public static void Delete(string username)
        {
            DirectoryEntry user = GetUser(username);
            DirectoryEntry ou = user.Parent;
            ou.Children.Remove(user);
            ou.CommitChanges();
        }

        public static bool ValidateUser(string username, string pass)
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, ldapIP, ldapDomain, adminitrator, password))
                {
                    bool kt = context.ValidateCredentials(username, pass);
                    //log.Info("Active directory: Login user '" + username + "' is " + kt);
                    return kt;
                }

            }
            catch (Exception ex)
            {
                //log.Error("Active directory: Login user '" + username + "' failed: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
        //OK
        public static void Enable(string username)
        {
            DirectoryEntry user = GetUser(username);
            int val = (int)user.Properties["userAccountControl"].Value;
            user.Properties["userAccountControl"].Value = val & ~0x2;
            //ADS_UF_NORMAL_ACCOUNT;

            user.CommitChanges();
            user.Close();

        }
        //OK
        public static void Disable(string username)
        {
            DirectoryEntry user = GetUser(username);
            int val = (int)user.Properties["userAccountControl"].Value;
            user.Properties["userAccountControl"].Value = val | 0x2;
            //ADS_UF_ACCOUNTDISABLE;

            user.CommitChanges();
            user.Close();

        }
        public static void SetPasswordNeverExpires(string username, bool PasswordNeverExpires)
        {
            DirectoryEntry user = GetUser(username);
            int val = (int)user.Properties["userAccountControl"].Value;
            if (PasswordNeverExpires)
            {
                user.Properties["userAccountControl"].Value = val | 0x10000;
            }
            else
            {
                user.Properties["userAccountControl"].Value = val ^ 0x10000;
            }
            user.CommitChanges();
            user.Close();
        }
        public static void Unlock(string username)
        {

            DirectoryEntry user = GetUser(username);
            user.Properties["LockOutTime"].Value = 0; //unlock account

            user.CommitChanges(); //may not be needed but adding it anyways

            user.Close();

        }
        public static void ResetPassword(string username, string newPassword)
        {
            DirectoryEntry user = GetUser(username);
            user.Invoke("SetPassword", new object[] { newPassword });
            user.Properties["LockOutTime"].Value = 0; //unlock account
            user.CommitChanges(); //may not be needed but adding it anyways
            user.Close();
        }
        public static void Rename(string username, string newName)
        {
            DirectoryEntry user = GetUser(username);
            user.Rename("CN=" + newName);
        }

        public static DirectoryEntry GetUser(string username)
        {
            try
            {
                //DirectoryEntry user = new DirectoryEntry(ldapPath + "/CN=" + username + ",OU=FRT," + ldapDomain, adminitrator, password);
                DirectoryEntry user = (DirectoryEntry)GetUserPrincipal(username).GetUnderlyingObject();

                return user;
            }
            catch
            {
                return null;
            }
        }
        public static UserPrincipal GetUserPrincipal(string username)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, ldapIP, ldapDomain, adminitrator, password))
            {
                UserPrincipal foundUser = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, username);
                return foundUser;

            }
        }
        public static ArrayList Groups()
        {
            ArrayList groups = new ArrayList();
            foreach (System.Security.Principal.IdentityReference group in
                System.Web.HttpContext.Current.Request.LogonUserIdentity.Groups)
            {
                groups.Add(group.Translate(typeof(System.Security.Principal.NTAccount)).ToString());
            }
            return groups;
        }
    }

    public class ValidateUtil
    {
        public static bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return false;
            }
            // source: http://thedailywtf.com/Articles/Validating_Email_Addresses.aspx
            Regex rx = new Regex(
            @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(email);
        }
    }

    public class MemberOfGroupActiveDirectory
    {
        public string GroupName { get; set; }
        public bool IsMemberOfGroup { get; set; }
    }
    public class UserForList
    {
        public string SamAccountName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public DateTime? ExpireDate { get; set; }

    }
}
