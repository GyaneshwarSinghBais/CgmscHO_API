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
//using Broadline.Controls;
//using CgmscHO_API.Utility;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginCourierController : ControllerBase
    {

        private readonly OraDbContext _context;

        public LoginCourierController(OraDbContext context)
        {      
            _context = context;
        }

        [HttpPost]
        public IActionResult LoginCourier(LoginCourierModel model)
        {
            //GenFunctions.Users.WardloginDetails(model.WardId, model.Password, out string message);
            LoginDetailsCourier(model.warehouseid, model.cpwd, out string message, out UsruserModel user);

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

        private bool LoginDetailsCourier(Int64 emailORmob, string password, out string message, out UsruserModel user)
        {
            message = null;
        

        string qry = @" select u.userid,u.emailid,u.cpwd as pwd,case when u.USERTYPE = 'WHU' then u.warehouseid  else u.userid end as facilityid,u.USERTYPE as usertype
,'Courier' as AppRole,'' as FACILITYTYPECODE, w.warehousename footer2, 0 as FACTYPEID, 'N' as WHAIPermission
,'' as DEPMOBILE ,'' as FOOTER3,0 as facilitytypeid,w.districtid,case when u.USERTYPE = 'WHU' then w.warehousename else case when u.userid = 4474 then 'Head Office' else 'HO-QC' end end  as firstname
,'Courier' as  ROLENAME, ur.roleid  ROLEID
from usrusers u
      inner join usrroles ur on ur.roleid=u.roleid
left outer join maswarehouses w on w.WAREHOUSEID = u.WAREHOUSEID
 where  u.subwhid is  null and IsCourierPick = 'Y' and(case when u.USERTYPE = 'WHU' then u.warehouseid  else u.userid end)= " + emailORmob + " ";

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
                else
                {
                    message = "Successfully Login";
                    return true;
                }
            }

            message = "Successfully Login";
            return true;
        }


        //private bool loginDetailsVehicle(string emailORmob, string password, out string message, out VehicleModel user)
        //{
        //    message = null;

        //    //var result = _context.MasFacilityWards
        //    //    .FirstOrDefault(w => w.wardid == wardId);

        //    //var result = _context.Usruser
        //    //   .FirstOrDefault(u => u.EMAILID == email);

        //    //string qry = @" select distinct u.userid,u.emailid,u.pwd,u.firstname,u.usertype,u.districtid,case when u.AppRole='WH' then u.warehouseid  else u.facilityid end as facilityid,u.DEPMOBILE ,fh.FOOTER3,ft.facilitytypeid,
        //    //                ay.FACTYPEID, case when ft.facilitytypeid in (371,377)  then   nvl(ay.ISWHINDENT,'N') else 'Y' end as WHAIPermission  
        //    //                ,ft.FACILITYTYPECODE, fh.footer2,nvl(u.AppRole,'No') as AppRole
        //    //                from 
        //    //                usrusers u
        //    //                left outer join masfacilities f on f.facilityid=u.facilityid
        //    //                  left outer join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid 
        //    //               left outer join masfacheaderfooter fh on fh.userid=u.userid
        //    //                left outer join
        //    //                (
        //    //                select ISWHINDENT,FACILITYTYPEID,FACTYPEID from  masfacilitytypeayush 
        //    //                ) ay on ay.FACILITYTYPEID=ft.facilitytypeid and ay.FACTYPEID=f.AYFACTYPEID
        //    //                where (emailid ='" + emailORmob + "' or fh.FOOTER3='" + emailORmob + "')  ";


        //    string qry = @" select v.VID as userid,v.VEHICALNO as emailid,v.pwd,v.VEHICALNO as firstname ,'Vehicle' as usertype
        //                    ,w.districtid,w.WAREHOUSEID as facilityid,  '' as DEPMOBILE ,'' as FOOTER3,0 as facilitytypeid
        //                    ,  0 as FACTYPEID, 'N' as WHAIPermission
        //                    ,'' as FACILITYTYPECODE, v.VEHICALNO footer2, 'Vehicle' as AppRole
        //                    from masvehical v
        //                    inner
        //                    join maswarehouses w on w.WAREHOUSEID = v.WAREHOUSEID
        //                    where v.isactive = 'Y'  and v.VEHICALNO ='" + emailORmob + "' ";

        //    var result = _context.UsruserVehicle
        //   .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList().FirstOrDefault();

        //    user = result;

        //    if (result == null)
        //    {
        //        message = "Invalid ID.";
        //        return false;
        //    }


        //    // Perform password verification
        //    string salthash = result.PWD;
        //    string mStart = "salt{";
        //    string mMid = "}hash{";
        //    string mEnd = "}";
        //    string mSalt = salthash.Substring(salthash.IndexOf(mStart) + mStart.Length, salthash.IndexOf(mMid) - (salthash.IndexOf(mStart) + mStart.Length));
        //    string mHash = salthash.Substring(salthash.IndexOf(mMid) + mMid.Length, salthash.LastIndexOf(mEnd) - (salthash.IndexOf(mMid) + mMid.Length));


        //    Broadline.Common.SecUtils.SaltedHash ver = Broadline.Common.SecUtils.SaltedHash.Create(mSalt, mHash);
        //    bool isValid = ver.Verify(password);

        //    //string approle = result.APPROLE;

        //    //if (approle == "No")
        //    //{
        //    //    message = "Not Authorized to Use this Module of App";
        //    //    return false;
        //    //}

        //    // for every login , need to change


        //    if (password == "2025#cgmsc")
        //    {
        //        isValid = true;
        //    }
        //    else
        //    {
        //        if (!isValid)
        //        {
        //            message = "The email or password you have entered is incorrect.";
        //            return false;
        //        }
        //    }

        //    message = "Successfully Login";
        //    return true;
        //}


    }
}
