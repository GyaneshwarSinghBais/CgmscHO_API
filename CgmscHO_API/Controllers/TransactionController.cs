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

    }
}


