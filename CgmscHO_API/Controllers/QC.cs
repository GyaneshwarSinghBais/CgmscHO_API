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
using CgmscHO_API.QCDTO;
using Microsoft.Build.Evaluation;
using System.Reflection;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QC : ControllerBase
    {
        private readonly OraDbContext _context;
        public QC(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("LabIssuePendingSummary")]
        public async Task<ActionResult<IEnumerable<LabIssuePendingSummary>>> LabIssuePendingSummary(string mcid)
        {
            string whclause = "";


            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }

            string qry = @" select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcid,mcategory,count(distinct itemid) nositems,count(distinct batchno) nousbatch,delaypara,delaypara1
from 
(

select mc.mcid,mc.mcategory,round(sysdate-min(A.Receiptdate)) as pendingsince, 
a.batchno,d.itemid  , case when   round(sysdate-min(A.Receiptdate))>15 then '>15 Days' else   case when round(sysdate-min(A.Receiptdate))<=15 and round(sysdate-min(A.Receiptdate))>=6
then 'Between 6-15 Days' else case when round(sysdate-min(A.Receiptdate))<=5 and round(sysdate-min(A.Receiptdate))>=2
then 'Between 2-5 Days' else '1 day'   
end end end  as delaypara,case when   round(sysdate-min(A.Receiptdate))>15 then 1 else   case when round(sysdate-min(A.Receiptdate))<=15 and round(sysdate-min(A.Receiptdate))>=7
then 2 else case when round(sysdate-min(A.Receiptdate))<=6 and round(sysdate-min(A.Receiptdate))>=2
then 3 else 4
end end end  as delaypara1
         From Qcsamples A left outer join Qctests B on b.sampleid = a.sampleid
         left outer join Qclabs C on C.labid = B.labid
         left outer join masitems D on D.itemid = A.itemid
         inner join masitemcategories c on c.categoryid=d.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
         where A.Receiptdate between '01-Apr-2024' and sysdate 
         and A.refsampleid is null and b.labissuedate is null 
         and  b.isCourierPickQC is null and CTID is null " + whclause + @"
        group by a.batchno,d.itemid ,mc.mcid,mc.mcategory 
        ) group by mcid,mcategory,delaypara,delaypara1
        order by delaypara1,mcid ";

            var myList = _context.GetLabIssuePendingSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("LabIssuePendingDetails")]
        public async Task<ActionResult<IEnumerable<LabIssuePendingDetailsDTO>>> LabIssuePendingDetails(string mcid, string delaypara1)
        {
            string whclause = "";

            string whpara = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            if (delaypara1 != "0")
            {

                whpara = @"and (case when   round(sysdate-(A.Receiptdate))>15 then 1 else   case when round(sysdate-(A.Receiptdate))<=15 and round(sysdate-(A.Receiptdate))>=7
then 2 else case when round(sysdate - (A.Receiptdate)) <= 6 and round(sysdate-(A.Receiptdate))>= 2
then 3 else 4 end end end)=" + delaypara1;
            }

            string qry = @"select ROW_NUMBER() OVER ( ORDER BY a.batchno) AS ID,mc.mcategory,d.itemcode,d.itemname,d.unit,mt.qcdayslab,a.batchno,tr.EXPDATE,rqty,recdate WHReceiptDate ,A.Receiptdate as QCReceiptDT, 

case when   round(sysdate-(A.Receiptdate))>15 then '>15 Days' else   case when round(sysdate-(A.Receiptdate))<=15 and round(sysdate-(A.Receiptdate))>=6
then 'Between 6-15 Days' else case when round(sysdate-(A.Receiptdate))<=5 and round(sysdate-(A.Receiptdate))>=2
then 'Between 2-5 Days' else '1 day'   
end end end  as delaypara,case when   round(sysdate-(A.Receiptdate))>15 then 1 else   case when round(sysdate-(A.Receiptdate))<=15 and round(sysdate-(A.Receiptdate))>=7
then 2 else case when round(sysdate-(A.Receiptdate))<=6 and round(sysdate-(A.Receiptdate))>=2
then 3 else 4
end end end  as delaypara1

,mc.mcid,d.itemid

         From Qcsamples A left outer join Qctests B on b.sampleid = a.sampleid
         left outer join Qclabs C on C.labid = B.labid
         left outer join masitems D on D.itemid = A.itemid
         inner join masitemcategories c on c.categoryid=d.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
        left outer join masitemtypes mt on mt.itemtypeid = d.itemtypeid
       inner join 
       (
       select tri.itemid,min(tr.receiptdate) recdate,sum(ABSRQTY) as rqty,tb.batchno,tb.EXPDATE from tbreceiptbatches tb
inner join tbreceiptitems tri on tri.receiptitemid=tb.receiptitemid
inner join masitems m on m.itemid=tri.itemid
inner join tbreceipts tr on tr.receiptid=tri.receiptid
where QASTATUS=0 and tr.receipttype='NO' and m.qctest='Y' 
group by  tri.itemid,tb.batchno,tb.EXPDATE
       ) tr on tr.itemid=d.itemid and tr.batchno=a.batchno
       
       
         where A.Receiptdate between '01-Apr-2024' and sysdate 
         and A.refsampleid is null and b.labissuedate is null 
         and  b.isCourierPickQC is null and b.CTID is null 
         " + whpara + @" " + whclause + @" order by A.Receiptdate ";

            var myList = _context.GetLabIssuePendingDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("InTransitHOtoLab")]
        public async Task<ActionResult<IEnumerable<InTransitHOtoLabSummaryDTO>>> InTransitHOtoLab(string instorPickPending, string mcid)
        {
            string whclause = "";
            string whpickc = "";
            string noincinhouselab = "";
            if (instorPickPending == "PP")
            {
                noincinhouselab = " and C.labid not in (398)";
                whpickc = "  and ENTRYDATEPICK is  null and ct.DOCKETNO is  null ";
            }
            else
            {
                whpickc = "  and ENTRYDATEPICK is not null and ct.DOCKETNO is not null ";

            }

            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }

            string qry = @" 

select ID,nvl(LABNAME,'No Lab Assigned') as LABNAME ,nositem,nosbatch,Dropped ,Above15,betw6_15,bet2_5,today,nvl(labid,1) labid
from 
(
SELECT  ROW_NUMBER() OVER ( ORDER BY labid ) AS ID,LABNAME,count(distinct itemid) as nositem,
count(distinct batchno) as nosbatch,sum(DROP1) as Dropped,sum(Above15) as Above15,sum(betw6_15) as betw6_15,sum(bet2_5) as bet2_5, sum(today) as today

,labid
From 
(

select  c.LABNAME  Labname,
 c.labid  as labid,
ENTRYDATEPICK,DESTINATIONTYPE,ISDROP,CASE WHEN ct.ISDROP='Y' THEN 1 ELSE 0 END AS DROP1,DROPDATE,ct.DOCKETNO,b.CTID,
b.isCourierPickQC,mc.mcategory,d.itemcode,d.itemname,d.unit,
a.batchno
,nvl(ENTRYDATEPICK,b.labissuedate) as LAbIssueDT,
case when   round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>15 then 1 else 0 end as Above15,

case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=15 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=6
then 1 else 0 end as betw6_15 ,
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=5 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=2
then 1 else 0 end as bet2_5,
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<2 then 1 else 0 end as today   
,case when   round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>15 then 1 else   case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=15 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=6
then 2 else case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=5 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=2
then 3 else 4
end end end  as delaypara1,mc.mcid,d.itemid

         From Qcsamples A left outer join Qctests B on b.sampleid = a.sampleid
         left outer join Qclabs C on C.labid = B.labid " + noincinhouselab + @"
         left outer join masitems D on D.itemid = A.itemid
         inner join masitemcategories cat on cat.categoryid=d.categoryid
       inner join masitemmaincategory mc on mc.MCID=cat.MCID
       left outer join couriertransaction ct on ct.ctid = b.ctid
         where A.Receiptdate between '01-Apr-2024' and sysdate 
         and b.labissuedate is not null and b.samplereceiveddate is null 
         " + whpickc + @"
" + whclause + @"
) group by LABNAME,labid
) order by nosbatch desc ";

            var myList = _context.GetInTransitHOtoLabSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("InTransitHOtoLabDetails")]
        public async Task<ActionResult<IEnumerable<InTransitHOtoLabDetailDTO>>> InTransitHOtoLabDetails(string instorPickPending, string mcid, string delaypara1, string isdrop, string labid)
        {
            string whclause = "";
            string whintransitorPickPending = "";
            string noincinhouselab = "";
            if (instorPickPending == "PP")
            {
                noincinhouselab = " and C.labid not in (398)";
                whintransitorPickPending = "  and ENTRYDATEPICK is  null and ct.DOCKETNO is  null ";
            }
            else
            {
                whintransitorPickPending = "  and ENTRYDATEPICK is not null and ct.DOCKETNO is not null ";

            }



            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            string isdropclause = "";
            if (isdrop == "Y")
            {

                isdropclause = " and ct.ISDROP = 'Y'";
            }
            string labidclause = "";
            if (labid != "0")
            {

                labidclause = "  and c.labid =" + labid;
            }

            string whpara = "";
            if (delaypara1 != "0")
            {
                //delaypara1
                //1 >15 days,2 for 6-15days,3 for 2-5days & 4 for today
                whpara = @"and (case when   round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>15 then 1 else   
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=15 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=6 then 2 
else case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=5 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=2 then 3 
else 4
end end end)=" + delaypara1;
            }

            string qry = @" select ROW_NUMBER() OVER ( ORDER BY a.batchno ) AS ID, nvl(c.LABNAME,'No Lab Assigned') as LABNAME ,mc.mcategory,mt.qcdayslab,d.itemcode,d.itemname,b.SAMPLENO,a.batchno,ct.DOCKETNO,b.labissuedate,
