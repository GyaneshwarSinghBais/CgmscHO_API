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
using MessagePack;
using System.Net.NetworkInformation;
using System.IO.Pipelines;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using CgmscHO_API.HODTO;
using CgmscHO_API.Utility;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Drawing;
using System.Net;
using CgmscHO_API.DTO;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : Controller
    {
       
        private readonly OraDbContext _context;
        public TransactionController(OraDbContext context)
        {
            _context = context;
        }
        //[HttpGet("DPDMISSupemdSummary")]
       // public async Task<ActionResult<IEnumerable<EMDSummaryDTO>>> SupemdSummary()
       // {
       //     string qry = "";

       //     qry = @" ";
       //}

    }
}


