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
using CgmscHO_API.DashHomeDTO;
using System.Reflection;
using System.Security.Principal;
using System.Text.RegularExpressions;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardHome : ControllerBase
    {
        private readonly OraDbContext _context;
        public DashboardHome(OraDbContext context)
        {
            _context = context;
        }
//        [HttpGet("CGMSCIndentPending")]
//        public async Task<ActionResult<IEnumerable<CGMSCIndentPendingDTO>>> CGMSCIndentPending()
//        {



//            string qry = "";
//            qry = @"  select count(distinct mn.facilityid) as nosfac ,count(distinct mn.nocid) as nosIndent,count(distinct ITEMID) as nositems
//from mascgmscnoc mn 
//inner join mascgmscnocitems mni on mni.nocid=mn.nocid
//inner join masfacilities f on f.facilityid=mn.facilityid
//where mn.status in ('C')  and mni.bookedqty>0
//and mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)
//and mn.nocdate between SYSDATE -30 and sysdate  ";

//            var myList = _context.CGMSCIndentPendingDbSet
//           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

//            return myList;

//        }

        [HttpGet("CGMSCIndentPending")]
        public async Task<ActionResult<IEnumerable<CGMSCIndentPendingDTO>>> CGMSCIndentPending(
     string mcid,
     string hodid)
        {
            string whclause = "";
            if (mcid != "0")
                whclause = " and mc.mcid =" + mcid;
            if (hodid != "0")
                whclause = whclause + " and  t.hodid = " + hodid;
            string qry = "";
            qry = "  select count(distinct mn.facilityid) as nosfac ,count(distinct mn.nocid) as nosIndent,count(distinct mni.ITEMID) as nositems\r\nfrom mascgmscnoc mn \r\ninner join mascgmscnocitems mni on mni.nocid=mn.nocid\r\n inner join masitems m on  m.itemid=mni.itemid\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\ninner join masfacilities f on f.facilityid=mn.facilityid\r\n     inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid\r\nwhere mn.status in ('C')  and mni.bookedqty>0 " + whclause + " \r\nand mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)\r\nand mn.nocdate between SYSDATE -30 and sysdate  ";
            List<CGMSCIndentPendingDTO> myList = this._context.CGMSCIndentPendingDbSet.FromSqlInterpolated<CGMSCIndentPendingDTO>(FormattableStringFactory.Create(qry)).ToList<CGMSCIndentPendingDTO>();
            ActionResult<IEnumerable<CGMSCIndentPendingDTO>> actionResult = (ActionResult<IEnumerable<CGMSCIndentPendingDTO>>)myList;
            whclause = (string)null;
            qry = (string)null;
            myList = (List<CGMSCIndentPendingDTO>)null;
            return actionResult;
        }

        [HttpGet("CGMSCStockHome")]
        public async Task<ActionResult<IEnumerable<StockHomeDTO>>> CGMSCStockHome(string mcid)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }

            string qry = "";
            qry = @"  select sr,EDLtpe,nositems,STKVALUE
from 
(
select sr, EDLtpe,count(distinct itemid) as nositems,round(sum(STKVALUE)/10000000,2) as STKVALUE

from 
(
select case when m.isedl2021='Y' then 1 else 2 end as sr, case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLtpe,b.batchno,b.MFGDATE,b.EXPDATE, nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,b.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid

from tbreceiptbatches b
left outer join 
(
select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
 inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
 ) p on p.ponoid=b.ponoid 
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 Where T.Status = 'C' 
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
"+ whclause + @"
 and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0
 ) b group by EDLtpe,sr
 
 union all
 
 select 3 as sr, 'Total' as EDLtpe,count(distinct itemid) as nositems,round(sum(STKVALUE)/10000000,2) as STKVALUE

from 
(
select b.batchno,b.MFGDATE,b.EXPDATE, nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,b.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid

from tbreceiptbatches b
left outer join 
(
select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
 inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
 ) p on p.ponoid=b.ponoid 
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 Where T.Status = 'C' 
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
"+ whclause + @"
 and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0
 ) c
 ) bc 
 order by sr  ";

            var myList = _context.StockHomeDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("TotalCGMSCStockHome")]
        public async Task<ActionResult<IEnumerable<TotalCurrentStockHomeDTO>>> TotalCGMSCStockHome(string mcid)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }

            string qry = "";
            qry = @"  select count(distinct itemid) as nositems,round(sum(STKVALUE)/10000000,2) as STKVALUE
,count(distinct EDLitem)-1 as EDLnositems,round(sum(EDLitemValue)/10000000,2) as EDLitemValue
,count(distinct NonEDL)-1 as NEDLnositems,round(sum(NonEDLitemValue)/10000000,2) as NonEDLitemValue
from 
(
select case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLtpe, nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,
b.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid
,case when m.isedl2021='Y' then b.itemid else 0  end as EDLitem
,case when m.isedl2021='Y' then  0  else b.itemid end as NonEDL
,case when m.isedl2021='Y' then (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst else 0  end as EDLitemValue
,case when m.isedl2021='Y' then 0  else  (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst end as NonEDLitemValue
from tbreceiptbatches b
left outer join 
(
select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
 inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
 ) p on p.ponoid=b.ponoid 
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 Where T.Status = 'C' 
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
 "+ whclause + @"
 and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0
 ) b ";

            var myList = _context.TotalCurrentStockHomeDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
    
        [HttpGet("IndentcntHome")]
        public async Task<ActionResult<IEnumerable<IndentCntHome>>> IndentcntHome(string mcid,string yrid)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }

            string whereyearid = "";
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
                whereyearid = " and i.accyrsetid  =" + whyearid;
            }
            else
            {
                whereyearid = " and i.accyrsetid  ="+yrid;
            }

            string qry = "";
            qry = @" select 'DHS' as HOD, count(distinct itemid) as nositems,sum(totalAIReurn) as Returned,count(distinct itemid) -sum(totalAIReurn) as ActualAI
from 
(
select m.itemid,nvl(DME_INDENTQTY, 0) as DMEAI,nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) as DHAI ,m.itemname,m.strength1
,ISAIRETURN_DME,ISAIRETURN_DHS, (case when ISAIRETURN_DME='Y' and nvl(DME_INDENTQTY, 0)>0  then 1 else case when  ISAIRETURN_DHS='Y' and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) )>0 then 1 else 0 end end ) AIReturn
,case when nvl(DME_INDENTQTY, 0)>0 and ISAIRETURN_DME is null then 1 else 0 end as DMEAIcnt
,case when (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) >0 and ISAIRETURN_DHS is null then 1 else 0 end as DHSAIcnt
,case when ISAIRETURN_DHS='Y' then 1 else 0 end as DHSReturnReturnCNT
,case when ISAIRETURN_DHS='Y' then 1 else 0 end  as totalAIReurn
from itemindent i
inner join masitems m on m.itemid=i.itemid
left outer join masitemgroups g on g.groupid=m.groupid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=ic.MCID
where M.ISFREEZ_ITPR is null  "+ whereyearid + @"  "+ whclause + @"
and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) >0
) a
union all

