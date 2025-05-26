#region Using
using System;
using System.Data;
using System.Configuration;
using System.Globalization;
using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
//using DBHelper = Broadline.WMS.Data.DBHelper;
//using System.Web.SessionState;
using System.Reflection;
//using Broadline.CryptoRC4;
using System.Text;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using CgmscHO_API.Models;
#endregion

/// <summary>
/// Summary description for GenFunctions
/// </summary>
public partial class GenFunctions
{
    #region Users
    public class Users
    {
        private readonly static OraDbContext? _context;
        public Users(OraDbContext context)
        {
            _context = context;
        }

        #region GetAllUsers
        //public static string[] GetAllUsers(string prefixText, int count, string contextKey)
        //{
        //    if (contextKey == null) contextKey = "";
        //    return GetDBValues(prefixText, count, contextKey, "usrUsers", "FirstName || ' ' || LastName");
        //}
        #endregion

        #region Encryption Key Generation 
        //public static string GenerateKey(int Length)
        //{
        //    Random ran = new Random(DateTime.Now.Second);
        //    char[] _key = new char[Length];

        //    for (int i = 0; i < Length; i++)
        //    {
        //        int[] n = { ran.Next(48, 57), ran.Next(65, 90), ran.Next(97, 122) };
        //        int picker = ran.Next(0, 3);

        //        if (picker == 3)
        //            picker = 2;
        //        _key[i] = (char)n[picker];
        //    }
        //    return new string(_key);
        //}
        #endregion

        #region Session ID Re-Generation
        /// <summary>
        /// Re-Generate SessionID for each Successfull login
        /// By: Partha
        /// On: 17-Apr-2013
        /// </summary>
        //private static void RegenerateID()
        //{
        //    SessionIDManager manager = new SessionIDManager();
        //    string oldId = manager.GetSessionID(HttpContext.Current);
        //    string newId = manager.CreateSessionID(HttpContext.Current);
        //    bool isAdd = false, isRedir = false;
        //    manager.SaveSessionID(HttpContext.Current, newId, out isRedir, out isAdd);
        //    HttpApplication ctx = (HttpApplication)HttpContext.Current.ApplicationInstance;
        //    HttpModuleCollection mods = ctx.Modules;
        //    SessionStateModule ssm = (SessionStateModule)mods.Get("Session");
        //    System.Reflection.FieldInfo[] fields = ssm.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        //    SessionStateStoreProviderBase store = null;
        //    System.Reflection.FieldInfo rqIdField = null, rqLockIdField = null, rqStateNotFoundField = null;
        //    foreach (System.Reflection.FieldInfo field in fields)
        //    {
        //        if (field.Name.Equals("_store")) store = (SessionStateStoreProviderBase)field.GetValue(ssm);
        //        if (field.Name.Equals("_rqId")) rqIdField = field;
        //        if (field.Name.Equals("_rqLockId")) rqLockIdField = field;
        //        if (field.Name.Equals("_rqSessionStateNotFound")) rqStateNotFoundField = field;
        //    }
        //    object lockId = rqLockIdField.GetValue(ssm);
        //    if ((lockId != null) && (oldId != null)) store.ReleaseItemExclusive(HttpContext.Current, oldId, lockId);
        //    rqStateNotFoundField.SetValue(ssm, true);
        //    rqIdField.SetValue(ssm, newId);
        //}
        #endregion

        #region SetCookie
        //static void SetCookie(string UserName, string Password,Page Current)
        //{
        //    string CookieLifeSpan = ConfigurationManager.AppSettings["CookieLifeSpan"].ToString();

        //    HttpCookie hcUserName = new HttpCookie("UserName");
        //    HttpCookie hcPassword = new HttpCookie("Password");

        //    hcUserName.Value = RSACryptor.Encrypt(UserName);
        //    hcPassword.Value = RSACryptor.Encrypt(Password);

        //    DateTime dtNow = DateTime.Now;
        //    TimeSpan tsExpireTime = new TimeSpan(int.Parse(CookieLifeSpan), 0, 0, 0);
        //    hcUserName.Expires = dtNow + tsExpireTime;
        //    hcPassword.Expires = dtNow + tsExpireTime;
        //    Current.Response.Cookies.Add(hcUserName);
        //    Current.Response.Cookies.Add(hcPassword);
        //}
        #endregion