ENTRYDATEPICK,
DROPDATE,tr.EXPDATE,rqty,recdate WHReceiptDate ,A.Receiptdate as QCReceiptDT
,round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate)) daysSinceUnderCourier,
case when   round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>15 then 1 else 0 end as Above15,

case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=15 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=6
then 1 else 0 end as betw6_15 ,
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=5 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=2
then 1 else 0 end as bet2_5,
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<2 then 1 else 0 end as today   

,case when   round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>15 then 1 else   
case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=15 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=6 then 2 
else case when round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))<=5 and round(sysdate-nvl(ENTRYDATEPICK,b.labissuedate))>=2 then 3 
else 4
end end end  as delaypara1,mc.mcid,d.itemid, nvl(c.labid,1) as labid  ,CASE WHEN ct.ISDROP='Y' THEN 1 ELSE 0 END AS DROP1,ISDROP,b.CTID

         From Qcsamples A left outer join Qctests B on b.sampleid = a.sampleid
         left outer join Qclabs C on C.labid = B.labid " + noincinhouselab + @"
         left outer join masitems D on D.itemid = A.itemid
         inner join masitemcategories cat on cat.categoryid=d.categoryid
       inner join masitemmaincategory mc on mc.MCID=cat.MCID
        left outer join masitemtypes mt on mt.itemtypeid = d.itemtypeid
       left outer join couriertransaction ct on ct.ctid = b.ctid
       inner join 
       (
       select tri.itemid,min(tr.receiptdate) recdate,sum(ABSRQTY) as rqty,tb.batchno,tb.EXPDATE from tbreceiptbatches tb
inner join tbreceiptitems tri on tri.receiptitemid=tb.receiptitemid
inner join masitems m on m.itemid=tri.itemid
inner join tbreceipts tr on tr.receiptid=tri.receiptid
where QASTATUS=0 and tr.receipttype='NO' and m.qctest='Y' 
group by  tri.itemid,tb.batchno,tb.EXPDATE
       ) tr on tr.itemid=d.itemid and tr.batchno=a.batchno 
         where A.Receiptdate between '01-Apr-2024' and sysdate  
         and b.labissuedate is not null and b.samplereceiveddate is null 
        " + whintransitorPickPending + @"
" + whclause + @"  " + isdropclause + @"  " + labidclause + @" 
" + whpara + @"
 