select  'CME' as HOD,  count(distinct itemid) as nositems,sum(totalAIReurn) as Returned,count(distinct itemid) -sum(totalAIReurn) as ActualAI
from 
(
select m.itemid,nvl(DME_INDENTQTY, 0) as DMEAI,nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) as DHAI ,m.itemname,m.strength1
,ISAIRETURN_DME,ISAIRETURN_DHS, (case when ISAIRETURN_DME='Y' and nvl(DME_INDENTQTY, 0)>0  then 1 else case when  ISAIRETURN_DHS='Y' and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) )>0 then 1 else 0 end end ) AIReturn
,case when nvl(DME_INDENTQTY, 0)>0 and ISAIRETURN_DME is null then 1 else 0 end as DMEAIcnt
,case when (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) >0 and ISAIRETURN_DHS is null then 1 else 0 end as DHSAIcnt
,case when ISAIRETURN_DME='Y' then 1 else 0 end  as totalAIReurn
from itemindent i
inner join masitems m on m.itemid=i.itemid
left outer join masitemgroups g on g.groupid=m.groupid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=ic.MCID
where M.ISFREEZ_ITPR is null "+ whereyearid + @"  "+ whclause + @" 
and (nvl(DME_INDENTQTY, 0)) >0
) b

union all

select 'Total' as HOD, count(distinct itemid) as nositems,sum(totalAIReurn) as Returned,count(distinct itemid) -sum(totalAIReurn) as ActualAI
from 
(
select m.itemid,nvl(DME_INDENTQTY, 0) as DMEAI,nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) as DHAI ,m.itemname,m.strength1
,ISAIRETURN_DME,ISAIRETURN_DHS, (case when ISAIRETURN_DME='Y' and nvl(DME_INDENTQTY, 0)>0  then 1 else case when  ISAIRETURN_DHS='Y' and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0) )>0 then 1 else 0 end end ) AIReturn
,case when nvl(DME_INDENTQTY, 0)>0 and ISAIRETURN_DME is null then 1 else 0 end as DMEAIcnt
,case when (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) >0 and ISAIRETURN_DHS is null then 1 else 0 end as DHSAIcnt
,case when ISAIRETURN_DME='Y' then 1 else 0 end as DMEReturnCNT
,case when ISAIRETURN_DHS='Y' then 1 else 0 end as DHSReturnReturnCNT
,case when ISAIRETURN_DME='Y' then 1 else case when ISAIRETURN_DHS='Y' then 1 else 0 end end as totalAIReurn
from itemindent i
inner join masitems m on m.itemid=i.itemid
left outer join masitemgroups g on g.groupid=m.groupid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=ic.MCID
where M.ISFREEZ_ITPR is null "+ whereyearid + @" "+ whclause + @"
and (nvl(DME_INDENTQTY, 0)+nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) >0
) c ";
            var myList = _context.IndentCntHomeDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

            [HttpGet("NearExpMonthHome")]
        public async Task<ActionResult<IEnumerable<NearExpMonthHomeDTO>>> NearExpMonthHome(string mcid,string nextXmonth)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            string whmonth = "";
            if (nextXmonth != "0")
            {

                whmonth = nextXmonth;
            }
            else
            {
                whmonth = "5";
            }


            string qry = "";
            qry = @"  select MM,md.MONTHNAME,md.MNAME,count(distinct itemid) as nositems,count(distinct batchno) as nosbatches,
round(sum(STKVALUE)/10000000,2) as STKVALUEcr
from 
(
select b.batchno,b.MFGDATE,b.EXPDATE, nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,
b.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid
,to_char(b.expdate,'YYYY-MM') as YrMonth
,to_date(TO_CHAR(b.expdate, 'MON-YYYY'), 'MON-YYYY') mon
,to_char(b.expdate,'mm') as MM
from tbreceiptbatches b
left outer join 
(
select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
 inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
 ) p on p.ponoid=b.ponoid 
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 Where T.Status = 'C' 
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
 and b.qastatus = '1' and b.EXPDATE between sysdate and 
 LAST_DAY(ADD_MONTHS(SYSDATE, "+ whmonth + @"))
"+ whclause + @"
 and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0
 ) d
 inner join masfymonth md on md.monthid=cast(d.MM as int)
 group by d.MM,md.MONTHNAME,md.MNAME
 order by d.MM ";

            var myList = _context.NearExpMonthHomeDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("IssuedCFY")]
        public async Task<ActionResult<IEnumerable<IssuedCFYDTO>>> IssuedCFY(string yrid, string mcid,string itemid)
        {
            string whclause = "";
            string whereyearid = "";
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
            }
            else
            {
                whyearid = yrid;
            }

            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            if (itemid != "0")
            {

                whclause += " and m.itemid =" + itemid;
            }

            string qry = "";
            qry = @"  select mcategory,mcid,sum(IssueQTY) as IssueQTY,round(sum(TotalVal)/10000000,2) as TotalValuecr,count(distinct itemid) as nositems,count(distinct facilityid) nosfacility from 
