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
using System.Diagnostics;
using System.Reflection;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NOC : ControllerBase
    {
        private readonly OraDbContext _context;
        public NOC(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("CGMSCNOCApprovedSummary")]
        public async Task<ActionResult<IEnumerable<NOCApprovedSummaryDTO>>> CGMSCNOCApprovedSummary()
        {
            string qry = "";      
                qry = @"   select districtname,facilityname,count(itemid) nosapplied,sum(CGMSCAppCnt) as Approved,sum(CGMSCrejCnt) as Rejected,facilityid
from 
(
select f.facilityname,m.itemcode,m.itemname,m.strength1,m.unit,mn.nocdate AppliedDT
 , case when nvl(mi.facreqqty,0)=0 then nvl(mi.Approvedqty,0) else nvl(mi.facreqqty,0) end AppliedQty, mi.itemremarks
   ,  case when f.facilitytypeid in (352,353) then nvl(mi.facreqqty,0) else  nvl(mi.CMHOAPRQTY,0) end as CMHOAPRQTY
    ,TO_CHAR(CMHOAppliedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CMHOAprDTTime
 ,nvl(mi.Approvedqty,0) ApprovedQTY
 ,TO_CHAR(CGMSCApprovedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CGMSCAprDTTime
 , case when nvl(mi.IsCGMSCAPR,'NA') ='N' then nvl(mi.facreqqty,0)  else 0 end   as RejectQty
 ,case when mn.nocdate>'01-SEP-2024' then  case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Approved' else 
 (case when nvl(mi.IsCGMSCAPR,'NA') ='N' then 'Rejected'  else 'NA' end )
 end else 'Y' end nocAPRStatus

 , mi.Cgmsclremarks
  ,case when nvl(mi.ISIWH,'NA') ='Y' then 'YES'  else 'NO' end as ISIWH
,nvl(mi.IsCGMSCAPR,'N')  as IsCGMSCAPR
,mn.nocid,mi.sr,mw.warehouseid,mw.warehousename WHName,d.districtname,mn.nocnumber
, case when nvl(mi.IsCGMSCAPR,'N')='Y' then 1 else 0 end as CGMSCAppCnt,case when nvl(mi.IsCGMSCAPR,'N')='N' then 1 else 0 end as CGMSCrejCnt
,f.facilityid,m.itemid
 from mascgmscnoc mn 
 inner join mascgmscnocitems mi on mn.nocid=mi.nocid  
 inner join masitems m on mi.itemid=m.itemid 
 inner join masfacilities f on f.facilityid=mn.facilityid
 inner join masdistricts d on d.districtid=f.districtid
 inner join maswarehouses mw on mw.warehouseid=d.warehouseid 

 where 
1=1  and nvl(IssendAPR,'N')='Y'
and (case when f.facilitytypeid in (352,353) then 'Y' else nvl(mi.ISCMHOAPR,'N')end)='Y'
and f.facilitytypeid in (381,386,382,388,372,369,354,355,356,357,358,365,379,352,353) 
and mn.nocdate>'01-SEP-2024'
and (
case when mn.nocdate>'01-SEP-2024' then 
case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Y' else nvl(mi.IsCGMSCAPR,'NA')  end else 'Y' end
) 
in ('Y','N')

)  group by facilityname,districtname,facilityid order by districtname ";
            
          

            var myList = _context.GetNOCApprovedSummaryDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
        [HttpGet("CGMSCNOCApprovedDetails")]
        public async Task<ActionResult<IEnumerable<NOCApprovedDetailsDTO>>> CGMSCNOCApprovedDetails(string facilityid,string YN)
        {
            string whfacclause = "";
            if (facilityid != "0")
            {
                whfacclause = " and f.facilityid=" + facilityid + @"";
            }
            string whapprej = @" and (
case when mn.nocdate > '01-SEP-2024' then
case when nvl(mi.IsCGMSCAPR, 'NA') = 'Y' then 'Y' else nvl(mi.IsCGMSCAPR, 'NA')  end else 'Y' end
) in ('N','Y')";
            if (YN == "Y")
            {
                whapprej = @" and (
case when mn.nocdate > '01-SEP-2024' then
case when nvl(mi.IsCGMSCAPR, 'NA') = 'Y' then 'Y' else nvl(mi.IsCGMSCAPR, 'NA')  end else 'Y' end
) 
in ('Y')";
            }
            else if (YN == "N")
            {
                whapprej = @" and (
case when mn.nocdate > '01-SEP-2024' then
case when nvl(mi.IsCGMSCAPR, 'NA') = 'Y' then 'Y' else nvl(mi.IsCGMSCAPR, 'NA')  end else 'Y' end
) 
in ('N')";
            }
            else
            {
                whapprej = @" and (
case when mn.nocdate > '01-SEP-2024' then
case when nvl(mi.IsCGMSCAPR, 'NA') = 'Y' then 'Y' else nvl(mi.IsCGMSCAPR, 'NA')  end else 'Y' end
) 
in ('N','Y')";
            }
            string qry = @"   select f.facilityname,m.itemcode,m.itemname,m.strength1,m.unit,mn.nocdate AppliedDT,mn.nocdate
 , case when nvl(mi.facreqqty,0)=0 then nvl(mi.Approvedqty,0) else nvl(mi.facreqqty,0) end AppliedQty, mi.itemremarks
   ,  case when f.facilitytypeid in (352,353) then nvl(mi.facreqqty,0) else  nvl(mi.CMHOAPRQTY,0) end as CMHOAPRQTY
    ,TO_CHAR(CMHOAppliedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CMHOAprDTTime
 ,nvl(mi.Approvedqty,0) ApprovedQTY
 ,TO_CHAR(CGMSCApprovedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CGMSCAprDTTime
 , case when nvl(mi.IsCGMSCAPR,'NA') ='N' then nvl(mi.facreqqty,0)  else 0 end   as RejectQty
 ,case when mn.nocdate>'01-SEP-2024' then  case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Approved' else 
 (case when nvl(mi.IsCGMSCAPR,'NA') ='N' then 'Rejected'  else 'NA' end )
 end else 'Y' end nocAPRStatus

 , mi.Cgmsclremarks
  ,case when nvl(mi.ISIWH,'NA') ='Y' then 'YES'  else 'NO' end as ISIWH
,nvl(mi.IsCGMSCAPR,'N')  as IsCGMSCAPR
,mn.nocid,mi.sr,mw.warehouseid,mw.warehousename WHName,mn.nocnumber
 from mascgmscnoc mn 
 inner join mascgmscnocitems mi on mn.nocid=mi.nocid  
 inner join masitems m on mi.itemid=m.itemid 
 inner join masfacilities f on f.facilityid=mn.facilityid
 inner join masdistricts d on d.districtid=f.districtid
 inner join maswarehouses mw on mw.warehouseid=d.warehouseid 

 where 1=1    and nvl(IssendAPR,'N')='Y' " + whfacclause + @"
and (case when f.facilitytypeid in (352,353) then 'Y' else nvl(mi.ISCMHOAPR,'N')end)='Y'
and f.facilitytypeid in (381,386,382,388,372,369,354,355,356,357,358,365,379,352,353) 
and mn.nocdate>'01-SEP-2024'
"+ whapprej + @"
 order by mn.nocdate desc,f.facilityname ";



            var myList = _context.GetNOCApprovedDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("CGMSCNOCPendingSummary")]
        public async Task<ActionResult<IEnumerable<NOCPendingSummaryDTO>>> CGMSCNOCPendingSummary()
        {
            string qry = "";
            qry = @"  select districtname,facilityname,count(itemid) nositems,to_date(cgmscrdate,'dd-MM-yyyy') as CMHOForwardDT,facilityid,nocnumber,nocid

from 
(

select mn.nocid,mi.sr,mw.warehouseid,mw.warehousename WHName,d.districtname,f.facilityname,mn.nocnumber,mn.nocdate,m.itemcode,m.itemname,m.strength1,m.unit

 , case when nvl(mi.facreqqty,0)=0 then nvl(mi.Approvedqty,0) else nvl(mi.facreqqty,0) end AppliedQty
   ,  case when f.facilitytypeid in (352,353) then nvl(mi.facreqqty,mi.Approvedqty) else  nvl(mi.CMHOAPRQTY,0) end as CMHOAPRQTY

  , case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then nvl(mi.Approvedqty,0)  else 0 end   as ApprovedQTY
 , case when nvl(mi.IsCGMSCAPR,'NA') ='N' then nvl(mi.facreqqty,0)  else 0 end   as RejectQty
 ,case when nvl(mi.ISIWH,'NA') ='Y' then 'YES'  else 'NO' end as ISIWH
 ,case when mn.nocdate>'01-SEP-2024' then  case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Approved' else 
 

 (case when nvl(mi.IsCGMSCAPR,'NA') ='N' then 'Rejected'  else 'NA' end )
 
 end else 'Y' end nocAPRStatus
 ,mi.itemremarks
 , mi.Cgmsclremarks
 
   ,  case when f.facilitytypeid in (352,353) then TO_CHAR(nvl(CMHOAppliedDTTime,NOCDATE), 'DD-Mon-YYYY HH:MI:SS AM') else  TO_CHAR(CMHOAppliedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') end as CMHOAprDTTime

   ,  case when f.facilitytypeid in (352,353) then TO_CHAR(nvl(CMHOAppliedDTTime,NOCDATE), 'dd-MM-yyyy') else  TO_CHAR(CMHOAppliedDTTime, 'dd-MM-yyyy') end as cgmscrdate
,TO_CHAR(CGMSCApprovedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CGMSCAprDTTime,f.facilityid,m.itemid

 from mascgmscnoc mn 
 inner join mascgmscnocitems mi on mn.nocid=mi.nocid  
 inner join masitems m on mi.itemid=m.itemid 
 inner join masfacilities f on f.facilityid=mn.facilityid
 inner join masdistricts d on d.districtid=f.districtid
 inner join maswarehouses mw on mw.warehouseid=d.warehouseid 

 where 1=1 and mn.status='C' and
    nvl(mi.Approvedqty,0) >0  
and (case when f.facilitytypeid in (352,353) then 'Y' else nvl(mi.ISCMHOAPR,'N')end)='Y'
and f.facilitytypeid in (381,
386,
382,
388,
372,
369,
354,
355,
356,
357,
358,
365,
379,352,353) 
and mn.nocdate>'01-SEP-2024'
and (
case when mn.nocdate>'01-SEP-2024' then 
case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Y' else nvl(mi.IsCGMSCAPR,'NA')  end else 'Y' end
) 
in ('NA')

) group by districtname,facilityname,cgmscrdate,facilityid,nocnumber,nocid
order by cgmscrdate desc ";



            var myList = _context.GetNOCPendingSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("CGMSCNOCPendingDetails")]
        public async Task<ActionResult<IEnumerable<NOCPendingDetailsDTO>>> CGMSCNOCPendingDetails(string nocid)
        {
            string whfacclause = "";
            if (nocid != "0")
            {
                whfacclause = " and mn.nocid= " + nocid + @"";
            }
            string qry = "";
            qry = @"   select SR,districtname, facilityname, nocnumber, itemcode, itemname, strength1, unit, AppliedQty, CMHOAPRQTY, to_date(cgmscrdate, 'dd-MM-yyyy') as CMHOForwardDT
,nvl(ReadyForIssue,0) as ReadyWH,nvl(Pending,0) as uqcwh
,nvl(transferQTY,0) transferQTY,nvl(tdate,'-') tdate
,nvl(ReadyTotal,0) TotalREadyCGMSC,nvl(UQCTotal,0) as UQCTotal,itemid,facilityid,nocid
        from
        (

select ReadyForIssue, Pending, transferQTY, tdate , ReadyTotal, UQCTotal, mn.nocid, mi.sr, mw.warehouseid, mw.warehousename WHName, d.districtname, f.facilityname, mn.nocnumber, mn.nocdate, m.itemcode, m.itemname, m.strength1, m.unit

 , case when nvl(mi.facreqqty,0)=0 then nvl(mi.Approvedqty,0) else nvl(mi.facreqqty,0) end AppliedQty
   ,  case when f.facilitytypeid in (352,353) then nvl(mi.facreqqty, mi.Approvedqty) else  nvl(mi.CMHOAPRQTY,0) end as CMHOAPRQTY

  , case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then nvl(mi.Approvedqty,0)  else 0 end   as ApprovedQTY
 , case when nvl(mi.IsCGMSCAPR,'NA') ='N' then nvl(mi.facreqqty,0)  else 0 end   as RejectQty
 ,case when nvl(mi.ISIWH,'NA') ='Y' then 'YES'  else 'NO' end as ISIWH
 ,case when mn.nocdate>'01-SEP-2024' then  case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Approved' else 
 

 (case when nvl(mi.IsCGMSCAPR,'NA') ='N' then 'Rejected'  else 'NA' end )
 
 end else 'Y' end nocAPRStatus
 , mi.itemremarks
 , mi.Cgmsclremarks
 
   ,  case when f.facilitytypeid in (352,353) then TO_CHAR(nvl(CMHOAppliedDTTime, NOCDATE), 'DD-Mon-YYYY HH:MI:SS AM') else  TO_CHAR(CMHOAppliedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') end as CMHOAprDTTime

   ,  case when f.facilitytypeid in (352,353) then TO_CHAR(nvl(CMHOAppliedDTTime, NOCDATE), 'dd-MM-yyyy') else  TO_CHAR(CMHOAppliedDTTime, 'dd-MM-yyyy') end as cgmscrdate
,TO_CHAR(CGMSCApprovedDTTime, 'DD-Mon-YYYY HH:MI:SS AM') as CGMSCAprDTTime,f.facilityid,m.itemid

 from mascgmscnoc mn
 inner join mascgmscnocitems mi on mn.nocid=mi.nocid
 inner join masitems m on mi.itemid= m.itemid
 inner join masfacilities f on f.facilityid= mn.facilityid
 inner join masdistricts d on d.districtid= f.districtid
 inner join maswarehouses mw on mw.warehouseid= d.warehouseid



  left outer join
(
         select warehouseid, itemid, sum(ReadyForIssue) as ReadyForIssue, sum(Pending) as Pending
         from
         (
            select w.warehouseid, mi.itemid, b.inwno,
   (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus= 2 then 0 else case when mi.Qctest = 'N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end end ) end ) ReadyForIssue,    
                    nvl(case when mi.qctest= 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                 inner join tbreceipts t on t.receiptid= i.receiptid
                 inner join masitems mi on mi.itemid= i.itemid
                 inner join MASWAREHOUSES w  on w.warehouseid= t.warehouseid
                 left outer join
                 (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid, tb.warehouseid, tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
                 Where  T.Status = 'C'  
                 And(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) and(b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) st on st.warehouseid=mw.warehouseid and st.itemid=m.itemid



 left outer join
(
         select itemid, sum(ReadyForIssue) as ReadyTotal, sum(Pending) as UQCTotal
         from
         (
            select w.warehouseid, mi.itemid, b.inwno,
   (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus= 2 then 0 else case when mi.Qctest = 'N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end end ) end ) ReadyForIssue,    
                    nvl(case when mi.qctest= 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
                 inner join tbreceipts t on t.receiptid= i.receiptid
                 inner join masitems mi on mi.itemid= i.itemid
                 inner join MASWAREHOUSES w  on w.warehouseid= t.warehouseid
                 left outer join
                 (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid, tb.warehouseid, tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
                 Where  T.Status = 'C'  
                 And(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) and(b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by itemid
) sttotal on  sttotal.itemid=m.itemid
        left outer join
        (
select i.itemid, sum(o.issueqty) as transferQTY, t.towarehouseid, max(TRANSFERDATE) as tdate
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join tbindents ti on ti.transferid = t.transferid
inner join tbindentitems tbi on tbi.indentid = ti.indentid and tbi.itemid = i.itemid
inner join tboutwards o on o.indentitemid = tbi.indentitemid
where t.status = 'C'
and t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
and t.transferdate between '01-APR-24' and sysdate
group by i.itemid, t.towarehouseid
) IWHPipe on  IWHPipe.itemid=m.itemid and   IWHPipe.towarehouseid=mw.warehouseid



 where 1=1 and mn.status='C'  and
    nvl(mi.Approvedqty,0) >0   " + whfacclause + @" 
and(case when f.facilitytypeid in (352,353) then 'Y' else nvl(mi.ISCMHOAPR,'N')end)='Y'
and f.facilitytypeid in (381,
386,
382,
388,
372,
369,
354,
355,
356,
357,
358,
365,
379,352,353)
and mn.nocdate>'01-SEP-2024'
and (
case when mn.nocdate>'01-SEP-2024' then
case when nvl(mi.IsCGMSCAPR,'NA') ='Y' then 'Y' else nvl(mi.IsCGMSCAPR,'NA')  end else 'Y' end
) 
in ('NA')) 
order by itemname ";



            var myList = _context.GetNOCPendingDetailsDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }




       

    }
}
