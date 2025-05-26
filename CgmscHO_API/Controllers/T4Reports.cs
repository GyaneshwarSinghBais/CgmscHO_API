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
using System.IO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class T4Reports : ControllerBase
    {
        private readonly OraDbContext _context;
        public T4Reports(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("OPDCountTotal")]
        public async Task<ActionResult<IEnumerable<OPDCountDTO>>> OPDCountTotal(string disid, string coll_cmho, string userid, string facid, string hodid, string daypara, string rpttype)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            string whhodid = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Collector")

                    {
                        districtid = f.getCollectorDisid(userid);
                        whhodid = " and  ft.hodid in (2,3)";
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DHS")
                    {
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DME")
                    {
                        whhodid = " and  ft.hodid in (3)";
                    }
                    if (coll_cmho == "SEC" || coll_cmho == "MDCGMSC" || coll_cmho == "Admin" || coll_cmho == "Technical" || coll_cmho == "MIN")
                    {
                        if (hodid == "7")
                        {
                            whhodid = " and  ft.hodid in (7)";
                        }
                        else
                        {
                            whhodid = " and  ft.hodid in (2,3)";
                        }
                    }


                }
            }
            else
            {
                districtid = disid;
            }




            string whfacid = "";
            if (facid != "0" && facid != "null")
            {

                whfacid = " and f.facilityid =" + facid;
                //whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whdistid = "";
            if (districtid == "0" || districtid == "")
            {

            }
            else
            {
                whdistid = " and f.districtid =" + districtid;
            }
            string whdayclause = "";
            if (daypara == "Today")
            {
                whdayclause = " and tbo.issuedate between sysdate - 1 and sysdate ";
            }
            else if (daypara == "Week")
            {
                whdayclause = " and tbo.issuedate between sysdate - 8 and sysdate";
            }
            else if (daypara == "Month")
            {
                whdayclause = " and tbo.issuedate between  trunc(sysdate, 'mm') and  sysdate+1";
            }
            else if (daypara == "Year")
            {
                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) and (select ENDDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) ";
            }
            else if (daypara == "LYear")
            {
                string yearidget = f.getACCYRSETID();
                string lyear = "";
                if (yearidget == "544")
                {
                    lyear = "542";
                }
                else
                {
                    int yearidint = Convert.ToInt16(yearidget) - 1;
                    lyear = yearidint.ToString();
                }

                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") and(select ENDDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") ";
            }
            else
            {

            }
            string qry = "";
            if (rpttype == "Summary")
            {
                qry = @" 
select 'All' as facilityname, count(distinct facilityid) as nosfac, count(distinct itemid) as NoItemid,count(distinct patid) patno,count(distinct drid) as nosdr,1 as facilityid
from (
select p.patid,tbo.issuedate ,tbo.wardid,f.facilityid,ti.itemid,tbo.drid
from maspatient p
inner join tbwardissues tbo on tbo.patid=p.patid
inner join  tbwardissueitems ti on ti.issueid=tbo.issueid
inner join tbwardoutwards tb on tb.issueitemid=ti.issueitemid
inner join masfacilitywards mf on mf.wardid=tbo.wardid
inner join masfacilities f on f.facilityid=mf.facilityid
 inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join masdistricts d on d.districtid=f.districtid
where 1=1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @" 
and tbo.status='C' " + whhodid + @"  and f.isactive=1  and ft.facilitytypeid not in (377,353)
)  ";
            }
            else
            {
                qry = @" select f.facilityname,0 as NOSFAC,nvl(NoItemid, 0) as NoItemid ,nvl(patno, 0)patno,nvl(nosdr, 0) as nosdr,f.facilityid from masfacilities f
 inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
left outer join
                (
                select d.facilityid, count(distinct itemid) as NoItemid, count(distinct patid) patno, count(distinct drid) as nosdr
from
                (
select p.patid, tbo.issuedate, tbo.wardid, f.facilityid, ti.itemid, tbo.drid, ft.orderdp
from maspatient p
inner join tbwardissues tbo on tbo.patid = p.patid
inner join tbwardissueitems ti on ti.issueid = tbo.issueid
inner join tbwardoutwards tb on tb.issueitemid = ti.issueitemid
inner join masfacilitywards mf on mf.wardid = tbo.wardid
inner join masfacilities f on f.facilityid = mf.facilityid
 inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
inner join masdistricts d on d.districtid = f.districtid
where 1 = 1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @"
and tbo.status = 'C' " + whhodid + @"  and tbo.issuedate > '01-Apr-2022'
and f.isactive = 1  and ft.facilitytypeid not in (377,353)
)d group by  d.facilityid
) dt on dt.facilityid = f.facilityid
where 1 = 1 " + whfacid + @" " + whdistid + @"
and f.isactive = 1  and ft.facilitytypeid not in (377, 353) " + whhodid + @"
 order by nvl(patno, 0) desc ";

            }

            //order by ft.orderdp "

            var myList = _context.OPDCountDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("DrCountTotal")]
        public async Task<ActionResult<IEnumerable<DRCountDTO>>> DrCountTotal(string disid, string coll_cmho, string userid, string facid, string hodid, string daypara, string rpttype, string drid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            string whhodid = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Collector")

                    {
                        districtid = f.getCollectorDisid(userid);
                        whhodid = " and  ft.hodid in (2,3)";
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DHS")
                    {
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DME")
                    {
                        whhodid = " and  ft.hodid in (3)";
                    }
                    if (coll_cmho == "SEC" || coll_cmho == "DME" || coll_cmho == "MDCGMSC" || coll_cmho == "Admin" || coll_cmho == "Technical" || coll_cmho == "MIN" || coll_cmho == "DHS" || coll_cmho == "AYUSH")
                    {
                        if (coll_cmho == "AYUSH")
                        {
                            whhodid = " and  ft.hodid in (7)";
                        }
                        else if (coll_cmho == "DHS")
                        {
                            whhodid = " and  ft.hodid in (2)";
                        }
                        else if (coll_cmho == "DME")
                        {
                            whhodid = " and  ft.hodid in (3)";
                        }

                        else
                        {
                            whhodid = " and  ft.hodid in (2,3)";
                        }
                    }

                }
            }
            else
            {
                districtid = disid;
            }

            string whfacid = "";
            if (facid != "0" && facid != "null")
            {

                whfacid = " and f.facilityid =" + facid;
                //whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whdistid = "";
            if (districtid == "0" || districtid == "")
            {

            }
            else
            {
                whdistid = " and f.districtid =" + districtid;
            }
            string whdayclause = "";
            if (daypara == "Today")
            {
                whdayclause = " and tbo.issuedate between sysdate - 1 and sysdate ";
            }
            else if (daypara == "Week")
            {
                whdayclause = " and tbo.issuedate between sysdate - 8 and sysdate";
            }
            else if (daypara == "Month")
            {
                whdayclause = " and tbo.issuedate between  trunc(sysdate, 'mm') and  sysdate+1";
            }
            else if (daypara == "Year")
            {
                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) and (select ENDDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) ";
            }
            else if (daypara == "LYear")
            {
                string yearidget = f.getACCYRSETID();
                string lyear = "";
                if (yearidget == "544")
                {
                    lyear = "542";
                }
                else
                {
                    int yearidint = Convert.ToInt16(yearidget) - 1;
                    lyear = yearidint.ToString();
                }

                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") and(select ENDDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") ";
            }
            else
            {

            }

            string whdrid = "";
            if (drid != "0" && drid != "null")
            {
                whdrid = " and tbo.drid =" + drid;
            }
            string qry = "";

            qry = @"   select DRName, facilityname,0 as NOSFAC ,count(distinct itemid) as NoItemid,count(distinct patid) patno,facilityid,drid,ROW_NUMBER() OVER (order by drid ) as id
