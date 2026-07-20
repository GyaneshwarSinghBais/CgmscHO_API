using CgmscHO_API.DTO;
using CgmscHO_API.EmdDTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
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


        [HttpGet("getemdpolist")]
        public async Task<ActionResult<IEnumerable<EMDPOListDTO>>> GetEMDPOList(string mcid)
        {

            string whMCId = "";
            if (mcid != "0")
            {
                whMCId = " and mc.mcid = " + mcid;
            }

            string qry = @" 

    select 
        mc.mcid,
        mc.mcategory as category,
        p.ponoid,
        p.pono,
        p.soissuedate as podate,
        fm.FILEID as fileno,
        p.budgetid,
        s.SANCTIONID,
        s.SANCTIONNO,
        s.SANCTIONDATE,
        round(s.totnetamount,0) grossamount,
        sup.suppliername
    from soorderplaced p

    inner join soordereditems i 
        on i.ponoid = p.ponoid

    inner join blpsanctions s 
        on s.ponoid = p.ponoid

    inner join massuppliers sup 
        on sup.supplierid = p.supplierid

    inner join masfilemovement fm 
        on fm.ponoid = p.ponoid

    inner join masitems m 
        on m.itemid = i.itemid

    inner join masitemcategories c 
        on c.categoryid = m.categoryid

    inner join masitemmaincategory mc 
        on mc.MCID = c.MCID 
      left outer join( select distinct ponoid as sentponoid from masfilemovement fm where fm.remarks = 'Forward to MD' and PRESENTFILEFLAG = 'Y' and TOUSERID = 4474) fmsent on fmsent.sentponoid =  p.ponoid
    where s.status = 'IN' 
        "+ whMCId + @"
        and s.SANCTIONDATE >= TO_DATE('01-APR-26','DD-MON-YY')
      
        and fm.presentfileflag = 'Y'
and sentponoid is NULL
    order by s.SANCTIONDATE
    ";

            var myList = await _context.GetEMDPOListDTODbSet
                .FromSqlInterpolated(FormattableStringFactory.Create(qry))
                .ToListAsync();

            return myList;
        }

        [HttpPost("ForwardToMD")]
        public async Task<IActionResult> ForwardToMD(DateTime toDate, decimal poNoId)
        {
            // Step 1 : Update old present file flag
            string updateQry = $@"
    
        update masfilemovement 
        set PRESENTFILEFLAG = 'N' 
        where PONOID = {poNoId}
    
    ";

            await _context.Database.ExecuteSqlRawAsync(updateQry);


            // Step 2 : Insert new movement entry
            string insertQry = $@"

        insert into masfilemovement
        (
            USERID,
            TODATE,
            ENTRYDT,
            REMARKS,
            PRESENTFILEFLAG,
            TOUSERID,
            FLAG,
            PONOID
        )
        values
        (
            2994,
            TO_DATE('{toDate:dd-MMM-yyyy}','DD-MON-YYYY'),
            SYSDATE,
            'Forward to MD',
            'Y',
            4474,
            'S',
            {poNoId}
        )

    ";

            await _context.Database.ExecuteSqlRawAsync(insertQry);

            return Ok("Forwarded To MD Successfully");
        }

        [HttpPost("ReceivedFromMD")]
        public async Task<IActionResult> ReceivedFromMD(DateTime toDate, decimal poNoId)
        {
            // Step 1 : Update old present file flag
            string updateQry = $@"
    
        update masfilemovement 
        set PRESENTFILEFLAG = 'N' 
        where PONOID = {poNoId}
    
    ";

            await _context.Database.ExecuteSqlRawAsync(updateQry);


            // Step 2 : Insert new movement entry
            string insertQry = $@"

        insert into masfilemovement
        (
            USERID,
            TODATE,
            ENTRYDT,
            REMARKS,
            PRESENTFILEFLAG,
            TOUSERID,
            FLAG,
            PONOID
        )
        values
        (
            4474,
            TO_DATE('{toDate:dd-MMM-yyyy}','DD-MON-YYYY'),
            SYSDATE,
            'Received from MD',
            'Y',
            2994,
            'S',
            {poNoId}
        )

    ";

            await _context.Database.ExecuteSqlRawAsync(insertQry);

            return Ok("Received from MD Successfully");
        }



        [HttpGet("getemdpolistReturnFromMD")]
        public async Task<ActionResult<IEnumerable<EMDPOListDTO>>> getemdpolistReturnFromMD(string mcid)
        {
            string whMCId = "";
                if(mcid != "0")
                {
                    whMCId = " and mc.mcid = " + mcid;
            }

            string qry = @"  

    select 
        mc.mcid,
        mc.mcategory as category,
        p.ponoid,
        p.pono,
        p.soissuedate as podate,
        fm.FILEID as fileno,
        p.budgetid,
        s.SANCTIONID,
        s.SANCTIONNO,
        s.SANCTIONDATE,
        round(s.totnetamount,0) grossamount,
        sup.suppliername
    from soorderplaced p

    inner join soordereditems i 
        on i.ponoid = p.ponoid
    inner join masfilemovement fm on fm.PONOID = p.ponoid and fm.remarks = 'Forward to MD' and PRESENTFILEFLAG = 'Y' and TOUSERID = 4474
    inner join blpsanctions s 
        on s.ponoid = p.ponoid

    inner join massuppliers sup 
        on sup.supplierid = p.supplierid

--    inner join masfilemovement fm 
--        on fm.ponoid = p.ponoid

    inner join masitems m 
        on m.itemid = i.itemid

    inner join masitemcategories c 
        on c.categoryid = m.categoryid

    inner join masitemmaincategory mc 
        on mc.MCID = c.MCID 

    where s.status = 'IN' 
        "+ whMCId + @"
        and s.SANCTIONDATE >= TO_DATE('01-APR-26','DD-MON-YY')

    order by s.SANCTIONDATE
    ";

            var myList = await _context.GetEMDPOListDTODbSet
                .FromSqlInterpolated(FormattableStringFactory.Create(qry))
                .ToListAsync();

            return myList;
        }


        //    [HttpPost("fileForwarding")]
        //    public async Task<IActionResult> fileForwarding(DateTime toDate, decimal poNoId,string toUserId, string fromUserId)
        //    {
        //        // Step 1 : Update old present file flag
        //        string updateQry = $@"

        //    update masfilemovement 
        //    set PRESENTFILEFLAG = 'N' 
        //    where PONOID = {poNoId}

        //";

        //        await _context.Database.ExecuteSqlRawAsync(updateQry);


        //        // Step 2 : Insert new movement entry
        //        string insertQry = $@"

        //    insert into masfilemovement
        //    (
        //        USERID,
        //        TODATE,
        //        ENTRYDT,
        //        REMARKS,
        //        PRESENTFILEFLAG,
        //        TOUSERID,
        //        FLAG,
        //        PONOID
        //    )
        //    values
        //    (
        //        "+ fromUserId + @",
        //        TO_DATE('{toDate:dd-MMM-yyyy}','DD-MON-YYYY'),
        //        SYSDATE,
        //        'Received from MD',
        //        'Y',
        //        "+ toUserId + @",
        //        'S',
        //        {poNoId}
        //    )

        //";

        //        await _context.Database.ExecuteSqlRawAsync(insertQry);

        //        return Ok("Received and Send Other Successfully");
        //    }



        [HttpPost("fileForwarding")]
        public async Task<IActionResult> fileForwarding(
    DateTime toDate,
    decimal poNoId,
    string toUserId,
    string fromUserId)
        {
            // Step 1 : Update old present file flag
            string updateQry = @"
        UPDATE masfilemovement
        SET PRESENTFILEFLAG = 'N'
        WHERE PONOID = :poNoId";

            await _context.Database.ExecuteSqlRawAsync(
                updateQry,
                new OracleParameter("poNoId", poNoId)
            );

            // Step 2 : Insert new movement entry
            string insertQry = @"
        INSERT INTO masfilemovement
        (
            USERID,
            TODATE,
            ENTRYDT,
            REMARKS,
            PRESENTFILEFLAG,
            TOUSERID,
            FLAG,
            PONOID
        )
        VALUES
        (
            :fromUserId,
            :toDate,
            SYSDATE,
            'Forwarded to Other',
            'Y',
            :toUserId,
            'S',
            :poNoId
        )";

            await _context.Database.ExecuteSqlRawAsync(
                insertQry,
                new OracleParameter("fromUserId", fromUserId),
                new OracleParameter("toDate", toDate),
                new OracleParameter("toUserId", toUserId),
                new OracleParameter("poNoId", poNoId)
            );

            return Ok("Received and Send Other Successfully");
        }


        [HttpGet("selectFileReceiver")]
        public async Task<ActionResult<IEnumerable<selectFileReceiverDTO>>> selectFileReceiver()
        {
            string qry = "";

            qry = @"  select USERID, EMAILID, FIRSTNAME, LASTNAME, DISPLAYNAME from usrusers 
where USERID  in (3000,
2999,
2998,
2997,
2995,2927) 

order by EMAILID ";



            var myList = _context.selectFileReceiverDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


    }
}