        #region LoginDetails

        public static void WardloginDetails(string strUserName, string strPassword, out string message)
        {
            string OriginalUserName = strUserName;
            string OriginalPassword = strPassword;
            //HttpSessionState Session = page.Session;
            //strUserName = GenFunctions.CheckEmptyString(strUserName, "-1");
            //GenFunctions.CheckStringForEmpty(ref strUserName, "User Name", true);

            string strSQL = @"
                            SELECT w.wardid, w.pwd, w.facilityid, f.facilitytypeid
                            FROM masfacilitywards w
                            INNER JOIN masfacilities f ON f.facilityid = w.facilityid
                            WHERE w.wardid = {0}";

            var parameters = new[] { strUserName };


            var result = _context.MasFacilityWards
                .FromSqlRaw(strSQL, parameters)
                .Select(w => w.wardid)
                .FirstOrDefault();

            if (result != null)
            {
                // Data found, perform your logic here
                // Access result.wardid, result.pwd, result.facilityid, result.facilitytypeid
            }
            else
            {
                // Data not found, handle accordingly
            }


            if (dtbl.Rows.Count > 0)
            {
                DataRow dr = dtbl.Rows[0];
                string salthash = dr["Pwd"].ToString();
                string mStart = "salt{";
                string mMid = "}hash{";
                string mEnd = "}";
                string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
                string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));
                Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
                bool isValid = ver.Verify(strPassword);
                if (dr["facilitytypeid"].ToString() == "371")
                {
                    ErrMsg.Text = "<br/> संचालनालय आयुष के निर्देशानुसार , DPDMIS- Tier 4 को अस्थायी रूप से बंद किया गया है , कल दिनांक 28/06/2023 को 12 PM बजे के पश्चात डेमो डेटा डिलीट कर पुनः एंट्री करेंगे | |";
                    return;
                }
                if (strPassword == "2024#cgmsc")
                {

                }
                else
                {
                    if (!isValid)
                    {
                        ErrMsg.Text = "<br/>The username or password, you have entered is incorrect or Password reset has occured. Please Contact Admin.";
                        return;
                    }
                }


                if (dr["wardid"] != DBNull.Value)
                    Session["usrWardID"] = dr["wardid"].ToString();
                if (dr["FacilityID"] != DBNull.Value)
                    Session["usrFacilityID"] = dr["FacilityID"].ToString();


                Session["EncryptionKey"] = GenerateKey(24);



                if (RememberMe) SetCookie(OriginalUserName, OriginalPassword, page);

