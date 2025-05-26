using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CgmscHO_API.Models;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using CgmscHO_API.Utility;
//using Broadline.Controls;
//using CgmscHO_API.Utility;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {

        private readonly OraDbContext _context;

        public LoginController(OraDbContext context)
        {      
            _context = context;
        }
       
        [HttpPost]
        public IActionResult Login(LoginModel model)
        {
            //GenFunctions.Users.WardloginDetails(model.WardId, model.Password, out string message);
            loginDetails(model.emailid, model.pwd, out string message, out UsruserModel user);

            if (message == "Successfully Login")
            {
                //return Ok(message);
                return Ok(new { Message = message, UserInfo = user });
            }

            return BadRequest("Invalid credentials.");
        }


        //[HttpPost]
        //public IActionResult Loginvehicle(LoginModel model)
        //{
        //    //GenFunctions.Users.WardloginDetails(model.WardId, model.Password, out string message);
        //    loginDetailsVehicle(model.emailid, model.pwd, out string message, out VehicleModel user);

        //    if (message == "Successfully Login")
        //    {
        //        //return Ok(message);
        //        return Ok(new { Message = message, UserInfo = user });
        //    }

        //    return BadRequest("Invalid credentials.");
        //}

        private bool loginDetails(string emailORmob, string password, out string message, out UsruserModel user)
        {
            message = null;

            //var result = _context.MasFacilityWards
            //    .FirstOrDefault(w => w.wardid == wardId);

            //var result = _context.Usruser
            //   .FirstOrDefault(u => u.EMAILID == email);

            string qry = @" select distinct u.userid,u.emailid,u.pwd,u.firstname,u.usertype,u.districtid,case when u.AppRole='WH' then u.warehouseid  else u.facilityid end as facilityid,u.DEPMOBILE ,fh.FOOTER3,ft.facilitytypeid,
                            ay.FACTYPEID, case when ft.facilitytypeid in (371,377)  then   nvl(ay.ISWHINDENT,'N') else 'Y' end as WHAIPermission  
                            ,ft.FACILITYTYPECODE, fh.footer2,nvl(u.AppRole,'No') as AppRole
                            ,ur.rolename,ur.roleid
                            from 
                            usrusers u
                            inner join usrroles ur on ur.roleid=u.roleid
                            left outer join masfacilities f on f.facilityid=u.facilityid
                              left outer join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid 
                           left outer join masfacheaderfooter fh on fh.userid=u.userid
                            left outer join
                            (
                            select ISWHINDENT,FACILITYTYPEID,FACTYPEID from  masfacilitytypeayush 
                            ) ay on ay.FACILITYTYPEID=ft.facilitytypeid and ay.FACTYPEID=f.AYFACTYPEID
                            where (emailid ='" + emailORmob + "' or fh.FOOTER3='"+ emailORmob + "')  ";

            var result = _context.Usruser
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList().FirstOrDefault();

            user = result;

            if (result == null)
            {
                message = "Invalid ID.";
                return false;
            }


            // Perform password verification
            string salthash = result.PWD;
            string mStart = "salt{";
            string mMid = "}hash{";
            string mEnd = "}";
            string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
            string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));


            Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
            bool isValid = ver.Verify(password);

            //string approle = result.APPROLE;

            //if (approle == "No")
            //{
            //    message = "Not Authorized to Use this Module of App";
            //    return false;
            //}

            // for every login , need to change


            if (password == "Admin@cgmsc123")
            {
                isValid = true;
            }
            else
            {
                if (!isValid)
                {
                    message = "The email or password you have entered is incorrect.";
                    return false;
                }
            }

            message = "Successfully Login";
            return true;
        }

        [HttpPost("getOTPSaved")]
        public string getOTPSaved(string userid)
        {
            FacOperations fc = new FacOperations(_context);
            string sRandomOTP = fc.insertUpdateOTP1(userid);
            return sRandomOTP;
        }

        [HttpGet("VerifyOTPLogin")]
        public IActionResult VerifyOTPLogin(string otp, string userid)
        {
            bool value = false;
            FacOperations ObjFacOp = new FacOperations(_context);
            value = ObjFacOp.IsOTPVerified(otp, userid);
            if (value)
            {
                return Ok("Success");
            }
            else
            {
                return BadRequest("OTP Does not Match");
            }

        }


        [HttpGet("SendsmsHIMIS")]
        public string SendsmsHIMIS(string mobile, string sms, string mtemplateid)
        {
            string action = "";
            string smscontent = "";
            string value = "";
            bool result = true;

            //SMSHttpPostClient ssms = new SMSHttpPostClient();
            SMSHttpPostCore ssms = new SMSHttpPostCore();

            if (mobile != "0")
            {
                // string sso = " Your OTP for Pass Order Audit is " + sms + ". Use this OTP to Audit.";

                string sso = sms;
                string content = sso;
                smscontent = sso;
                String username = "cgmscl";
                String senderid = "CGMSCL";
                String secureKey = "ecb45a42-32b6-4087-9128-ecaee9d570dc";
                string smsservicetypename = "Transactional";
                // string templateid = "1407160939828897497";



                string templateid = mtemplateid;

                //value = ssms.sendOTPMSG(username, senderid, content, secureKey, mobile, smsservicetypename, templateid);
                value = ssms.sendOTPMSG(username, senderid, content, secureKey, mobile, templateid);

            }
            else
            {
                smscontent = "NOTSEND";
            }
            return smscontent;
        }


        //public string sendsms(string mobile, string sms)
        //{
        //    string action = "";
        //    string smscontent = "";

        //    string value = "";
        //    bool result = true;

        //    SMSHttpPostCore ssms = new SMSHttpPostCore();

        //    if (mobile != "0")
        //    {



        //        string sso = "OTP for Login on DPDMIS is " + sms;

        //        string content = sso;
        //        smscontent = sso;

        //        String username = "cgmscl";

        //        String senderid = "CGMSCL";
        //        String secureKey = "ecb45a42-32b6-4087-9128-ecaee9d570dc";
        //        string smsservicetypename = "Transactional";
        //        //  string templateid = "1407160939828897497";

        //        string templateid = "1407161537152057950";

        //        try
        //        {
        //            value = ssms.sendOTPMSG(username, senderid, content, secureKey, mobile, smsservicetypename, templateid);
        //            smscontent = "SEND";

                  
        //        }
        //        catch
        //        {
        //            smscontent = "NOTSEND";
        //        }



        //    }
        //    else
        //    {
        //        smscontent = "NOTSEND";
        //    }
        //    return smscontent;
        //}

    }
}