(
select  mc.mcid,mc.mcategory, m.itemid,tb.facilityid
      ,round(sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
    ,sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) as IssueQTY
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
       where 1=1  and tb.status = 'C'   " + whclause + @"
       and 
       tb.issuetype='NO'  
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID=" + whyearid + @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID=" + whyearid + @")  
        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,tb.facilityid
        ) group by mcategory,mcid
        
        order by mcid ";



            var myList = _context.IssuedCFYDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        //        [HttpGet("Last7DaysIssue")]
        //        public async Task<ActionResult<IEnumerable<IssueDaysDTO>>> Last7DaysIssue(string days, string mcid, string yrid)
        //        {
        //            string whclause = "";
        //            if (mcid != "0")
        //            {

        //                whclause = " and mc.mcid =" + mcid;
        //            }
        //            string whereyearid = "";
        //            string whyearid = "";
        //            string tofromDT = "";

        //            if (yrid == "0")
        //            {
        //                FacOperations f = new FacOperations(_context);


        //                whyearid = f.getACCYRSETID();
        //                whereyearid = " and yrid =" + whyearid;
        //                tofromDT = " and tb.indentdate between(select STARTDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + @") and (select ENDDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + @")";
        //            }
        //            string daycluase = "";
        //            if (days != "0")
        //            {

        //                daycluase = " and tb.indentdate >sysdate-" + days;
        //            }
        //            string qry = "";
        //            if (yrid != "0")
        //            {

        //                qry = @" select round(sum(TotalVal)/10000000,2) as TotalValuecr,count(distinct itemid) as nositems,count(distinct facilityid) nosfacility,indentdate,to_char(indentdate,'dd-MM-yyyy') as IndentDT from 
        //(
        //select  mc.mcid,mc.mcategory, m.itemid,tb.facilityid
        //      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
        //    ,tb.indentdate
        //  from tbindents tb
        //         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
        //         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
        //         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
        //          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
        // inner join tbreceipts r on r.receiptid=ri.receiptid
        //         inner join masitems m on m.itemid = tbi.itemid
        //         inner join masfacilities f on f.facilityid = tb.facilityid
        //         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
        //          inner join masitemcategories c on c.categoryid=m.categoryid
        //       inner join masitemmaincategory mc on mc.MCID=c.MCID
        //           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
        //    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
        //       where 1=1  and tb.status = 'C' " + whclause + @"
        //       and        tb.issuetype='NO'  " + daycluase + @" " + tofromDT + @"
        //        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,tb.facilityid,tb.indentdate
        //        ) group by indentdate
        //        order by indentdate ";
        //            }
        //            else
        //            {

        //                qry = @" select round(sum(TotalVal)/10000000,2) as TotalValuecr,count(distinct itemid) as nositems,count(distinct facilityid) nosfacility,'-' as indentdate,'-' as IndentDT from 
        //(
        //select  mc.mcid,mc.mcategory, m.itemid,tb.facilityid
        //      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
        //  from tbindents tb
        //         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
        //         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
        //         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
        //          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
        // inner join tbreceipts r on r.receiptid=ri.receiptid
        //         inner join masitems m on m.itemid = tbi.itemid
        //         inner join masfacilities f on f.facilityid = tb.facilityid
        //         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
        //          inner join masitemcategories c on c.categoryid=m.categoryid
        //       inner join masitemmaincategory mc on mc.MCID=c.MCID
        //           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
        //    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
        //       where 1=1  and tb.status = 'C' " + whclause + @"
        //       and        tb.issuetype='NO'  " + daycluase + @" " + tofromDT + @"
        //        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,tb.facilityid
        //        )  ";
        //            }

        //            var myList = _context.IssueDaysDbSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;

        //        }


        [HttpGet("Last7DaysIssue")]
        public async Task<ActionResult<IEnumerable<IssueDaysDTO>>> Last7DaysIssue(
     string days,
     string mcid,
     string yrid,
     string hodid,
     string ltflag)
        {
            string whclause = "";
            if (mcid != "0")
                whclause = " and mc.mcid =" + mcid;
            if (hodid != "0")
                whclause = whclause + " and  t.hodid = " + hodid;
            string whereyearid = "";
            string whyearid = "";
            string tofromDT = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
                tofromDT = " and tb.indentdate between(select STARTDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ") and (select ENDDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ")";
                f = (FacOperations)null;
            }
            string daycluase = "";
            if (days != "0")
                daycluase = " and tb.indentdate >sysdate-" + days;
            string qry = "";
            if (ltflag != "0")
                qry = " select round(sum(TotalVal)/10000000,2) as TotalValuecr,count(distinct itemid) as nositems,count(distinct facilityid) nosfacility,indentdate,to_char(indentdate,'dd-MM-yyyy') as IndentDT from \r\n(\r\nselect  mc.mcid,mc.mcategory, m.itemid,tb.facilityid\r\n      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal\r\n    ,tb.indentdate\r\n  from tbindents tb\r\n         inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid\r\n inner join tbreceipts r on r.receiptid=ri.receiptid\r\n         inner join masitems m on m.itemid = tbi.itemid\r\n         inner join masfacilities f on f.facilityid = tb.facilityid\r\n         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid\r\n          inner join masitemcategories c on c.categoryid=m.categoryid\r\n       inner join masitemmaincategory mc on mc.MCID=c.MCID\r\n           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   \r\n    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n       where 1=1  and tb.status = 'C' " + whclause + "\r\n       and        tb.issuetype='NO'  " + daycluase + " " + tofromDT + "\r\n        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,tb.facilityid,tb.indentdate\r\n        ) group by indentdate\r\n        order by indentdate ";
            else
                qry = " select round(sum(TotalVal)/10000000,2) as TotalValuecr,count(distinct itemid) as nositems,count(distinct facilityid) nosfacility,'-' as indentdate,'-' as IndentDT from \r\n(\r\nselect  mc.mcid,mc.mcategory, m.itemid,tb.facilityid\r\n      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal\r\n  from tbindents tb\r\n         inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid\r\n inner join tbreceipts r on r.receiptid=ri.receiptid\r\n         inner join masitems m on m.itemid = tbi.itemid\r\n         inner join masfacilities f on f.facilityid = tb.facilityid\r\n         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid\r\n          inner join masitemcategories c on c.categoryid=m.categoryid\r\n       inner join masitemmaincategory mc on mc.MCID=c.MCID\r\n           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   \r\n    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid\r\n       where 1=1  and tb.status = 'C' " + whclause + "\r\n       and        tb.issuetype='NO'  " + daycluase + " " + tofromDT + "\r\n        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,tb.facilityid\r\n        )  ";
            List<IssueDaysDTO> myList = this._context.IssueDaysDbSet.FromSqlInterpolated<IssueDaysDTO>(FormattableStringFactory.Create(qry)).ToList<IssueDaysDTO>();
            ActionResult<IEnumerable<IssueDaysDTO>> actionResult = (ActionResult<IEnumerable<IssueDaysDTO>>)myList;
            whclause = (string)null;
            whereyearid = (string)null;
            whyearid = (string)null;
            tofromDT = (string)null;
            daycluase = (string)null;
            qry = (string)null;
            myList = (List<IssueDaysDTO>)null;
            return actionResult;
        }


        //        [HttpGet("POCountCFY")]
        //        public async Task<ActionResult<IEnumerable<HODYearWisePODTONew>>> POCountCFY(string yrid, string mcid)
        //        {
        //            string whclause = "";
        //            if (mcid != "0")
        //            {

        //                whclause = " and mc.mcid =" + mcid;
        //            }

        //            string whereyearid = "";
        //            string whyearid = "";
        //            if (yrid == "0")
        //            {
        //                FacOperations f = new FacOperations(_context);


        //                whyearid = f.getACCYRSETID();
        //                whereyearid = " and yrid =" + whyearid;
        //            }

        //            string qry = "";
        //            qry = @"  
        // select to_char(AIFINYEAR) as ID,AIFINYEAR as ACCYRSETID,(select SHACCYEAR from masaccyearsettings  where ACCYRSETID=" + whyearid + @") as SHACCYEAR,count(distinct dhsitem) DHSPOITEMS,
        //round(sum(dhspovalue)/10000000,2) DHSPOVALUE,
        //count(distinct dmeitem) DMEPOITEMS,
        //round(sum(dmepovalue)/10000000,2) DMEPOVALUE,
        //count(distinct itemcode) TOTALPOITEMS,
        //round(sum(dhspovalue)/10000000,2)+round(sum(dmepovalue)/10000000,2) TOTALPOVALUE, 
        //round(sum(dhsrvalue)/10000000,2) DHSRECVALUE,
        //round(sum(dmervalue)/10000000,2) DMERECVALUE,
        //round(sum(nvl(dhsrvalue,0))/10000000,2)+round(sum(nvl(dmervalue,0))/10000000,2) TOTALRECVALUE
        //from (
        //select mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,
        //                            oi.absqty as orderedqty, 
        //                            nvl(sum(od.dhsqty),0) dhspoqty,
        //                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,
        //                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,
        //                            nvl(sum(od.dmeqty),0) dmepoqty,
        //                            nvl(sum(od.dmeqty),0)*c.finalrategst dmepovalue,
        //                            nvl(r2.dmerqty,0) dmerqty ,nvl(r2.dmerqty,0)*c.finalrategst dmervalue,
        //                            case when nvl(sum(od.dhsqty),0) > 0 then 1 else 0 end dhspocnt,
        //                            case when nvl(sum(od.dmeqty),0) > 0 then 1 else 0 end dmepocnt,
        //                            case when nvl(sum(od.dhsqty),0) > 0 then mi.itemid else null end dhsitem,
        //                            case when nvl(sum(od.dmeqty),0) > 0 then mi.itemid else null end dmeitem,
        //                            op.AIFINYEAR
        //                            from masItems MI
        //                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) 
        //                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
        //                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) 
        //                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
        //                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid
        //                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID
        //                                              left outer join  
        //                                                (
        //                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
        //                                                from tbreceipts t 
        //                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        //                                                where T.Status = 'C' and  T.receipttype = 'NO' 
        //                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
        //                                                and t.receiptid not in (select tr.receiptid
        //                                                                           from tbindents t  
        //                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
        //                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //                                                                           where t.status = 'C' and t.issuetype in ('RS') )
        //                                                group by I.ItemID, T.PoNoID                       
        //                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) 
        //                                                left outer join  
        //                                                (
        //                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dmerqty                  
        //                                                from tbreceipts t 
        //                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
        //                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
        //                                                where T.Status = 'C' and  T.receipttype = 'NO'
        //                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
        //                                                and t.receiptid not in (select tr.receiptid
        //                                                                           from tbindents t  
        //                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
        //                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
        //                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
        //                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
        //                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
        //                                                                           where t.status = 'C' and t.issuetype in ('RS') )
        //                                                group by I.ItemID, T.PoNoID                       
        //                                                ) r2 on (r2.itemid =oi.itemid and r2.ponoid =op.ponoid) 
        //                                              where 1=1 and op.AIFINYEAR=" + whyearid + @"   " + whclause + @"
        //                                              group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate,mi.itemcode,mi.itemid,itemname,
        //                                              oi.absqty,mi.unit,c.finalrategst,r1.dhsrqty, r2.dmerqty ,op.AIFINYEAR
        //                                              ) group by AIFINYEAR order by AIFINYEAR ";



        //            var myList = _context.HODYearWisePODTONewDbSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;

        //        }


        [HttpGet("POCountCFY")]
        public async Task<ActionResult<IEnumerable<HODYearWisePODTONew>>> POCountCFY(
      string yrid,
      string mcid,
      string hodid)
        {
            string whclause = "";
            if (mcid != "0")
                whclause = " and mc.mcid =" + mcid;
            string whdeptid = "";
            if (hodid != "0")
            {
                if (hodid == "2")
                    whdeptid += " and   op.DEPTID = 367";
                if (hodid == "7")
                    whdeptid += " and   op.DEPTID = 371";
                if (hodid == "3")
                    whdeptid += " and   op.DEPTID in (364,378)";
            }
            string whereyearid = "";
            string whyearid = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
                f = (FacOperations)null;
            }
            string qry = "";
            qry = "  \r\n select to_char(AIFINYEAR) as ID,AIFINYEAR as ACCYRSETID,(select SHACCYEAR from masaccyearsettings  where ACCYRSETID=" + whyearid + ") as SHACCYEAR,count(distinct dhsitem) DHSPOITEMS,\r\nround(sum(dhspovalue)/10000000,2) DHSPOVALUE,\r\ncount(distinct dmeitem) DMEPOITEMS,\r\nround(sum(dmepovalue)/10000000,2) DMEPOVALUE,\r\ncount(distinct itemcode) TOTALPOITEMS,\r\nround(sum(dhspovalue)/10000000,2)+round(sum(dmepovalue)/10000000,2) TOTALPOVALUE, \r\nround(sum(dhsrvalue)/10000000,2) DHSRECVALUE,\r\nround(sum(dmervalue)/10000000,2) DMERECVALUE,\r\nround(sum(nvl(dhsrvalue,0))/10000000,2)+round(sum(nvl(dmervalue,0))/10000000,2) TOTALRECVALUE\r\nfrom (\r\nselect mc.mcid,mc.MCATEGORY,mi.itemcode,itemname,mi.unit,op.pono pono,op.ponoid,to_char( op.soissuedate,'dd/mm/yyyy') podate,\r\n                            oi.absqty as orderedqty, \r\n                            nvl(sum(od.dhsqty),0) dhspoqty,\r\n                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,\r\n                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,\r\n                            nvl(sum(od.dmeqty),0) dmepoqty,\r\n                            nvl(sum(od.dmeqty),0)*c.finalrategst dmepovalue,\r\n                            nvl(r2.dmerqty,0) dmerqty ,nvl(r2.dmerqty,0)*c.finalrategst dmervalue,\r\n                            case when nvl(sum(od.dhsqty),0) > 0 then 1 else 0 end dhspocnt,\r\n                            case when nvl(sum(od.dmeqty),0) > 0 then 1 else 0 end dmepocnt,\r\n                            case when nvl(sum(od.dhsqty),0) > 0 then mi.itemid else null end dhsitem,\r\n                            case when nvl(sum(od.dmeqty),0) > 0 then mi.itemid else null end dmeitem,\r\n                            op.AIFINYEAR\r\n                            from masItems MI\r\n                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID) \r\n                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) " + whdeptid + "\r\n                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) \r\n                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n                                              inner join masitemcategories ic on ic.categoryid = mi.categoryid\r\n                                              inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n                                              left outer join  \r\n                                                (\r\n                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  \r\n                                                from tbreceipts t \r\n                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) \r\n                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)\r\n                                                where T.Status = 'C' and  T.receipttype = 'NO' \r\n                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)\r\n                                                and t.receiptid not in (select tr.receiptid\r\n                                                                           from tbindents t  \r\n                                                                           inner join tbindentitems i on (i.indentid = t.indentid) \r\n                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) \r\n                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)\r\n                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid\r\n                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid\r\n                                                                           where t.status = 'C' and t.issuetype in ('RS') )\r\n                                                group by I.ItemID, T.PoNoID                       \r\n                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) \r\n                                                left outer join  \r\n                                                (\r\n                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dmerqty                  \r\n                                                from tbreceipts t \r\n                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) \r\n                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)\r\n                                                where T.Status = 'C' and  T.receipttype = 'NO'\r\n                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)\r\n                                                and t.receiptid not in (select tr.receiptid\r\n                                                                           from tbindents t  \r\n                                                                           inner join tbindentitems i on (i.indentid = t.indentid) \r\n                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) \r\n                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)\r\n                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid\r\n                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid\r\n                                                                           where t.status = 'C' and t.issuetype in ('RS') )\r\n                                                group by I.ItemID, T.PoNoID                       \r\n                                                ) r2 on (r2.itemid =oi.itemid and r2.ponoid =op.ponoid) \r\n                                              where 1=1 and op.AIFINYEAR=" + whyearid + "   " + whclause + "\r\n                                              group by mc.mcid,mc.MCATEGORY,op.pono,op.ponoid,op.soissuedate,mi.itemcode,mi.itemid,itemname,\r\n                                              oi.absqty,mi.unit,c.finalrategst,r1.dhsrqty, r2.dmerqty ,op.AIFINYEAR\r\n                                              ) group by AIFINYEAR order by AIFINYEAR ";
            List<HODYearWisePODTONew> myList = this._context.HODYearWisePODTONewDbSet.FromSqlInterpolated<HODYearWisePODTONew>(FormattableStringFactory.Create(qry)).ToList<HODYearWisePODTONew>();
            ActionResult<IEnumerable<HODYearWisePODTONew>> actionResult = (ActionResult<IEnumerable<HODYearWisePODTONew>>)myList;
            whclause = (string)null;
            whdeptid = (string)null;
            whereyearid = (string)null;
            whyearid = (string)null;
            qry = (string)null;
            myList = (List<HODYearWisePODTONew>)null;
            return actionResult;
        }




        //       [HttpGet("Last7DaysReceipt")]
        //       public async Task<ActionResult<IEnumerable<ReceiptValuesDTO>>> Last7DaysReceipt(string days, string mcid, string yrid)
        //       {
        //           string whclause = "";
        //           if (mcid != "0")
        //           {

        //               whclause = " and mc.mcid =" + mcid;
        //           }
        //           string whereyearid = "";
        //           string whyearid = "";
        //           string tofromDT = "";
        //           if (yrid == "0")
        //           {
        //               FacOperations f = new FacOperations(_context);


        //               whyearid = f.getACCYRSETID();
        //               whereyearid = " and yrid =" + whyearid;
        //               tofromDT = " and RECEIPTDATE between(select STARTDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + @") and (select ENDDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + @")";
        //           }
        //           string daycluase = "";
        //           if (days != "0")
        //           {

        //               daycluase = " and RECEIPTDATE >sysdate-" + days;
        //           }


        //           string qry = "";
        //           qry = @"   select count(distinct ponoid) nosPO,count(distinct itemid) as nositems,round(sum(RValue)/10000000,2) as Rvalue,RECEIPTDATE,to_char(RECEIPTDATE,'dd-MM-yyyy') as ReceiptDT
        // from 
        // (
        // select mc.mcid,mc.mcategory,i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dhsrqty,nvl(sum(tb.absrqty), 0)*c.finalrategst as RValue,RECEIPTDATE
        // from tbreceipts t
        //       inner join soorderplaced op on (op.ponoid = t.ponoid and op.status not in ( 'OC','WA1','I' )) 

        // inner   join tbreceiptitems i on (i.receiptid = t.receiptid)
        //       inner join soOrderedItems OI on (OI.ItemID = i.ItemID) and (OI.ponoid = op.ponoid)
        // inner   join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)
        // inner join aoccontractitems c on c.contractitemid = oi.contractitemid
        // inner join masItems MI on mi.itemid=i.itemid
        // inner join masitemcategories ic on ic.categoryid = mi.categoryid
        //inner join masitemmaincategory mc on mc.MCID=ic.MCID
        // where T.Status = 'C' and  T.receipttype = 'NO' " + whclause + @" " + daycluase + @"
        //" + tofromDT + @"
        // group by i.itemid, t.ponoid,c.finalrategst,mc.mcid,mc.mcategory,RECEIPTDATE
        // ) group  by RECEIPTDATE order by RECEIPTDATE ";

        //           var myList = _context.ReceiptValuesDbSet
        //          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //           return myList;

        //       }

        [HttpGet("Last7DaysReceipt")]
        public async Task<ActionResult<IEnumerable<ReceiptValuesDTO>>> Last7DaysReceipt(
     string days,
     string mcid,
     string yrid,
     string hodid)
        {
            string whclause = "";
            if (mcid != "0")
                whclause = " and mc.mcid =" + mcid;
            string whereyearid = "";
            string whyearid = "";
            string tofromDT = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
                tofromDT = " and RECEIPTDATE between(select STARTDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ") and (select ENDDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ")";
                f = (FacOperations)null;
            }
            string daycluase = "";
            if (days != "0")
                daycluase = " and RECEIPTDATE >sysdate-" + days;
            string whdeptid = "";
            if (hodid != "0")
            {
                if (hodid == "2")
                    whdeptid += " and   op.DEPTID = 367";
                if (hodid == "7")
                    whdeptid += " and   op.DEPTID = 371";
                if (hodid == "3")
                    whdeptid += " and   op.DEPTID in (364,378)";
            }
            string qry = "";
            qry = "   select count(distinct ponoid) nosPO,count(distinct itemid) as nositems,round(sum(RValue)/10000000,2) as Rvalue,RECEIPTDATE,to_char(RECEIPTDATE,'dd-MM-yyyy') as ReceiptDT\r\n  from \r\n  (\r\n  select mc.mcid,mc.mcategory,i.itemid, t.ponoid, nvl(sum(tb.absrqty), 0) as dhsrqty,nvl(sum(tb.absrqty), 0)*c.finalrategst as RValue,RECEIPTDATE\r\n  from tbreceipts t\r\n        inner join soorderplaced op on (op.ponoid = t.ponoid and op.status not in ( 'OC','WA1','I' ))  \r\n        \r\n  inner   join tbreceiptitems i on (i.receiptid = t.receiptid)\r\n        inner join soOrderedItems OI on (OI.ItemID = i.ItemID) and (OI.ponoid = op.ponoid)\r\n  inner   join tbreceiptbatches tb on (i.receiptitemid = tb.receiptitemid)\r\n  inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n  inner join masItems MI on mi.itemid=i.itemid\r\n  inner join masitemcategories ic on ic.categoryid = mi.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n  where T.Status = 'C' and  T.receipttype = 'NO' " + whclause + " " + daycluase + "\r\n " + tofromDT + " " + whdeptid + "\r\n  group by i.itemid, t.ponoid,c.finalrategst,mc.mcid,mc.mcategory,RECEIPTDATE\r\n  ) group  by RECEIPTDATE order by RECEIPTDATE ";
            List<ReceiptValuesDTO> myList = this._context.ReceiptValuesDbSet.FromSqlInterpolated<ReceiptValuesDTO>(FormattableStringFactory.Create(qry)).ToList<ReceiptValuesDTO>();
            ActionResult<IEnumerable<ReceiptValuesDTO>> actionResult = (ActionResult<IEnumerable<ReceiptValuesDTO>>)myList;
            whclause = (string)null;
            whereyearid = (string)null;
            whyearid = (string)null;
            tofromDT = (string)null;
            daycluase = (string)null;
            whdeptid = (string)null;
            qry = (string)null;
            myList = (List<ReceiptValuesDTO>)null;
            return actionResult;
        }



        //        [HttpGet("DeliveryInMonth")]
        //        public async Task<ActionResult<IEnumerable<DeliveryMonthDTsDTO>>> DeliveryInMonth(string IndentfromDT, string Indenttodt)
        //        {



        //            string qry = "";
        //            qry = @"  select count(distinct facilityid) as nooffacIndented,count(distinct NOCID) as nosindent,count(distinct indentid) as IndentIssued,
        //sum(dropindentid) as dropindentid,count(distinct dropfacid)-1 as dropfac
        //from 
        //(
        //select ind.NOCID,ind.facilityid,d.districtid,w.warehousename,w.warehouseid,d.districtname,nvl(nositems,0) IssueItems, case when nvl(nositems,0)>0 then 1 else 0 end as IndentCleared
        //,tbi.indentid,case when dropindentid is not null then 1 else 0 end as dropindentid,
        //case when DROPDATE is not null then round((to_date(to_char(DROPDATE,'dd-MM-yyyy'),'dd-MM-yyyy'))-ind.nocdate,0) else 0 end  as DaysTakenafterIssue
        //,case when dropindentid is not null then ind.facilityid else 0 end as dropfacid
        //from mascgmscnoc ind 
        //inner join 
        //(
        //select distinct nocid as IndentNOCID from  mascgmscnocitems where nvl(BOOKEDQTY,0)>0
        //) indi on indi.IndentNOCID=ind.nocid
        //inner join masfacilities f on f.facilityid=ind.facilityid
        //inner join masdistricts d on d.districtid=f.districtid
        //INNER JOIN maswarehouses w on w.warehouseid=d.warehouseid
        //inner join
        //(
        //select count(VID) as nosvehicle,vh.warehouseid from  masvehical vh 
        //where  ISACTIVE='Y'
        //group by vh.warehouseid 

        //) vh on vh.warehouseid=w.warehouseid 

        //left outer join 
        //(
        //select tb.nocid,tb.indentid,nositems,di.dropindentid,DROPDATE from tbindents tb
        //left outer join
        //(
        //SELECT INDENTID AS dropindentid,MIN(ENTRYDATE) AS DROPDATE FROM TBINDENTTRAVALE 
        //WHERE ISDROP='Y' GROUP BY INDENTID
        //) di on di.dropindentid=tb.indentid

        //inner join 
        //(
        //select tbi.INDENTID,count(distinct ITEMID) as nositems from tbindentitems tbi
        //group by tbi.INDENTID
        //) tbi on tbi.INDENTID=tb.indentid
        //where tb.nocid is not null and tb.issuetype='NO' and tb.status='C'
        //) tbi on tbi.nocid=ind.nocid

        //where  ind.status='C' and ind.NOCDATE between '" + IndentfromDT + @"' and '" + Indenttodt + @"' )  ";

        //            var myList = _context.DeliveryMonthDbSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;

        //        }



        [HttpGet("DeliveryInMonth")]
        public async Task<ActionResult<IEnumerable<DeliveryMonthDTsDTO>>> DeliveryInMonth(
      string yrid,
      string IndentfromDT,
      string Indenttodt,
      string hodid,
      string mcid)
        {
            string whereyearid = "";
            string whyearid = "";
            string datebetween = "";
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(this._context);
                whyearid = f.getACCYRSETID();
                whereyearid = " and yrid =" + whyearid;
                datebetween = " and ind.NOCDATE between(select STARTDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ") and (select ENDDATE from masaccyearsettings  where ACCYRSETID = " + whyearid + ")";
                f = (FacOperations)null;
            }
            string whclause = "";
            if (hodid != "0")
                whclause = whclause + " and  t.hodid = " + hodid;
            string whmcclause = "";
            if (mcid != "0")
                whmcclause = " and mc.mcid =" + mcid;
            string qry = "";
            qry = "  select count(distinct facilityid) as nooffacIndented,count(distinct NOCID) as nosindent,count(distinct indentid) as IndentIssued,\r\nsum(dropindentid) as dropindentid,count(distinct dropfacid)-1 as dropfac\r\nfrom \r\n(\r\nselect ind.NOCID,ind.facilityid,d.districtid,w.warehousename,w.warehouseid,d.districtname,nvl(nositems,0) IssueItems, case when nvl(nositems,0)>0 then 1 else 0 end as IndentCleared\r\n,tbi.indentid,case when dropindentid is not null then 1 else 0 end as dropindentid,\r\ncase when DROPDATE is not null then round((to_date(to_char(DROPDATE,'dd-MM-yyyy'),'dd-MM-yyyy'))-ind.nocdate,0) else 0 end  as DaysTakenafterIssue\r\n,case when dropindentid is not null then ind.facilityid else 0 end as dropfacid\r\nfrom mascgmscnoc ind \r\ninner join \r\n(\r\nselect distinct msi.nocid as IndentNOCID from  mascgmscnocitems msi\r\n inner join masItems MI on mi.itemid=msi.itemid\r\n  inner join masitemcategories ic on ic.categoryid = mi.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\nwhere nvl(BOOKEDQTY,0)>0  " + whmcclause + "\r\n) indi on indi.IndentNOCID=ind.nocid\r\ninner join masfacilities f on f.facilityid=ind.facilityid\r\n         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid\r\ninner join masdistricts d on d.districtid=f.districtid\r\nINNER JOIN maswarehouses w on w.warehouseid=d.warehouseid\r\ninner join\r\n(\r\nselect count(VID) as nosvehicle,vh.warehouseid from  masvehical vh \r\nwhere  ISACTIVE='Y'\r\ngroup by vh.warehouseid \r\n\r\n) vh on vh.warehouseid=w.warehouseid \r\n\r\nleft outer join \r\n(\r\nselect tb.nocid,tb.indentid,nositems,di.dropindentid,DROPDATE from tbindents tb\r\nleft outer join\r\n(\r\nSELECT INDENTID AS dropindentid,MIN(ENTRYDATE) AS DROPDATE FROM TBINDENTTRAVALE \r\nWHERE ISDROP='Y' GROUP BY INDENTID\r\n) di on di.dropindentid=tb.indentid\r\n\r\ninner join \r\n(\r\nselect tbi.INDENTID,count(distinct ITEMID) as nositems from tbindentitems tbi\r\ngroup by tbi.INDENTID\r\n) tbi on tbi.INDENTID=tb.indentid\r\nwhere tb.nocid is not null and tb.issuetype='NO' and tb.status='C'\r\n) tbi on tbi.nocid=ind.nocid\r\n\r\nwhere  ind.status='C' " + datebetween + " " + whclause + "\r\n\r\n)  ";
            List<DeliveryMonthDTsDTO> myList = this._context.DeliveryMonthDbSet.FromSqlInterpolated<DeliveryMonthDTsDTO>(FormattableStringFactory.Create(qry)).ToList<DeliveryMonthDTsDTO>();
            ActionResult<IEnumerable<DeliveryMonthDTsDTO>> actionResult = (ActionResult<IEnumerable<DeliveryMonthDTsDTO>>)myList;
            whereyearid = (string)null;
            whyearid = (string)null;
            datebetween = (string)null;
            whclause = (string)null;
            whmcclause = (string)null;
            qry = (string)null;
            myList = (List<DeliveryMonthDTsDTO>)null;
            return actionResult;
        }


        [HttpGet("MasIndentitems")]
        public async Task<ActionResult<IEnumerable<MasItemsDash>>> MasIndentitems(string mcid,string yearid,string hodid,string medclgid)
        {
            string whclause = "";
            string whclause1 = "";
            string whai = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
                whclause1 = " and mcid =" + mcid;
            }

            if (hodid != "0")
            {
                if (hodid == "2")
                {
                    whai = " and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) > 0";
                }
                if (hodid == "3")
                {
                    whai = " and nvl(DME_INDENTQTY, 0)>0";
                }
                if (hodid == "7")
                {
                    whai = " and (nvl(DHS_INDENTQTY, 0)) > 0 and mc.MCID=4 ";
                }
            }
            else
            {
                whai = " and (nvl(DHS_INDENTQTY, 0) +nvl(DME_INDENTQTY, 0) + nvl(MITANIN, 0)) > 0";
            }

            string qry = "";
            if (yearid == "0" && hodid=="0")
            {
                qry = " select itemid,NameText,itemname,mcid from VdashItem m where 1=1 "+ whclause1 + " ORDER BY mcid,itemname";
            }


            // order by mc.mcid ,m.itemname 
            else
            {
                FacOperations f = new FacOperations(_context);
                string whyearid = f.getACCYRSETID();
                //  whereyearid = " and yrid =" + whyearid;
                qry = @" select  m.itemid,m.itemname||''||nvl(m.strength1,'-')||'-'|| case when mc.mcid  =1 then g.groupname else '' end  ||'-'||  m.itemcode as NameText, m.itemname,mc.mcid  
,(nvl(DHS_INDENTQTY, 0) +nvl(DME_INDENTQTY, 0) + nvl(MITANIN, 0)) IndentQTY
,m.itemcode
from itemindent i
inner join masitems m on m.itemid=i.itemid
left outer join masitemgroups g on g.groupid=m.groupid
   inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
where M.ISFREEZ_ITPR is null and i.accyrsetid = " + whyearid + @"  " + whclause + @" "+ whai + @"
order by (nvl(DHS_INDENTQTY, 0) +nvl(DME_INDENTQTY, 0) + nvl(MITANIN, 0)) desc";
            }
            var myList = _context.MasItemsDashDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("StockoutPer")]
        public async Task<ActionResult<IEnumerable<StockoutPerHome>>> StockoutPer(string mcid, string edltype,string yrid,string HOD)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause += " and mc.mcid =" + mcid;
        
            }
            if (edltype == "EDL")
            {

                whclause += " and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='EDL'";

            }
            else if (edltype == "Non EDL")
            {
                whclause += " and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='Non EDL'";
            }
            else
            {
                
            }
            if (yrid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whclause += " and i.accyrsetid =" + f.getACCYRSETID();
            }
            else
            {
                whclause += " and i.accyrsetid =" + yrid;
            }
            string qry = "";
            if (HOD == "2")
            {
                qry = @" select EDLtypeid,EDLtpe,count(itemid) as nositems,sum(stockout) as stockout,sum(stockin) as stockin,round(sum(stockout)/count(itemid)*100,0) as stockoutp
from 
(
select case when m.isedl2021='Y' then 1 else 2 end as EDLtypeid, case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLtpe,m.itemid,m.itemname,mc.mcid
,(nvl(DHS_INDENTQTY, 0) +nvl(DME_INDENTQTY, 0) + nvl(MITANIN, 0)) IndentQTY,nvl(STK,0) as STK
,m.itemcode ,case when nvl(STK,0)=0 then 1 else 0 end as stockout,case when nvl(STK,0)=0 then 0 else 1 end as stockin
from itemindent i
inner join masitems m on m.itemid=i.itemid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID=ic.MCID
left outer join 
(
 select sum(STK)STK,itemid
 from 
 (
 select  nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,b.itemid
from tbreceiptbatches b
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 Where T.Status = 'C' 
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
 and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0
 ) group by itemid
)stk on stk.itemid=i.itemid
where M.ISFREEZ_ITPR is null  and (nvl(DHS_INDENTQTY, 0) + nvl(MITANIN, 0)) > 0
" + whclause + @" and (case when ISAIRETURN_DHS='Y' then 1 else 0 end)=0
) group by EDLtpe,EDLtypeid
order by EDLtypeid ";
            }
            else
            {

                qry = @" select EDLtypeid, EDLtpe, count(itemid) as nositems,sum(stockout) as stockout,sum(stockin) as stockin,round(sum(stockout) / count(itemid) * 100, 2) as stockoutp
from
(
select case when m.isedl2021 = 'Y' then 1 else 2 end as EDLtypeid,
case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDLtpe,m.itemid,m.itemname,mc.mcid
,nvl(DME_INDENTQTY, 0) IndentQTY,nvl(STK, 0) as STK
,m.itemcode ,case when nvl(STK, 0) = 0 then 1 else 0 end as stockout,case when nvl(STK, 0) = 0 then 0 else 1 end as stockin
from itemindent i
inner
join masitems m on m.itemid = i.itemid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join
(
 select sum(STK)STK, itemid
 from
 (
 select  nvl(ABSRQTY, 0) - nvl(ALLOTQTY, 0) as STK, b.itemid
from tbreceiptbatches b
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner
join tbreceipts t on t.receiptid = i.receiptid
 inner
join masitems m on m.itemid = b.itemid and m.itemid = b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID = ic.MCID
 Where T.Status = 'C'
 And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate
 and(nvl(ABSRQTY, 0) - nvl(ALLOTQTY, 0)) > 0
 ) group by itemid
)stk on stk.itemid = i.itemid
where M.ISFREEZ_ITPR is null  and nvl(DME_INDENTQTY, 0) > 0
 " + whclause + @"  and(case when ISAIRETURN_DME= 'Y' then 1 else 0 end)= 0
) group by EDLtpe,EDLtypeid
order by EDLtypeid ";
            }
            var myList = _context.StockoutPerHomeDbSet
        .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

            [HttpGet("PartiIndent")]
        public async Task<ActionResult<IEnumerable<IndentHomeDTO>>> PartiIndent(string itemid)
        {


            string qry = @" select ACCYEAR,case when mc.mcid in (1,2,4) then m.unit else '' end as SKU, (nvl(DHS_INDENTQTY,0)+nvl(MITANIN,0) ) as DHSAI ,nvl(DME_INDENTQTY,0) DMEAI
,yr.accyrsetid,m.itemid
from itemindent i
inner join masitems m on m.itemid=i.itemid
   inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID= ic.MCID
inner join masaccyearsettings yr on yr.accyrsetid= i.accyrsetid
where i.itemid= " + itemid+@"
order by yr.accyrsetid desc ";
            
            var myList = _context.IndentHomeDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }
        [HttpGet("PartPOsSince1920")]
        public async Task<ActionResult<IEnumerable<YearwisePODTO>>> PartPOsSince1920(string itemid)
        {


            string qry = @" 
select ACCYRSETID,SHACCYEAR,sum(dhspoqty) as dhspoqty,round(sum(dhspovalue)/100000,2) as dhspovalue,sum(DMEPOQTY) as DMEPOQTY,round(sum(DMEPOVALUE)/100000,2) as DMEPOVALUE
,sum(DHSRQTY) as DHSRQTY,round(sum(DHSRVALUE)/100000,2) as DHSRVALUE,sum(DMERQTY) as DMERQTY,round(sum(DMERVALUE)/100000,2) as DMERVALUE
,sum(dhspoqty)+sum(DMEPOQTY) TotalPQTY,round(sum(dhspovalue)/100000,2)+round(sum(DMEPOVALUE)/100000,2)  as TotalPOvalue
,sum(DHSRQTY)+sum(DMERQTY) TotalRQTY,round(sum(DHSRVALUE)/100000,2)+round(sum(DMERVALUE)/100000,2) as TotalRecvalue
from 
(
select yr.SHACCYEAR,
op.pono pono,op.ponoid,
to_char( op.soissuedate,'dd/mm/yyyy') podate,
                            oi.absqty as orderedqty, 
                            nvl(sum(od.dhsqty),0) dhspoqty,
                            nvl(sum(od.dhsqty),0)*c.finalrategst dhspovalue,
                            nvl(r1.dhsrqty,0) dhsrqty,nvl(r1.dhsrqty,0)*c.finalrategst dhsrvalue,
                            nvl(sum(od.dmeqty),0) dmepoqty,
                            nvl(sum(od.dmeqty),0)*c.finalrategst dmepovalue,
                            nvl(r2.dmerqty,0) dmerqty ,nvl(r2.dmerqty,0)*c.finalrategst dmervalue
                            ,op.ACCYRSETID
                            from masItems MI
                      
                                              inner join soOrderedItems OI on (OI.ItemID = MI.ItemID)  and  OI.itemid=" + itemid + @" 
                                              inner join soorderplaced op on (op.ponoid = oi.ponoid and op.status not in ( 'OC','WA1','I' )) 
                                              inner join soorderdistribution od on (od.orderitemid = oi.orderitemid) 
                                              inner join aoccontractitems c on c.contractitemid = oi.contractitemid
                                                    inner join masaccyearsettings yr on yr.ACCYRSETID=op.ACCYRSETID
                                              left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid)  and  i.itemid="+ itemid + @"
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO' 
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 367)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r1 on (r1.itemid =oi.itemid and r1.ponoid =op.ponoid) 
                                                left outer join  
                                                (
                                                select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dmerqty                  
                                                from tbreceipts t 
                                                inner join tbreceiptitems i on (i.receiptid = t.receiptid) and  i.itemid="+ itemid + @" 
                                                inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
                                                where T.Status = 'C' and  T.receipttype = 'NO'
                                                and t.ponoid in (select ponoid from soorderplaced where deptid = 364)
                                                and t.receiptid not in (select tr.receiptid
                                                                           from tbindents t  
                                                                           inner join tbindentitems i on (i.indentid = t.indentid) 
                                                                           inner join tboutwards o on (o.indentitemid =i.indentitemid) 
                                                                           inner join tbreceiptbatches tb on (tb.inwno = o.inwno)
                                                                           inner join tbreceiptitems ti on ti.receiptitemid = tb.receiptitemid
                                                                           inner join tbreceipts tr on tr.receiptid = ti.receiptid
                                                                           where t.status = 'C' and t.issuetype in ('RS') )
                                                group by I.ItemID, T.PoNoID                       
                                                ) r2 on (r2.itemid =oi.itemid and r2.ponoid =op.ponoid) 
                                              where 1=1 and op.soissuedate>'01-Apr-2019' and mi.itemid="+ itemid + @"  
                                              group by op.pono,op.ponoid,op.soissuedate, oi.absqty,c.finalrategst,r1.dhsrqty, r2.dmerqty ,yr.SHACCYEAR,op.ACCYRSETID,yr.SHACCYEAR
                           )           
