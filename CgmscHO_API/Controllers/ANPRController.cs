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
using System.Reflection;
using Microsoft.Data.SqlClient;
using Oracle.ManagedDataAccess.Client;
using CgmscHO_API.ANPRDTO;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ANPRController : ControllerBase
    {
        private readonly OraDbContext _context;
        public ANPRController(OraDbContext context)
        {
            _context = context;
        }

        [HttpPost("insertMASWHVEHICLETRANSPORT")]
        public IActionResult InsertMASWHVEHICLETRANSPORT([FromBody] InsertMASWHVEHICLETRANSPORTDTO request)
        {
            if (request == null)
            {
                return BadRequest("Request data is missing.");
            }

            // Parse the ISO 8601 date string into a DateTime object
            DateTime parsedDate;
            if (!DateTime.TryParse(request.date, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsedDate))
            {
                return BadRequest("Invalid date format.");
            }
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");
            string qry = @"INSERT INTO maswhvehicletransport (vplateno, vdate, direction, entrydate, camid)
                   VALUES (:vplateno, :vdate, :direction, :entrydate, :camid)";

            var parameters = new OracleParameter[]
            {
        new OracleParameter("vplateno", request.plate),
        new OracleParameter("vdate", OracleDbType.Date) { Value = parsedDate },
        new OracleParameter("direction", request.direction),
          new OracleParameter("entrydate", dt1) ,
        new OracleParameter("camid", request.id)
            };

            _context.Database.ExecuteSqlRaw(qry, parameters);
            return Ok("Successfully Saved");
        }


        [HttpGet("VhicleInfo")]
        public async Task<ActionResult<IEnumerable<VhicleInfoDTO>>> VhicleInfo(bool IsStopped,bool isWarehouseVhicle, string fromDate, string toDate)
        {
            string qry = "";
            string whIsStopped = "";
            string whIsWarehouseVhicle = "";
            string whDateBetween = "";

            if(!IsStopped)
            {
                whIsStopped = "  AND v.DIRECTION NOT IN ('stopped') ";
            }

            if(fromDate != "0" && toDate != "0")
            {
                whDateBetween = @"  AND v.entrydate BETWEEN TO_DATE('"+ fromDate + @"', 'dd-Mon-yyyy') 
                      AND TO_DATE('"+ toDate +"', 'dd-Mon-yyyy') + 1  ";
            }
            if (!isWarehouseVhicle)
            {
                whIsWarehouseVhicle = "  AND vh.warehouseid IS NULL ";
            }

            qry = @" SELECT v.TRANID,
       v.VPLATENO,
       v.DIRECTION,
       TO_CHAR(v.VDATE, 'dd-MM-yyyy') AS VDATE,
       v.ENTRYDATE,
       v.camid,
       vh.WAREHOUSEID,
       w.WAREHOUSENAME
FROM maswhvehicletransport v
LEFT OUTER JOIN masvehical vh ON vh.VEHICALNO = v.VPLATENO
LEFT OUTER JOIN maswarehouses w ON w.warehouseid = vh.warehouseid
WHERE 1 = 1 
  "+ whIsStopped + @"
  "+ whDateBetween + @"
  "+ whIsWarehouseVhicle + @"
ORDER BY 1 DESC ";

            var myList = _context.VhicleInfoDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        //[HttpPost("insertMASWHVEHICLETRANSPORT")]
        //public IActionResult insertMASWHVEHICLETRANSPORT(string vplateno, string vdate, string direction, string camid)
        //{
        //    // Parse the ISO 8601 date string into a DateTime object
        //    DateTime parsedDate;
        //    if (!DateTime.TryParse(vdate, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsedDate))
        //    {
        //        return BadRequest("Invalid date format.");
        //    }

        //    string qry = @"INSERT INTO maswhvehicletransport (vplateno, vdate, direction, entrydate, camid)
        //           VALUES (:vplateno, :vdate, :direction, SYSDATE, :camid)";

        //    var parameters = new OracleParameter[]
        //    {
        //new OracleParameter("vplateno", vplateno),
        //new OracleParameter("vdate", OracleDbType.Date) { Value = parsedDate },
        //new OracleParameter("direction", direction),
        //new OracleParameter("camid", camid)
        //    };

        //    _context.Database.ExecuteSqlRaw(qry, parameters);
        //    return Ok("Successfully Saved");
        //}
    }
}
