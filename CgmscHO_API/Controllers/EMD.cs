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
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EMD : ControllerBase
    {
        private readonly OraDbContext _context;
        public EMD(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("DPDMISSupemdSummary")]
        public async Task<ActionResult<IEnumerable<EMDSummaryDTO>>> SupemdSummary()
        {
            string qry = "";
           
                qry = @"   select  distinct  supplierid,suppliername, count(schemeid) as nostender,sum(EMD) as TotalEMD ,sum(RealseAmount) as ReleasedEMDAmt,sum(EMD)-sum(RealseAmount) as PendingEMD from 
(


select my.accyrsetid,my.accyear,ef.fileno,ef.fileid,s.schemeid ponoid,s.schemeid , s.schemename, ms.nitdate,ms.status
,sd.SCHSTATUSDID,sp.suppliername,sp.supplierid,sd.EMD,sd.EMDDOCNO,to_char(sd.EMDDOCDT,'dd-MM-yyyy') as EMDDOCDT
,nvl(sd.ISRELEASE,'N') as ISRELEASE,to_char(sd.RELEASEDATE,'dd-MM-yyyy') as RELEASEDATE,er.EMDRID,er.CHEQUENO,er.CHEQUEDT,nvl(er.emdpaid,0) as  RealseAmount
from  masschemes s
inner join masschemesstatus ms on ms.schemeid = s.schemeid
inner join masaccyearsettings my on my.accyrsetid = s.accyrsetid
inner JOIN masemdfiles ef ON ef.schemeid = s.schemeid
inner join masschemesstatusdetails sd on sd.SCHEMEID=s.SCHEMEID
inner join massuppliers sp on sp.supplierid=sd.supplierid
left outer join 
(
 select P.EMDRID,s.schstatusdid,s.emd,P.EMDRELEASEID,p.emdpaid,P.CHEQUENO, to_char(P.CHEQUEDT,'dd-MM-yyyy') as CHEQUEDT, P.SCHEMEID from masschemesstatusdetails s
                             inner join blpEMDRelease  P On (p.schstatusdid = S.schstatusdid) 
                             inner join blpEMDReleasemaster erm on erm.EMDRID=P.EMDRID
                             where isrelease='Y' 
                             and releaseDate is not null 
                             and erm.status='C'
)er on er.schstatusdid=sd.schstatusdid
where 1=1   


) a 
where accyrsetid >= 542 
group by suppliername,supplierid having sum(EMD)>0
order by sum(EMD)-sum(RealseAmount) desc ";
            
          

            var myList = _context.GetEMDSummaryDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DPDMISEMDTenderwisePending")]
        public async Task<ActionResult<IEnumerable<EMDSummaryTenderDTO>>> EMDTenderwisePending()
        {
            string qry = "";

            qry = @"  select  distinct  schemeid,schemename,status,Statusdata, count(supplierid) as nossupplier,sum(EMD) as TotalEMD ,sum(RealseAmount) as ReleasedEMDAmt,sum(EMD)-sum(RealseAmount) as PendingEMD from 
(


select my.accyrsetid,my.accyear,ef.fileno,ef.fileid,s.schemeid ponoid,s.schemeid , s.schemename, ms.nitdate,ms.status, (case when ms.Status='1' then 'Tender Live' when ms.Status='2' then 'Cover A Opened' when ms.Status='3' then 'Cover B Opened' 
 when ms.Status='4' then 'Price Bid Opened' When ms.Status='5' then 'Cancelled' else '' end) Statusdata 
,sd.SCHSTATUSDID,sp.suppliername,sp.supplierid,sd.EMD,sd.EMDDOCNO,to_char(sd.EMDDOCDT,'dd-MM-yyyy') as EMDDOCDT
,nvl(sd.ISRELEASE,'N') as ISRELEASE,to_char(sd.RELEASEDATE,'dd-MM-yyyy') as RELEASEDATE,er.EMDRID,er.CHEQUENO,er.CHEQUEDT,nvl(er.emdpaid,0) as  RealseAmount
from  masschemes s
inner join masschemesstatus ms on ms.schemeid = s.schemeid
inner join masaccyearsettings my on my.accyrsetid = s.accyrsetid
inner JOIN masemdfiles ef ON ef.schemeid = s.schemeid
inner join masschemesstatusdetails sd on sd.SCHEMEID=s.SCHEMEID
inner join massuppliers sp on sp.supplierid=sd.supplierid
left outer join 
(
 select P.EMDRID,s.schstatusdid,s.emd,P.EMDRELEASEID,p.emdpaid,P.CHEQUENO, to_char(P.CHEQUEDT,'dd-MM-yyyy') as CHEQUEDT, P.SCHEMEID from masschemesstatusdetails s
                             inner join blpEMDRelease  P On (p.schstatusdid = S.schstatusdid) 
                             inner join blpEMDReleasemaster erm on erm.EMDRID=P.EMDRID
                             where isrelease='Y' 
                             and releaseDate is not null 
                             and erm.status='C'
)er on er.schstatusdid=sd.schstatusdid
where 1=1   


) a 
where accyrsetid >= 542 
group by  schemeid,schemename,status,Statusdata having sum(EMD)>0 and (sum(EMD)-sum(RealseAmount))>0
order by status desc ";



            var myList = _context.GetEMDSummaryTenderDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




        [HttpGet("DPDMISEMDDetails")]
        public async Task<ActionResult<IEnumerable<EMDDetailsDTO>>> DPDMISEMDDetails()
        {
            string qry = "";

            qry = @" select ROW_NUMBER() OVER ( ORDER BY s.schemeid,sp.supplierid ) AS ID,mc.categoryname,my.accyear ,s.schemename, (case when ms.Status='1' then 'Tender Live' when ms.Status='2' then 'Cover A Opened' when ms.Status='3' then 'Cover B Opened' 
 when ms.Status='4' then 'Price Bid Opened' When ms.Status='5' then 'Cancelled' else '' end) Statusdata , sp.suppliername,sd.EMD,case when sd.ISRELEASE='Y' then 'Yes' else 'No' end as ISRELEASE, nvl(er.emdpaid,0) as  RealseAmount
,to_char(sd.RELEASEDATE,'dd-MM-yyyy') as RELEASEDATE,er.CHEQUENO,er.CHEQUEDT,ef.fileno,ef.fileid,s.schemeid ponoid,s.schemeid,my.accyrsetid,ms.status,sd.SCHSTATUSDID,sp.supplierid,sd.EMDDOCNO,er.EMDRID
from  masschemes s
inner join masschemesstatus ms on ms.schemeid = s.schemeid
inner join masaccyearsettings my on my.accyrsetid = s.accyrsetid
inner JOIN masemdfiles ef ON ef.schemeid = s.schemeid
inner join masschemesstatusdetails sd on sd.SCHEMEID=s.SCHEMEID
inner join massuppliers sp on sp.supplierid=sd.supplierid
inner join masitemcategories mc on mc.categoryid = ms.categoryid
left outer join 
(
 select P.EMDRID,s.schstatusdid,s.emd,P.EMDRELEASEID,p.emdpaid,P.CHEQUENO, to_char(P.CHEQUEDT,'dd-MM-yyyy') as CHEQUEDT, P.SCHEMEID from masschemesstatusdetails s
                             inner join blpEMDRelease  P On (p.schstatusdid = S.schstatusdid) 
                             inner join blpEMDReleasemaster erm on erm.EMDRID=P.EMDRID
                             where isrelease='Y' 
                             and releaseDate is not null 
                             and erm.status='C'
)er on er.schstatusdid=sd.schstatusdid
where 1=1 and my.accyrsetid > =542
order by suppliername ";



            var myList = _context.GetEMDDetailsDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DPDMISEMDDashboard")]
        public async Task<ActionResult<IEnumerable<EMDDashDTO>>> DPDMISEMDDashboard()
        {
            string qry = "";

            qry = @" select    count(distinct supplierid) as nossupplierid, count(distinct schemeid) as nostender,round(sum(EMD)/10000000,2) as TotalEMD ,round(sum(RealseAmount)/10000000,2) as ReleasedEMDAmt,round((sum(EMD)-sum(RealseAmount))/10000000,2) as PendingEMD from 
(


select my.accyrsetid,my.accyear,ef.fileno,ef.fileid,s.schemeid ponoid,s.schemeid , s.schemename, ms.nitdate,ms.status
,sd.SCHSTATUSDID,sp.suppliername,sp.supplierid,sd.EMD,sd.EMDDOCNO,to_char(sd.EMDDOCDT,'dd-MM-yyyy') as EMDDOCDT
,nvl(sd.ISRELEASE,'N') as ISRELEASE,to_char(sd.RELEASEDATE,'dd-MM-yyyy') as RELEASEDATE,er.EMDRID,er.CHEQUENO,er.CHEQUEDT,nvl(er.emdpaid,0) as  RealseAmount
,sd.RELEASEDATE 
from  masschemes s
inner join masschemesstatus ms on ms.schemeid = s.schemeid
inner join masaccyearsettings my on my.accyrsetid = s.accyrsetid
inner JOIN masemdfiles ef ON ef.schemeid = s.schemeid
inner join masschemesstatusdetails sd on sd.SCHEMEID=s.SCHEMEID
inner join massuppliers sp on sp.supplierid=sd.supplierid
left outer join 
(
 select P.EMDRID,s.schstatusdid,s.emd,P.EMDRELEASEID,p.emdpaid,P.CHEQUENO, to_char(P.CHEQUEDT,'dd-MM-yyyy') as CHEQUEDT, P.SCHEMEID from masschemesstatusdetails s
                             inner join blpEMDRelease  P On (p.schstatusdid = S.schstatusdid) 
                             inner join blpEMDReleasemaster erm on erm.EMDRID=P.EMDRID
                             where isrelease='Y' 
                             and releaseDate is not null 
                             and erm.status='C'
)er on er.schstatusdid=sd.schstatusdid
where 1=1   


) a 
where accyrsetid >= 542  having sum(EMD)>0
 ";



            var myList = _context.GetEMDDashDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("DPDMISEMDReleseddetails")]
        public async Task<ActionResult<IEnumerable<EMDReleaseddetDTO>>> DPDMISEMDReleseddetails(string fromdt, string todate)
        {

            string wh = " ";
            string qry = "";

            if (fromdt != null && todate != null)
            {

                 wh = " and p.cHEQUEDT between '" +  fromdt + "' and '" + todate + "'";

            }

            qry = @" select    schemeid,schemename,suppliername, EMD ,RealseAmount,EMD-RealseAmount as PendingEMD,Statusdata ,CHEQUEDT,chequdta

from 
(


select my.accyrsetid,my.accyear,ef.fileno,ef.fileid,s.schemeid ponoid,s.schemeid , s.schemename, ms.nitdate,

(case when ms.Status='1' then 'Tender Live' when ms.Status='2' then 'Cover A Opened' when ms.Status='3' then 'Cover B Opened' 
 when ms.Status='4' then 'Price Bid Opened' When ms.Status='5' then 'Cancelled' else '' end) Statusdata 
,sd.SCHSTATUSDID,sp.suppliername,sp.supplierid,sd.EMD,sd.EMDDOCNO,to_char(sd.EMDDOCDT,'dd-MM-yyyy') as EMDDOCDT
,nvl(sd.ISRELEASE,'N') as ISRELEASE,to_char(sd.RELEASEDATE,'dd-MM-yyyy') as RELEASEDATE,er.EMDRID,er.CHEQUENO,er.CHEQUEDT,nvl(er.emdpaid,0) as  RealseAmount,chequdta
from  masschemes s
inner join masschemesstatus ms on ms.schemeid = s.schemeid
inner join masaccyearsettings my on my.accyrsetid = s.accyrsetid
inner JOIN masemdfiles ef ON ef.schemeid = s.schemeid
inner join masschemesstatusdetails sd on sd.SCHEMEID=s.SCHEMEID
inner join massuppliers sp on sp.supplierid=sd.supplierid
inner join
(
 select P.EMDRID,s.schstatusdid,s.emd,P.EMDRELEASEID,p.emdpaid,P.CHEQUENO, to_char(P.CHEQUEDT,'dd-MM-yyyy') as CHEQUEDT, P.SCHEMEID,P.CHEQUEDT as chequdta from masschemesstatusdetails s
                             inner join blpEMDRelease  P On (p.schstatusdid = S.schstatusdid) 
                             inner join blpEMDReleasemaster erm on erm.EMDRID=P.EMDRID
                             where isrelease='Y' 
                             and releaseDate is not null 
                             and erm.status='C' "+ wh + @"
)er on er.schstatusdid=sd.schstatusdid
where 1=1   


) a 
where accyrsetid >= 542  
order by chequdta desc
 ";



            var myList = _context.GetEMDReleaseddetDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



    }
}