from (
select p.patid, tbo.issuedate, tbo.wardid, f.facilityid, ti.itemid, tbo.drid, dr.DRName, f.facilityname, ft.facilitytypeid
from maspatient p
inner join tbwardissues tbo on tbo.patid = p.patid
inner join masdocter dr on dr.drid = tbo.drid
inner join tbwardissueitems ti on ti.issueid = tbo.issueid
inner join tbwardoutwards tb on tb.issueitemid = ti.issueitemid
inner join masfacilitywards mf on mf.wardid = tbo.wardid
inner join masfacilities f on f.facilityid = mf.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
inner join masdistricts d on d.districtid = f.districtid
where 1 = 1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @"  " + whdrid + @"
and tbo.status = 'C' and tbo.issuedate > '01-apr-2022'
and f.isactive = 1  and ft.facilitytypeid not in (377) )  group by facilityname,DRName,facilityid,drid
order by count(distinct patid) desc ";

            var myList = _context.DrCountDbSet
               .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("getAllDoctor")]
        public async Task<ActionResult<IEnumerable<MasDoctorDTO>>> getAllDoctor(bool Isall,string userid,string facid,string distid,string coll_cmho)
        {
            FacOperations f = new FacOperations(_context);
            string districtid = "";
            if (coll_cmho == "Collector")

            {
                districtid = f.getCollectorDisid(userid);
         
            }
            if (coll_cmho == "CMHO")
            {
                districtid = Convert.ToString(f.geFACTDistrictid(userid));
      
            }
            string whfacid = "";
            if (facid != "0" && facid != "null")
            {

                whfacid = " and f.facilityid =" + facid;
                //whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whdistid = "";
            if (distid != "0" && distid != "null")
            {

                whdistid = " and f.districtid =" + distid;
                //whmcid1 = " and mc.MCID =" + mcatid;
            }
            else if (districtid != "")
            {
                whdistid = " and f.districtid =" + districtid;
            }
            string qry = "";
            if (Isall)
            {
                qry = @" 
           select drid,drname from 
            (
            select  dr.drid, dr.drname  from masdocter dr
            inner join masfacilities f on f.facilityid = dr.facilityid
            where 1 = 1 and dr.ISACTIVE = 1 " + whdistid + @" " + whfacid + @"
            union all
            select 0 as drid, 'All' as drname from dual
            )
            order by drid ";
               
            }
            else
            {
                qry = @" select  dr.drid,dr.drname  from masdocter dr
inner join masfacilities f on f.facilityid=dr.facilityid
where 1=1 and dr.ISACTIVE=1  " + whdistid + @" " + whfacid + @"
order by dr.drname ";

            }
            var myList = _context.MasDoctorDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DiagnosysCount")]
        public async Task<ActionResult<IEnumerable<DiagnosysDTO>>> DiagnosysCount(string disid, string coll_cmho, string userid, string facid, string hodid, string daypara, string rpttype, string diagnosysId)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            string whhodid = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Collector")

                    {
                        districtid = f.getCollectorDisid(userid);
                        whhodid = " and  ft.hodid in (2,3)";
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DHS")
                    {
                        whhodid = " and  ft.hodid in (2)";
                    }
                    if (coll_cmho == "DME")
                    {
                        whhodid = " and  ft.hodid in (3)";
                    }
                    if (coll_cmho == "SEC" || coll_cmho == "DME" || coll_cmho == "MDCGMSC" || coll_cmho == "Admin" || coll_cmho == "Technical" || coll_cmho == "MIN" || coll_cmho == "DHS" || coll_cmho == "AYUSH")
                    {
                        if (coll_cmho == "AYUSH")
                        {
                            whhodid = " and  ft.hodid in (7)";
                        }
                        else if (coll_cmho == "DHS")
                        {
                            whhodid = " and  ft.hodid in (2)";
                        }
                        else if (coll_cmho == "DME")
                        {
                            whhodid = " and  ft.hodid in (3)";
                        }

                        else
                        {
                            whhodid = " and  ft.hodid in (2,3)";
                        }
                    }

                }
            }
            else
            {
                districtid = disid;
            }

            string whfacid = "";
            if (facid != "0" && facid != "null")
            {

                whfacid = " and f.facilityid =" + facid;
                //whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whdistid = "";
            if (districtid == "0" || districtid == "")
            {

            }
            else
            {
                whdistid = " and f.districtid =" + districtid;
            }
            string whdayclause = "";
            if (daypara == "Today")
            {
                whdayclause = " and tbo.issuedate between sysdate - 1 and sysdate ";
            }
            else if (daypara == "Week")
            {
                whdayclause = " and tbo.issuedate between sysdate - 8 and sysdate";
            }
            else if (daypara == "Month")
            {
                whdayclause = " and tbo.issuedate between  trunc(sysdate, 'mm') and  sysdate+1";
            }
            else if (daypara == "Year")
            {
                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) and (select ENDDATE from masaccyearsettings where sysdate between STARTDATE and ENDDATE) ";
            }
            else if (daypara == "LYear")
            {
                string yearidget = f.getACCYRSETID();
                string lyear = "";
                if (yearidget == "544")
                {
                    lyear = "542";
                }
                else
                {
                    int yearidint = Convert.ToInt16(yearidget) - 1;
                    lyear = yearidint.ToString();
                }

                whdayclause = " and tbo.issuedate between (select STARTDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") and(select ENDDATE from masaccyearsettings where ACCYRSETID=" + lyear + @") ";
            }
            else
            {

            }

            string whdrid = "";
            if (diagnosysId != "0" && diagnosysId != "null")
            {
                whdrid = " and tbo.patienttypeid =" + diagnosysId;
            }
            string qry = "";
            if (rpttype == "Summary")
            {

                //                qry = @"   select DRName, facilityname,0 as NOSFAC ,count(distinct itemid) as NoItemid,count(distinct patid) patno,facilityid,drid,ROW_NUMBER() OVER (order by drid ) as id
                //from (
                //select p.patid, tbo.issuedate, tbo.wardid, f.facilityid, ti.itemid, tbo.drid, dr.DRName, f.facilityname, ft.facilitytypeid
                //from maspatient p
                //inner join tbwardissues tbo on tbo.patid = p.patid
                //inner join masdocter dr on dr.drid = tbo.drid
                //inner join tbwardissueitems ti on ti.issueid = tbo.issueid
                //inner join tbwardoutwards tb on tb.issueitemid = ti.issueitemid
                //inner join masfacilitywards mf on mf.wardid = tbo.wardid
                //inner join masfacilities f on f.facilityid = mf.facilityid
                //inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
                //inner join masdistricts d on d.districtid = f.districtid
                //where 1 = 1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @"  " + whdrid + @"
                //and tbo.status = 'C' and tbo.issuedate > '01-apr-2022'
                //and f.isactive = 1  and ft.facilitytypeid not in (377) )  group by facilityname,DRName,facilityid,drid
                //order by count(distinct patid) desc ";

                qry = @" select 'All' as facilityname, PateintType,count(distinct itemid) as NoItemid,count(distinct patid) patno, patienttypeid as ID,1 as Facilityid
from (
select p.patid,tbo.issuedate ,tbo.wardid,f.facilityid,ti.itemid,tbo.drid,pt.PateintType,f.facilityname,tbo.patienttypeid
from maspatient p
inner join tbwardissues tbo on tbo.patid=p.patid
inner join  tbwardissueitems ti on ti.issueid=tbo.issueid
inner join tbwardoutwards tb on tb.issueitemid=ti.issueitemid
inner join masfacilitywards mf on mf.wardid=tbo.wardid
inner join masfacilities f on f.facilityid=mf.facilityid
inner join MaspateintType pt on pt.patienttypeid=tbo.patienttypeid
 inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join masdistricts d on d.districtid=f.districtid
where 1 = 1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @"  " + whdrid + @"
and tbo.status = 'C'  and tbo.issuedate > '01-APR-2022'
and f.isactive = 1  and ft.facilitytypeid not in (377)
)  group by PateintType,patienttypeid
order by count(distinct patid) desc ";
            }
            else
            {
                qry = @" select facilityname, PateintType,count(distinct itemid) as NoItemid,count(distinct patid) patno,facilityid,ROW_NUMBER() OVER (order by facilityid )   as id
from (
select p.patid,tbo.issuedate ,tbo.wardid,f.facilityid,ti.itemid,tbo.drid,pt.PateintType,f.facilityname
from maspatient p
inner join tbwardissues tbo on tbo.patid=p.patid
inner join  tbwardissueitems ti on ti.issueid=tbo.issueid
inner join tbwardoutwards tb on tb.issueitemid=ti.issueitemid
inner join masfacilitywards mf on mf.wardid=tbo.wardid
inner join masfacilities f on f.facilityid=mf.facilityid
inner join MaspateintType pt on pt.patienttypeid=tbo.patienttypeid
 inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join masdistricts d on d.districtid=f.districtid
where 1 = 1 " + whfacid + @" " + whdistid + @"  " + whdayclause + @"  " + whdrid + @"
and tbo.status = 'C'  and tbo.issuedate > '01-APR-2022'
and f.isactive = 1  and ft.facilitytypeid not in (377)
)  group by PateintType,facilityname,facilityid
order by count(distinct patid) desc ";

            }

            var myList = _context.DiagnosysDbSet
               .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
    }
}