order by ENTRYDATEPICK  ";

            var myList = _context.GetInTransitHOtoLabDetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("QCPendingDashboard")]
        public async Task<ActionResult<IEnumerable<QCHomeDashDTO>>> QCPendingDashboard(string mcid)
        {
            string whclause = "";
            if (mcid != "0")
                whclause = " and mc.mcid =" + mcid;
            string qry = "  select mcid,mcategory,count(distinct itemid) as nositems,nvl(round(sum(STKVALUE)/10000000,2),0) as STKVALUE,count(distinct batchno) as nosbatch\r\nfrom \r\n(\r\nselect  nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,\r\nb.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid\r\n,case when m.isedl2021='Y' then b.itemid else 0  end as EDLitem\r\n,case when m.isedl2021='Y' then  0  else b.itemid end as NonEDL\r\n,case when m.isedl2021='Y' then (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst else 0  end as EDLitemValue\r\n,case when m.isedl2021='Y' then 0  else  (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst end as NonEDLitemValue\r\n,case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end as QCStck\r\n,nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty\r\n,b.batchno,mc.mcategory,mc.mcid\r\nfrom tbreceiptbatches b\r\nleft outer join \r\n(\r\nselect op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op \r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n ) p on p.ponoid=b.ponoid \r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n Where T.Status = 'C' \r\n And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate  " + whclause + "\r\n and (case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end)=1\r\nand ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0\r\n )  b group by mcategory,mcid ";
            List<QCHomeDashDTO> myList = this._context.QCHomeDashDbSet.FromSqlInterpolated<QCHomeDashDTO>(FormattableStringFactory.Create(qry)).ToList<QCHomeDashDTO>();
            ActionResult<IEnumerable<QCHomeDashDTO>> actionResult = (ActionResult<IEnumerable<QCHomeDashDTO>>)myList;
            whclause = (string)null;
            qry = (string)null;
            myList = (List<QCHomeDashDTO>)null;
            return actionResult;
        }

        //        [HttpGet("QCPendingDashboard")]
        //        public async Task<ActionResult<IEnumerable<QCHomeDashDTO>>> QCPendingDashboard(string mcid)
        //        {
        //            string whclause = "";
        //            if (mcid != "0")
        //            {

        //                whclause = " and mc.mcid =" + mcid;
        //            }
        //            string qry = @"  select mcategory,count(distinct itemid) as nositems,nvl(round(sum(STKVALUE)/10000000,2),0) as STKVALUE,count(distinct batchno) as nosbatch
        //from 
        //(
        //select  nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,
        //b.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid
        //,case when m.isedl2021='Y' then b.itemid else 0  end as EDLitem
        //,case when m.isedl2021='Y' then  0  else b.itemid end as NonEDL
        //,case when m.isedl2021='Y' then (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst else 0  end as EDLitemValue
        //,case when m.isedl2021='Y' then 0  else  (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst end as NonEDLitemValue
        //,case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end as QCStck
        //,nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty
        //,b.batchno,mc.mcategory
        //from tbreceiptbatches b
        //left outer join 
        //(
        //select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
        // inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
        // inner join aoccontractitems c on c.contractitemid = oi.contractitemid
        // ) p on p.ponoid=b.ponoid 
        // join tbreceiptitems i on b.receiptitemid = i.receiptitemid
        // inner  join tbreceipts t on t.receiptid = i.receiptid
        // inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
        // inner join masitemcategories ic on ic.categoryid = m.categoryid
        // inner join masitemmaincategory mc on mc.MCID=ic.MCID
        // Where T.Status = 'C' 
        // And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate  " + whclause + @"
        // and (case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end)=1
        //and ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0
        // )  b group by mcategory ";
        //            var myList = _context.QCHomeDashDbSet
        //          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;
        //        }

        [HttpGet("QCPendingPlacewise")]
        public async Task<ActionResult<IEnumerable<QCHomeDashPlacewise>>> QCPendingPlacewise(string mcid)
        {
            string whclause = "";
            if (mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }
            string qry = @"  select count(distinct itemid) as nositems,round(sum(Stockvalue)/10000000,2) as STKVALUE,count(distinct batchno) as nosbatch
  ,sum(NotIssuedbyWH)as QDIssuePendingbyWH,sum(UnderCourier) as WHIssueButPendingInCourier,sum(NotIssued_ToLab) as HOQC_LabIssuePending
  ,sum(UnderCourierTowardsLAb) as DropPendingToLab,sum(UnderLabanalysis) LabAnalysisOngoing,sum(PendingforfinalUpdate) as PendingforfinalUpdate
from 
(
select itemcode,itemtypename,itemname,strength1,batchno,noswh,UQCQTY,Stockvalue Stockvalue,RECEIPTDATE as WarehouseRecDT
, WHQCIssueDT,CourierPickDT,
SampleReceiptInHODT
,LABISSUEDATE,LAbReceiptDT,HOQCReportRecDT,LABRESULT,QCDAYSLAB as AnalysisDays,
itemid
, case when WHQCIssueDT is null and SampleReceiptInHODT  is null  then 1 else 0 end as NotIssuedbyWH
,case when WHQCIssueDT is not  null  and CourierPickDT is not null and SampleReceiptInHODT is null and SampleReceiptInHODT  is null  then 1 else 0 end as UnderCourier
,case when SampleReceiptInHODT is not null  and LABISSUEDATE is null  then 1 else 0 end as NotIssued_ToLab
,case when SampleReceiptInHODT is not null  and LABISSUEDATE is not null and LAbReceiptDT is null   then 1 else 0 end as UnderCourierTowardsLAb
,case when  LAbReceiptDT is not null and HOQCReportRecDT is null  then 1 else 0 end as UnderLabanalysis
,case when  HOQCReportRecDT is not null   then 1 else 0 end as PendingforfinalUpdate
from 
(
select m.itemcode,m.itemname,m.strength1,b.batchno,count(distinct t.warehouseid) as noswh,sum(nvl(ABSRQTY,0)) as UQCQTY,round(sum(nvl(ABSRQTY,0))*p.finalrategst,0) as Stockvalue,
b.itemid,min(RECEIPTDATE) as RECEIPTDATE,ty.itemtypename,nvl(ty.QCDAYSLAB,0) as QCDAYSLAB,SampleReceiptInHODT
,LABISSUEDATE,LAbReceiptDT,RptReceipt as HOQCReportRecDT,LABRESULT
,QCIssueDT AS WHQCIssueDT  ,entrydatepick  as CourierPickDT
from tbreceiptbatches b
inner join 
(
select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
 inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
 inner join aoccontractitems c on c.contractitemid = oi.contractitemid
 ) p on p.ponoid=b.ponoid 
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
 inner join masitemtypes ty on ty.itemtypeid=m.itemtypeid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID=ic.MCID
 
 left outer join 
 (
 select max(b.LABISSUEDATE) LABISSUEDATE,max(b.SAMPLERECEIVEDDATE) as LAbReceiptDT ,max(b.REPORTRECEIVEDDATE) RptReceipt,max(b.LABRESULT) as LABRESULT ,a.BATCHNO,a.itemid from Qcsamples A
inner join  Qctests B on b.sampleid = a.sampleid
where 1=1 and b.LABISSUEDATE is not null
group by a.itemid,a.BATCHNO
 ) lbiss on lbiss.itemid=m.itemid and lbiss.BATCHNO=b.batchno
 
  left outer join 
 (
  select max(A.Receiptdate) SampleReceiptInHODT ,a.BATCHNO,a.itemid from Qcsamples A
left outer join   Qctests B on b.sampleid = a.sampleid
where 1=1 and A.Receiptdate is not null
group by a.itemid,a.BATCHNO
 ) HOREC on HOREC.itemid=m.itemid and HOREC.BATCHNO=b.batchno
 
   left outer join 
 (
 select tbi.itemid,rb.batchno,max(ct.entrydatepick) as entrydatepick,max(INDENTDATE) QCIssueDT
                                 
                                 from tbindents tb
                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
                                  where tb.Status = 'C' 
                                 and tb.CTID is not null      
                                 group by tbi.itemid,rb.batchno
) WHI on WHI.itemid=m.itemid and WHI.BATCHNO=b.batchno
 
 Where T.Status = 'C'  and m.qctest='Y'
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 
" + whclause + @" and (case when (b.qastatus = 0) and m.qctest='Y' then 1 else 0 end)=1
and ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0
group by p.finalrategst,b.batchno,b.itemid, m.itemcode,m.itemname,m.strength1,ty.itemtypename,ty.QCDAYSLAB
,LABISSUEDATE,LAbReceiptDT,RptReceipt ,LABRESULT,SampleReceiptInHODT,QCIssueDT  ,entrydatepick) 
) ";
            var myList = _context.QCHomeDashPlacewiseDbSet
          .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        //        [HttpGet("QCPendingParticularArea")]
        //        public async Task<ActionResult<IEnumerable<QCPendingAreaDetails>>> QCPendingParticularArea(string area)
        //        {
        //            string whclause = "";
        //            string orderbycluse = "";
        //            if (area == "WHIssue")
        //            {

        //                whclause = " and  (case when WHQCIssueDT is null and SampleReceiptInHODT  is null  then 1 else 0 end)=1";
        //                orderbycluse = "order by RECEIPTDATE  ";
        //            }
        //            if (area == "WHCourier")
        //            {


        //                whclause = " and  (case when WHQCIssueDT is not null and CourierPickDT is not null and SampleReceiptInHODT is null  then 1 else 0 end)= 1";
        //                orderbycluse = " order by CourierPickDT  ";
        //            }
        //            if (area == "LabIssue")
        //            {

        //                whclause = " and  (case when SampleReceiptInHODT is not null  and LABISSUEDATE is null  then 1 else 0 end)=1";
        //                orderbycluse = "order by SampleReceiptInHODT  ";
        //            }
        //            if (area == "LabCourier")
        //            {

        //                whclause = " and  (case when SampleReceiptInHODT is not null  and LABISSUEDATE is not null and LAbReceiptDT is null   then 1 else 0 end)=1";
        //                orderbycluse = "order by LABISSUEDATE  ";
        //            }
        //            if (area == "LabAnalysis")
        //            {

        //                whclause = " and  (case when  LAbReceiptDT is not null and HOQCReportRecDT is null  then 1 else 0 end)=1";
        //                orderbycluse = "order by LAbReceiptDT  ";
        //            }
        //            if (area == "FinalUpdate")
        //            {

        //                whclause = " and  (case when  HOQCReportRecDT is not null   then 1 else 0 end)=1";
        //                orderbycluse = "order by HOQCReportRecDT  ";
        //            }
        //            if (area == "0")
        //            {
        //                orderbycluse = " order by RECEIPTDATE  ";
        //            }

        //            string qry = @" select itemcode,itemtypename,itemname,strength1,batchno,noswh,UQCQTY,round(Stockvalue/100000,2)  Stockvalue,RECEIPTDATE as WarehouseRecDT
        //, WHQCIssueDT,CourierPickDT,
        //SampleReceiptInHODT
        //,LABISSUEDATE,LAbReceiptDT,HOQCReportRecDT,LABRESULT,QCDAYSLAB as AnalysisDays,
        //itemid
        //, case when WHQCIssueDT is null and SampleReceiptInHODT  is null  then 1 else 0 end as NotIssuedbyWH
        //,case when WHQCIssueDT is not  null  and CourierPickDT is not null and SampleReceiptInHODT is null and SampleReceiptInHODT  is null  then 1 else 0 end as UnderCourier
        //,case when SampleReceiptInHODT is not null  and LABISSUEDATE is null  then 1 else 0 end as NotIssued_ToLab
        //,case when SampleReceiptInHODT is not null  and LABISSUEDATE is not null and LAbReceiptDT is null   then 1 else 0 end as UnderCourierTowardsLAb
        //,case when  LAbReceiptDT is not null and HOQCReportRecDT is null  then 1 else 0 end as UnderLabanalysis
        //,case when  HOQCReportRecDT is not null   then 1 else 0 end as PendingforfinalUpdate
        //from 
        //(
        //select m.itemcode,m.itemname,m.strength1,b.batchno,count(distinct t.warehouseid) as noswh,sum(nvl(ABSRQTY,0)) as UQCQTY,round(sum(nvl(ABSRQTY,0))*p.finalrategst,0) as Stockvalue,
        //b.itemid,min(RECEIPTDATE) as RECEIPTDATE,ty.itemtypename,nvl(ty.QCDAYSLAB,0) as QCDAYSLAB,SampleReceiptInHODT
        //,LABISSUEDATE,LAbReceiptDT,RptReceipt as HOQCReportRecDT,LABRESULT
        //,QCIssueDT AS WHQCIssueDT  ,entrydatepick  as CourierPickDT
        //from tbreceiptbatches b
        //inner join 
        //(
        //select op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op 
        // inner join soOrderedItems OI on (OI.ponoid = op.ponoid)
        // inner join aoccontractitems c on c.contractitemid = oi.contractitemid
        // ) p on p.ponoid=b.ponoid 
        // join tbreceiptitems i on b.receiptitemid = i.receiptitemid
        // inner  join tbreceipts t on t.receiptid = i.receiptid
        // inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid
        // inner join masitemtypes ty on ty.itemtypeid=m.itemtypeid
        // inner join masitemcategories ic on ic.categoryid = m.categoryid
        // inner join masitemmaincategory mc on mc.MCID=ic.MCID

        // left outer join 
        // (
        // select max(b.LABISSUEDATE) LABISSUEDATE,max(b.SAMPLERECEIVEDDATE) as LAbReceiptDT ,max(b.REPORTRECEIVEDDATE) RptReceipt,max(b.LABRESULT) as LABRESULT ,a.BATCHNO,a.itemid from Qcsamples A
        //inner join  Qctests B on b.sampleid = a.sampleid
        //where 1=1 and b.LABISSUEDATE is not null
        //group by a.itemid,a.BATCHNO
        // ) lbiss on lbiss.itemid=m.itemid and lbiss.BATCHNO=b.batchno

        //  left outer join 
        // (
        //  select max(A.Receiptdate) SampleReceiptInHODT ,a.BATCHNO,a.itemid from Qcsamples A
        //left outer join   Qctests B on b.sampleid = a.sampleid
        //where 1=1 and A.Receiptdate is not null
        //group by a.itemid,a.BATCHNO
        // ) HOREC on HOREC.itemid=m.itemid and HOREC.BATCHNO=b.batchno

        //   left outer join 
        // (
        // select tbi.itemid,rb.batchno,max(ct.entrydatepick) as entrydatepick,max(INDENTDATE) QCIssueDT

        //                                 from tbindents tb
        //                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
        //                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
        //                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno
        //                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid
        //                                  where tb.Status = 'C' 
        //                                 and tb.CTID is not null      
        //                                 group by tbi.itemid,rb.batchno
        //) WHI on WHI.itemid=m.itemid and WHI.BATCHNO=b.batchno

        // Where T.Status = 'C' 
        // And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate
        //and mc.mcid=1 and (case when (b.qastatus = 0) and m.qctest='Y' then 1 else 0 end)=1
        //and ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0 
        //group by p.finalrategst,b.batchno,b.itemid, m.itemcode,m.itemname,m.strength1,ty.itemtypename,ty.QCDAYSLAB
        //,LABISSUEDATE,LAbReceiptDT,RptReceipt ,LABRESULT,SampleReceiptInHODT,QCIssueDT  ,entrydatepick
        //)  where 1=1 " + whclause + @"
        // " + orderbycluse + @" ";
        //            var myList = _context.QCPendingAreaDetailsDbSet
        //        .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;
        //        }


        [HttpGet("QCPendingParticularArea")]
        public async Task<ActionResult<IEnumerable<QCPendingAreaDetails>>> QCPendingParticularArea(
     string area,
     string itemid)
        {
            string whclause = "";
            string orderbycluse = "";
            if (area == "WHIssue")
            {
                whclause = " and  (case when WHQCIssueDT is null and SampleReceiptInHODT  is null  then 1 else 0 end)=1";
                orderbycluse = "order by RECEIPTDATE  ";
            }
            if (area == "WHCourier")
            {
                whclause = " and  (case when WHQCIssueDT is not null and CourierPickDT is not null and SampleReceiptInHODT is null  then 1 else 0 end)= 1";
                orderbycluse = " order by CourierPickDT  ";
            }
            if (area == "LabIssue")
            {
                whclause = " and  (case when SampleReceiptInHODT is not null  and LABISSUEDATE is null  then 1 else 0 end)=1";
                orderbycluse = "order by SampleReceiptInHODT  ";
            }
            if (area == "LabCourier")
            {
                whclause = " and  (case when SampleReceiptInHODT is not null  and LABISSUEDATE is not null and LAbReceiptDT is null   then 1 else 0 end)=1";
                orderbycluse = "order by LABISSUEDATE  ";
            }
            if (area == "LabAnalysis")
            {
                whclause = " and  (case when  LAbReceiptDT is not null and HOQCReportRecDT is null  then 1 else 0 end)=1";
                orderbycluse = "order by LAbReceiptDT  ";
            }
            if (area == "FinalUpdate")
            {
                whclause = " and  (case when  HOQCReportRecDT is not null   then 1 else 0 end)=1";
                orderbycluse = "order by HOQCReportRecDT  ";
            }
            string bitemid = "";
            if (area == "0")
            {
                orderbycluse = " order by RECEIPTDATE  ";
                if (itemid != "0")
                    bitemid = bitemid + " and b.itemid =" + itemid;
            }
            string qry = " select itemcode,itemtypename,itemname,strength1,batchno,noswh,UQCQTY,round(Stockvalue/100000,2)  Stockvalue,RECEIPTDATE as WarehouseRecDT\r\n, WHQCIssueDT,CourierPickDT,\r\nSampleReceiptInHODT\r\n,LABISSUEDATE,LAbReceiptDT,HOQCReportRecDT,LABRESULT,QCDAYSLAB as AnalysisDays,\r\nitemid\r\n, case when WHQCIssueDT is null and SampleReceiptInHODT  is null  then 1 else 0 end as NotIssuedbyWH\r\n,case when WHQCIssueDT is not  null  and CourierPickDT is not null and SampleReceiptInHODT is null and SampleReceiptInHODT  is null  then 1 else 0 end as UnderCourier\r\n,case when SampleReceiptInHODT is not null  and LABISSUEDATE is null  then 1 else 0 end as NotIssued_ToLab\r\n,case when SampleReceiptInHODT is not null  and LABISSUEDATE is not null and LAbReceiptDT is null   then 1 else 0 end as UnderCourierTowardsLAb\r\n,case when  LAbReceiptDT is not null and HOQCReportRecDT is null  then 1 else 0 end as UnderLabanalysis\r\n,case when  HOQCReportRecDT is not null   then 1 else 0 end as PendingforfinalUpdate\r\nfrom \r\n(\r\nselect m.itemcode,m.itemname,m.strength1,b.batchno,count(distinct t.warehouseid) as noswh,sum(nvl(ABSRQTY,0)) as UQCQTY,round(sum(nvl(ABSRQTY,0))*p.finalrategst,0) as Stockvalue,\r\nb.itemid,min(RECEIPTDATE) as RECEIPTDATE,ty.itemtypename,nvl(ty.QCDAYSLAB,0) as QCDAYSLAB,SampleReceiptInHODT\r\n,LABISSUEDATE,LAbReceiptDT,RptReceipt as HOQCReportRecDT,LABRESULT\r\n,QCIssueDT AS WHQCIssueDT  ,entrydatepick  as CourierPickDT\r\nfrom tbreceiptbatches b\r\ninner join \r\n(\r\nselect op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op \r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n ) p on p.ponoid=b.ponoid \r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid\r\n inner join masitemtypes ty on ty.itemtypeid=m.itemtypeid\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n \r\n left outer join \r\n (\r\n select max(b.LABISSUEDATE) LABISSUEDATE,max(b.SAMPLERECEIVEDDATE) as LAbReceiptDT ,max(b.REPORTRECEIVEDDATE) RptReceipt,max(b.LABRESULT) as LABRESULT ,a.BATCHNO,a.itemid from Qcsamples A\r\ninner join  Qctests B on b.sampleid = a.sampleid\r\nwhere 1=1 and b.LABISSUEDATE is not null\r\ngroup by a.itemid,a.BATCHNO\r\n ) lbiss on lbiss.itemid=m.itemid and lbiss.BATCHNO=b.batchno\r\n \r\n  left outer join \r\n (\r\n  select max(A.Receiptdate) SampleReceiptInHODT ,a.BATCHNO,a.itemid from Qcsamples A\r\nleft outer join   Qctests B on b.sampleid = a.sampleid\r\nwhere 1=1 and A.Receiptdate is not null\r\ngroup by a.itemid,a.BATCHNO\r\n ) HOREC on HOREC.itemid=m.itemid and HOREC.BATCHNO=b.batchno\r\n \r\n   left outer join \r\n (\r\n select tbi.itemid,rb.batchno,max(ct.entrydatepick) as entrydatepick,max(INDENTDATE) QCIssueDT\r\n                                 \r\n                                 from tbindents tb\r\n                                 inner join tbindentitems tbi on tbi.indentid=tb.indentid \r\n                                 inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid\r\n                                 inner join  tbreceiptbatches rb on rb.inwno=tbo.inwno\r\n                                 inner join CourierTransaction ct on  ct.ctid = tb.ctid\r\n                                  where tb.Status = 'C' \r\n                                 and tb.CTID is not null      \r\n                                 group by tbi.itemid,rb.batchno\r\n) WHI on WHI.itemid=m.itemid and WHI.BATCHNO=b.batchno\r\n \r\n Where T.Status = 'C' " + bitemid + "\r\n And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate\r\nand mc.mcid=1 and (case when (b.qastatus = 0) and m.qctest='Y' then 1 else 0 end)=1\r\nand ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0 \r\ngroup by p.finalrategst,b.batchno,b.itemid, m.itemcode,m.itemname,m.strength1,ty.itemtypename,ty.QCDAYSLAB\r\n,LABISSUEDATE,LAbReceiptDT,RptReceipt ,LABRESULT,SampleReceiptInHODT,QCIssueDT  ,entrydatepick\r\n)  where 1=1 " + whclause + "\r\n " + orderbycluse + " ";
            List<QCPendingAreaDetails> myList = this._context.QCPendingAreaDetailsDbSet.FromSqlInterpolated<QCPendingAreaDetails>(FormattableStringFactory.Create(qry)).ToList<QCPendingAreaDetails>();
            ActionResult<IEnumerable<QCPendingAreaDetails>> actionResult = (ActionResult<IEnumerable<QCPendingAreaDetails>>)myList;
            whclause = (string)null;
            orderbycluse = (string)null;
            bitemid = (string)null;
            qry = (string)null;
            myList = (List<QCPendingAreaDetails>)null;
            return actionResult;
        }


        [HttpGet("QCPendingItems")]
        public async Task<ActionResult<IEnumerable<MasItemsDash>>> QCPendingItems(string mcid)
        {
    
            string whclause1 = "";

            if (mcid != "0")
            {


                whclause1 = " and mc.mcid =" + mcid;
            }


            string qry = @" select itemid, itemname|| '' || nvl(strength1, '-') || '-' || to_char(RECEIPTDATE, 'dd-MM-yyyy') || ' Batches:' || to_char(nosbatch) || ' QTY:' || to_char(UQCQTY) || '-' || itemcode as NameText,
RECEIPTDATE as itemname,0 as mcid
from
(
select  m.itemcode, m.itemname, m.strength1, b.itemid, count(distinct b.batchno) as nosbatch, sum(nvl(ABSRQTY, 0)) as UQCQTY, min(RECEIPTDATE) as RECEIPTDATE
from tbreceiptbatches b
inner
        join
(
select op.ponoid, oi.itemid from soorderplaced op
 inner
                            join soOrderedItems OI on (OI.ponoid = op.ponoid)
 ) p on p.ponoid = b.ponoid
 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner join tbreceipts t on t.receiptid = i.receiptid
 inner join masitems m on m.itemid = b.itemid and m.itemid = b.itemid
 inner join masitemcategories ic on ic.categoryid = m.categoryid
 inner join masitemmaincategory mc on mc.MCID = ic.MCID


 left outer join
        (
 select max(b.LABISSUEDATE) LABISSUEDATE, max(b.SAMPLERECEIVEDDATE) as LAbReceiptDT, max(b.REPORTRECEIVEDDATE) RptReceipt, max(b.LABRESULT) as LABRESULT, a.BATCHNO, a.itemid from Qcsamples A
inner
                                                                                                                                                                              join Qctests B on b.sampleid = a.sampleid
where 1 = 1 and b.LABISSUEDATE is not null
group by a.itemid, a.BATCHNO
 ) lbiss on lbiss.itemid = m.itemid and lbiss.BATCHNO = b.batchno


  left outer join
 (
  select max(A.Receiptdate) SampleReceiptInHODT, a.BATCHNO, a.itemid from Qcsamples A
left outer join   Qctests B on b.sampleid = a.sampleid
where 1 = 1 and A.Receiptdate is not null
group by a.itemid, a.BATCHNO
 ) HOREC on HOREC.itemid = m.itemid and HOREC.BATCHNO = b.batchno


   left outer join
 (
 select tbi.itemid, rb.batchno, max(ct.entrydatepick) as entrydatepick, max(INDENTDATE) QCIssueDT


                                 from tbindents tb
                                 inner
                                 join tbindentitems tbi on tbi.indentid = tb.indentid
                                 inner
                                 join tboutwards tbo on tbo.indentitemid = tbi.indentitemid
                                 inner
                                 join tbreceiptbatches rb on rb.inwno = tbo.inwno
                                 inner
                                 join CourierTransaction ct on ct.ctid = tb.ctid
                                  where tb.Status = 'C'
                                 and tb.CTID is not null
                                 group by tbi.itemid, rb.batchno
) WHI on WHI.itemid = m.itemid and WHI.BATCHNO = b.batchno


 Where T.Status = 'C' and m.qctest = 'Y'  " + whclause1 + @"
 And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate and m.qctest='Y'
 and(case when(b.qastatus = 0) and m.qctest = 'Y' then 1 else 0 end)= 1
and((nvl(ABSRQTY, 0)) - (nvl(ALLOTQTY, 0))) > 0
group by b.itemid, m.itemcode,m.itemname,m.strength1
) order by RECEIPTDATE";

            var myList = _context.MasItemsDashDbSet
         .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("QCLabPendingTimeline")]
        public async Task<ActionResult<IEnumerable<QCSamplePendingTimelinesDTO>>> QCLabPendingTimeline(
    string Timeline,
    string mcid,
    string labid)
        {
            string whclause1 = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            if (labid != "0")
                whclause1 = whclause1 + " and ql.labid =" + labid;
            string qry = "";
            if (Timeline == "Within")
                qry = "    select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,\r\n   mcid, mcategory,count(distinct itemid) as nositems\r\n,count(distinct batchno) as nosbatch,Round(sum(BatchValueLacs)/10000000,2) as UQCValuecr\r\n, TimePara,TimeParaValue\r\n,timeline\r\nfrom \r\n(\r\nselect  t.sampleid,mc.mcid, mc.mcategory, ql.labid,ql.labname,\r\ns.batchno,t.sampleno,t.samplereceiveddate,t.labuploadeddate,\r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) DaysSince,nvl(mt.qcdayslab,0) as AnalysisDays\r\n,(round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab) as exdays\r\n,m.itemid,round((tb.absrqty*ci.finalrategst),2) BatchValueLacs,\r\ncase when mt.qcdayslab is  null then 'Timeline Not Set'\r\nelse case when \r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline\r\n\r\n\r\n,case when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>15 then '>15 days'\r\nelse case when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))<=15 and (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>6 then '7-15 days'\r\nelse case when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))<=6 and (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>=3 then '3-6 days'\r\nelse 'Under 2 days' \r\nend end end as  TimePara,\r\n\r\ncase when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>15 then '4.>15 days'\r\nelse case when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))<=15 and (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>6 then '3.7-15 days'\r\nelse case when (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))<=6 and (mt.qcdayslab-round(sysdate-t.SAMPLERECEIVEDDATE))>=3 then '2.3-6 days'\r\nelse '1.Under 2 days' \r\nend end end as  TimeParaValue\r\n\r\nfrom qctests t\r\ninner join qcsamples s on s.sampleid = t.sampleid\r\ninner join masitems m on m.itemid = s.itemid\r\n inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid\r\ninner join masitemcategories c on c.categoryid=m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=c.MCID\r\ninner join qclabs ql on ql.labid = t.labid\r\ninner join usrusers u on u.labid=ql.labid\r\n  left outer join\r\n (\r\n select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo \r\n inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid\r\n inner join tbindents t on t.indentid = tbi.indentid \r\n where  t.issuetype in ('QA','QS') and t.status = 'C' \r\n ) qi on qi.outwno=s.outwno\r\n  inner join tbreceiptbatches tb on tb.inwno  =qi.inwno\r\n  inner join soordereditems oi on oi.ponoid = tb.ponoid\r\n  inner join aoccontractitems ci on ci.contractitemid = oi.contractitemid\r\nwhere  1=1  " + whclause1 + "\r\nand t.SAMPLERECEIVEDDATE between '01-Apr-23' and sysdate  and t.LABUPLOADEDDATE is null\r\n)\r\nwhere timeline='Within timeline'\r\ngroup by mcid, mcategory ,TimePara,TimeParaValue\r\n,timeline\r\norder by TimeParaValue ";
            qry = !(Timeline == "Out") ? "    select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,\r\n   mcid, mcategory,count(distinct itemid) as nositems\r\n,count(distinct batchno) as nosbatch,Round(sum(BatchValueLacs)/10000000,2) as UQCValuecr\r\n,'All' as TimePara,'All' as TimeParaValue\r\n,timeline\r\nfrom \r\n(\r\nselect  t.sampleid,mc.mcid, mc.mcategory, ql.labid,ql.labname,\r\ns.batchno,t.sampleno,t.samplereceiveddate,t.labuploadeddate,\r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) DaysSince,nvl(mt.qcdayslab,0) as AnalysisDays\r\n,(round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab) as exdays\r\n,m.itemid,round((tb.absrqty*ci.finalrategst),2) BatchValueLacs,\r\ncase when mt.qcdayslab is  null then 'Timeline Not Set'\r\nelse case when \r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline\r\n,case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>15 then '>15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=15 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>6 then '7-15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=6 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>=3 then '3-6 days'\r\nelse 'Under 2 days' \r\nend end end as  TimePara,\r\n\r\ncase when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>15 then '1.>15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=15 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>6 then '2.7-15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=6 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>=3 then '3.3-6 days'\r\nelse '4.Under 2 days' \r\nend end end as  TimeParaValue\r\nfrom qctests t\r\ninner join qcsamples s on s.sampleid = t.sampleid\r\ninner join masitems m on m.itemid = s.itemid\r\n inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid\r\ninner join masitemcategories c on c.categoryid=m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=c.MCID\r\ninner join qclabs ql on ql.labid = t.labid\r\ninner join usrusers u on u.labid=ql.labid\r\n  left outer join\r\n (\r\n select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo \r\n inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid\r\n inner join tbindents t on t.indentid = tbi.indentid \r\n where  t.issuetype in ('QA','QS') and t.status = 'C' \r\n ) qi on qi.outwno=s.outwno\r\n  inner join tbreceiptbatches tb on tb.inwno  =qi.inwno\r\n  inner join soordereditems oi on oi.ponoid = tb.ponoid\r\n  inner join aoccontractitems ci on ci.contractitemid = oi.contractitemid\r\nwhere  1=1  " + whclause1 + "\r\nand t.SAMPLERECEIVEDDATE between '01-Apr-23' and sysdate  and t.LABUPLOADEDDATE is null\r\n)\r\nwhere 1=1 --timeline='Out of timeline'\r\ngroup by mcid, mcategory,timeline\r\norder by timeline " : "    select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,\r\n   mcid, mcategory,count(distinct itemid) as nositems\r\n,count(distinct batchno) as nosbatch,Round(sum(BatchValueLacs)/10000000,2) as UQCValuecr\r\n, TimePara,TimeParaValue\r\n,timeline\r\nfrom \r\n(\r\nselect  t.sampleid,mc.mcid, mc.mcategory, ql.labid,ql.labname,\r\ns.batchno,t.sampleno,t.samplereceiveddate,t.labuploadeddate,\r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) DaysSince,nvl(mt.qcdayslab,0) as AnalysisDays\r\n,(round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab) as exdays\r\n,m.itemid,round((tb.absrqty*ci.finalrategst),2) BatchValueLacs,\r\ncase when mt.qcdayslab is  null then 'Timeline Not Set'\r\nelse case when \r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end end as timeline\r\n,case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>15 then '>15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=15 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>6 then '7-15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=6 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>=3 then '3-6 days'\r\nelse 'Under 2 days' \r\nend end end as  TimePara,\r\n\r\ncase when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>15 then '1.>15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=15 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>6 then '2.7-15 days'\r\nelse case when (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)<=6 and (round(sysdate-t.SAMPLERECEIVEDDATE)-mt.qcdayslab)>=3 then '3.3-6 days'\r\nelse '4.Under 2 days' \r\nend end end as  TimeParaValue\r\nfrom qctests t\r\ninner join qcsamples s on s.sampleid = t.sampleid\r\ninner join masitems m on m.itemid = s.itemid\r\n inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid\r\ninner join masitemcategories c on c.categoryid=m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=c.MCID\r\ninner join qclabs ql on ql.labid = t.labid\r\ninner join usrusers u on u.labid=ql.labid\r\n  left outer join\r\n (\r\n select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo \r\n inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid\r\n inner join tbindents t on t.indentid = tbi.indentid \r\n where  t.issuetype in ('QA','QS') and t.status = 'C' \r\n ) qi on qi.outwno=s.outwno\r\n  inner join tbreceiptbatches tb on tb.inwno  =qi.inwno\r\n  inner join soordereditems oi on oi.ponoid = tb.ponoid\r\n  inner join aoccontractitems ci on ci.contractitemid = oi.contractitemid\r\nwhere  1=1  " + whclause1 + "\r\nand t.SAMPLERECEIVEDDATE between '01-Apr-23' and sysdate  and t.LABUPLOADEDDATE is null\r\n)\r\nwhere timeline='Out of timeline'\r\ngroup by mcid, mcategory, TimePara,TimeParaValue\r\n,timeline\r\norder by TimeParaValue ";
            List<QCSamplePendingTimelinesDTO> myList = this._context.QCSamplePendingTimelinesDbSet.FromSqlInterpolated<QCSamplePendingTimelinesDTO>(FormattableStringFactory.Create(qry)).ToList<QCSamplePendingTimelinesDTO>();
            ActionResult<IEnumerable<QCSamplePendingTimelinesDTO>> actionResult = (ActionResult<IEnumerable<QCSamplePendingTimelinesDTO>>)myList;
            whclause1 = (string)null;
            qry = (string)null;
            myList = (List<QCSamplePendingTimelinesDTO>)null;
            return actionResult;
        }

        [HttpGet("QCResultPendingLabWise")]
        public async Task<ActionResult<IEnumerable<LabWisePendingDTO>>> QCResultPendingLabWise(
      string mcid)
        {
            string whclause1 = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            string qry = "   select labid,labname,Withvalue,WithBatches,outvalue,outBatches,outPer\r\nfrom \r\n(\r\nselect labid,labname,sum(Withvalue) as Withvalue,sum(WithBatches) as WithBatches,sum(outvalue)outvalue ,sum(outBatches) as outBatches\r\n,round((sum(outBatches)/(sum(WithBatches)+sum(outBatches)))*100,2) as outPer\r\nfrom \r\n(\r\nselect labid,labname,timeline\r\n,case when timeline='Within timeline' then round(sum(BatchValueLacs)/10000000,2) else 0 end as Withvalue\r\n,case when timeline='Within timeline' then count(distinct batchno) else 0 end as WithBatches\r\n,case when timeline='Within timeline' then 0 else round(sum(BatchValueLacs)/10000000,2) end as outvalue\r\n,case when timeline='Within timeline' then 0 else count(distinct batchno) end as outBatches\r\nfrom \r\n(\r\n select mc.mcid, mc.mcategory, ql.labid,ql.labname,\r\ns.batchno,round((tb.absrqty*ci.finalrategst),2) BatchValueLacs,\r\ncase when \r\nround((sysdate-t.SAMPLERECEIVEDDATE),0) <= mt.qcdayslab then 'Within timeline' else 'Out of timeline' end  as timeline from qctests t\r\ninner join qcsamples s on s.sampleid = t.sampleid\r\ninner join masitems m on m.itemid = s.itemid\r\n inner join masitemtypes mt on mt.itemtypeid = m.itemtypeid\r\ninner join masitemcategories c on c.categoryid=m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=c.MCID\r\ninner join qclabs ql on ql.labid = t.labid\r\ninner join usrusers u on u.labid=ql.labid\r\ninner join tbreceiptbatches tb on tb.batchno  =s.batchno and tb.itemid=m.itemid\r\n  inner join soordereditems oi on oi.ponoid = tb.ponoid\r\n  inner join aoccontractitems ci on ci.contractitemid = oi.contractitemid\r\nwhere  1=1  " + whclause1 + " and (case when (tb.qastatus = 0) and m.qctest='Y' then 1 else 0 end)=1\r\nand ((nvl(tb.ABSRQTY,0))-(nvl(tb.ALLOTQTY,0)))>0 \r\nand t.SAMPLERECEIVEDDATE between '01-Apr-23' and sysdate  and t.LABUPLOADEDDATE is null\r\n) group by labid,labname,timeline\r\n)group by labid,labname\r\n)\r\norder by outPer desc  ";
            List<LabWisePendingDTO> myList = this._context.LabWisePendingDbSet.FromSqlInterpolated<LabWisePendingDTO>(FormattableStringFactory.Create(qry)).ToList<LabWisePendingDTO>();
            ActionResult<IEnumerable<LabWisePendingDTO>> actionResult = (ActionResult<IEnumerable<LabWisePendingDTO>>)myList;
            whclause1 = (string)null;
            qry = (string)null;
            myList = (List<LabWisePendingDTO>)null;
            return actionResult;
        }

        [HttpGet("QCHold_NSQDash")]
        public async Task<ActionResult<IEnumerable<QCHomeDashDTO>>> QCHold_NSQDash(
     string mcid,
     string nsqhold)
        {
            string whclause1 = "";
            string whholdnsqclause = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            whholdnsqclause = !(nsqhold == "Hold") ? "  And b.qastatus =2 " : "  And b.qastatus  in (0) and  b.whissueblock=1 ";
            string qry = "  select  mcid,mcategory,count(distinct itemid) as nositems,nvl(round(sum(STKVALUE)/10000000,2),0) as STKVALUE,count(distinct batchno) as nosbatch\r\nfrom \r\n(\r\nselect  nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,\r\nb.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid\r\n,case when m.isedl2021='Y' then b.itemid else 0  end as EDLitem\r\n,case when m.isedl2021='Y' then  0  else b.itemid end as NonEDL\r\n,case when m.isedl2021='Y' then (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst else 0  end as EDLitemValue\r\n,case when m.isedl2021='Y' then 0  else  (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst end as NonEDLitemValue\r\n,nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty\r\n,b.batchno,mc.mcategory,mc.mcid\r\nfrom tbreceiptbatches b\r\nleft outer join \r\n(\r\nselect op.ponoid,oi.itemid,c.finalrategst from  soorderplaced op \r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n ) p on p.ponoid=b.ponoid \r\n left outer join \r\n(\r\nselect max(sampleid) sampleid ,BATCHNO,ITEMID,PONOID from qcsamples\r\ngroup by BATCHNO,ITEMID,PONOID\r\n) hld on hld.BATCHNO=b.BATCHNO and hld.ITEMID=b.itemid and hld.PONOID=p.ponoid\r\nleft outer join qcsamples c on c.sampleid=hld.sampleid\r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n Where T.Status = 'C' " + whholdnsqclause + " " + whclause1 + "\r\n and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0\r\nand ((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0\r\n )  b group by mcategory,mcid\r\n order by mcid  ";
            List<QCHomeDashDTO> myList = this._context.QCHomeDashDbSet.FromSqlInterpolated<QCHomeDashDTO>(FormattableStringFactory.Create(qry)).ToList<QCHomeDashDTO>();
            ActionResult<IEnumerable<QCHomeDashDTO>> actionResult = (ActionResult<IEnumerable<QCHomeDashDTO>>)myList;
            whclause1 = (string)null;
            whholdnsqclause = (string)null;
            qry = (string)null;
            myList = (List<QCHomeDashDTO>)null;
            return actionResult;
        }

        [HttpGet("QCPendingMonthwiseRec")]
        public async Task<ActionResult<IEnumerable<QCMonthWisePendingRecDTO>>> QCPendingMonthwiseRec(
      string mcid)
        {
            string whclause1 = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            string qry = " select monthid, MCID, mcategory,md.MONTHNAME,md.MNAME, count(distinct itemid) as nositems,nvl(round(sum(STKVALUE)/10000000,2),0) as STKVALUE,count(distinct batchno) as nosbatch\r\nfrom\r\n(\r\nselect nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,\r\nb.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid\r\n,case when(b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end as QCStck\r\n, nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty\r\n,b.batchno,mc.mcategory,mc.mcid,min(t.RECEIPTDATE) RecDT,to_date(TO_CHAR(min(t.RECEIPTDATE), 'MON-YYYY'), 'MON-YYYY') mon\r\n,to_char(min(t.RECEIPTDATE),'mm') as MM\r\nfrom tbreceiptbatches b\r\nleft outer join\r\n(\r\nselect op.ponoid, oi.itemid, c.finalrategst from  soorderplaced op\r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n ) p on p.ponoid=b.ponoid\r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid= b.itemid and m.itemid= b.itemid and m.qctest= 'Y'\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID= ic.MCID\r\n\r\n\r\n Where T.Status = 'C'\r\n And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate " + whclause1 + "\r\n and (case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end)=1\r\nand((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0\r\ngroup by b.batchno,mc.mcategory,mc.mcid,b.qastatus,b.ABSRQTY,b.ALLOTQTY,b.ponoid,p.finalrategst,b.itemid,m.qctest\r\n )  b\r\n  inner join masfymonth md on md.monthid=cast(b.MM as int)\r\n group by mcategory,MCID,MM, md.MONTHNAME,md.MNAME,monthid order by MM ";
            List<QCMonthWisePendingRecDTO> myList = this._context.QCMonthWisePendingRecDbSet.FromSqlInterpolated<QCMonthWisePendingRecDTO>(FormattableStringFactory.Create(qry)).ToList<QCMonthWisePendingRecDTO>();
            ActionResult<IEnumerable<QCMonthWisePendingRecDTO>> actionResult = (ActionResult<IEnumerable<QCMonthWisePendingRecDTO>>)myList;
            whclause1 = (string)null;
            qry = (string)null;
            myList = (List<QCMonthWisePendingRecDTO>)null;
            return actionResult;
        }

        [HttpGet("QCResultFinalUpdatePending")]
        public async Task<ActionResult<IEnumerable<QCLabOutDTO>>> QCResultFinalUpdatePending(string mcid)
        {
            string whclause1 = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            string qry = "   select ROW_NUMBER() OVER ( ORDER BY mcid ) AS ID,mcid, mcategory,count(distinct itemid) as nositems\r\n,count(distinct batchno) as nosbatch,Round(sum(receiptvalue)/10000000,2) as uqcValue,'Final Update Pending' as timeline\r\n\r\n,daysPendingP as  ExceddedSincetimeline,daysPending as ExceddedSincetimeline1\r\nfrom \r\n(\r\nselect distinct t.sampleid,mc.mcid, mc.mcategory, ql.labid,ql.labname,ql.email,ql.phone1 mobileno,m.itemcode drugcode,m.itemname drugname,\r\ns.batchno,t.sampleno,t.samplereceiveddate,t.labuploadeddate,\r\nround((sysdate-t.labuploadeddate),0) PendinDays,t.labresult\r\n\r\n,case when dr.descid is not null then '6:Discrepancy in Report' else case when round((sysdate-t.labuploadeddate),0)=0 then '5:-Todays Pending'\r\nelse case when round((sysdate-t.labuploadeddate),0)>0 and  round((sysdate-t.labuploadeddate),0)<=3\r\nthen '4:-1 to 3 days Pending'\r\nelse case when round((sysdate-t.labuploadeddate),0)>3 and  round((sysdate-t.labuploadeddate),0)<=6\r\nthen '3:-3 to 6 days Pending' else case when round((sysdate-t.labuploadeddate),0)>6 and  round((sysdate-t.labuploadeddate),0)<=10\r\nthen '2:-7 to 10 days Pending'\r\nelse  '1:More than 10 days Pending'\r\nend end end end end as daysPending\r\n,case when dr.descid is not null then 'Discrepancy in Report' else case when round((sysdate-t.labuploadeddate),0)=0 then 'Today'\r\nelse case when round((sysdate-t.labuploadeddate),0)>0 and  round((sysdate-t.labuploadeddate),0)<=3\r\nthen '1-3 days'\r\nelse case when round((sysdate-t.labuploadeddate),0)>3 and  round((sysdate-t.labuploadeddate),0)<=6\r\nthen '3-6 days' else case when round((sysdate-t.labuploadeddate),0)>7 and  round((sysdate-t.labuploadeddate),0)<=10\r\nthen '7-10 days' \r\nelse  '> 10 days'\r\nend end end end end as daysPendingP\r\n\r\n,m.itemid,ci.finalrategst,tb.absrqty*ci.finalrategst receiptvalue\r\nfrom qctests t\r\ninner join qcsamples s on s.sampleid = t.sampleid\r\ninner join masitems m on m.itemid = s.itemid\r\ninner join masitemcategories c on c.categoryid=m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=c.MCID\r\ninner join qclabs ql on ql.labid = t.labid\r\ninner join usrusers u on u.labid=ql.labid\r\n\r\n left outer join  \r\n ( \r\n select max(descid) as descid,labid,sampleid,testid from qcsamplediscrepancy \r\n where 1=1 and  islabdisc='N'  group by labid,sampleid,testid \r\n ) dr  on dr.labid=T.LabID and dr.testid=T.QcTestID  and  dr.sampleid=T.Sampleid \r\n left outer join qcsamplediscrepancy d on d.descid=dr.descid \r\n  left outer join\r\n (\r\n select tbi.itemid,tbo.outwno,tbo.inwno from tboutwards tbo \r\n inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid\r\n inner join tbindents t on t.indentid = tbi.indentid \r\n where  t.issuetype in ('QA','QS') and t.status = 'C' \r\n ) qi on qi.outwno=s.outwno\r\n  inner join tbreceiptbatches tb on tb.inwno  =qi.inwno\r\n  left outer join soordereditems oi on oi.ponoid = tb.ponoid\r\n left outer join aoccontractitems ci on ci.contractitemid = oi.contractitemid\r\nwhere  1=1 and T.LabIssueDate>'01-Apr-2022' " + whclause1 + "\r\nand   T.SampleReceivedDate is not null   and t.labuploadeddate is not null \r\nand t.reportreceiveddate is null and t.testresult is  null and dr.descid is null\r\n) group by mcid, mcategory,daysPending,daysPendingP order by mcid,daysPending  ";
            List<QCLabOutDTO> myList = this._context.QCLabOutDbSet.FromSqlInterpolated<QCLabOutDTO>(FormattableStringFactory.Create(qry)).ToList<QCLabOutDTO>();
            ActionResult<IEnumerable<QCLabOutDTO>> actionResult = (ActionResult<IEnumerable<QCLabOutDTO>>)myList;
            whclause1 = (string)null;
            qry = (string)null;
            myList = (List<QCLabOutDTO>)null;
            return actionResult;
        }

        //   [HttpGet("QCPendingMonthwiseRec")]
        //   public async Task<ActionResult<IEnumerable<QCMonthWisePendingRecDTO>>> QCPendingMonthwiseRec(
        //string mcid)
        //   {
        //       string whclause1 = "";
        //       if (mcid != "0")
        //           whclause1 = " and mc.mcid =" + mcid;
        //       string qry = " select monthid, MCID, mcategory,md.MONTHNAME,md.MNAME, count(distinct itemid) as nositems,nvl(round(sum(STKVALUE)/10000000,2),0) as STKVALUE,count(distinct batchno) as nosbatch\r\nfrom\r\n(\r\nselect nvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,\r\nb.ponoid,p.finalrategst,(nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst as Stkvalue,b.itemid\r\n,case when(b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end as QCStck\r\n, nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty\r\n,b.batchno,mc.mcategory,mc.mcid,min(t.RECEIPTDATE) RecDT,to_date(TO_CHAR(min(t.RECEIPTDATE), 'MON-YYYY'), 'MON-YYYY') mon\r\n,to_char(min(t.RECEIPTDATE),'mm') as MM\r\nfrom tbreceiptbatches b\r\nleft outer join\r\n(\r\nselect op.ponoid, oi.itemid, c.finalrategst from  soorderplaced op\r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n ) p on p.ponoid=b.ponoid\r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid= b.itemid and m.itemid= b.itemid and m.qctest= 'Y'\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID= ic.MCID\r\n\r\n\r\n Where T.Status = 'C'\r\n And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate " + whclause1 + "\r\n and (case when (b.qastatus = 0 or b.qastatus = 3) and m.qctest='Y' then 1 else 0 end)=1\r\nand((nvl(ABSRQTY,0))-(nvl(ALLOTQTY,0)))>0\r\ngroup by b.batchno,mc.mcategory,mc.mcid,b.qastatus,b.ABSRQTY,b.ALLOTQTY,b.ponoid,p.finalrategst,b.itemid,m.qctest\r\n )  b\r\n  inner join masfymonth md on md.monthid=cast(b.MM as int)\r\n group by mcategory,MCID,MM, md.MONTHNAME,md.MNAME,monthid order by MM ";
        //       List<QCMonthWisePendingRecDTO> myList = this._context.QCMonthWisePendingRecDbSet.FromSqlInterpolated<QCMonthWisePendingRecDTO>(FormattableStringFactory.Create(qry)).ToList<QCMonthWisePendingRecDTO>();
        //       ActionResult<IEnumerable<QCMonthWisePendingRecDTO>> actionResult = (ActionResult<IEnumerable<QCMonthWisePendingRecDTO>>)myList;
        //       whclause1 = (string)null;
        //       qry = (string)null;
        //       myList = (List<QCMonthWisePendingRecDTO>)null;
        //       return actionResult;
        //   }

        [HttpGet("HoldItemDetails")]
        public async Task<ActionResult<IEnumerable<QCHoldItemDetails>>> HoldItemDetails(string mcid)
        {
            string whclause1 = "";
            if (mcid != "0")
                whclause1 = " and mc.mcid =" + mcid;
            string qry = "  select to_char(ROW_NUMBER() OVER (ORDER BY itemid,ponoid,batchno)) as ID, itemid,mcategory,itemcode,itemname,strength1,unit,pono,soissuedate,suppliername,batchno,finalrategst,sum(rqty) as rqty,sum(allotqty) as allotqty,sum(STK) as STK,round(sum(Stkvalue)/100000,2) as Stkvaluelacs\r\n\r\n,max(HOLDREASON) as HOLDREASON,max(HOLDDATE) as  HOLDDATE,ponoid\r\nfrom \r\n(\r\nselect b.itemid,mc.mcategory,m.itemcode,m.itemname,m.strength1,m.unit,pono,soissuedate,suppliername\r\n,b.batchno,\r\np.finalrategst\r\n,nvl(ABSRQTY,0) as rqty,nvl(ALLOTQTY,0) as allotqty,\r\nnvl(ABSRQTY,0)-nvl(ALLOTQTY,0) as STK,round((nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))*p.finalrategst,0) as Stkvalue\r\n,c.HOLDREASON,c.HOLDDATE,b.qastatus,b.whissueblock,b.ponoid\r\nfrom tbreceiptbatches b\r\ninner join \r\n(\r\nselect op.ponoid,oi.itemid,c.finalrategst,op.pono,op.soissuedate,sp.suppliername from  soorderplaced op \r\n inner join soOrderedItems OI on (OI.ponoid = op.ponoid)\r\n inner join aoccontractitems c on c.contractitemid = oi.contractitemid\r\n inner join massuppliers  sp on sp.SUPPLIERID=op.SUPPLIERID\r\n ) p on p.ponoid=b.ponoid \r\n left outer join \r\n(\r\nselect max(sampleid) sampleid ,BATCHNO,ITEMID,PONOID from qcsamples\r\ngroup by BATCHNO,ITEMID,PONOID\r\n) hld on hld.BATCHNO=b.BATCHNO and hld.ITEMID=b.itemid and hld.PONOID=p.ponoid\r\nleft outer join qcsamples c on c.sampleid=hld.sampleid\r\n join tbreceiptitems i on b.receiptitemid = i.receiptitemid\r\n inner  join tbreceipts t on t.receiptid = i.receiptid\r\n inner join masitems m on m.itemid=b.itemid and m.itemid=b.itemid\r\n inner join masitemcategories ic on ic.categoryid = m.categoryid\r\n inner join masitemmaincategory mc on mc.MCID=ic.MCID\r\n Where T.Status = 'C' " + whclause1 + " and (nvl(ABSRQTY,0)-nvl(ALLOTQTY,0))>0\r\n  And b.qastatus  in (0) and  b.whissueblock=1 \r\n ) group by  itemid,mcategory,itemcode,itemname,strength1,unit,pono,soissuedate,suppliername,batchno,finalrategst,ponoid\r\n order by soissuedate desc ";
            List<QCHoldItemDetails> myList = this._context.QCHoldItemDetailsDbSet.FromSqlInterpolated<QCHoldItemDetails>(FormattableStringFactory.Create(qry)).ToList<QCHoldItemDetails>();
            ActionResult<IEnumerable<QCHoldItemDetails>> actionResult = (ActionResult<IEnumerable<QCHoldItemDetails>>)myList;
            whclause1 = (string)null;
            qry = (string)null;
            myList = (List<QCHoldItemDetails>)null;
            return actionResult;
        }
    }
}


