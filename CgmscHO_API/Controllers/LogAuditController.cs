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
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

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

    }
}