group by ACCYRSETID,SHACCYEAR order by ACCYRSETID desc ";

            var myList = _context.YearwisePODbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("PartItem_RCs")]
        public async Task<ActionResult<IEnumerable<PartiItem_RCDTO>>> PartItem_RCs(string itemid)
        {
           

        string qry = @" select schemename,suppliername,RANKID,rcstart,rcenddt,
rcduration,basicrate,Tax,RateWithTax,
isextended,RCStatus,ISGEMTENDER as Remarks,


contractitemid,contractid,blacklisted,schemeid
from (
select 
case when  (sysdate between a.contractstartdate and a.contractenddate or 
sysdate between a.contractstartdate and  (case when ci.isextended='Y' then ci.rcextendedupto else a.contractenddate end)) then 'RC Valid'
else 'Not Valid' end as RCStatus,
ms.schemecode,s.suppliername,
to_char(a.ContractStartDate,'dd-MM-yyyy') rcstart,a.ContractEndDate,case when (ci.isextended='Y') then to_char(ci.rcextendedupto,'dd-MM-yyyy') else to_char(ContractEndDate,'dd-MM-yyyy') end RCEndDT,
round(Months_Between(to_date(a.ContractEndDate),to_date(a.ContractStartDate)),0) as RCDuration
,round(Months_Between(to_date(sysdate),to_date(a.ContractStartDate)),0) as TotalDurFromStart
,ci.rcextendedfrom,ci.rcextendedupto,ci.isextended ,a.contractid,ci.contractitemid,
 case when ci.basicratenew is null then ci.basicrate else basicratenew end basicrate,
 nvl(ci.finalrategst,ci.singleunitprice) as RateWithTax,ci.singleunitprice,
                               case when   ci.percentvaluegst is null then   ci.percentvalue  else ci.percentvaluegst end Tax   
                            ,   ci.itemid,ms.schemeid,s.supplierid
, case when sysdate between s.blacklistfdate and  s.blacklisttdate then 'Yes' else 'No' end as blacklisted,
case when (ci.isextended='Y') then round(rcextendedupto-sysdate,0) else  round(ContractEndDate-sysdate,0)  end as DayRemaining
,ci.rankid,ms.schemename,case when ms.ISGEMTENDER='Y' then 'GeM Rate' else '' end as ISGEMTENDER
from aoccontractitems ci
inner join aoccontracts a on a.contractid=ci.contractid
inner join massuppliers s on s.supplierid=a.supplierid
inner join masschemes ms on ms.schemeid=a.schemeid
inner join masitems m on m.itemid=ci.itemid
where   a.status = 'C'  
 and m.itemid="+ itemid + @"
) 
order by schemeid desc ";

            var myList = _context.PartiItem_RCDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("PartItem_Issueyrwise")]
        public async Task<ActionResult<IEnumerable<IssueItemsYearwiseDTO>>> PartItem_Issueyrwise(string itemid)
        {


            string qry = @" select yr.ACCYEAR,mcid,yrid,sum(DHSQTY) as DHSQTY,round(sum(DHSValue)/100000,2) as DHSValuelacs,sum(DMEQTY) as DMEQTY,round(sum(DMEValue)/100000,2) as DMEValuelacs
 from 
 (
select  
mc.mcid, m.itemid,tb.facilityid,(select ACCYRSETID from masaccyearsettings  where tb.indentdate between STARTDATE and ENDDATE) as yrid
      ,round(sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
    ,sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) as IssueQTY
    ,  t.hodid  
    ,case when t.hodid =2 then sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) else 0 end as DHSQTY 
        ,case when t.hodid =2 then round(sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) else 0 end as DHSValue
        
        ,case when t.hodid =3 then sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0)) else 0 end as DMEQTY 
        ,case when t.hodid =3 then round(sum(nvl(tbo.issueqty,0) + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) else 0 end as DMEValue
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
       where 1=1  and tb.status = 'C'  and m.itemid="+ itemid + @"
       and 
       tb.issuetype='NO'  
           and tb.indentdate
           between (select STARTDATE from masaccyearsettings  where ACCYRSETID=525)
           and  (select ENDDATE from masaccyearsettings  where ACCYRSETID=545)  
        group by  mc.mcid,m.itemid,aci.finalrategst,si.singleunitprice,tb.facilityid,t.hodid,tb.indentdate
        ) b 
        inner join masaccyearsettings yr on yr.ACCYRSETID=b.yrid
        group by yrid,yr.ACCYEAR,mcid
        order by yrid desc ";

            var myList = _context.IssueItemsYearwiseDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }




    }
}
