using CgmscHO_API.AttendenceDTO;
using CgmscHO_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendenceController : ControllerBase
    {
        private readonly SqlDbContext _context;

        public AttendenceController(SqlDbContext context)
        {
            _context = context;
        }


        [HttpGet("GetLocation")]
        public async Task<ActionResult<IEnumerable<GetLocationDTO>>> GetLocation(string iswh)
        {
            string whlocation = "";
            if (iswh != "0")
            {
                whlocation = " and LocationCode like '%WH%' ";
            }
            string qry = "";


            qry = @"  select LocationId,LocationName from Locations where 1=1  "+ whlocation;

            var myList = _context.GetLocationDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetHardcodedJson")]
        public IActionResult GetHardcodedJson()
        {
            var response = new
            {
                Status = "Success",
                Message = "This is a hardcoded JSON response",
                Data = new
                {
                    Id = 1,
                    Name = "Sample Item",
                    Description = "This is a test item for debugging purposes",
                    Timestamp = DateTime.UtcNow
                }
            };

            return Ok(response);
        }

        [HttpGet("GetEmployeeDetail")]
        public async Task<ActionResult<IEnumerable<EmdDetailDTO>>> GetEmployeeDetail(Int32 locationId)
        {
            string qry = "";
            string whLocation = "";
            // string whStatus = "";

            if (locationId != 0)
            {
                whLocation = " and e.Location = " + locationId + " ";
            }

            //if (status != "0") // and e.Status='Working' 
            //{
            //    whStatus = " and e.Status='Working' ";
            //}


            qry = @"  select e.EmployeeId, e.EmployeeName,e.EmployeeCode,e.Gender,e.ContactNo,d.DesignationsName,dp.DepartmentFName from Employees e
inner join Designations d on d.DesignationId=e.Designation
inner join Departments dp on dp.DepartmentId=e.DepartmentId
where 1=1 
" + whLocation + @"
and e.Status='Working' ";

            var myList = _context.EmdDetailDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("AttendenceRecord")]
        public async Task<ActionResult<IEnumerable<AttendenceRecordDTO>>> AttendenceRecord(string startDate, string endDate, string DepartmentFName, Int32 locationid)
        {
            string qry = "";
            string whLocation = "";
            string whDate = "";
            string whDepartmentFName = "";

            if (locationid != 0)
            {
                whLocation = " and e.Location = " + locationid + " ";
            }

            if (startDate != "0" || endDate != "0")
            {
                whDate = "and at.AttendanceDate between '" + startDate + "' and '" + endDate + "'  ";
            }

            if (DepartmentFName != "0")
            {
                whDepartmentFName = " and DepartmentFName='" + DepartmentFName + @"'  ";
            }


            qry = @" select e.EmployeeId, e.EmployeeName,e.EmployeeCode,e.Gender,e.ContactNo,d.DesignationsName,dp.DepartmentFName
,at.AttendanceDate,at.AttendanceDateStr,InTime,OutTime,at.PunchDirections,at.Status,at.StatusCode,at.ReportPunchRecords,at.TotalDurationInHHMM
from AttendanceLogs  at 
inner join Employees e on e.EmployeeId=at.EmployeeId
inner join Designations d on d.DesignationId=e.Designation
inner join Departments dp on dp.DepartmentId=e.DepartmentId
where 1=1 " + whLocation + @" and e.Status='Working'
" + whDate + @" 
" + whDepartmentFName + @"
--order by d.DesignationsName
order by e.EmployeeId, at.AttendanceDate ";

            var myList = _context.AttendenceRecordDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("PresentAbsent")]
        public async Task<ActionResult<IEnumerable<AttendenceRecordDTO>>> PresentAbsent(string startDate, string endDate, Int32 locationid, string statusCode, Int32 desigId)
        {
            string qry = "";
            string whLocation = "";
            string whDate = "";
            //string whDepartmentFName = "";
            string whStatusCode = "";
            string whDesignation = "";

            if (locationid != 0)
            {
                whLocation = " and e.Location = " + locationid + " ";
            }

            if (startDate != "0" || endDate != "0")
            {
                whDate = "and at.AttendanceDate between '" + startDate + "' and '" + endDate + "'  ";
            }

            //if (DepartmentFName != "0")
            //{
            //    whDepartmentFName = " and DepartmentFName='" + DepartmentFName + @"'  ";
            //}

            if (statusCode != "0")
            {
                whStatusCode = " and at.StatusCode='"+ statusCode + @"'  ";
            }

            if (desigId != 0)
            {
                whDesignation = " and d.DesignationId="+ desigId + @"  ";
            }


            qry = @" select e.EmployeeId, e.EmployeeName,e.EmployeeCode,e.Gender,e.ContactNo,d.DesignationsName,dp.DepartmentFName
,at.AttendanceDate,at.AttendanceDateStr,InTime,OutTime,at.PunchDirections,at.Status,at.StatusCode,at.ReportPunchRecords,at.TotalDurationInHHMM
from AttendanceLogs  at 
inner join Employees e on e.EmployeeId=at.EmployeeId
inner join Designations d on d.DesignationId=e.Designation
inner join Departments dp on dp.DepartmentId=e.DepartmentId
where  1=1 "+ whLocation + @"
and e.Status='Working'
"+ whDate + @"
"+ whStatusCode + @" "+ whDesignation + @"
--order by d.DesignationsName
order by at.AttendanceDate
 ";

            var myList = _context.AttendenceRecordDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetDesignation")]
        public async Task<ActionResult<IEnumerable<GetDesignationDTO>>> GetDesignation()
        {

            string qry = "";

            qry = @" select DesignationsName,DesignationId from Designations
where len(DesignationsName)>0 and DesignationId not in (1) ";

            var myList = _context.GetDesignationDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }
    }
}
