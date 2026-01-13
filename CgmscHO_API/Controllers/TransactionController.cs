using CgmscHO_API.DTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.Models;
using CgmscHO_API.TransactionDTO;
using CgmscHO_API.Utility;
using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        [HttpGet("HoldBatchHistory")]
        public async Task<ActionResult<IEnumerable<HoldBatchHistoryDTO>>> HoldBatchHistory(string fromDate, string ToDate, string mcid,string itemId, string nsqholdflag)
        {
            string whToDate = "";
            string whMcid = "";
            string whItemid = "";

            string qry = "";
            //validate fromDate

            if (string.IsNullOrEmpty(fromDate) || fromDate == "undefined" || fromDate == "0")
            {

                return BadRequest("From Date can not be null or 0");
            }


            //validate ToDate for condition if ToDate is null or 0 the set to current date
            if (string.IsNullOrEmpty(ToDate) || ToDate == "undefined" || ToDate == "0")
            {
                whToDate = "sysdate";
            }
            else
            {
                whToDate = "'" + ToDate + "'";
            }

            if (string.IsNullOrEmpty(mcid) || mcid == "undefined" || mcid == "0")
            {
                whMcid = " and mc.mcid = "+ mcid +" ";
            }
            else
            {
               
            }

            //validate itemid

            if ( itemId != "0")
            {
                whItemid = " and mi.itemid = " + itemId + @" ";
            }

            if(nsqholdflag == "HOLD")
            {
                qry = @" select WAREHOUSENAME,MCID,CATEGORY,s.ITEMCODE,ITEMNAME,STRENGTH, SKU,s.BATCHNO,MFGDATE, EXPDATE, SUM(holdStock) holdStock,to_char( h.holddate,'dd-MM-yyyy') holddate,h.holdreason,
 PONO, PODATE, SUPPLIERNAME,nvl(nrqty,0) as SupplierReceipt ,nvl(IWHReceiptQTy,0) as IWHReceiptQTy,nvl(Fac_iss_qty,0) as Fac_iss_qty,nvl(RF_Qty,0) as RF_Qty, nvl(rs.rsqty,0) rsqty,nvl(rpqty,0) as rpqty

 from

 (
                 select w.warehouseid,w.WAREHOUSENAME,mc.mcid,mc.mcategory as category,mi.itemid,mi.ITEMCODE, 
                 b.batchno,b.mfgdate,b.expdate,b.inwno,mi.ITEMNAME ,
                 mi.strength1 as strength,mi.unit as SKU ,   
                  (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  holdStock,o.ponoid,o.pono,o.soissuedate as podate,sp.supplierid,sp.suppliername
                 from tbreceiptbatches b   
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid " + whItemid + @"
                 inner join masitemcategories c on c.categoryid = mi.categoryid
                 inner join masitemmaincategory mc on mc.mcid = c.mcid "+ whMcid + @"
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid
                 left outer join soorderplaced o on o.ponoid = b.ponoid
                 inner join masschemes sc on sc.schemeid = o.schemeid
                 inner join massuppliers sp on sp.supplierid = o.supplierid
                 left outer join  
                 (   
                         select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                         from tboutwards tbo, tbindentitems tbi , tbindents tb  
                         where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null  
                         group by tbi.itemid,tb.warehouseid,tbo.inwno   
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid   
                 Where  T.Status = 'C'  And b.Whissueblock = 1  " + whMcid + @"
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
                 ) s
                                    left join
                   (
select ITEMID, ITEMCODE, BATCHNO, max(holddate) holddate,holdreason
from
(
select m.itemid,m.itemcode,s.batchno,nvl(s.NEWTESTRESULT,s.testresult) testresult,
s.holddate as holddate,s.holdreason
from qcsamples s
inner join masitems m on m.itemid = s.itemid
where nvl(s.NEWTESTRESULT,s.testresult)  not in ('NSQ') and s.holddate is not null
)

group by ITEMID, ITEMCODE, BATCHNO,holdreason

                   ) h on h.ItemCode = s.ItemCode and h.batchno = s.batchno

left join
                   (
select warehouseid,supplierid,ponoid,ITEMID,BATCHNO, sum(issueqty) rsqty
from
(
      select tb.warehouseid,tb.supplierid, tbi.itemid,b.ponoid, b.batchno,sum(nvl(tbo.issueqty,0)) issueqty
      from tboutwards tbo
      inner join tbindentitems tbi on tbo.indentitemid=tbi.indentitemid
      inner join tbindents tb on tbi.indentid=tb.indentid
      inner join tbreceiptbatches b on b.inwno = tbo.inwno
      inner join masitems m on m.itemid = tbi.itemid
      where tb.status='C' and tb.issuetype = 'RS' --and m.itemcode = 'SP19448' 
      group by tb.warehouseid, tbi.itemid, b.batchno,b.ponoid,tb.supplierid
) group by warehouseid,supplierid,ITEMID,BATCHNO,ponoid

                   ) rs on rs.ITEMID = s.ITEMID and rs.ponoid = s.ponoid and rs.batchno = s.batchno and rs.warehouseid = s.warehouseid and rs.supplierid = s.supplierid

 left join
                   (
                     select  I.ItemID,t.warehouseid,tb.ponoid,t.supplierid,SUM(TB.ABSRQTY) AS rpqty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            where T.Status = 'C' and T.receiptType = 'NO'  and T.RECTYPEID in (2,3,4)
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,t.supplierid            
                   ) rp on rp.ITEMID = rs.ITEMID and rp.ponoid = rs.ponoid and rp.warehouseid = rs.warehouseid and rp.supplierid = rs.supplierid 

left outer join 
                    (
                              select  I.ItemID,t.warehouseid,tb.ponoid,tb.batchno,t.supplierid,SUM(TB.ABSRQTY) AS nrqty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            where T.Status = 'C' and T.receiptType = 'NO' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null 
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,tb.batchno ,t.supplierid             

                    ) nr on nr.itemid = s.itemid and nr.ponoid = s.ponoid and nr.batchno = s.batchno and nr.warehouseid = s.warehouseid and nr.supplierid = s.supplierid 

                    left outer join
                    (
                    select  I.ItemID,t.warehouseid,tb.ponoid,tb.batchno,so.supplierid,SUM(TB.ABSRQTY) AS IWHReceiptQTy
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            inner join soorderplaced  so on so.ponoid=tb.ponoid
                                            where T.Status = 'C' and T.receiptType = 'SP' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null  and T.transferid is not null
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,tb.batchno ,so.supplierid

                    )IWH on IWH.itemid = s.itemid and IWH.ponoid = s.ponoid and IWH.batchno = s.batchno and IWH.warehouseid = s.warehouseid and IWH.supplierid = s.supplierid 
                     left outer join
                    (
                         select
                        tb.warehouseid,rb.ponoid,rb.batchno,so.supplierid,
                         tbi.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) Fac_iss_qty 
                                    from tbindents tb
                                    inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                    inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                     inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                     inner join soorderplaced  so on so.ponoid=rb.ponoid
                                    inner join masitems m on m.itemid = tbi.itemid
                                    inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                    inner join masfacilities f on f.facilityid = tb.facilityid
                                    where tb.status = 'C' and tb.issuetype='NO'                                  
                                    and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
                                    and tbo.notindpdmis is null and rb.notindpdmis is null   
                                    group by tb.warehouseid,rb.ponoid,rb.batchno,so.supplierid,tbi.itemid
                    ) faci on faci.itemid =s.itemid and faci.ponoid = s.ponoid and faci.batchno = s.batchno and faci.warehouseid = s.warehouseid and faci.supplierid = s.supplierid 

        left outer join 
                    (
                              select   T.warehouseid,TB.ponoid,TB.batchno,so.supplierid,
                         I.itemid,SUM(TB.ABSRQTY) AS RF_Qty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                             inner join soorderplaced  so on so.ponoid=TB.ponoid
                                            where T.Status = 'C' and T.receiptType = 'RF' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null 
                                           -- and  T.receiptdate  Between '01-APR-2021' and '31-MAR-2022'
                                            GROUP BY T.warehouseid,TB.ponoid,TB.batchno,so.supplierid,I.ITEMID              

                    ) rf on rf.itemid = s.itemid and rf.ponoid = s.ponoid and rf.batchno = s.batchno and rf.warehouseid = s.warehouseid and rf.supplierid = s.supplierid 

                   where 1=1  --and  s.ITEMCODE = 'SP19448' 

                   and h.holddate between '" + fromDate + @"' and " + whToDate + @"
                 group by WAREHOUSENAME,MCID,CATEGORY,s.ITEMCODE,ITEMNAME,STRENGTH, SKU,s.BATCHNO,MFGDATE, EXPDATE,h.holddate,h.holdreason,rpqty,nrqty
                 ,Fac_iss_qty,RF_Qty,IWHReceiptQTy,
                 PONO, PODATE, SUPPLIERNAME,rs.rsqty --having SUM(holdStock)>0
                 order by WAREHOUSENAME,MCID,ITEMCODE  ";
            }

            else
            {
                qry = @" select WAREHOUSENAME,MCID,CATEGORY,s.ITEMCODE,ITEMNAME,STRENGTH, SKU,s.BATCHNO,MFGDATE, EXPDATE, SUM(holdStock) holdStock,to_char( h.NSQDATE,'dd-MM-yyyy') as  holddate ,h.HOLDREMARKS as holdreason ,
 PONO, PODATE, SUPPLIERNAME,nvl(nrqty,0) as SupplierReceipt ,nvl(IWHReceiptQTy,0) as IWHReceiptQTy,nvl(Fac_iss_qty,0) as Fac_iss_qty,nvl(RF_Qty,0) as RF_Qty, nvl(rs.rsqty,0) rsqty,nvl(rpqty,0) as rpqty

 from

 (
                 select w.warehouseid,w.WAREHOUSENAME,mc.mcid,mc.mcategory as category,mi.itemid,mi.ITEMCODE, 
                 b.batchno,b.mfgdate,b.expdate,b.inwno,mi.ITEMNAME ,
                 mi.strength1 as strength,mi.unit as SKU ,   
                  (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  holdStock,o.ponoid,o.pono,o.soissuedate as podate,sp.supplierid,sp.suppliername
                 from tbreceiptbatches b   
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  " + whItemid + @"
                 inner join masitemcategories c on c.categoryid = mi.categoryid
                 inner join masitemmaincategory mc on mc.mcid = c.mcid   "+ whMcid + @"
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid
                 left outer join soorderplaced o on o.ponoid = b.ponoid
                 inner join masschemes sc on sc.schemeid = o.schemeid
                 inner join massuppliers sp on sp.supplierid = o.supplierid
                 left outer join  
                 (   
                         select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                         from tboutwards tbo, tbindentitems tbi , tbindents tb  
                         where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null  
                         group by tbi.itemid,tb.warehouseid,tbo.inwno   
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid   
                 Where  T.Status = 'C'  And b.qastatus = 2
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
                 ) s
                                    left join
                   (

select ITEMID, ITEMCODE, BATCHNO, max(NSQDATE) NSQDATE,HOLDREMARKS
from
(
select m.itemid,m.itemcode,s.batchno,
case when sq.NSQDATE is not null then sq.NSQDATE else
case when s.holddate is not null then s.holddate else s.TESTRESULTDATE end end as nsqdate,nvl(HOLDREMARKS,s.holdreason) as HOLDREMARKS
from qcsamples s
left outer join QCRestusDeclarationDetails sq on sq.sampleid=s.sampleid or  sq.sampleid=s.refsampleid
inner join masitems m on m.itemid = s.itemid
where nvl(s.NEWTESTRESULT,s.testresult) = 'NSQ'
) group by ITEMID, ITEMCODE, BATCHNO,HOLDREMARKS


                   ) h on h.ItemCode = s.ItemCode and h.batchno = s.batchno

left join
                   (
select warehouseid,supplierid,ponoid,ITEMID,BATCHNO, sum(issueqty) rsqty
from
(
      select tb.warehouseid,tb.supplierid, tbi.itemid,b.ponoid, b.batchno,sum(nvl(tbo.issueqty,0)) issueqty
      from tboutwards tbo
      inner join tbindentitems tbi on tbo.indentitemid=tbi.indentitemid
      inner join tbindents tb on tbi.indentid=tb.indentid
      inner join tbreceiptbatches b on b.inwno = tbo.inwno
      inner join masitems m on m.itemid = tbi.itemid
      where tb.status='C' and tb.issuetype = 'RS' --and m.itemcode = 'SP19448' 
      group by tb.warehouseid, tbi.itemid, b.batchno,b.ponoid,tb.supplierid
) group by warehouseid,supplierid,ITEMID,BATCHNO,ponoid

                   ) rs on rs.ITEMID = s.ITEMID and rs.ponoid = s.ponoid and rs.batchno = s.batchno and rs.warehouseid = s.warehouseid and rs.supplierid = s.supplierid

 left join
                   (
                     select  I.ItemID,t.warehouseid,tb.ponoid,t.supplierid,SUM(TB.ABSRQTY) AS rpqty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            where T.Status = 'C' and T.receiptType = 'NO'  and T.RECTYPEID in (2,3,4)
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,t.supplierid            
                   ) rp on rp.ITEMID = rs.ITEMID and rp.ponoid = rs.ponoid and rp.warehouseid = rs.warehouseid and rp.supplierid = rs.supplierid 

left outer join 
                    (
                              select  I.ItemID,t.warehouseid,tb.ponoid,tb.batchno,t.supplierid,SUM(TB.ABSRQTY) AS nrqty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            where T.Status = 'C' and T.receiptType = 'NO' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null 
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,tb.batchno ,t.supplierid             

                    ) nr on nr.itemid = s.itemid and nr.ponoid = s.ponoid and nr.batchno = s.batchno and nr.warehouseid = s.warehouseid and nr.supplierid = s.supplierid 

                    left outer join
                    (
                    select  I.ItemID,t.warehouseid,tb.ponoid,tb.batchno,so.supplierid,SUM(TB.ABSRQTY) AS IWHReceiptQTy
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                            inner join soorderplaced  so on so.ponoid=tb.ponoid
                                            where T.Status = 'C' and T.receiptType = 'SP' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null  and T.transferid is not null
                                            GROUP BY I.ItemID,t.warehouseid,tb.ponoid,tb.batchno ,so.supplierid

                    )IWH on IWH.itemid = s.itemid and IWH.ponoid = s.ponoid and IWH.batchno = s.batchno and IWH.warehouseid = s.warehouseid and IWH.supplierid = s.supplierid 
                     left outer join
                    (
                         select
                        tb.warehouseid,rb.ponoid,rb.batchno,so.supplierid,
                         tbi.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) Fac_iss_qty 
                                    from tbindents tb
                                    inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                    inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                     inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
                                     inner join soorderplaced  so on so.ponoid=rb.ponoid
                                    inner join masitems m on m.itemid = tbi.itemid
                                    inner join maswarehouses w on w.warehouseid = tb.warehouseid
                                    inner join masfacilities f on f.facilityid = tb.facilityid
                                    where tb.status = 'C' and tb.issuetype='NO'                                  
                                    and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
                                    and tbo.notindpdmis is null and rb.notindpdmis is null   
                                    group by tb.warehouseid,rb.ponoid,rb.batchno,so.supplierid,tbi.itemid
                    ) faci on faci.itemid =s.itemid and faci.ponoid = s.ponoid and faci.batchno = s.batchno and faci.warehouseid = s.warehouseid and faci.supplierid = s.supplierid 

        left outer join 
                    (
                              select   T.warehouseid,TB.ponoid,TB.batchno,so.supplierid,
                         I.itemid,SUM(TB.ABSRQTY) AS RF_Qty
                                            from tbreceipts T
                                            inner join tbreceiptItems I on (I.receiptid = T.receiptid)
                                            inner join tbreceiptbatches TB on (I.receiptitemid =TB.receiptitemid)
                                             inner join soorderplaced  so on so.ponoid=TB.ponoid
                                            where T.Status = 'C' and T.receiptType = 'RF' and T.notindpdmis is null and I.notindpdmis is null
                                            and TB.notindpdmis is null 
                                           -- and  T.receiptdate  Between '01-APR-2021' and '31-MAR-2022'
                                            GROUP BY T.warehouseid,TB.ponoid,TB.batchno,so.supplierid,I.ITEMID              

                    ) rf on rf.itemid = s.itemid and rf.ponoid = s.ponoid and rf.batchno = s.batchno and rf.warehouseid = s.warehouseid and rf.supplierid = s.supplierid 

                   where 1=1 and mcid in (1,2) --and  s.ITEMCODE = 'SP19448' 

                   and h.NSQDATE between '" + fromDate + @"' and " + whToDate + @"
                 group by WAREHOUSENAME,MCID,CATEGORY,s.ITEMCODE,ITEMNAME,STRENGTH, SKU,s.BATCHNO,MFGDATE, EXPDATE,h.NSQDATE,h.HOLDREMARKS ,rpqty,nrqty
                 ,Fac_iss_qty,RF_Qty,IWHReceiptQTy,
                 PONO, PODATE, SUPPLIERNAME,rs.rsqty --having SUM(holdStock)>0
                 order by WAREHOUSENAME,MCID,ITEMCODE
  ";
            }





                var myList = _context.HoldBatchHistoryDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("NonSupplySummary")]
        public async Task<ActionResult<IEnumerable<NonSupplySummaryDTO>>> NonSupplySummary(string fromDate, string ToDate)
        {
          
            string whBtwDate = " ";
            string qry = "";

            string whToDate = "";
            string whMcid = "";
            string whItemid = "";

          
            //validate fromDate

            if (string.IsNullOrEmpty(fromDate) || fromDate == "undefined" || fromDate == "0")
            {

                return BadRequest("From Date can not be null or 0");
            }


            //validate ToDate for condition if ToDate is null or 0 the set to current date
            if (string.IsNullOrEmpty(ToDate) || ToDate == "undefined" || ToDate == "0")
            {
                whToDate = "sysdate";
            }
            else
            {
                whToDate = "'" + ToDate + "'";
            }


            qry = @" select supplierid,suppliername,count(distinct ponoid) as nos
from 
(
select sc.schemeid,sc.schemecode tenderno
,sc.schemename tendername
,m.itemcode,m.itemname,m.strength1,m.unit

,MCATEGORY
,case when nvl(m.isedl2021,'N') = 'Y' then 'Yes' else 'No' end as EDLType
,so.ponoid,so.pono,

to_char(so.soissuedate,'dd-MM-yyyy') as  podate
,to_char(so.extendeddate,'dd-MM-yyyy') as extendeddate


,nvl(soi.absqty,0) as poqty, 
nvl(rec.receiptabsqty,0) receiptqty
,nvl(pip.pipelineQTY,0) pipelineqty,
round((nvl(rec.receiptabsqty,0)/nvl(soi.absqty,0)*100),2) supplyper,
t.duration ,s.supplierid,s.suppliername,round(sysdate-so.soissuedate,0) noofdays 

from soorderplaced so
inner join soordereditems soi on soi.ponoid=so.ponoid and so.status not in ( 'OC','WA1','I' ) 
inner join masitems m on m.itemid=soi.itemid
inner join sotranches t on t.ponoid = so.ponoid
inner join massuppliers s on s.supplierid = so.supplierid
inner join masschemes sc on sc.schemeid = so.schemeid
left outer join aoccontractitems ci on ci.contractitemid = soi.contractitemid
left outer join aoccontracts c on c.contractid = ci.contractid
inner join masitemcategories cc on cc.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.mcid=cc.mcid
left outer join 
(
select tr.ponoid,tri.itemid,sum(nvl(tri.receiptabsqty,0)) receiptabsqty from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' 
group by tr.ponoid,tri.itemid
) rec on rec.ponoid=so.PoNoID and rec.itemid=SOI.itemid
left outer join
(
select  m.itemcode,OI.itemid,op.ponoid,op.soissuedate,op.extendeddate,sum(soi.ABSQTY) as absqty,nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY
from   soOrderPlaced OP  
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
left outer join 
(
select tr.ponoid,tri.itemid,sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr 
inner join tbreceiptitems tri on tri.receiptid=tr.receiptid 
where tr.receipttype='NO' and tr.status='C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid,tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid 
 where op.status  in ('C','O')

 --and m.itemcode = 'D117'
 group by m.itemcode,m.nablreq,op.ponoid,op.soissuedate,op.extendeddate,OI.itemid ,rec.receiptabsqty,
 op.soissuedate,op.extendeddate ,receiptdelayexception  
 having (case when m.nablreq = 'Y' and  round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 130 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then  sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) 
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate 
and round(sysdate-op.soissuedate,0) <= 130 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) pip on pip.ponoid=so.PoNoID and pip.itemid=SOI.itemid
where MC.MCID IN (1,2) 
and  so.soissuedate between '"+ fromDate + @"' and "+ whToDate + @"
and (nvl(rec.receiptabsqty,0)/nvl(soi.absqty,0)*100) < 70
and nvl(pip.pipelineQTY,0) = 0
) group by supplierid,suppliername
order by suppliername ";

            var myList = _context.NonSupplySummaryDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("NonSupplySummaryDetail")]
        public async Task<ActionResult<IEnumerable<NonSupplySummaryDetailDTO>>> NonSupplySummaryDetail(string fromDate, string ToDate, Int32 supplierId,string itemCode, Int32 schemeId, Int32 ponoId)
        {

            string whBtwDate = " ";
            string qry = "";

            string whToDate = "";
            string whSupplierId = "";
            string whItemCode = "";
            string whSchemeId = "";
            string whPonoId = "";


            //validate fromDate

            if (string.IsNullOrEmpty(fromDate) || fromDate == "undefined" || fromDate == "0")
            {

                return BadRequest("From Date can not be null or 0");
            }


            //validate ToDate for condition if ToDate is null or 0 the set to current date
            if (string.IsNullOrEmpty(ToDate) || ToDate == "undefined" || ToDate == "0")
            {
                whToDate = "sysdate";
            }
            else
            {
                whToDate = "'" + ToDate + "'";
            }

            if(supplierId != 0)
            {
                whSupplierId = "    AND s.supplierid = "+ supplierId + " ";
              
            }

            if(itemCode != "0")
            {
                whItemCode = "  AND   m.itemcode = '"+ itemCode + "' ";
            }

            if (schemeId != 0)
            {
                whSchemeId = "   AND   sc.schemeid = "+ schemeId + "";
            }

            if (ponoId != 0)
            {
                whPonoId = "  AND   so.ponoid = "+ ponoId + "  ";
            }


            qry = @" /* Drill-down details for a particular supplier (one row per PO × Item) */
SELECT
  sc.schemeid,
  sc.schemecode                AS tenderno,
  sc.schemename                AS tendername,
  m.itemcode,
  m.itemname,
  m.strength1,
  m.unit,
  mc.mcategory,
  CASE WHEN NVL(m.isedl2021,'N') = 'Y' THEN 'Yes' ELSE 'No' END AS edltype,
  so.ponoid,
  so.pono,
  TO_CHAR(so.soissuedate,'dd-MM-yyyy')  AS podate,
  TO_CHAR(so.extendeddate,'dd-MM-yyyy') AS extendeddate,
  NVL(soi.absqty,0)                      AS poqty,
  NVL(rec.receiptabsqty,0)               AS receiptqty,
  NVL(pip.pipelineqty,0)                 AS pipelineqty,
  ROUND( (NVL(rec.receiptabsqty,0) / NULLIF(NVL(soi.absqty,0),0)) * 100, 2 ) AS supplyper,
  t.duration,
  s.supplierid,
  s.suppliername,
  ROUND(SYSDATE - so.soissuedate, 0)     AS noofdays
FROM soorderplaced so
JOIN soordereditems       soi ON soi.ponoid = so.ponoid
                             AND so.status NOT IN ('OC','WA1','I')
JOIN masitems             m   ON m.itemid  = soi.itemid
JOIN sotranches           t   ON t.ponoid  = so.ponoid
JOIN massuppliers         s   ON s.supplierid = so.supplierid
JOIN masschemes           sc  ON sc.schemeid  = so.schemeid
LEFT JOIN aoccontractitems ci ON ci.contractitemid = soi.contractitemid
LEFT JOIN aoccontracts     c  ON c.contractid     = ci.contractid
JOIN masitemcategories     cc ON cc.categoryid    = m.categoryid
JOIN masitemmaincategory   mc ON mc.mcid          = cc.mcid
/* receipts aggregated by PO × Item */
LEFT JOIN (
  SELECT tr.ponoid, tri.itemid, SUM(NVL(tri.receiptabsqty,0)) AS receiptabsqty
  FROM tbreceipts tr
  JOIN tbreceiptitems tri ON tri.receiptid = tr.receiptid
  WHERE tr.receipttype = 'NO'
    AND tr.status      = 'C'
  GROUP BY tr.ponoid, tri.itemid
) rec
  ON rec.ponoid = so.ponoid AND rec.itemid = soi.itemid
/* pipeline qty (0 means nothing still legitimately pending) */
LEFT JOIN (
  SELECT
    m.itemcode,
    oi.itemid,
    op.ponoid,
    op.soissuedate,
    op.extendeddate,
    SUM(soi.absqty)                      AS absqty,
    NVL(rec.receiptabsqty,0)             AS receiptabsqty,
    op.receiptdelayexception,
    ROUND(SYSDATE - op.soissuedate, 0)   AS days,
    CASE
      WHEN m.nablreq = 'Y' AND ROUND(SYSDATE - op.soissuedate, 0) <= 150
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.extendeddate IS NULL AND ROUND(SYSDATE - op.soissuedate, 0) <= 90
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.receiptdelayexception = 1 AND SYSDATE <= op.extendeddate + 1
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.extendeddate IS NOT NULL
           AND op.receiptdelayexception = 1
           AND (op.extendeddate + 1) <= op.soissuedate
           AND ROUND(SYSDATE - op.soissuedate, 0) <= 90
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      ELSE 0
    END AS pipelineqty
  FROM soorderplaced op
  JOIN soordereditems oi       ON oi.ponoid = op.ponoid
  JOIN soorderdistribution soi ON soi.orderitemid = oi.orderitemid
  JOIN masitems m              ON m.itemid = oi.itemid
  LEFT JOIN (
    SELECT tr.ponoid, tri.itemid, SUM(tri.receiptabsqty) AS receiptabsqty
    FROM tbreceipts tr
    JOIN tbreceiptitems tri ON tri.receiptid = tr.receiptid
    WHERE tr.receipttype = 'NO'
      AND tr.status      = 'C'
      AND tr.notindpdmis IS NULL
      AND tri.notindpdmis IS NULL
    GROUP BY tr.ponoid, tri.itemid
  ) rec ON rec.ponoid = op.ponoid AND rec.itemid = oi.itemid
  WHERE op.status IN ('C','O')
  GROUP BY
    m.itemcode, m.nablreq, oi.itemid,
    op.ponoid, op.soissuedate, op.extendeddate,
    rec.receiptabsqty, op.receiptdelayexception
  HAVING
    CASE
      WHEN m.nablreq = 'Y' AND ROUND(SYSDATE - op.soissuedate, 0) <= 150
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.extendeddate IS NULL AND ROUND(SYSDATE - op.soissuedate, 0) <= 130
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.receiptdelayexception = 1 AND SYSDATE <= op.extendeddate + 1
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      WHEN op.extendeddate IS NOT NULL
           AND op.receiptdelayexception = 1
           AND (op.extendeddate + 1) <= op.soissuedate
           AND ROUND(SYSDATE - op.soissuedate, 0) <= 130
        THEN SUM(soi.absqty) - NVL(rec.receiptabsqty,0)
      ELSE 0
    END > 0
) pip
  ON pip.ponoid = so.ponoid AND pip.itemid = soi.itemid
WHERE
  mc.mcid IN (1,2)                                   -- Drugs / Consumables & Others
  AND so.soissuedate BETWEEN  '"+ fromDate + @"' AND "+ whToDate + @"
  "+ whSupplierId + @"                           -- << particular supplier >>
  "+ whItemCode + @"
 "+ whSchemeId + @"
 "+ whPonoId + @" 
  AND (NVL(rec.receiptabsqty,0) / NULLIF(NVL(soi.absqty,0),0)) * 100 < 70
  AND NVL(pip.pipelineqty,0) = 0
ORDER BY
  so.soissuedate DESC,
  m.itemcode;
 ";

            var myList = _context.NonSupplySummaryDetailDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }



        [HttpGet("DmeFacNocSummary")]
        public async Task<ActionResult<IEnumerable<DmeFacNocSummaryDTO>>> DmeFacNocSummary(string fromDate, string ToDate, string mcid, string yearId)
        {

         
            string qry = "";

            string whToDate = "";
            string whMcid = "";
            string whYearId = "";
           // string whYearId1 = "";


            //validate fromDate

            if (string.IsNullOrEmpty(fromDate) || fromDate == "undefined" || fromDate == "0")
            {

                return BadRequest("From Date can not be null or 0");
            }


            //validate ToDate for condition if ToDate is null or 0 the set to current date
            if (string.IsNullOrEmpty(ToDate) || ToDate == "undefined" || ToDate == "0")
            {
                whToDate = "sysdate";   // keep sysdate as is
            }
            else
            {
                whToDate = "TO_DATE('" + ToDate + "','dd-mm-yyyy')";
            }

            string whFromDate = "TO_DATE('" + fromDate + "','dd-mm-yyyy')";



            if ( mcid != "0")
            {
                whMcid = " and mc.mcid in (" + mcid + ") ";
            }

            if (yearId != "0")
            {
                whYearId = " and a.accyrsetid = " + yearId + " ";
               // whYearId1 = " accyrsetid = " + yearId + " ";
            }



            qry = @" select
    mcategory,
    facilityname,

   
   nvl( sum(case when isedl2021 = 'Y' then cntNOCItems end),0) as EDL_CNT,
   nvl( sum(case when isedl2021 = 'Y' then nvl(NOCValue,0) end),0 )   as EDL_VAL,

   
   nvl( sum(case when isedl2021 = 'N' then cntNOCItems end),0) as NON_EDL_CNT,
    nvl(sum(case when isedl2021 = 'N' then nvl(NOCValue,0) end),0)    as NON_EDL_VAL
,districtname,facilityid
from (
    select MCATEGORY,
           ISEDL2021,
           FACILITYNAME,districtname,
           count(distinct itemid) cntNOCItems,
           sum(NOCValue) as NOCValue,facilityid
    from (


        select x.mcid,
               x.mcategory,
               x.isedl2021,
               x.facilityname,x.districtname,
               x.itemid,
               itemcode,
               itemname,
               strength1,
               unit,
               unitc,
               nvl(x.facindentqty,0) facindentqty,
               nvl(x.chcaprqty,0) chcaprqty,
               nvl(x.cmhoqty,0) cmhoqty,
               nvl(x.dhsaprqty,0) dhsaprqty,
             
               sum(NocQty) NocQty,
               sum(NOCValue) as NOCValue,
               round(sum(poQTY)/unit,0) as POSKU,
               sum(povalue) as povalue,
               round(sum(nvl(receiptqty,0))/unit,0) as ReceiptqtySKU,
               sum(recvalue) as recvalue,
               categoryname,
               categoryid,x.facilityid
        from (
            select m.itemid,
                   m.unit as unitc,
                   m.itemcode,
                   m.itemname,
                   nvl(m.isedl2021,'N') as isedl2021,
                   m.strength1,
                   m.unitcount,
                   iin.facindentqty,
                   iin.chcaprqty,
                   iin.cmhoqty,
                   iin.dhsaprqty,
                   mn.nocid,
                   mn.facilityid,
                   f.facilityname,d.districtname,
                   mni.approvedqty NocQty,
                   nvl(so.absqty,0) poQTY,
                   nvl(so.povalue,0) as povalue,
                   so.receiptqty,
                   nvl(so.recvalue,0) as recvalue,
                   nvl(m.unitcount,1) as unit,
                   mic.categoryname,
                   m.categoryid,
                   mc.mcid,
                   mc.mcategory,
                   round(mni.approvedqty*rt.SKUFINALRATE,0) as NOCValue
            from mascgmscnoc mn
            inner join mascgmscnocitems mni on mni.nocid=mn.nocid  and nvl(mni.ISCANCEL,'N') ='N'
            inner join masitems m on m.itemid=mni.itemid
            inner join masfacilities f on f.facilityid=mn.facilityid
            inner join masdistricts d on d.districtid=f.districtid
            inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
            inner join masitemcategories mic on mic.categoryid = m.categoryid
            inner join masitemmaincategory mc on mc.mcid=mic.mcid
            left outer join v_itemrate rt on rt.itemid=m.itemid
            left outer join (
                select a.ANUALINDENTID,a.facilityid,a.itemid,
                       round(nvl(a.facilityindentqty,0)/nvl(m.unitcount,1),0) facindentqty,
                       a.status flag,
                       round(nvl(a.bmoapprovedqty,0)/nvl(m.unitcount,1),0) chcaprqty,
                       round(nvl(a.cmhoapprovedqty,0)/nvl(m.unitcount,1),0) cmhoqty,
                       round(nvl(a.cmhodistqty,0)/nvl(m.unitcount,1),0) dhsaprqty 
                from anualindent a
                inner join masitems m on m.itemid=a.itemid
                inner join masfacilities f on f.facilityid=a.facilityid
                where 1=1 "+ whYearId + @"
                  and a.status='C'
            ) iin on iin.facilityid=mn.facilityid and iin.itemid=mni.itemid
            left outer join (
                select facilityid,nocid,lpitemid,edlitemcode,
                       sum(nvl(absqty,0)) absqty,
                       sum(povalue) as povalue,
                       sum(receiptqty) receiptqty,
                       sum(recvalue) as recvalue
                from (
                    select f.facilityid, si.nocid, so.ponoid, si.lpitemid,
                           vp.edlitemcode,
                           sum(nvl(si.absqty,0)) absqty,
                           sum(nvl(si.itemvalue,0)) as povalue,
                           nvl(r.receiptqty,0) receiptqty,
                           nvl(r.receiptqty,0)*nvl(singleunitprice,0) as recvalue
                    from lpsoorderplaced so
                    inner join lpSOORDEREDITEMS si on si.ponoid=so.ponoid
                    inner join masfacilities f on f.facilityid=so.psaid
                    inner join vmasitems vp on vp.itemid=si.lpitemid
                    left outer join (
                        select tb.ponoid,m.itemid,m.edlitemcode,
                               sum(tbr.absrqty) receiptqty,
                               f.facilityid
                        from tbfacilityreceipts tb
                        inner join tbfacilityreceiptitems tbi on tbi.facreceiptid=tb.facreceiptid
                        inner join tbfacilityreceiptbatches tbr on tbr.facreceiptitemid=tbi.facreceiptitemid
                        inner join masfacilities f on f.facilityid=tb.facilityid
                        inner join vmasitems m on m.itemid=tbi.itemid
                        where tb.ponoid is not null
                          and m.edlitemcode is not null
                        group by tb.ponoid,m.itemid,m.edlitemcode,f.facilityid
                    ) r on r.ponoid=so.ponoid and r.itemid=si.lpitemid and r.facilityid=f.facilityid
                    where si.nocid is not null
                      and vp.edlitemcode is not null
                      and so.podate between "+ whFromDate + @"
                                         and "+ whToDate + @"
                    group by si.nocid,si.lpitemid,vp.edlitemcode,so.ponoid,r.receiptqty,si.singleunitprice,f.facilityid
                )
                group by nocid,lpitemid,edlitemcode,facilityid
            ) so on so.nocid=mni.nocid and so.edlitemcode=m.itemcode and so.facilityid=f.facilityid
            where ft.hodid in(3)
              and mni.approvedqty>0
              and mn.status='C'
             "+ whMcid + @"
             and mn.nocdate between " + whFromDate + @" and " + whToDate + @"
        ) x
       
      
        group by x.mcid,x.mcategory,x.facilityid,x.facilityname,
                 x.itemcode,x.unit,x.unitc,x.itemid,x.itemname,
                 x.facindentqty,x.chcaprqty,x.cmhoqty,x.dhsaprqty,
                 x.isedl2021,x.strength1,categoryname,categoryid,x.districtname,x.facilityid
      

    ) 
    group by MCATEGORY, ISEDL2021, FACILITYNAME,districtname,facilityid
) final
group by mcategory, facilityname,districtname,facilityid
order by districtname, facilityname ";

            var myList = _context.DmeFacNocSummaryDbSet
      .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("DmeFacNocDetail")]
        public async Task<ActionResult<IEnumerable<DmeFacNocDetailDTO>>> DmeFacNocDetail(
    string fromDate, string toDate, string mcid, string yearId, string facilityId)
        {
            if (string.IsNullOrEmpty(fromDate) || fromDate == "undefined" || fromDate == "0")
                return BadRequest("From Date can not be null or 0");

            string whFromDate = "TO_DATE('" + fromDate + "','dd-mm-yyyy')";
            string whToDate = string.IsNullOrEmpty(toDate) || toDate == "undefined" || toDate == "0"
                ? "sysdate"
                : "TO_DATE('" + toDate + "','dd-mm-yyyy')";

            string whMcid = (mcid != "0") ? " and mc.mcid in (" + mcid + ") " : "";
            string whYearId = (yearId != "0") ? " and a.accyrsetid = " + yearId + " " : "";
            string whFacility = (facilityId != "0") ? " and x.facilityid in (" + facilityId + ") " : "";

            var qry = @"
        select x.mcid,
               x.mcategory,
               case when x.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDlType,
               x.facilityname,
               x.districtname,
               x.itemid,
               itemcode,
               itemname,
               strength1,
               unit,
               unitc,
               nvl(x.facindentqty,0) FacAIQty,
               nvl(iss.issueqty,0) CGMSCissueqty,
               count(distinct nocid) cntNoc,
               sum(NocQty) NocQty,
               sum(NOCValue) as NOCValue,
               round(sum(poQTY)/unit,0) as POSKU,
               sum(povalue) as povalue,
               round(sum(nvl(receiptqty,0))/unit,0) as ReceiptqtySKU,
               sum(recvalue) as recvalue,
               x.facilityid
        from (
            /* your inner query — same as you pasted, just replace date conditions */
            select m.itemid, 

 m.unit as unitc,
                   m.itemcode,
                   m.itemname,
                   nvl(m.isedl2021,'N') as isedl2021,
                   m.strength1,
                   m.unitcount,
                   iin.facindentqty,
                   iin.chcaprqty,
                   iin.cmhoqty,
                   iin.dhsaprqty,
                   mn.nocid,
                   mn.facilityid,
                   f.facilityname,d.districtname,
                   mni.approvedqty NocQty,
                   nvl(so.absqty,0) poQTY,
                   nvl(so.povalue,0) as povalue,
                   so.receiptqty,
                   nvl(so.recvalue,0) as recvalue,
                   nvl(m.unitcount,1) as unit,
                   mic.categoryname,
                   m.categoryid,
                   mc.mcid,
                   mc.mcategory,
                   round(mni.approvedqty*rt.SKUFINALRATE,0) as NOCValue
            from mascgmscnoc mn
            inner join mascgmscnocitems mni on mni.nocid=mn.nocid  and nvl(mni.ISCANCEL,'N') ='N'
            inner join masitems m on m.itemid=mni.itemid
            inner join masfacilities f on f.facilityid=mn.facilityid
            inner join masdistricts d on d.districtid=f.districtid
            inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
            inner join masitemcategories mic on mic.categoryid = m.categoryid
            inner join masitemmaincategory mc on mc.mcid=mic.mcid
            left outer join v_itemrate rt on rt.itemid=m.itemid
            left outer join (
                select a.ANUALINDENTID,a.facilityid,a.itemid,
                       round(nvl(a.facilityindentqty,0)/nvl(m.unitcount,1),0) facindentqty,
                       a.status flag,
                       round(nvl(a.bmoapprovedqty,0)/nvl(m.unitcount,1),0) chcaprqty,
                       round(nvl(a.cmhoapprovedqty,0)/nvl(m.unitcount,1),0) cmhoqty,
                       round(nvl(a.cmhodistqty,0)/nvl(m.unitcount,1),0) dhsaprqty 
                from anualindent a
                inner join masitems m on m.itemid=a.itemid
                inner join masfacilities f on f.facilityid=a.facilityid
                where a.accyrsetid=546
                  and a.status='C'
            ) iin on iin.facilityid=mn.facilityid and iin.itemid=mni.itemid
            left outer join (
                select facilityid,nocid,lpitemid,edlitemcode,
                       sum(nvl(absqty,0)) absqty,
                       sum(povalue) as povalue,
                       sum(receiptqty) receiptqty,
                       sum(recvalue) as recvalue
                from (
                    select f.facilityid, si.nocid, so.ponoid, si.lpitemid,
                           vp.edlitemcode,
                           sum(nvl(si.absqty,0)) absqty,
                           sum(nvl(si.itemvalue,0)) as povalue,
                           nvl(r.receiptqty,0) receiptqty,
                           nvl(r.receiptqty,0)*nvl(singleunitprice,0) as recvalue
                    from lpsoorderplaced so
                    inner join lpSOORDEREDITEMS si on si.ponoid=so.ponoid
                    inner join masfacilities f on f.facilityid=so.psaid
                    inner join vmasitems vp on vp.itemid=si.lpitemid
                    left outer join (
                        select tb.ponoid,m.itemid,m.edlitemcode,
                               sum(tbr.absrqty) receiptqty,
                               f.facilityid
                        from tbfacilityreceipts tb
                        inner join tbfacilityreceiptitems tbi on tbi.facreceiptid=tb.facreceiptid
                        inner join tbfacilityreceiptbatches tbr on tbr.facreceiptitemid=tbi.facreceiptitemid
                        inner join masfacilities f on f.facilityid=tb.facilityid
                        inner join vmasitems m on m.itemid=tbi.itemid
                        where tb.ponoid is not null
                          and m.edlitemcode is not null
                        group by tb.ponoid,m.itemid,m.edlitemcode,f.facilityid
                    ) r on r.ponoid=so.ponoid and r.itemid=si.lpitemid and r.facilityid=f.facilityid
                    where si.nocid is not null
                      and vp.edlitemcode is not null
                      and so.podate between (select startdate from masaccyearsettings where accyrsetid=546)
                                         and (select enddate from masaccyearsettings where accyrsetid=546)
                    group by si.nocid,si.lpitemid,vp.edlitemcode,so.ponoid,r.receiptqty,si.singleunitprice,f.facilityid
                )
                group by nocid,lpitemid,edlitemcode,facilityid
            ) so on so.nocid=mni.nocid and so.edlitemcode=m.itemcode and so.facilityid=f.facilityid

            where ft.hodid in (3)
              and mni.approvedqty > 0
              and mn.status='C'
              " + whMcid + @"
              and mn.nocdate between " + whFromDate + @" and " + whToDate + @"
        ) x
        left outer join (
            select f.facilityid,
                   tbi.itemid,
                   sum(nvl(tbo.issueqty,0)) issueqty
            from tbindents tb
            inner join tbindentitems tbi on tbi.indentid=tb.indentid
            inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
            inner join masfacilities f on f.facilityid=tb.facilityid
            where tb.status = 'C'
              and tb.indentdate between " + whFromDate + @" and " + whToDate + @"
            group by tbi.itemid, f.facilityid
        ) iss on iss.itemid=x.itemid and iss.facilityid=x.facilityid
        where 1=1 " + whFacility + @"
        group by x.mcid, x.mcategory, x.facilityid, x.facilityname,
                 x.itemcode, x.unit, x.unitc, x.itemid, x.itemname,
                 x.facindentqty, x.isedl2021, x.strength1, iss.issueqty,
                 x.districtname, x.facilityid
    ";

            var myList = _context.DmeFacNocDetailDbSet
                .FromSqlInterpolated(FormattableStringFactory.Create(qry))
                .ToList();

            return myList;
        }


    }
}