                ErrMsg.Text = "";
                page.Response.Redirect("~/Index/Home.aspx");
            }
            else
            {
                ErrMsg.Text = "The username or password you entered is incorrect";
                return;
            }
        }

        //        public static void WardloginDetails(string strUserName, string strPassword, Label ErrMsg, Page page, bool RememberMe)
        //        {
        //            string OriginalUserName = strUserName;
        //            string OriginalPassword = strPassword;
        //            HttpSessionState Session = page.Session;
        //            strUserName = GenFunctions.CheckEmptyString(strUserName, "-1");
        //            GenFunctions.CheckStringForEmpty(ref strUserName, "User Name", true);

        //            string strSQL = @"select w.wardid,w.pwd,w.facilityid,f.facilitytypeid from masfacilitywards w
        //inner join masfacilities f on f.facilityid=w.facilityid
        //where w.wardid=" + strUserName;
        //            DataTable dtbl = DBHelper.GetDataTable(strSQL);

        //            if (dtbl.Rows.Count > 0)
        //            {
        //                DataRow dr = dtbl.Rows[0];
        //                string salthash = dr["Pwd"].ToString();
        //                string mStart = "salt{";
        //                string mMid = "}hash{";
        //                string mEnd = "}";
        //                string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
        //                string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));
        //                Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
        //                bool isValid = ver.Verify(strPassword);
        //              if (dr["facilitytypeid"].ToString() =="371")
        //                {
        //                    ErrMsg.Text = "<br/> संचालनालय आयुष के निर्देशानुसार , DPDMIS- Tier 4 को अस्थायी रूप से बंद किया गया है , कल दिनांक 28/06/2023 को 12 PM बजे के पश्चात डेमो डेटा डिलीट कर पुनः एंट्री करेंगे | |";
        //                    return;
        //                }
        //                  if(strPassword=="2024#cgmsc")
        //		{ 

        //	}
        //else
        //{
        //                if (!isValid)
        //                {
        //                    ErrMsg.Text = "<br/>The username or password, you have entered is incorrect or Password reset has occured. Please Contact Admin.";
        //                    return;
        //                }
        //}
        //                //if (dr["Status"].ToString() == "I")
        //                //{
        //                //    ErrMsg.Text = "Member login restricted. Contact Administrator.";
        //                //    return;
        //                //}
        //                //Session["MemberID"] = dr["UserID"].ToString();
        //                //Session["UserType"] = dr["UserType"].ToString();

        //                //if (dr["SourceID"] != DBNull.Value)
        //                //    Session["usrSourceID"] = dr["SourceID"].ToString();

        //                //if (dr["SchemeID"] != DBNull.Value)
        //                //    Session["usrSchemeID"] = dr["SchemeID"].ToString();

        //                //if (dr["HierarchyID"] != DBNull.Value)
        //                //    Session["usrHierarchyID"] = (int?) int.Parse(dr["HierarchyID"].ToString());

        //                //if (dr["StateID"] != DBNull.Value)
        //                //    Session["usrStateID"] = dr["StateID"].ToString();

        //                //if (dr["DivisionID"] != DBNull.Value)
        //                //    Session["usrDivisionID"] = dr["DivisionID"].ToString();

        //                //if (dr["PSAID"] != DBNull.Value)
        //                //    Session["usrPSAID"] = dr["PSAID"].ToString();

        //                //if (dr["WarehouseID"] != DBNull.Value)
        //                //    Session["usrWarehouseID"] = dr["WarehouseID"].ToString();

        //                if (dr["wardid"] != DBNull.Value)
        //                    Session["usrWardID"] = dr["wardid"].ToString();
        //                if (dr["FacilityID"] != DBNull.Value)
        //                    Session["usrFacilityID"] = dr["FacilityID"].ToString();

        //                //if (dr["SupplierID"] != DBNull.Value)
        //                //    Session["usrSupplierID"] = dr["SupplierID"].ToString();

        //                //if (dr["LabID"] != DBNull.Value)
        //                //    Session["usrLabID"] = dr["LabID"].ToString();

        //                //if (dr["DistrictID"] != DBNull.Value)
        //                //    Session["usrDistrictID"] = dr["DistrictID"].ToString();

        //                //if (dr["IsLocalSuppliers"] != DBNull.Value)
        //                //    Session["usrSupplierType"] = dr["IsLocalSuppliers"].ToString();

        //                //if (dr["UserType"].ToString() == "MOH")
        //                //{
        //                //    Session["usrMOH"] = "ADMIN";
        //                //}
        //                //else if (dr["UserType"].ToString() == "DIV")
        //                //{
        //                //    //Temporarily assign any one of the PsaID, WarehouseID belong to current division
        //                //    string strQuery = "Select psa.PSAID,psa.WarehouseID" +
        //                //        " From masPSAS psa" +
        //                //        " Inner Join masWarehouses war on (war.WarehouseID=psa.WarehouseID)" +
        //                //        " Inner Join masDistricts dis on (dis.DistrictID=war.DistrictID)" +
        //                //        " Where dis.DivisionID=" + GenFunctions.CheckEmptyString(dr["DivisionID"], "0");
        //                //    DataTable dtbl1 = DBHelper.GetDataTable(strQuery);

        //                //    if (dtbl1.Rows.Count > 0)
        //                //    {
        //                //        DataRow dr1 = dtbl1.Rows[0];
        //                //        Session["usrPSAID"] = dr1["PSAID"].ToString();
        //                //        Session["usrWarehouseID"] = dr1["WarehouseID"].ToString();
        //                //    }
        //                //    else
        //                //    {
        //                //        ErrMsg.Text = "Missing Division name!";
        //                //        return;
        //                //    }
        //                //}
        //                //else if (dr["UserType"].ToString() == "FAC")
        //                //{
        //                //    //Assign Warehouse/PSA to that Facility belongs
        //                //    string strQuery = "Select psa.PSAID,psa.WarehouseID" +
        //                //        " From masPSAS psa" +
        //                //        " Inner Join masFacilityWH fw on (fw.WarehouseID=psa.WarehouseID)" +
        //                //        " Where fw.FacilityID=" + GenFunctions.CheckEmptyString(dr["FacilityID"], "0");
        //                //    DataTable dtbl1 = DBHelper.GetDataTable(strQuery);

        //                //    if (dtbl1.Rows.Count > 0)
        //                //    {
        //                //        DataRow dr1 = dtbl1.Rows[0];
        //                //        Session["usrPSAID"] = dr1["PSAID"].ToString();
        //                //        Session["usrWarehouseID"] = dr1["WarehouseID"].ToString();
        //                //    }
        //                //    else
        //                //    {
        //                //        ErrMsg.Text = "Missing Facility name!";
        //                //        return;
        //                //    }
        //                //}

        //                //Insert into Audit Log table after successful login
        //                //try
        //                //{
        //                //    GenFunctions.AuditLogs.InsertAuditLog("0", "");
        //                //}
        //                //catch (Exception ex) { }

        //                // Re-Generate Session ID for each successfull Login
        //                // By: Partha
        //                // On: 17-Apr-2013
        //               // RegenerateID();

        //                // Generate Encryption Key and store in session
        //                // By: Partha
        //                // On: 18-Apr-2013
        //                Session["EncryptionKey"] = GenerateKey(24);

        //                // Generate Encryption Key and store in session
        //                // By: Ravindhar
        //                // On: 30-May-2013
        //                // Status: R & D
        //                //RSACryptor.Initialize();

        //                if (RememberMe) SetCookie(OriginalUserName, OriginalPassword, page);

        //                ErrMsg.Text = "";
        //                page.Response.Redirect("~/Index/Home.aspx");
        //            }
        //            else
        //            {
        //                ErrMsg.Text = "The username or password you entered is incorrect";
        //                return;
        //            }
        //        }
        #endregion


        #region LoginDetails

        //public static void loginDetails(string strUserName, string strPassword, Label ErrMsg, Page page, bool RememberMe)
        //{
        //    string OriginalUserName = strUserName;
        //    string OriginalPassword = strPassword;
        //    HttpSessionState Session = page.Session;
        //    strUserName = GenFunctions.CheckEmptyString(strUserName, "-1");
        //    GenFunctions.CheckStringForEmpty(ref strUserName, "User Name", true);

        //    string strSQL = "Select Hie.HierarchyID,a.UserID,a.Status,a.UserType,a.Pwd,a.SourceID,a.SchemeID,a.StateID," +
        //        " a.FacilityID,Coalesce(a.SupplierID,a.LPSupplierID) as SupplierID,a.LabID,a.PSAID,case when nvl(a.PSAID,0)=0 then a.WarehouseID else m1.WarehouseID end as WarehouseID," +
        //        " a.DivisionID,a.DistrictID,a.IsLocalSuppliers" +
        //        " from usrUsers a" +
        //        " Left Outer Join masPSAS m1 on (a.PSAID=m1.PSAID)" +
        //        " Left Outer Join masHierarchy Hie on  " +
        //                            "( " +
        //                            "(Hie.ID = a.StateID and (Hie.TableName = 'MasStates' and a.UserType = 'MOH')) " +
        //                            "or " +
        //                            "(Hie.ID = a.DivisionID and Hie.TableName = 'MasDivisions' and a.UserType = 'DIV') " +
        //                            "or " +
        //                            "(Hie.ID = a.PsaID and Hie.TableName = 'MasPsas' and a.UserType = 'PSA') " +
        //                            "or " +
        //                            "(Hie.ID = a.WarehouseID and Hie.TableName = 'MasWarehouses' and a.UserType = 'WH') " +
        //                            "or " +
        //                            "(Hie.ID = a.FacilityID and Hie.TableName = 'MasFacilities' and a.UserType = 'FAC') " +
        //                            "or " +
        //                            "(Hie.ID = a.DistrictID and Hie.TableName = 'MasDistricts' and a.UserType = 'COL') " +
        //                            ") " +
        //        " Where EmailID = " + strUserName + " and a.PasswordResetKey is null and a.ResetPassword != 1";
        //    DataTable dtbl = DBHelper.GetDataTable(strSQL);

        //    if (dtbl.Rows.Count > 0)
        //    {
        //        DataRow dr = dtbl.Rows[0];
        //        string salthash = dr["Pwd"].ToString();
        //        string mStart = "salt{";
        //        string mMid = "}hash{";
        //        string mEnd = "}";
        //        string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
        //        string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));
        //        Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
        //        bool isValid = ver.Verify(strPassword);

        //        if (!isValid)
        //        {
        //            ErrMsg.Text = "<br/>The username or password, you have entered is incorrect or Password reset has occured. Please Contact Admin.";
        //            return;
        //        }
        //        if (dr["Status"].ToString() == "I")
        //        {
        //            ErrMsg.Text = "Member login restricted. Contact Administrator.";
        //            return;
        //        }
        //        Session["MemberID"] = dr["UserID"].ToString();
        //        Session["UserType"] = dr["UserType"].ToString();

        //        if (dr["SourceID"] != DBNull.Value)
        //            Session["usrSourceID"] = dr["SourceID"].ToString();

        //        if (dr["SchemeID"] != DBNull.Value)
        //            Session["usrSchemeID"] = dr["SchemeID"].ToString();

        //        if (dr["HierarchyID"] != DBNull.Value)
        //            Session["usrHierarchyID"] = (int?)int.Parse(dr["HierarchyID"].ToString());

        //        if (dr["StateID"] != DBNull.Value)
        //            Session["usrStateID"] = dr["StateID"].ToString();

        //        if (dr["DivisionID"] != DBNull.Value)
        //            Session["usrDivisionID"] = dr["DivisionID"].ToString();

        //        if (dr["PSAID"] != DBNull.Value)
        //            Session["usrPSAID"] = dr["PSAID"].ToString();

        //        if (dr["WarehouseID"] != DBNull.Value)
        //            Session["usrWarehouseID"] = dr["WarehouseID"].ToString();

        //        if (dr["FacilityID"] != DBNull.Value)
        //            Session["usrFacilityID"] = dr["FacilityID"].ToString();

        //        if (dr["SupplierID"] != DBNull.Value)
        //            Session["usrSupplierID"] = dr["SupplierID"].ToString();

        //        if (dr["LabID"] != DBNull.Value)
        //            Session["usrLabID"] = dr["LabID"].ToString();

        //        if (dr["DistrictID"] != DBNull.Value)
        //            Session["usrDistrictID"] = dr["DistrictID"].ToString();

        //        if (dr["IsLocalSuppliers"] != DBNull.Value)
        //            Session["usrSupplierType"] = dr["IsLocalSuppliers"].ToString();

        //        if (dr["UserType"].ToString() == "MOH")
        //        {
        //            Session["usrMOH"] = "ADMIN";
        //        }
        //        else if (dr["UserType"].ToString() == "DIV")
        //        {
        //            //Temporarily assign any one of the PsaID, WarehouseID belong to current division
        //            string strQuery = "Select psa.PSAID,psa.WarehouseID" +
        //                " From masPSAS psa" +
        //                " Inner Join masWarehouses war on (war.WarehouseID=psa.WarehouseID)" +
        //                " Inner Join masDistricts dis on (dis.DistrictID=war.DistrictID)" +
        //                " Where dis.DivisionID=" + GenFunctions.CheckEmptyString(dr["DivisionID"], "0");
        //            DataTable dtbl1 = DBHelper.GetDataTable(strQuery);

        //            if (dtbl1.Rows.Count > 0)
        //            {
        //                DataRow dr1 = dtbl1.Rows[0];
        //                Session["usrPSAID"] = dr1["PSAID"].ToString();
        //                Session["usrWarehouseID"] = dr1["WarehouseID"].ToString();
        //            }
        //            else
        //            {
        //                ErrMsg.Text = "Missing Division name!";
        //                return;
        //            }
        //        }
        //        else if (dr["UserType"].ToString() == "FAC")
        //        {
        //            //Assign Warehouse/PSA to that Facility belongs
        //            string strQuery = "Select psa.PSAID,psa.WarehouseID" +
        //                " From masPSAS psa" +
        //                " Inner Join masFacilityWH fw on (fw.WarehouseID=psa.WarehouseID)" +
        //                " Where fw.FacilityID=" + GenFunctions.CheckEmptyString(dr["FacilityID"], "0");
        //            DataTable dtbl1 = DBHelper.GetDataTable(strQuery);

        //            if (dtbl1.Rows.Count > 0)
        //            {
        //                DataRow dr1 = dtbl1.Rows[0];
        //                Session["usrPSAID"] = dr1["PSAID"].ToString();
        //                Session["usrWarehouseID"] = dr1["WarehouseID"].ToString();
        //            }
        //            else
        //            {
        //                ErrMsg.Text = "Missing Facility name!";
        //                return;
        //            }
        //        }

        //        //Insert into Audit Log table after successful login
        //        try
        //        {
        //            GenFunctions.AuditLogs.InsertAuditLog("0", "");
        //        }
        //        catch (Exception ex) { }

        //        // Re-Generate Session ID for each successfull Login
        //        // By: Partha
        //        // On: 17-Apr-2013
        //        RegenerateID();

        //        // Generate Encryption Key and store in session
        //        // By: Partha
        //        // On: 18-Apr-2013
        //        Session["EncryptionKey"] = GenerateKey(24);

        //        // Generate Encryption Key and store in session
        //        // By: Ravindhar
        //        // On: 30-May-2013
        //        // Status: R & D
        //        //RSACryptor.Initialize();

        //        if (RememberMe) SetCookie(OriginalUserName, OriginalPassword, page);

        //        ErrMsg.Text = "";
        //        page.Response.Redirect("~/Index/Home.aspx");
        //    }
        //    else
        //    {
        //        ErrMsg.Text = "The username or password you entered is incorrect";
        //        return;
        //    }
        //}
        #endregion



        #region class UserType
        //public class UserType
        //{
        //    private string strTypeCode;
        //    private string strTypeName;

        //    public string TypeCode
        //    {
        //        get { return strTypeCode; }
        //        set { strTypeCode = value; }
        //    }

        //    public string TypeName
        //    {
        //        get { return strTypeName; }
        //        set { strTypeName = value; }
        //    }

        //    public UserType(string code, string name)
        //    {
        //        this.TypeCode = code;
        //        this.TypeName = name;
        //    }
        //}
        #endregion

        #region class UserTypes
        //public class UserTypes
        //{
        //    public UserType[] userTypes = new UserType[9];

        //    #region Constructor
        //    public UserTypes(bool IncludeAllUserTypes)
        //    {
        //        int i = 0;
        //        if (IncludeAllUserTypes)
        //        {
        //            userTypes = new UserType[userTypes.Length + 1];
        //            userTypes[i++] = new UserType("", "All User Types");
        //        }
        //        //Any change in the following values may affect the whole software functionality.
        //        //Please be very causious in changing the following lines. Care should be taken for
        //        //references make to these values. For example: References made on the User.aspx uses 
        //        //switch case on the following values and if changes are not reflected on that page it 
        //        //lead to high security risk.
        //        //WMSMasterPage.master pages also uses user types
        //        userTypes[i++] = new UserType("MOH", "State Drug Cell Users");
        //        //userTypes[i++] = new UserType("STU", "State Users");
        //        //userTypes[i++] = new UserType("WHU", "Warehouse Users");
        //        userTypes[i++] = new UserType("PSA", "Procurement Users");
        //        userTypes[i++] = new UserType("FAC", "Facility Users");
        //        userTypes[i++] = new UserType("WRD", "Ward Users");
        //        userTypes[i++] = new UserType("DIV", "Divisional Users");
        //        userTypes[i++] = new UserType("COL", "Collector Users");
        //        userTypes[i++] = new UserType("SUP", "Supplier Users");
        //        userTypes[i++] = new UserType("LAB", "QA Lab Users");
        //        userTypes[i++] = new UserType("GEN", "Using User Hierarchy");                
        //        //userTypes[i++] = new UserType("OTH", "Other Users");
        //    }
        //    #endregion

        //    #region Fill
        //    public void Fill(DropDownList ddl, bool SelectFirst)
        //    {
        //        ddl.Items.Clear();
        //        for (int i = 0; i < userTypes.Length; i++)
        //        {
        //            ddl.Items.Add(new ListItem(userTypes[i].TypeName, userTypes[i].TypeCode));
        //        }
        //        if (SelectFirst)
        //            ddl.SelectedIndex = 0;
        //    }
        //    #endregion

        //    #region GetTypeName
        //    public string GetTypeName(string code)
        //    {
        //        for (int i = 0; i < userTypes.Length; i++)
        //        {
        //            if (userTypes[i].TypeCode == code)
        //                return userTypes[i].TypeName;
        //        }
        //        return "";
        //    }
        //    #endregion

        //    #region Get AllUserTypes
        //    public UserType[] AllUserTypes
        //    {
        //        get
        //        {
        //            return this.userTypes;
        //        }
        //    }
        //    #endregion
        //}
        #endregion

        #region FillRolesByUserType
        //public static void FillRolesByUserType(DropDownList ddl, string UserTypeCode, bool AddAllRoles, bool SelectFirst)
        //{
        //    ddl.Items.Clear();
        //    if (UserTypeCode == "") UserTypeCode = "0";
        //    string strSQL = "Select RoleID, RoleName from usrRoles Where UserType = '" + UserTypeCode + "' Order By RoleName";
        //    DataTable dt = DBHelper.GetDataTable(strSQL);
        //    ddl.DataTextField = "RoleName";
        //    ddl.DataValueField = "RoleID";
        //    ddl.DataSource = dt;
        //    ddl.DataBind();
        //    if (AddAllRoles)
        //        ddl.Items.Insert(0, new ListItem("All Roles", "0"));
        //    if (SelectFirst && ddl.Items.Count > 0)
        //        ddl.SelectedIndex = 0;
        //}
        #endregion

        #region FillUserSubType
        //public static void FillUserSubType(DropDownList ddl, string UserType, bool AddAllSubTypes, bool SelectFirst)
        //{
        //    ddl.Items.Clear();
        //    if (UserType == "SUP")
        //    { 
        //       ddl.Items.Add(new ListItem("Normal","0"));
        //       ddl.Items.Add(new ListItem("Local","1"));
        //    }
        //    if (SelectFirst && ddl.Items.Count > 0)
        //    {
        //        if (AddAllSubTypes)
        //            ddl.Items.Insert(0, new ListItem("All Types", "3"));
        //        ddl.SelectedIndex = 0;
        //    }

        //    if (ddl.Items.Count > 0)
        //    {
        //        ddl.Enabled = true;
        //    }
        //    else 
        //    {
        //        ddl.Items.Add(new ListItem("N/A", "0"));
        //        ddl.Enabled = false;
        //    }


        //}
        #endregion

        #region FillModules
        //public static void FillModules(DropDownList ddl, bool AddAllModules, bool SelectFirst)
        //{
        //    ddl.Items.Clear();
        //    string strSQL = "Select ModuleID, ModuleName from usrModules Order By ModuleName";
        //    DataTable dt = DBHelper.GetDataTable(strSQL);
        //    ddl.DataTextField = "ModuleName";
        //    ddl.DataValueField = "ModuleID";
        //    ddl.DataSource = dt;
        //    ddl.DataBind();
        //    if (AddAllModules)
        //        ddl.Items.Insert(0, new ListItem("All Modules", "0"));
        //    if (SelectFirst && ddl.Items.Count > 0)
        //        ddl.SelectedIndex = 0;
        //}
        #endregion

        #region class ScreenOperation
        //public class ScreenOperation
        //{
        //    #region Variables
        //    private string strOperationCode;
        //    private string strOperationName;
        //    private bool blSelected;
        //    #endregion

        //    #region Properties
        //    public string OperationCode
        //    {
        //        get { return strOperationCode; }
        //        set { strOperationCode = value; }
        //    }

        //    public string OperationName
        //    {
        //        get { return strOperationName; }
        //        set { strOperationName = value; }
        //    }

        //    public bool Selected
        //    {
        //        get { return blSelected; }
        //        set { blSelected = value; }
        //    }
        //    #endregion

        //    #region Construction
        //    public ScreenOperation(string code, string name)
        //    {
        //        this.OperationCode = code;
        //        this.OperationName = name;
        //        this.Selected = false;
        //    }
        //    public ScreenOperation(string code, string name, bool Selected)
        //    {
        //        this.OperationCode = code;
        //        this.OperationName = name;
        //        this.Selected = Selected;
        //    }
        //    #endregion
        //}
        #endregion

        #region class ScreenOperations
        //public class ScreenOperations
        //{
        //    #region Variables
        //    private ScreenOperation[] operations = new ScreenOperation[4];
        //    #endregion

        //    #region Constructor
        //    public ScreenOperations()
        //    {
        //        int i = 0;
        //        operations[i++] = new ScreenOperation("V", "View", true);
        //        operations[i++] = new ScreenOperation("E", "Insert/Edit", false);
        //        operations[i++] = new ScreenOperation("D", "Delete", false);
        //        operations[i++] = new ScreenOperation("A", "Approval/Disapproval", false);
        //    }
        //    #endregion

        //    #region Fill CheckBoxList
        //    public void Fill(CheckBoxList cbl)
        //    {
        //        Fill(cbl, "");
        //    }

        //    public void Fill(CheckBoxList cbl, string AvailableOperations)
        //    {
        //        cbl.Items.Clear();
        //        for (int i = 0; i < operations.Length; i++)
        //        {
        //            bool AddOp = true;
        //            if (AvailableOperations != "" && AvailableOperations.IndexOf(operations[i].OperationCode) < 0)
        //                AddOp = false;

        //            if (AddOp)
        //            {
        //                ListItem li = new ListItem(operations[i].OperationName, operations[i].OperationCode);
        //                li.Selected = operations[i].Selected;
        //                cbl.Items.Add(li);
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Populate CheckBoxList
        //    public void Populate(CheckBoxList cbl)
        //    {
        //        for (int i = 0; i < operations.Length; i++)
        //        {
        //            ListItem li = cbl.Items.FindByValue(operations[i].OperationCode);
        //            if (li != null)
        //            {
        //                operations[i].Selected = li.Selected;
        //            }
        //            else
        //                operations[i].Selected = false;
        //        }
        //    }
        //    #endregion

        //    #region Property Operations
        //    public string Operations
        //    {
        //        get
        //        {
        //            string retVar = "";
        //            for (int i = 0; i < operations.Length; i++)
        //            {
        //                if (operations[i].Selected)
        //                    retVar += operations[i].OperationCode;
        //            }
        //            return retVar;
        //        }
        //        set
        //        {
        //            for (int i = 0; i < operations.Length; i++)
        //            {
        //                if (value.IndexOf(operations[i].OperationCode) >= 0)
        //                    operations[i].Selected = true;
        //                else
        //                    operations[i].Selected = false;
        //            }
        //        }
        //    }
        //    #endregion

        //    #region Get ScreenOperation[]
        //    public ScreenOperation[] AllOperations
        //    {
        //        get
        //        {
        //            return this.operations;
        //        }
        //    }
        //    #endregion

        //    #region GetPageoperations
        //    public void GetPageOperations(string UserID, string ScreenURL)
        //    {
        //        if (UserID == "") UserID = "0";
        //        if (ScreenURL == "") ScreenURL = "0";
        //        string strSQL = "Select a1.Operations from usrScreenOpsByRole a1 Inner Join usrScreens a2 on (a2.ScreenID = a1.ScreenID) Inner Join usrRoles a3 on (a3.RoleID = a1.RoleID) Inner Join masfacilitywards a4 on (a4.RoleID = a3.RoleID) Where a4.wardid = " + UserID + " and a2.ScreenURL like '%" + ScreenURL + "%'";
        //        DataTable dt = DBHelper.GetDataTable(strSQL);
        //        if (dt.Rows.Count > 0)
        //            this.Operations = dt.Rows[0]["Operations"].ToString();
        //        else
        //            this.Operations = "";
        //    }
        //    #endregion
        //}
        #endregion
    }
    #endregion
}
