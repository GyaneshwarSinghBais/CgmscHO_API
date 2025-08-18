using CgmscHO_API.HodDTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.LogAuditDTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol.Plugins;
using System;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogAuditController : ControllerBase
    {
        private readonly OraDbContext _context;

        public LogAuditController(OraDbContext context)
        {
            _context = context;
        }

        [HttpPost("InsertUserLoginLog")]
        public async Task<IActionResult> InsertUserLoginLog(UserLoginLogDTO userLoginLog)
        {
            if (userLoginLog == null)
            {
                return BadRequest("Invalid log data.");
            }

            var logEntry = new UserLoginLogDTO
            {
                // Do NOT set LogId here!
                // LogId = userLoginLog.LogId,
                UserId = userLoginLog.UserId,
                RoleId = userLoginLog.RoleId,
                RoleIdName = userLoginLog.RoleIdName,
                UserName = userLoginLog.UserName,
                IPAddress = userLoginLog.IPAddress,
                UserAgent = userLoginLog.UserAgent,
                LoginTime = userLoginLog.LoginTime
            };

            _context.UserLoginLogDbSet.Add(logEntry);
            await _context.SaveChangesAsync();
            return Ok("User login log inserted successfully.");
        }





        //insert method for UserPageViewLogs table

        [HttpPost("InsertUserPageViewLog")]
        public async Task<IActionResult> InsertUserPageViewLog(UserPageViewLogDTO userPageViewLog)
        {
            if (userPageViewLog == null)
            {
                return BadRequest("Invalid log data.");
            }
            var logEntry = new UserPageViewLogDTO
            {
                // Do NOT set LogId here!
                // LogId = userPageViewLog.LogId,
                UserId = userPageViewLog.UserId,
                RoleId = userPageViewLog.RoleId,
                RoleIdName = userPageViewLog.RoleIdName,
                PageUrl = userPageViewLog.PageUrl,
                PageName = userPageViewLog.PageName,
                ViewTime = userPageViewLog.ViewTime,
                IPAddress = userPageViewLog.IPAddress,
                UserAgent = userPageViewLog.UserAgent
            };
            _context.UserPageViewLogDbSet.Add(logEntry);
            await _context.SaveChangesAsync();
            return Ok("User page view log inserted successfully.");
        }


        [HttpGet("getUserLoginLogs")]
        public async Task<ActionResult<IEnumerable<getUserLoginLogsDTO>>> getUserLoginLogs(string usertype, string roleid)
        {
            string whusertype = "  ";
            string whroleid = "";

            if (usertype != "0")
            {
                whusertype = " AND r.usertype = '" + usertype + "'  ";
            }

            if (roleid != "0")
            {
                whusertype = " AND r.roleid =  '" + roleid + "'  ";
            }


            string qry = @" SELECT 
                    ul.logid,
                    ul.userid,
                    COALESCE(NULLIF(TRIM(ul.username), ''), u.firstname) AS username,
                    ul.roleid,
                    COALESCE(NULLIF(TRIM(ul.roleidname), ''), r.rolename) AS roleidname,
                    r.rolecode,
                    r.usertype,
                    ul.logintime,
                    ul.ipaddress,
                    ul.useragent
                FROM userloginlogs ul
                LEFT JOIN usrusers u 
                    ON u.userid = ul.userid
                LEFT JOIN usrroles r 
                    ON r.roleid = ul.roleid
                WHERE 1=1
                " + whusertype + @"
                " + whroleid + @"
                ORDER BY ul.logintime DESC ";





            var myList = _context.getUserLoginLogsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        //        SELECT
        //    l.logid           AS login_logid,
        //    l.userid,
        //    COALESCE(NULLIF(TRIM(l.username), ''), u.firstname) AS username,
        //    l.roleid,
        //    COALESCE(NULLIF(TRIM(l.roleidname), ''), r.rolename) AS role_name,
        //    r.rolecode,
        //    r.usertype,
        //    l.logintime,
        //    l.ipaddress AS login_ip,
        //    l.useragent AS login_useragent,
        //    p.logid AS pageview_logid,
        //    p.pageurl,
        //    p.pagename,
        //    p.viewtime,
        //    p.ipaddress AS pageview_ip,
        //    p.useragent AS pageview_useragent
        //FROM userloginlogs l
        //LEFT JOIN usrusers u
        //    ON u.userid = l.userid
        //LEFT JOIN usrroles r
        //    ON r.roleid = l.roleid
        //LEFT JOIN userpageviewlogs p
        //    ON p.userid = l.userid
        //    AND p.roleid = l.roleid
        //    AND p.viewtime >= l.logintime   -- Only pages after login time
        //--WHERE (:usertype IS NULL OR r.usertype = :usertype)
        // -- AND(:roleid IS NULL OR r.roleid = :roleid)
        //--  AND(:fromDate IS NULL OR l.logintime >= :fromDate)
        //--  AND(:toDate IS NULL OR l.logintime <= :toDate)
        //ORDER BY l.logintime DESC, p.viewtime ASC;

        //now create a method to get user page view logs
        [HttpGet("getUserPageViewLogs")]
        public async Task<ActionResult<IEnumerable<getUserPageViewLogsDTO>>> getUserPageViewLogs(string usertype, string roleid)
        {
            string whusertype = "  ";
            string whroleid = "";
            if (usertype != "0")
            {
                whusertype = " AND r.usertype = '" + usertype + "'  ";
            }
            if (roleid != "0")
            {
                whusertype = " AND r.roleid =  '" + roleid + "'  ";
            }           

            
                string qry = @" SELECT 
                      CAST(p.logid AS VARCHAR2(100)) AS logid,
            CAST(p.userid AS VARCHAR2(100)) AS userid,
            CAST(u.firstname AS VARCHAR2(200)) AS username,
            CAST(p.roleid AS VARCHAR2(100)) AS roleid,
            CAST(COALESCE(NULLIF(TRIM(p.roleidname), ''), r.rolename) AS VARCHAR2(200)) AS roleidname,
            CAST(r.rolecode AS VARCHAR2(100)) AS rolecode,
            CAST(r.usertype AS VARCHAR2(100)) AS usertype,
            CAST(p.pageurl AS VARCHAR2(500)) AS pageurl,
            CAST(p.pagename AS VARCHAR2(500)) AS pagename,
            TO_CHAR(p.viewtime, 'YYYY-MM-DD HH24:MI:SS') AS viewtime,
            CAST(p.ipaddress AS VARCHAR2(100)) AS ipaddress,
            CAST(p.useragent AS VARCHAR2(500)) AS useragent
                FROM userpageviewlogs p
                LEFT JOIN usrusers u 
                    ON u.userid = p.userid
                LEFT JOIN usrroles r 
                    ON r.roleid = p.roleid
                WHERE 1=1
                " + whusertype + @"
                " + whroleid + @"
                ORDER BY p.viewtime DESC ";
            var myList = _context.getUserPageViewLogsDbSet
                .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


    }
}


