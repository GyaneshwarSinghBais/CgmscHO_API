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
using CgmscHO_API.DistrictDTO;
using CgmscHO_API.DistrictDTOs;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DistrictController : ControllerBase
    {
        private readonly OraDbContext _context;
        public DistrictController(OraDbContext context)
        {
            _context = context;
        }

        [HttpGet("DistFACwiseStockPostionNew")]
        public async Task<ActionResult<IEnumerable<DistFACwiseStockPostionDTO>>> DistFACwiseStockPostionNew(string disid, string coll_cmho, string mcatid, string EDLNedl, string mitemid, string userid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Coll")
                    {
                        districtid = f.getCollectorDisid(userid);
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                    }

                }
            }
            else
            {
                districtid = disid;
            }

            string cmhoid = f.getCMHOFacFromDistrict(districtid);
            Int64 whid = f.getWHID(cmhoid.ToString());

            string whmcid = ""; string whmcid1 = "";
            if (mcatid != "0")
            {

                whmcid = " and MCID =" + mcatid;
                whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whedlnedl = "";
            string whedledl1 = "";

            string qry = "";



            if (EDLNedl == "Y")
            {
                //EDL

                qry = @"select to_char(ROW_NUMBER() OVER (ORDER BY orderdp))|| '.' || facilityname as facilityname,count(distinct itemid) as nositems,count(distinct itemid) - sum(facstkcnt) as STOCKOUTNOS,
sum(facstkcnt) as facstkcnt,Round(((count(distinct itemid) - (sum(facstkcnt))) / count(distinct itemid)) * 100, 0) as stockoutP
,sum(WHIssuePending_L180cnt) as RecPendingatFAcilily,
sum(WHStkcnt) as WHStkcnt,
0 as cmhostkcnt,
0 as WHUQCStkcnt,
0 as Indent_toWH_Pending,0 as WHIssue_Rec_Pending_L180cnt,0 as BalIFT6Month
,0 as LP_Pipeline180cnt
,0 as NocTaken_No_LPO,
facilityid,orderdp,facilitytypeid
from
(

select f.facilityname, f.itemcode, nvl(inhand_qty, 0) as facstock
, nvl(READYFORISSUE, 0) whReady,
nvl(whIssueP.intransit, 0) as WHIssuePending_L180
, sttusEDL, orderdp, f.facilityid, f.districtid, isedl2021, mcid, f.itemid
,case when nvl(inhand_qty, 0) > 0 then 1 else 0 end as facstkcnt
,case when nvl(inhand_qty, 0) = 0 and nvl(whIssueP.intransit,0)= 0 and nvl(READYFORISSUE,0)> 0 then 1 else 0 end as WHStkcnt
,case when nvl(inhand_qty, 0) = 0 and nvl(whIssueP.intransit,0)> 0 then 1 else 0 end WHIssuePending_L180cnt
, ISDEDL, ISRFORLP, isfreez_itpr, facilitytypeid, f.isactive
from
(
select f.facilityname
, itemcode, m.edlcat, ft.eldcat, m.itemid, m.categoryid
,case when m.edlcat <= ft.eldcat then 1 else 0 end sttusEDL, f.districtid,f.facilityid,ft.facilitytypeid,ft.orderdp,isedl2021,c.mcid,m.ISDEDL,m.ISRFORLP,m.isfreez_itpr,f.isactive
from masitems m, masfacilities f,masitemcategories c,
masfacilitytypes ft where 1 = 1  and f.facilitytypeid not in (377, 353, 382,371)
and ft.hodid = 2 and f.isactive = 1 and f.facilitytypeid = ft.facilitytypeid and c.categoryid = m.categoryid
and m.edlcat <= ft.eldcat and m.isfreez_itpr is null and c.mcid = " + mcatid + @"  and m.isedl2021 = 'Y'
and f.districtid = " + districtid + @"
) f



left outer join
(
  select i.itemid, t.warehouseid, sum(nvl(b.absrqty,0))-sum(nvl(b.ALLOTQTY, 0)) as READYFORISSUE
                 from tbreceiptbatches b
                  inner             join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner
                 join tbreceipts t on t.receiptid = i.receiptid
             
                  where 1 = 1  and T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                  and t.warehouseid = " + whid + @"
                  group by i.itemid,t.warehouseid
                   having(sum(nvl(b.absrqty, 0)) - sum(nvl(b.ALLOTQTY, 0))) > 0
 ) wh on wh.itemid = f.itemid

     left outer join
    (
select m.itemid, m.edlitemcode as edlitemcode, sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0)) as inhand_qty, f.facilityid,
 case when mi.edlcat <= t.eldcat and(sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0))) > 0 then 1 else 0 end stkavil
from tbfacilityreceiptbatches b
inner
join tbfacilityreceiptitems i on i.facreceiptitemid = b.facreceiptitemid
inner
join tbfacilityreceipts t on t.facreceiptid = i.facreceiptid
inner
join vmasitems m on m.itemid = i.itemid  and m.ISEDL2021 = 'Y'
inner join masitems mi on mi.itemcode = m.edlitemcode  and m.edlitemcode is not null
inner join masitemcategories c on c.categoryid = mi.categoryid and c.mcid = " + mcatid + @"
inner join masfacilities f  on f.facilityid = t.facilityid and f.districtid = " + districtid + @"
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
  where t.status = 'C'   and b.expdate > sysdate
group by m.itemid,f.facilityid,t.eldcat ,mi.edlcat,m.edlitemcode
having(sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0))) > 0
    )st on st.facilityid = f.facilityid and st.edlitemcode = f.itemcode



     left outer join
     (

select i.itemid, i.facilityid,
sum(i.issue) whissue, sum(nvl(r.rcpts, 0)) receipts,
sum(i.issue) - sum(nvl(r.rcpts, 0)) intransit
from
(
select t.indentitemid, t.itemid, m.itemcode, tb.warehouseid, tb.facilityid, sum(tbo.issueqty * m.unitcount) issue, f.districtid, ft.facilitytypeid, ft.hodid from tbindentitems t
left outer join tbindents tb on tb.indentid = t.indentid
left outer join tboutwards tbo on tbo.indentitemid = t.indentitemid
left outer join masitems m on m.itemid = t.itemid
inner join masfacilities f  on f.facilityid = tb.facilityid and f.districtid = " + districtid + @"
inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
where tbo.status = 'C' and tb.issuedate >= sysdate - 30  and tb.facilityid is not null and m.isedl2021 = 'Y'
group by t.indentitemid, t.itemid, m.itemcode, tb.warehouseid, tb.facilityid, f.districtid, ft.facilitytypeid, ft.hodid
) i
left outer join
(
select tbr.indentitemid, tbb.itemid, tbf.warehouseid, tbf.facilityid, sum(tbb.absrqty) rcpts from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid = tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid = tbr.facreceiptid
where tbf.status = 'C' and tbf.warehouseid is not null and tbf.facreceiptdate >= sysdate - 30
group by tbr.indentitemid, tbb.itemid, tbf.warehouseid, tbf.facilityid
) r
on r.indentitemid = i.indentitemid
where 1 = 1
group by i.itemid, i.facilityid


having sum(i.issue) - sum(nvl(r.rcpts, 0)) > 0
     ) whIssueP on whIssueP.facilityid = f.facilityid and whIssueP.itemid = f.itemid

 where 1 = 1    and f.isedl2021 = 'Y' and f.isactive = '1' and f.isfreez_itpr is null
 and MCID = " + mcatid + @"
 ) group by facilityname,facilityid,orderdp,facilitytypeid
 order by orderdp ";

            }
            else
            {
                qry = @"select to_char(ROW_NUMBER() OVER (ORDER BY orderdp))|| '.' || facilityname as facilityname,count(distinct itemid) as nositems,count(distinct itemid) - sum(facstkcnt) as STOCKOUTNOS,
sum(facstkcnt) as facstkcnt,Round(((count(distinct itemid) - (sum(facstkcnt))) / count(distinct itemid)) * 100, 0) as stockoutP
,sum(WHIssuePending_L180cnt) as RecPendingatFAcilily,
sum(WHStkcnt) as WHStkcnt,0 as cmhostkcnt,0 as WHUQCStkcnt
,0 as Indent_toWH_Pending,0 as WHIssue_Rec_Pending_L180cnt,0 as BalIFT6Month
,0 as LP_Pipeline180cnt
,0 as NocTaken_No_LPO,
facilityid,orderdp,facilitytypeid
from
(

select f.facilityname, f.itemcode, nvl(inhand_qty, 0) as facstock
, nvl(READYFORISSUE, 0) whReady,
nvl(whIssueP.intransit, 0) as WHIssuePending_L180
, sttusEDL, orderdp, f.facilityid, f.districtid, isedl2021, mcid, f.itemid
,case when nvl(inhand_qty, 0) > 0 then 1 else 0 end as facstkcnt
,case when nvl(inhand_qty, 0) = 0 and nvl(whIssueP.intransit,0)= 0 and nvl(READYFORISSUE,0)> 0 then 1 else 0 end as WHStkcnt
,case when nvl(inhand_qty, 0) = 0 and nvl(whIssueP.intransit,0)> 0 then 1 else 0 end WHIssuePending_L180cnt
, ISDEDL, ISRFORLP, isfreez_itpr, facilitytypeid, f.isactive
from
(
select f.districtid, f.facilityid, ft.facilitytypeid, ft.orderdp, f.facilityname
, m.itemcode, m.edlcat, ft.eldcat, m.itemid, m.categoryid
,case when m.edlcat <= ft.eldcat then 1 else 0 end sttusEDL, isedl2021, c.mcid,m.ISDEDL,m.ISRFORLP,m.isfreez_itpr,f.isactive from masanualindent a
inner join anualindent ai on ai.indentid = a.indentid
inner join masfacilities f on f.facilityid = a.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
inner join masitems m on m.itemid = ai.itemid
inner join masitemcategories c on c.categoryid = m.categoryid
and ft.hodid = 2 and f.isactive = 1 and f.facilitytypeid not in (377, 353, 382,371)
where a.status = 'C' and a.accyrsetid in (select accyrsetid from masaccyearsettings where sysdate between startdate and enddate  )
and f.districtid = " + districtid + @"   and c.mcid = " + mcatid + @"
and m.edlcat <= ft.eldcat and m.isfreez_itpr is null and nvl(isedl2021, 'N') = 'N'
 and nvl(ai.cmhodistqty,0)> 0
) f



left outer join
(
  select i.itemid, t.warehouseid, sum(nvl(b.absrqty,0))-sum(nvl(b.ALLOTQTY, 0)) as READYFORISSUE
                 from tbreceiptbatches b
                  inner
                 join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner
                 join tbreceipts t on t.receiptid = i.receiptid
                  where 1 = 1  and T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                  and t.warehouseid =  " + whid + @"
                  group by i.itemid,t.warehouseid
                   having(sum(nvl(b.absrqty, 0)) - sum(nvl(b.ALLOTQTY, 0))) > 0
 ) wh on wh.itemid = f.itemid

     left outer join
    (
select m.itemid, m.edlitemcode as edlitemcode, sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0)) as inhand_qty, f.facilityid,
 case when mi.edlcat <= t.eldcat and(sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0))) > 0 then 1 else 0 end stkavil
from tbfacilityreceiptbatches b
inner
join tbfacilityreceiptitems i on i.facreceiptitemid = b.facreceiptitemid
inner
join tbfacilityreceipts t on t.facreceiptid = i.facreceiptid
inner
join vmasitems m on m.itemid = i.itemid  and nvl(m.ISEDL2021, 'N') = 'N'
inner join masitems mi on mi.itemcode = m.edlitemcode  and m.edlitemcode is not null
inner join masitemcategories c on c.categoryid = mi.categoryid and c.mcid = " + mcatid + @"
inner join masfacilities f  on f.facilityid = t.facilityid and f.districtid = " + districtid + @"
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
  where t.status = 'C'   and b.expdate > sysdate
group by m.itemid,f.facilityid,t.eldcat ,mi.edlcat,m.edlitemcode
having(sum(b.ABSRQTY) - sum(nvl(b.ALLOTQTY, 0))) > 0
    )st on st.facilityid = f.facilityid and st.edlitemcode = f.itemcode



     left outer join
     (

select i.itemid, i.facilityid,
sum(i.issue) whissue, sum(nvl(r.rcpts, 0)) receipts,
sum(i.issue) - sum(nvl(r.rcpts, 0)) intransit
from
(
select t.indentitemid, t.itemid, m.itemcode, tb.warehouseid, tb.facilityid, sum(tbo.issueqty * m.unitcount) issue, f.districtid, ft.facilitytypeid, ft.hodid from tbindentitems t
left outer join tbindents tb on tb.indentid = t.indentid
left outer join tboutwards tbo on tbo.indentitemid = t.indentitemid
left outer join masitems m on m.itemid = t.itemid
inner join masfacilities f  on f.facilityid = tb.facilityid and f.districtid = " + districtid + @"
inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
where tbo.status = 'C' and tb.issuedate >= sysdate - 30  and tb.facilityid is not null and nvl(m.isedl2021, 'N') = 'N'
group by t.indentitemid, t.itemid, m.itemcode, tb.warehouseid, tb.facilityid, f.districtid, ft.facilitytypeid, ft.hodid
) i
left outer join
(
select tbr.indentitemid, tbb.itemid, tbf.warehouseid, tbf.facilityid, sum(tbb.absrqty) rcpts from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid = tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid = tbr.facreceiptid
where tbf.status = 'C' and tbf.warehouseid is not null and tbf.facreceiptdate >= sysdate - 30
group by tbr.indentitemid, tbb.itemid, tbf.warehouseid, tbf.facilityid
) r
on r.indentitemid = i.indentitemid
where 1 = 1
group by i.itemid, i.facilityid


having sum(i.issue) - sum(nvl(r.rcpts, 0)) > 0
     ) whIssueP on whIssueP.facilityid = f.facilityid and whIssueP.itemid = f.itemid

 where 1 = 1    and nvl(f.isedl2021,'N')= 'N' and f.isactive = '1' and f.isfreez_itpr is null
 and MCID = " + mcatid + @" and f.FACILITYTYPEID not in (377, 353, 382,371)
 ) group by facilityname,facilityid,orderdp,facilitytypeid
 order by orderdp ";
            }

            var myList = _context.DistFACwiseStockPostionDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }


        [HttpGet("DistFACwiseStockPostion")]
        public async Task<ActionResult<IEnumerable<DistFACwiseStockPostionDTO>>> DistFACwiseStockPostion(string disid,string coll_cmho,string mcatid, string EDLNedl, string mitemid, string userid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Coll")
                    {
                        districtid = f.getCollectorDisid(userid);
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                    }

                }
            }
            else
            {
                districtid = disid;
            }

            string cmhoid = f.getCMHOFacFromDistrict(districtid);
            Int64 whid  = f.getWHID(cmhoid.ToString());
        
            string whmcid = ""; string whmcid1 = "";
            if (mcatid != "0")
            {

                whmcid = " and MCID =" + mcatid;
                whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whedlnedl = "";
            string whedledl1 = "";
            string nedlquer = "";
            if (EDLNedl == "Y")
            {
               
                whedlnedl = "  and f.isedl2021='Y'";
                whedledl1= " and m.isedl2021 = 'Y'";
            }
            else if (EDLNedl == "N")
            {
                whedlnedl = " and (case when isedl2021= 'Y' then 'EDL' else 'Non EDL' end )='Non EDL' ";
                whedledl1 = " and (case when m.isedl2021= 'Y' then 'EDL' else 'Non EDL' end )='Non EDL' ";
                nedlquer= @" inner join
                (
            select a.itemid, sum(nvl(CMHODISTQTY,0)) aiqty,a.FACILITYID
            from anualindent a
            inner join masfacilities f  on f.facilityid = a.facilityid   and f.districtid =" + districtid + @"
             inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
            where a.status = 'C' and f.isactive = 1 and a.accyrsetid = " + yearid + @" and t.hodid = 2 and f.districtid = " + districtid + @"
            group by a.itemid,a.FACILITYID having sum(nvl(CMHODISTQTY, 0)) > 0
            ) ai on ai.itemid = f.itemid   and ai.facilityid = f.facilityid ";

            }
            else
            {
                whedlnedl = ""; 
            }


            string qry = "";
        

                 qry = @" select to_char(ROW_NUMBER() OVER ( ORDER BY orderdp ))||'.'|| facilityname as facilityname,count(distinct itemid) as nositems,count(distinct itemid) - sum(facstkcnt) as STOCKOUTNOS,
sum(facstkcnt) as facstkcnt,Round(((count(distinct itemid)-(sum(facstkcnt)))/count(distinct itemid))*100,0) as stockoutP
,sum(WHIssuePending_L180cnt) as RecPendingatFAcilily,
sum(WHStkcnt) as WHStkcnt,
sum(cmhostkcnt) as cmhostkcnt,
sum(WHUQCStkcnt) as WHUQCStkcnt
,sum(IndentQTYPendingcnt) as Indent_toWH_Pending,sum(WHIssuePending_L180cnt) as WHIssue_Rec_Pending_L180cnt,sum(BalIFT6Month) as BalIFT6Month
,sum(BalLP_L180cnt) as LP_Pipeline180cnt
,sum(BalNOCcnt) as NocTaken_No_LPO,
facilityid,orderdp,facilitytypeid
from 
(

select f.facilityname,f.itemcode,nvl(inhand_qty,0) as facstock,nvl(cmhostk,0) as cmhostk,nvl(READYFORISSUE,0) whReady,
nvl(PENDING,0) whpending,nvl(IndenttoWH,0) IndentQTYPending ,
nvl(whIssueP.intransit,0) as WHIssuePending_L180, 
nvl(IFTTransit.BalIFT,0) as BalIFT_L180,
nvl(lpTransit.BalLP,0) as BalLP_L180 
,nvl(noc.BalNOCAfterLPO,0) as BalNOCAfterLPO, 
sttusEDL,orderdp,f.facilityid,f.districtid,isedl2021,mcid,f.itemid 
,case when nvl(inhand_qty,0)>0 then 1 else 0 end as facstkcnt
,case when nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)=0 and nvl(cmhostk,0)>0 then 1 else 0 end as cmhostkcnt
,case when nvl(inhand_qty,0)=0 and nvl(cmhostk,0)=0 and nvl(whIssueP.intransit,0)=0 and nvl(READYFORISSUE,0)>0 then 1 else 0 end as WHStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)>0 then 1 else 0 end as WHUQCStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)>0 then 1 else 0 end WHIssuePending_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(IndenttoWH,0)>0 and nvl(READYFORISSUE,0)>0 then 1 else 0 end IndentQTYPendingcnt
,case when nvl(inhand_qty,0)=0 and nvl(IFTTransit.BalIFT,0)>0 then 1 else 0 end BalIFT6Month
,case when nvl(inhand_qty,0)=0 and nvl(lpTransit.BalLP,0)>0 then 1 else 0 end BalLP_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(noc.BalNOCAfterLPO,0)>0 then 1 else 0 end BalNOCcnt 
,ISDEDL,ISRFORLP,isfreez_itpr,facilitytypeid,f.isactive
from 
(
select f.facilityname
,itemcode,m.edlcat,ft.eldcat,m.itemid,m.categoryid
,case when m.edlcat<=ft.eldcat then 1 else 0 end sttusEDL,f.districtid,f.facilityid,ft.facilitytypeid,ft.orderdp,isedl2021,c.mcid,m.ISDEDL,m.ISRFORLP,m.isfreez_itpr,f.isactive
from masitems m, masfacilities f,masitemcategories c,
masfacilitytypes ft where 1=1 "+ whedledl1 + @"
and ft.hodid=2 and f.isactive=1 and  f.facilitytypeid=ft.facilitytypeid and c.categoryid = m.categoryid
and m.edlcat<=ft.eldcat and  m.isfreez_itpr is  null 
and  m.ISDEDL is null and m.ISRFORLP is null
and f.districtid=" + districtid + @" and ft.facilitytypeid not in (353)
) f

"+nedlquer+@"

left outer join 
(
select  (case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue)*A.unitcount else 0 end) as READYFORISSUE,
(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end)*A.unitcount PENDING
,A.itemid ,A.warehouseid,A.unitcount
                  from
                 (
                 select t.warehouseid,  b.inwno, 
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when m.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when m.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
,m.itemid,nvl(m.unitcount,1) as unitcount

                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                  inner join masitems m on m.itemid = i.itemid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null and t.warehouseid="+ whid + @"
                 " + whedledl1 + @"
                 ) A group by warehouseid,A.itemid,unitcount
                 having (sum(nvl(A.ReadyForIssue,0))+sum(nvl(A.Pending,0)))>0
 ) wh on wh.itemid=f.itemid

     left outer join 
    (
   select m.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty ,f.facilityid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = "+ districtid + @"
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and t.hodid=2 and f.isactive=1 
                 and T.Status = 'C' and b.expdate>sysdate  and m.edlitemcode is not null
               group by f.facilityid,m.edlitemcode 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
    )st on st.facilityid=f.facilityid and st.edlitemcode= f.itemcode

         left outer join 
     (
     select itemid,sum(IndentQTY) IndenttoWH ,facilityid
from (

select mn.nocid,mni.itemid,m.itemcode,mni.bookedqty*nvl(m.unitcount,1) as IndentQTY
,mn.facilityid  
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid=mn.nocid
inner join masitems m on m.itemid=mni.itemid
inner join masfacilities f on f.facilityid=mn.facilityid and f.districtid = "+ districtid + @"
left outer join 
(
select t.indentitemid,tb.facilityid, sum(tbo.issueqty) issueQTYSKU,tb.nocid,t.itemid from tbindentitems t
left outer join tbindents tb on tb.indentid=t.indentid
left outer join tboutwards tbo on tbo.indentitemid=t.indentitemid
where tbo.status='C' and tb.issuetype='NO' and tb.facilityid is not null 
 group by t.indentitemid,tb.facilityid,tb.nocid,t.itemid
) WHI on WHI.facilityid=mn.facilityid  and whi.nocid=mn.nocid and whi.itemid=mni.itemid
where 1=1 and   mn.nocdate>=sysdate-15
and mn.status='C' and mni.status='N' and mni.approvedqty=0
and nvl(WHI.issueQTYSKU,0)=0  " + whedledl1 + @"
) group by itemid,facilityid
     ) WHIndentP on  WHIndentP.facilityid=f.facilityid and  WHIndentP.itemid=f.itemid


         left outer join 
         (
         select tofacilityid,edlitemcode,sum(BalIFT) as BalIFT from 
(
select  fs.facilityid,fromfacid,fsi.itemid,sum(nvl(ftbo.issueqty,0)) issueqty,nvl(rcpts,0) as ReceiptQTY,fs.issueid,fs.tofacilityid,recFacilityid,vm.edlitemcode,sum(nvl(ftbo.issueqty,0))-nvl(rcpts,0) as BalIFT  from tbfacilityissues fs 
inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
inner join vmasitems vm on vm.itemid=fsi.itemid
inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
left outer join 
(

select tbb.itemid,sum(tbb.absrqty) rcpts,tbf.facilityid as recFacilityid,tbf.tofacilityid as fromfacid,tbf.issueid from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where tbf.status='C'  and tbf.facreceipttype='SP' and tbf.issueid is not null
group by tbb.itemid,tbf.warehouseid,tbf.facilityid,tbf.tofacilityid,tbf.issueid
) IFR on IFR.issueid=fs.issueid and IFR.itemid=fsi.itemid
where fs.status = 'C' and fs.issueid is not null  and fs.issuetype='SP'       
and fs.issuedate>=sysdate-180  and recFacilityid is  null and vm.edlitemcode is not null
group by fsi.itemid,fs.facilityid ,fs.issueid,fs.tofacilityid ,fromfacid,recFacilityid,rcpts,vm.edlitemcode 
) group by tofacilityid,edlitemcode
         ) IFTTransit on IFTTransit.tofacilityid=f.facilityid and  IFTTransit.edlitemcode=f.itemcode

           left outer join 
     (
     select edlitemcode,sum(LpbalToReceipt) as BalLP,psaid from 
(
select  vm.itemcode,vm.edlitemcode,lp.ponoid,lpi.lpitemid,lpi.absqty,nvl(rcpts,0) as LpReceipt,lp.psaid,lpi.absqty-nvl(rcpts,0) as LpbalToReceipt from lpsoorderplaced lp
inner join lpsoordereditems lpi on lpi.ponoid=lp.ponoid
inner join vmasitems vm on vm.itemid=lpi.lpitemid
left outer join 
(
select tbb.itemid,tbf.facilityid,sum(tbb.absrqty) rcpts,tbf.ponoid from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where tbf.status='C'  and tbf.facreceipttype='NO'
and tbf.ponoid is not null
group by tbb.itemid,tbf.warehouseid,tbf.facilityid,tbf.ponoid
) lpr on lpr.ponoid=lp.ponoid and lpr.itemid=lpi.lpitemid and lpr.facilityid=lp.psaid
where lp.status='O' and lp.podate>=sysdate-180 and vm.edlitemcode is not null
and lpi.absqty-nvl(rcpts,0)>0
) group by edlitemcode,psaid
     ) lpTransit on lpTransit.psaid=f.facilityid and  lpTransit.edlitemcode=f.itemcode


     left outer join 
     (

select i.itemid,i.facilityid,
sum(i.issue) whissue,sum(nvl(r.rcpts,0)) receipts,
sum(i.issue)-sum(nvl(r.rcpts,0)) intransit
from
(
select t.indentitemid, t.itemid,m.itemcode, tb.warehouseid, tb.facilityid, sum(tbo.issueqty*m.unitcount) issue,f.districtid,ft.facilitytypeid,ft.hodid from tbindentitems t
left outer join tbindents tb on tb.indentid=t.indentid
left outer join tboutwards tbo on tbo.indentitemid=t.indentitemid
left outer join masitems m on m.itemid=t.itemid
inner join masfacilities f  on f.facilityid=tb.facilityid  
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
where tbo.status='C' and tb.issuedate>=sysdate-180  and tb.facilityid is not null "+ whedledl1 + @"
group by t.indentitemid, t.itemid,m.itemcode, tb.warehouseid, tb.facilityid,f.districtid,ft.facilitytypeid,ft.hodid
) i
left outer join
(
select tbr.indentitemid,tbb.itemid,tbf.warehouseid,tbf.facilityid,sum(tbb.absrqty) rcpts from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where tbf.status='C' and tbf.warehouseid is not null and tbf.facreceiptdate>=sysdate-180  
group by tbr.indentitemid,tbb.itemid,tbf.warehouseid,tbf.facilityid
) r
on r.indentitemid=i.indentitemid
where 1=1 
group by i.itemid,i.facilityid
 
having sum(i.issue)-sum(nvl(r.rcpts,0))>0
     ) whIssueP on whIssueP.facilityid=f.facilityid and  whIssueP.itemid=f.itemid

            left outer join 
            (
            select mni.itemid,sum(mni.approvedqty*nvl(m.unitcount,1)) as NOCQTY,sum((mni.approvedqty*nvl(m.unitcount,1))-nvl(absqty,0)) as BalNOCAfterLPO
,mn.facilityid  
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid=mn.nocid
inner join masitems m on m.itemid=mni.itemid
inner join masfacilities f on f.facilityid=mn.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
left outer join 
(
select lpi.nocid,lpi.absqty,vm.edlitemcode  from lpsoorderplaced lp
inner join lpsoordereditems lpi on lpi.ponoid=lp.ponoid
inner join vmasitems vm on vm.itemid=lpi.lpitemid
) lpo on lpo.nocid=mn.nocid and lpo.edlitemcode=m.itemcode
where 1=1 and   mn.nocdate>=sysdate-15 and f.districtid=" + districtid + @"
and mn.status='C' and mni.status='Y' and mni.approvedqty>0
and ((mni.approvedqty*nvl(m.unitcount,1))-nvl(absqty,0))>0  " + whedledl1 + @"
group by mni.itemid,mn.facilityid 
            ) noc on noc.itemid=f.itemid and noc.facilityid=f.facilityid


                    left outer join 
    (
  select m.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   cmhostk ,f.facilityid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = "+ districtid + @"
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and t.hodid=2 and f.isactive=1 
                 and T.Status = 'C' and b.expdate>sysdate  and m.edlitemcode is not null and f.facilityid="+ cmhoid + @" 
               group by f.facilityid,m.edlitemcode
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0   
    )stcmho on stcmho.edlitemcode= f.itemcode


 where 1=1  " + whedlnedl + @" and f.isactive='1' and f.ISDEDL is null and f.ISRFORLP is null and f.isfreez_itpr is null 
"+ whmcid + @" and f.FACILITYTYPEID not in (377,353,382)
 ) group by facilityname,facilityid,orderdp,facilitytypeid
 order by orderdp ";
            
            
           
            var myList = _context.DistFACwiseStockPostionDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
     

        [HttpGet("FACwiseStockPostion")]
        public async Task<ActionResult<IEnumerable<FACwiseStockPostionDTO>>> FACwiseStockPostion(string ftype,string facid,string mcatid, string EDLNedl, string mitemid, string userid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
          
            districtid = Convert.ToString(f.geFACTDistrictid(facid));
            string cmhoid = f.getCMHOFacFromDistrict(districtid);
            Int64 whid = f.getWHID(cmhoid.ToString());

            string whmcid = ""; string whmcid1 = "";
            if (mcatid != "0")
            {

                whmcid = " and f.MCID =" + mcatid;
                whmcid1 = " and mc.MCID =" + mcatid;
            }
            string whedlnedl = "";
            string whedledl1 = "";
            string nedlquer = "";
            string whnonedldata = "";
            if (EDLNedl == "Y")
            {

                whedlnedl = "  and f.isedl2021='Y'";
                whedledl1 = " and m.isedl2021 = 'Y'";
            }
            else if (EDLNedl == "N")
            {
                whedlnedl = " and (case when f.isedl2021= 'Y' then 'EDL' else 'Non EDL' end )='Non EDL' ";
                whedledl1 = " and (case when m.isedl2021= 'Y' then 'EDL' else 'Non EDL' end )='Non EDL' ";

                whnonedldata = " and nvl(aiqty,0)>0 ";
            }
            else
            {
          
            }
            string whftype = " nvl(aiqty,0)>0 ";
            if (ftype == "1")
            {
                // stock out and ready in wh
                whftype = " and nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)>0 ";
            }
            if (ftype == "2")
            {
                // stock out and issued by WH Not received
                whftype = " and nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)>0 ";
            }
            if (ftype == "3")
            {
                // stock out total in health facility
                whftype = " and nvl(inhand_qty,0)=0 ";
            }
            if (ftype == "4")
            {
                // stock availble in health facility 
                whftype = " and nvl(inhand_qty,0)>0  ";
            }


            string qry = "";


            qry = @" select f.facilityname,ty.itemtypename,g.groupname,f.itemcode,mia.itemname,mia.strength1,ed.edl,nvl(inhand_qty,0) as facstock,nvl(cmhostk,0) as cmhostk,nvl(READYFORISSUE,0) whReady,
nvl(PENDING,0) whpending,nvl(IndenttoWH,0) IndentQTYPending ,
nvl(whIssueP.intransit,0) as WHIssuePending_L180, 
nvl(IFTTransit.BalIFT,0) as BalIFT_L180,
nvl(lpTransit.BalLP,0) as BalLP_L180 
,nvl(noc.BalNOCAfterLPO,0) as BalNOCAfterLPO, 
sttusEDL,orderdp,f.facilityid,f.districtid,f.isedl2021,mcid,f.itemid 
,case when nvl(inhand_qty,0)>0 then 1 else 0 end as facstkcnt
,case when nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)=0 and nvl(cmhostk,0)>0 then 1 else 0 end as cmhostkcnt
,case when nvl(inhand_qty,0)=0 and nvl(cmhostk,0)=0 and nvl(whIssueP.intransit,0)=0 and nvl(READYFORISSUE,0)>0 then 1 else 0 end as WHStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)>0 then 1 else 0 end as WHUQCStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)>0 then 1 else 0 end WHIssuePending_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(IndenttoWH,0)>0 and nvl(READYFORISSUE,0)>0 then 1 else 0 end IndentQTYPendingcnt
,case when nvl(inhand_qty,0)=0 and nvl(IFTTransit.BalIFT,0)>0 then 1 else 0 end BalIFT6Month
,case when nvl(inhand_qty,0)=0 and nvl(lpTransit.BalLP,0)>0 then 1 else 0 end BalLP_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(noc.BalNOCAfterLPO,0)>0 then 1 else 0 end BalNOCcnt 
,f.ISDEDL,f.ISRFORLP,f.isfreez_itpr,facilitytypeid,f.isactive
,nvl(aiqty,0) as aiqty
from 
(
select f.facilityname
,m.itemcode,m.edlcat,ft.eldcat,m.itemid,m.categoryid
,case when m.edlcat<=ft.eldcat then 1 else 0 end sttusEDL,f.districtid,f.facilityid,ft.facilitytypeid,ft.orderdp,isedl2021,c.mcid,m.ISDEDL,m.ISRFORLP,m.isfreez_itpr,f.isactive
from masitems m, masfacilities f,masitemcategories c,
masfacilitytypes ft where 1=1  "+ whedledl1 + @"
and ft.hodid=2 and f.isactive=1 and  f.facilitytypeid=ft.facilitytypeid and c.categoryid = m.categoryid
and m.edlcat<=ft.eldcat and  m.isfreez_itpr is  null 
and  m.ISDEDL is null and m.ISRFORLP is null and ft.facilitytypeid not in (353) and f.facilityid=" + facid + @"
) f
 inner join masitems mia on mia.itemid=f.itemid            
     left outer join masedl ed on ed.edlcat=mia.edlcat 
     left outer join masitemgroups g on g.groupid=mia.groupid  
     left outer join  masitemtypes ty on ty.itemtypeid=mia.itemtypeid

left outer join
                (
            select a.itemid, sum(nvl(CMHODISTQTY,0)) aiqty
            from anualindent a
            inner join masfacilities f  on f.facilityid = a.facilityid   and f.facilityid =" + facid + @"
             inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
            where a.status = 'C' and f.isactive = 1 and a.accyrsetid = " + yearid + @"  and f.facilityid =" + facid + @"
            group by a.itemid having sum(nvl(CMHODISTQTY, 0)) > 0
            ) ai on ai.itemid = f.itemid and ai.itemid= mia.itemid 

                left outer join 
(
select  (case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue)*A.unitcount else 0 end) as READYFORISSUE,
(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end)*A.unitcount PENDING
,A.itemid ,A.warehouseid,A.unitcount
                  from
                 (
                 select t.warehouseid,  b.inwno, 
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when m.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when m.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
,m.itemid,nvl(m.unitcount,1) as unitcount

                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                  inner join masitems m on m.itemid = i.itemid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null and t.warehouseid=" + whid + @"
                 " + whedledl1 + @"
                 ) A group by warehouseid,A.itemid,unitcount
                 having (sum(nvl(A.ReadyForIssue,0))+sum(nvl(A.Pending,0)))>0
 ) wh on wh.itemid=f.itemid

     left outer join 
    (
   select m.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.facilityid = " + facid + @"
                 left outer join 
                 (  
                   select  fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid= " + facid + @"       
                   group by fsi.itemid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid               
                 Where 1=1 and f.isactive=1 and f.facilityid = " + facid + @"
                 and T.Status = 'C' and b.expdate>sysdate  and m.edlitemcode is not null
               group by m.edlitemcode 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
    )st on st.edlitemcode= f.itemcode

         left outer join 
     (
     select itemid,sum(IndentQTY) IndenttoWH
from (

select mn.nocid,mni.itemid,m.itemcode,mni.bookedqty*nvl(m.unitcount,1) as IndentQTY
,mn.facilityid  
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid=mn.nocid
inner join masitems m on m.itemid=mni.itemid
inner join masfacilities f on f.facilityid=mn.facilityid and f.facilityid = " + facid + @"
left outer join 
(
select t.indentitemid,tb.facilityid, sum(tbo.issueqty) issueQTYSKU,tb.nocid,t.itemid from tbindentitems t
left outer join tbindents tb on tb.indentid=t.indentid
left outer join tboutwards tbo on tbo.indentitemid=t.indentitemid
where tbo.status='C' and tb.issuetype='NO' and tb.facilityid is not null and tb.facilityid=" + facid + @"
 group by t.indentitemid,tb.facilityid,tb.nocid,t.itemid
) WHI on WHI.facilityid=mn.facilityid  and whi.nocid=mn.nocid and whi.itemid=mni.itemid
where 1=1 and   mn.nocdate>=sysdate-15
and mn.status='C' and mni.status='N' and mni.approvedqty=0
and nvl(WHI.issueQTYSKU,0)=0  " + whedledl1 + @"
) group by itemid
     ) WHIndentP on  WHIndentP.itemid=f.itemid


         left outer join 
         (
         select edlitemcode,sum(BalIFT) as BalIFT from 
(
select  fs.facilityid,fromfacid,fsi.itemid,sum(nvl(ftbo.issueqty,0)) issueqty,nvl(rcpts,0) as ReceiptQTY,fs.issueid,fs.tofacilityid,recFacilityid,vm.edlitemcode,sum(nvl(ftbo.issueqty,0))-nvl(rcpts,0) as BalIFT  from tbfacilityissues fs 
inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
inner join vmasitems vm on vm.itemid=fsi.itemid
inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
left outer join 
(

select tbb.itemid,sum(tbb.absrqty) rcpts,tbf.facilityid as recFacilityid,tbf.tofacilityid as fromfacid,tbf.issueid from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where tbf.status='C'  and tbf.facreceipttype='SP' and tbf.issueid is not null and tbf.facilityid=" + facid + @"
group by tbb.itemid,tbf.warehouseid,tbf.facilityid,tbf.tofacilityid,tbf.issueid
) IFR on IFR.issueid=fs.issueid and IFR.itemid=fsi.itemid
where fs.status = 'C' and fs.issueid is not null  and fs.issuetype='SP'       
and fs.issuedate>=sysdate-180  and recFacilityid is  null and vm.edlitemcode is not null and fs.tofacilityid=" + facid + @"
group by fsi.itemid,fs.facilityid ,fs.issueid,fs.tofacilityid ,fromfacid,recFacilityid,rcpts,vm.edlitemcode 
) group by edlitemcode
         ) IFTTransit on  IFTTransit.edlitemcode=f.itemcode

           left outer join 
     (
     select edlitemcode,sum(LpbalToReceipt) as BalLP from 
(
select  vm.itemcode,vm.edlitemcode,lp.ponoid,lpi.lpitemid,lpi.absqty,nvl(rcpts,0) as LpReceipt,lp.psaid,lpi.absqty-nvl(rcpts,0) as LpbalToReceipt from lpsoorderplaced lp
inner join lpsoordereditems lpi on lpi.ponoid=lp.ponoid
inner join vmasitems vm on vm.itemid=lpi.lpitemid
left outer join 
(
select tbb.itemid,tbf.facilityid,sum(tbb.absrqty) rcpts,tbf.ponoid from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where tbf.status='C'  and tbf.facreceipttype='NO'
and tbf.ponoid is not null and tbf.facilityid=" + facid + @"
group by tbb.itemid,tbf.warehouseid,tbf.facilityid,tbf.ponoid
) lpr on lpr.ponoid=lp.ponoid and lpr.itemid=lpi.lpitemid and lpr.facilityid=lp.psaid
where lp.status='O' and lp.podate>=sysdate-180 and vm.edlitemcode is not null
and lpi.absqty-nvl(rcpts,0)>0 and lp.psaid=" + facid + @"
) group by edlitemcode
     ) lpTransit on  lpTransit.edlitemcode=f.itemcode


     left outer join 
     (

select i.itemid,sum(i.issue) whissue,sum(nvl(r.rcpts,0)) receipts,
sum(i.issue)-sum(nvl(r.rcpts,0)) intransit
from
(
select t.indentitemid, t.itemid,m.itemcode, tb.warehouseid, tb.facilityid, sum(tbo.issueqty*m.unitcount) issue,f.districtid,ft.facilitytypeid,ft.hodid from tbindentitems t
left outer join tbindents tb on tb.indentid=t.indentid
left outer join tboutwards tbo on tbo.indentitemid=t.indentitemid
left outer join masitems m on m.itemid=t.itemid
inner join masfacilities f  on f.facilityid=tb.facilityid  
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
where tbo.status='C' and tb.issuedate>=sysdate-180  and tb.facilityid=" + facid + @" " + whedledl1 + @"
group by t.indentitemid, t.itemid,m.itemcode, tb.warehouseid, tb.facilityid,f.districtid,ft.facilitytypeid,ft.hodid
) i
left outer join
(
select tbr.indentitemid,tbb.itemid,tbf.warehouseid,tbf.facilityid,sum(tbb.absrqty) rcpts from tbfacilityreceiptbatches tbb
left outer join tbfacilityreceiptitems tbr on tbr.facreceiptitemid=tbb.facreceiptitemid
left outer join tbfacilityreceipts tbf on tbf.facreceiptid=tbr.facreceiptid
where  tbf.facilityid=" + facid + @" and tbf.status='C' and tbf.warehouseid is not null and tbf.facreceiptdate>=sysdate-180  
group by tbr.indentitemid,tbb.itemid,tbf.warehouseid,tbf.facilityid
) r
on r.indentitemid=i.indentitemid
where 1=1 
group by i.itemid
 
having sum(i.issue)-sum(nvl(r.rcpts,0))>0
     ) whIssueP on   whIssueP.itemid=f.itemid

            left outer join 
            (
            select mni.itemid,sum(mni.approvedqty*nvl(m.unitcount,1)) as NOCQTY,sum((mni.approvedqty*nvl(m.unitcount,1))-nvl(absqty,0)) as BalNOCAfterLPO
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid=mn.nocid
inner join masitems m on m.itemid=mni.itemid
inner join masfacilities f on f.facilityid=mn.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
left outer join 
(
select lpi.nocid,lpi.absqty,vm.edlitemcode  from lpsoorderplaced lp
inner join lpsoordereditems lpi on lpi.ponoid=lp.ponoid
inner join vmasitems vm on vm.itemid=lpi.lpitemid
where lp.psaid=" + facid + @" and lpi.nocid is not null
) lpo on lpo.nocid=mn.nocid and lpo.edlitemcode=m.itemcode
where 1=1 and   mn.nocdate>=sysdate-15 and f.facilityid=" + facid + @"
and mn.status='C' and mni.status='Y' and mni.approvedqty>0
and ((mni.approvedqty*nvl(m.unitcount,1))-nvl(absqty,0))>0  " + whedledl1 + @"
group by mni.itemid
            ) noc on noc.itemid=f.itemid 


                    left outer join 
    (
  select m.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   cmhostk 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = " + districtid + @"
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and t.hodid=2 and f.isactive=1 
                 and T.Status = 'C' and b.expdate>sysdate  and m.edlitemcode is not null and f.facilityid=" + cmhoid + @" 
               group by m.edlitemcode
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0   
    )stcmho on stcmho.edlitemcode= f.itemcode


 where 1=1  " + whedlnedl + @" and f.isactive='1' and f.ISDEDL is null and f.ISRFORLP is null and f.isfreez_itpr is null 
" + whmcid + @" and f.facilityid=" + facid + @" "+ whftype + @"  "+ whnonedldata + @" order by mia.itemname ";



            var myList = _context.FACwiseStockPostionDbset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }








        [HttpGet("DistDHSStock")]

        public async Task<ActionResult<IEnumerable<StockPostionDTO>>> DistDHSStock(string disid, string coll_cmho, string mcatid, string userid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string districtid = "";
            string whhod = "";
            if (disid == "0")
            {
                if (coll_cmho != "0")
                {
                    if (coll_cmho == "Coll")
                       
                    {
                        districtid = f.getCollectorDisid(userid);
                        whhod = " and t.hodid in (2, 3) ";
                    }
                    if (coll_cmho == "CMHO")
                    {
                        districtid = Convert.ToString(f.geFACTDistrictid(userid));
                        whhod = " and t.hodid in (2) ";
                    }

                }
            }
            else
            {
                districtid = disid;
            }

            string cmhoid = f.getCMHOFacFromDistrict(districtid);
            Int64 whid = f.getWHID(cmhoid.ToString());

            string whmcid = ""; string whmcid1 = "";
            if (mcatid != "0")
            {

              
                whmcid1 = " and mc.MCID =" + mcatid;
            }
       


            string qry = "";


            qry = @"  select facilityid,facilityname, sum(edlstock) edlstock,sum(nedlstock) as nedlstock, sum(edlstock)+sum(nedlstock) as totalstock from 
 (
 select f.facilityname,mi.edlitemcode,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   DHSStock,mc.mcid ,
 case when mi.isedl2021= 'Y' and sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)) >0 then 1 else 0 end as edlstock,
  case when mi.isedl2021= 'Y' and sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0)) >0 then 0 else 1 end as nedlstock
  ,t.orderdp,f.facilityid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                  inner join masitemcategories c on c.categoryid=mi.categoryid
                  inner join masitemmaincategory mc on mc.MCID = c.MCID
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  and f.isactive=1   and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
                " + whmcid1 + @" and f.districtid= "+ districtid+ @"
            "+ whhod + @" group by mi.edlitemcode,mc.mcid ,mi.isedl2021,f.facilityname,t.orderdp,f.facilityid
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
              ) group by facilityname,orderdp,facilityid
               order by orderdp  ";


            var myList = _context.StockPostionDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("DistrictWiseStock")]
        public async Task<ActionResult<IEnumerable<DistrictWiseStockDTO>>> DistrictWiseStock(Int64 mcid)
        {
            string whMcId = "";
            if (mcid != 0)
            {
                whMcId = "  and mc.mcid= " + mcid + " ";
            }
            string qry = @"  select districtid,districtname,sum( CNTEDL) as CNTEDL ,sum(CNTNONEDL) as CNTNONEDL
 
from (


select distinct districtid,districtname,EDLType,itemcode
,case when EDLType='EDL' then 1 else 0 end CNTEDL 
,case when EDLType='Non EDL' then 1 else 0 end CNTNONEDL
from (
select (case when m.edlitemcode is null then m.itemcode else m.edlitemcode end)   as itemcode, case when nvl(m.isedl2021,'N') ='Y'  then 'EDL' else 'Non EDL' end as  EDLType 
,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty ,f.facilityid,d.districtid,d.districtname
             
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masitemcategories c on c.categoryid=m.categoryid
                 inner join masfacilities f  on f.facilityid=t.facilityid   --and f.districtid = 2204
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 inner join masitemmaincategory mc on mc.mcid=c.mcid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  --and t.hodid=2 
                 and f.isactive=1  
                 --and nvl(m.isedl2021,'N')='Y'
                 and T.Status = 'C' and b.expdate>sysdate   " + whMcId + @"
                -- and m.edlitemcode is not null
               group by f.facilityid,m.edlitemcode ,d.districtid,d.districtname,m.itemcode,m.isedl2021
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
              ) group by districtid,districtname,EDLType,itemcode
              
              ) group  by districtid,districtname
              order by districtname; ";
            var myList = _context.DistrictWiseStockDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DdlItemWiseInHandQty")]
        public async Task<ActionResult<IEnumerable<DdlItemWiseInHandQtyDTO>>> DdlItemWiseInHandQty(Int64 distId)
        {
            string whDistId = "";
            if (distId != 0)
            {
                whDistId = "  and f.districtid =  " + distId + " ";
            }
            string qry = @" select m.itemid,m.edlitemcode as edlitemcode,sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)) as inhand_qty
,mi.itemname||'('||mi.itemcode||')-'||to_char((sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)))) as detail
,mi.itemname,mi.itemcode,mi.strength1
from tbfacilityreceiptbatches b
inner join tbfacilityreceiptitems i on  i.facreceiptitemid =b.facreceiptitemid
inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid 
inner join vmasitems m on m.itemid=i.itemid  and m.ISEDL2021='Y'
inner join masitems mi on mi.itemcode=m.edlitemcode  and m.edlitemcode is not null
inner join masitemcategories c on c.categoryid=mi.categoryid and c.mcid=1
inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = "+ distId + @"
inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
  where t.status='C'   and b.expdate>sysdate
group by m.itemid,m.edlitemcode,mi.itemname,m.strength1,mi.itemcode,mi.itemname,mi.strength1
having (sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)))>0
order by mi.itemname ";
            var myList = _context.DdlItemWiseInHandQtyDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("ItemWiseInHandQtyDetail")]
        public async Task<ActionResult<IEnumerable<ItemWiseInHandQtyDetailDTO>>> ItemWiseInHandQtyDetail(Int64 itemId,Int64 DistId)
        {
            string whItemid = "";
            string whDistId = "";

            if (itemId != 0)
            {
                whItemid = " and mi.itemid="+ itemId + @" ";
            }

            if (DistId != 0)
            {
                whDistId = "   and f.districtid = "+ DistId + @" ";
            }
            string qry = @"select ROW_NUMBER() over (order by f.facilityid) as id, m.itemid,f.facilityname,sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)) as inhand_qty,f.facilityid
from tbfacilityreceiptbatches b
inner join tbfacilityreceiptitems i on  i.facreceiptitemid =b.facreceiptitemid
inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid 
inner join vmasitems m on m.itemid=i.itemid  and m.ISEDL2021='Y'
inner join masitems mi on mi.itemcode=m.edlitemcode  and m.edlitemcode is not null
inner join masitemcategories c on c.categoryid=mi.categoryid and c.mcid=1
inner join masfacilities f  on f.facilityid=t.facilityid " + whDistId + @"
inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
where t.status='C'   and b.expdate>sysdate "+ whItemid + @"
group by m.itemid,f.facilityid,f.facilityname,m.edlitemcode,t.orderdp
order by t.orderdp
 ";
            var myList = _context.ItemWiseInHandQtyDetailDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("GetItemQty")]
        public async Task<ActionResult<IEnumerable<GetItemQtyDTO>>> GetItemQty(string isEDL, Int64 mcid, Int64 districtId)
        {

            string whIsEDL = "";
            string whMcid = "";
            string whDistrictId = "";

            if (isEDL.Trim() != null)
            {
                whIsEDL = @"  and nvl(m.isedl2021,'N')='" + isEDL.Trim() + @"' ";
            }

            if (mcid != 0)
            {
                whMcid = " and mc.mcid=" + mcid + " ";
            }

            if (districtId != 0)
            {
                whDistrictId = " and d.districtid = " + districtId + " ";
            }

            string qry = @" select  districtname, x.itemcode,m.itemname,m.STRENGTH1 as STRENGTH  ,m.unit,sum(inhand_qty) as inhand_qty from 
(
select (case when m.edlitemcode is null then m.itemcode else m.edlitemcode end)   as itemcode,
case when nvl(m.isedl2021,'N') ='Y'  then 'EDL' else 'Non EDL' end as  EDLType 
,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty ,f.facilityid,f.facilityname,d.districtid,d.districtname
             
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems m on m.itemid=i.itemid 
                 inner join masitemcategories c on c.categoryid=m.categoryid
                 inner join masfacilities f  on f.facilityid=t.facilityid   
                 inner join masdistricts d on d.districtid=f.districtid
                 inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
                 inner join masitemmaincategory mc on mc.mcid=c.mcid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'                 
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1  --and t.hodid=2 
                 and f.isactive=1  
                 " + whIsEDL + @"
                 and T.Status = 'C' and b.expdate>sysdate   " + whMcid + @"
                 and m.edlitemcode is not null " + whDistrictId + @"
               group by f.facilityid,m.edlitemcode ,d.districtid,d.districtname,m.itemcode,m.isedl2021,f.facilityname
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0
              )x
              inner join vmasitems m on m.itemcode=x.itemcode
              
              group by  districtname, x.itemcode,m.STRENGTH1,m.unit,m.itemname
              
              order by x.itemcode ";
            var myList = _context.GetItemQtyDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("DistDrugCount")]
        public async Task<ActionResult<IEnumerable<DistrictStkCount>>>DistDrugCount(Int64 districtId,Int64 mcid, Int64 hodid)
        {
            string whDistrictId = "";
            if (districtId != 0)
            {
                whDistrictId = " and f.districtid = " + districtId + " ";
            }
            string whhodid = "";
            if (hodid != 0)
            {
                whhodid = " and t.HODID = " + hodid + " ";
            }

            string qry = @"   select sum(EDL) as EDL,sum(NEDL) as NEDL, sum(EDL)+ sum(NEDL) as total,districtid,districtname from 
 (
 select itemid,EDL,NEDL,sum(inhand_qty) as stockQTY,districtid,districtname
 from 
 (

 select mi.itemid,case when mi.isedl2021='Y' then 1 else 0 end as EDL,case when mi.isedl2021='Y' then 0 else 1 end as NEDL,sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)) as inhand_qty
 ,f.districtid,d.districtname
from tbfacilityreceiptbatches b
inner join tbfacilityreceiptitems i on  i.facreceiptitemid =b.facreceiptitemid
inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid 
inner join vmasitems m on m.itemid=i.itemid  
inner join masitems mi on mi.itemcode=m.edlitemcode  
inner join masitemcategories c on c.categoryid=mi.categoryid and c.mcid="+ mcid + @"
inner join masfacilities f  on f.facilityid=t.facilityid "+ whDistrictId + @"
inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
inner join masdistricts d on d.districtid=f.districtid
  where t.status='C'   and b.expdate>sysdate "+ whhodid + @"
group by mi.itemid,mi.isedl2021,f.districtid,d.districtname
having (sum(b.ABSRQTY)-sum(nvl(b.ALLOTQTY,0)))>0
) group by itemid,EDL,NEDL,districtid,districtname
) group by districtid,districtname
order by districtname ";
            var myList = _context.DistrictStkCountDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("WHDrugCount")]
        public async Task<ActionResult<IEnumerable<WHStkcount>>> WHDrugCount(Int64 districtId, Int64 mcid, Int64 whid)
        {
            FacOperations f = new FacOperations(_context);
            Int64 whidA = 0;


            
            if (districtId != 0)
            {
        

                string cmhoid = f.getCMHOFacFromDistrict(Convert.ToString(districtId));
                 whidA = f.getWHID(cmhoid.ToString());

            }
            if (whid != 0)
            {
            
                whidA = whid;

            }

            string whmcid = "";
            if (mcid != 0)
            {
                whmcid = " and c.mcid = " + mcid + " ";
            }
            string whclause = "";
            if (whidA != 0)
            {
                whclause = " and w.warehouseid = " + whidA + " ";
            }

            string qry = @"  
select warehousename,warehouseid,sum(NEDL) as NEDL,sum(EDL) as edl from (
select warehousename,warehouseid, a.itemid,
(case when sum( A.ReadyForIssue)>0 then sum( A.ReadyForIssue) else 0 end)+((case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end)) as ReadyForIssue

,NEDL,EDL

                  from 
                 (  
                 select w.warehousename,w.warehouseid,mi.itemid,case when mi.isedl2021='Y' then 1 else 0 end as EDL,case when mi.isedl2021='Y' then 0 else 1 end as NEDL,
               nvl((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ),0) ReadyForIssue,  
               
                nvl(case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end,0) Pending 
             
                  from tbreceiptbatches b  
                  inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid 
                  inner join tbreceipts t on t.receiptid=i.receiptid 
                  inner join masitems mi on mi.itemid=i.itemid 
                  inner join masitemcategories c on c.categoryid=mi.categoryid "+ whmcid + @"
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  "+ whclause + @"
                 left outer join 
                  (  
                         select ti.warehouseid,tbo.inwno,tbi.itemid,sum(nvl(tbo.issueqty,0)) issueqty   from tbindents ti
inner join tbindentitems tbi on tbi.indentid=ti.indentid
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
where ti.status='C'   and ti.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null 
                   group by tbi.itemid,ti.warehouseid,tbo.inwno  
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
                 Where 1=1 "+ whclause + @" and   T.Status = 'C' and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) And (b.Whissueblock = 0 or b.Whissueblock is null)  
          
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and
      
      
               (nvl((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ),0))+  
               
                +(nvl(case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end,0) )>0

                 ) A  
                 where 1=1 
                 group by warehouseid, a.itemid,EDL,NEDL,warehousename
                 )group by warehouseid,warehousename
                  ";
            var myList = _context.WHStkcountDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("DHSissueItems")]
        public async Task<ActionResult<IEnumerable<DHSissueItemsDTO>>> DHSissueItems(Int64 districtId, Int64 mcid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();


            string whDistId = "";
            string whMcid = "";

            if (districtId != 0)
            {
                whDistId = " and f.districtid="+ districtId + " ";

            }
            if (mcid != 0)
            {

                whMcid = " and c.mcid="+ mcid +" ";

            }

            

            string qry = @"  select 
 count(distinct CMHOItem)-1 as CMHOItem,
  round(sum(CMHOValue)/100000,2) as CMHOValue,
 count(distinct DHItem)-1 as DHItem,
 round(sum(DHValue)/100000,2) as DHValueLac,
 count(distinct CHCitem)-1 as CHCitem,
  round(sum(CHCValue)/100000,2) as CHCValue,
 count(distinct otherfacitems)-1 as otherfacitems,
   round(sum(Otherfacvalue)/100000,2) as Otherfacvalue
from 
(
 select 
        f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,c.MCID,     rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,rb.ponoid
        ,f.facilitytypeid, 
        case when f.facilitytypeid =353 then tbi.itemid else 0 end as CMHOItem
        , case when f.facilitytypeid =353 then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as CMHOValue
        ,case when f.facilitytypeid =352 then tbi.itemid else 0 end as DHItem
        ,case when f.facilitytypeid =352 then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as DHValue
        , case when f.facilitytypeid in (355,354,358,385,388) then tbi.itemid else 0 end as CHCitem
       , case when f.facilitytypeid in (355,354,358,385,388) then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as CHCValue
        , case when f.facilitytypeid in (355,354,358,385,388,352,353) then 0 else tbi.itemid end as otherfacitems
          , case when f.facilitytypeid in (355,354,358,385,388,352,353) then  0 else   sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)    end as Otherfacvalue
        ,oci.finalrategst,oci.singleunitprice
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join SoOrderedItems OI on OI.PoNoID = rb.PoNoID and oi.itemid=rb.itemid
         inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid "+ whDistId + @"
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
      where 1=1 and t.hodid=2  "+ whMcid + @" and tb.indentdate 
      between  ( select startdate from masaccyearsettings where ACCYRSETID="+ yearid + @") and  ( select enddate from masaccyearsettings where ACCYRSETID="+ yearid + @")  
      group by  f.facilityid,tb.indentdate,tbi.itemid, rb.batchno,  tb.indentid,c.MCID,rb.ponoid,f.facilitytypeid,
      oci.finalrategst,oci.singleunitprice
      )
 



   ";
            var myList = _context.DHSissueItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("DMEissueItems")]
        public async Task<ActionResult<IEnumerable<DMEissueItemsDTO>>> DMEissueItems(Int64 districtId, Int64 mcid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();


            string whDistId = "";
            string whMcid = "";

            if (districtId != 0)
            {
                whDistId = " and f.districtid=" + districtId + " ";

            }
            if (mcid != 0)
            {

                whMcid = " and c.mcid=" + mcid + " ";

            }



            string qry = @"  select facilityid,facilityname,
 count(distinct itemid)-1 as NosIssueitems,
  round(sum(DMEValue)/100000,2) as Facvalue
  ,facilitytypeid
from 
(
 select 
        f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,c.MCID,     rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,rb.ponoid
        ,f.facilitytypeid,f.facilityname ,  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   DMEValue
        ,oci.finalrategst,oci.singleunitprice
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join SoOrderedItems OI on OI.PoNoID = rb.PoNoID and oi.itemid=rb.itemid
         inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid "+ whDistId + @"
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
      where 1=1 and t.hodid=3  "+ whMcid + @" and tb.indentdate 
      between  ( select startdate from masaccyearsettings where ACCYRSETID="+ yearid + @") and  ( select enddate from masaccyearsettings where ACCYRSETID="+ yearid + @")  
      group by  f.facilityid,tb.indentdate,tbi.itemid, rb.batchno,  tb.indentid,c.MCID,rb.ponoid,f.facilitytypeid,
      oci.finalrategst,oci.singleunitprice,f.facilityname
      ) group by facilityid,facilityname,facilitytypeid
      order by facilitytypeid   ";
            var myList = _context.DMEissueItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("AyushIssueItems")]
        public async Task<ActionResult<IEnumerable<DMEissueItemsDTO>>> AyushIssueItems(Int64 districtId, Int64 mcid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();


            string whDistId = "";
            string whMcid = "";

            if (districtId != 0)
            {
                whDistId = " and f.districtid=" + districtId + " ";

            }
            if (mcid != 0)
            {

                whMcid = " and c.mcid=" + mcid + " ";

            }



            string qry = @"   select facilityid,facilityname,
 count(distinct itemid)-1 as NosIssueitems,
  round(sum(DMEValue)/100000,2) as Facvalue
  ,facilitytypeid
from 
(
 select 
        f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,c.MCID,     rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,rb.ponoid
        ,f.facilitytypeid,f.facilityname ,  sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   DMEValue
        ,oci.finalrategst,oci.singleunitprice
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join SoOrderedItems OI on OI.PoNoID = rb.PoNoID and oi.itemid=rb.itemid
         inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid  "+ whDistId + @"
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
      where 1=1 and t.hodid=7  "+ whMcid + @" and tb.indentdate 
      between  ( select startdate from masaccyearsettings where ACCYRSETID="+ yearid + ") and  ( select enddate from masaccyearsettings where ACCYRSETID="+ yearid + @")  
      group by  f.facilityid,tb.indentdate,tbi.itemid, rb.batchno,  tb.indentid,c.MCID,rb.ponoid,f.facilitytypeid,
      oci.finalrategst,oci.singleunitprice,f.facilityname
      ) group by facilityid,facilityname,facilitytypeid
      order by facilitytypeid
         ";
            var myList = _context.DMEissueItemsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("DisYrGrowth")]
        public async Task<ActionResult<IEnumerable<DistIssueGrowthDTO>>> DisYrGrowth(Int64 districtId, Int64 mcid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();

            string whDistId = "";
            string whMcid = "";
            if (districtId != 0)
            {
                whDistId = " and f.districtid=" + districtId + " ";

            }

            if (mcid == 1)
            {

                whMcid = "    and c.mcid in (1,4)  ";

            }
            else
            {
                whMcid = " and c.mcid=" + mcid + " ";
            }

            string qry = @"   select ACCYRSETID,SHACCYEAR,
  count(distinct DMEItems)-1 as DMEItems,
  round(sum(DMEValue)/100000,2) as DMEValue,
 count(distinct CMHOItem)-1 as CMHOItem,
  round(sum(CMHOValue)/100000,2) as CMHOValue,
 count(distinct DHItem)-1 as DHItem,
 round(sum(DHValue)/100000,2) as DHValueLac,
 count(distinct CHCitem)-1 as CHCitem,
  round(sum(CHCValue)/100000,2) as CHCValue,
 count(distinct otherfacitems)-1 as otherfacitems,
   round(sum(Otherfacvalue)/100000,2) as Otherfacvalue
   , count(distinct AYitems)-1 as AYitems,
   round(sum(AYValue)/100000,2) as AYValue
   
from 
(
 select (select ACCYRSETID  from masaccyearsettings where tb.indentdate between  startdate and enddate) as ACCYRSETID
 ,(select SHACCYEAR  from masaccyearsettings where tb.indentdate between  startdate and enddate) as SHACCYEAR,
        f.facilityid,
        tb.indentid,tb.indentdate,tbi.itemid,c.MCID,     rb.batchno iss_batchno,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,rb.ponoid
        ,f.facilitytypeid, 
           case when f.facilitytypeid =371 then tbi.itemid else 0 end as AYitems
        , case when f.facilitytypeid =371 then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as AYValue,
        case when f.facilitytypeid in (364,378) then tbi.itemid else 0 end as DMEItems
        , case when f.facilitytypeid in (364,378) then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as DMEValue,
        case when f.facilitytypeid =353 then tbi.itemid else 0 end as CMHOItem
        , case when f.facilitytypeid =353 then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as CMHOValue
        ,case when f.facilitytypeid =352 then tbi.itemid else 0 end as DHItem
        ,case when f.facilitytypeid =352 then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as DHValue
        , case when f.facilitytypeid in (355,354,358,385,388) then tbi.itemid else 0 end as CHCitem
       , case when f.facilitytypeid in (355,354,358,385,388) then    sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)   else 0 end as CHCValue
        , case when f.facilitytypeid in (355,354,358,385,388,352,353,364,378,371) then 0 else tbi.itemid end as otherfacitems
          , case when f.facilitytypeid in (355,354,358,385,388,352,353,364,378,371) then  0 else   sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * nvl(oci.finalrategst,oci.singleunitprice)    end as Otherfacvalue
        ,oci.finalrategst,oci.singleunitprice
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join SoOrderedItems OI on OI.PoNoID = rb.PoNoID and oi.itemid=rb.itemid
         inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid  "+ whDistId + @"
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
      where 1=1 and t.hodid in (2,3,7)  "+whMcid+@"
      group by  f.facilityid,tb.indentdate,tbi.itemid, rb.batchno,  tb.indentid,c.MCID,rb.ponoid,f.facilitytypeid,
      oci.finalrategst,oci.singleunitprice
      ) group by ACCYRSETID,SHACCYEAR
      order by ACCYRSETID ";
            var myList = _context.DistIssueGrowthDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("DistCGMSCSupplyDHS")]
        public async Task<ActionResult<IEnumerable<DistCGMSCSupplyDTO>>> DistCGMSCSupplyDHS(Int64 districtId, Int64 mcid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();

            string whDistId = "";
            string whMcid = "";
            if (districtId != 0)
            {
                whDistId = " and f.districtid=" + districtId + " ";

            }

            if (mcid != 0)
            {

                whMcid = "    and c.mcid = "+ mcid;

            }
           

            string qry = @"  select sum(DHSAIFixed) as DHSAIFixed,sum(IssueaainstIndent) as IssueaainstIndent,sum(TotalIssuedCGMSCitems)- sum(IssueaainstIndent)  as withoutAI,sum(TotalIssuedCGMSCitems) as TotalIssuedCGMSCitems

,round(sum(IssueaainstIndent)/sum(DHSAIFixed)*100,2) aginAI_Perc
,round(sum(TotalIssuedCGMSCitems)/sum(DHSAIFixed)*100,2) totalPEr
from 
(

select m.itemid,nvl(DHSApprovedNos,0) as DHSApprovedNos,nvl(iss.DHSIssueQTYY,0)* nvl(m.unitcount,1) as CGMSCSuppliedQTY 
,case when nvl(DHSApprovedNos,0) >0 then 1 else 0 end as DHSAIFixed,case when nvl(iss.DHSIssueQTYY,0)>0 then 1 else 0 end as TotalIssuedCGMSCitems
,case when nvl(DHSApprovedNos,0) >0 and nvl(iss.DHSIssueQTYY,0)>0 then 1 else 0 end as IssueaainstIndent
from masitems m
      inner join masitemcategories c on c.categoryid=m.categoryid
left outer join
(

select  a.itemid,sum(a.rindentqty) as facilityindentqty
 ,sum(a.DHSAPRQTY) as DHSApprovedNos
  ,round(sum(nvl(a.DHSAPRQTY,0))/nvl(m.unitcount,1),0) as DHSApprovedSKU
from masannualindentdhs dma 
inner join masrevisedannualindent ma on ma.dhsaiid=dma.dhsaiid
inner join revisedannualindentitem a on a.RANNUALINDENTID=ma.RANNUALINDENTID 
inner join masitems m on m.itemid =a.itemid
inner join masitemcategories c on c.categoryid=m.categoryid    
inner join masfacilities f on f.facilityid=ma.facilityid
inner join masdistricts d on d.districtid=f.districtid
    inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where ma.status='C' and   ma.accyrsetid="+ yearid + @"  and dma.accyrsetid=" + yearid + @"  and t.hodid=2
and nvl(a.rindentqty,0)>0  and ma.dhsaiid is not null and nvl(dma.ISCGMSCRECV,'N')='Y' 
"+ whDistId + @" "+ whMcid + @"
group by a.itemid,m.unitcount
)  distai on distai.itemid=m.itemid

left outer join 
(

select itemid,sum(DHSIssueQTYY) as DHSIssueQTYY
from 
(
  select  tbi.itemid, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  DHSIssueQTYY
 from tbindents tb
 inner join tbindentitems tbi on tbi.indentid=tb.indentid 
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join masfacilities f on f.facilityid = tb.facilityid and f.districtid=2208
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and  t.hodid=2 and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+ yearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid=" + yearid + @") 
group by tbi.itemid,t.hodid 
) group by itemid
) iss on iss.itemid=m.itemid where 1=1 and isfreez_itpr is null "+ whMcid + @"
and (nvl(DHSApprovedNos,0)+nvl(iss.DHSIssueQTYY,0))>0
) ";
            var myList = _context.DistCGMSCSupplyDBset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("DistFacwiseCGMSCSupplyIndent")]
        public async Task<ActionResult<IEnumerable<DistFacwiseCGMSCSupplyIndentDTO>>> DistFacwiseCGMSCSupplyIndent(Int64 districtId, string yrid)
        {
            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();
            string whyearid = "";
            string whDistId = "";
            if (districtId != 0)
            {
                whDistId = " and f.districtid=" + districtId + " ";

            }

            if (yrid != "0")
            {

                whyearid = yrid;

            }
            else
            {
                whyearid = yearid;
            }


            string qry = @"  select f.facilityid, f.facilityname,nvl(nosindent,0) as nosindent,case when nvl(avgdaystaken,0)=0 then 1 else nvl(avgdaystaken,0) end as avgdaystaken,nvl(drugscount,0) as drugscount,nvl(consumables,0) as consumables,nvl(reagent,0) as reagent,nvl(ayushdrugs,0) as ayushdrugs  from masfacilities f
inner join masdistricts d on d.districtid=f.districtid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join
(
select distinct facilityid from  tbindents tb 

where issuetype='NO'
) tb on tb.facilityid=f.facilityid
left outer join 
(

select facilityid,count(distinct indentid) as nosindent,count(distinct drugs)-1 as drugscount, count(distinct consumables)-1 as consumables, count(distinct reagent)-1 as reagent,count(distinct ayushdrugs)-1 as ayushdrugs
from 
(
select tb.facilityid,tb.indentid,case when c.mcid=1 then tbi.itemid else 0 end as drugs,case when c.mcid=2 then tbi.itemid else 0 end as consumables 
,case when c.mcid=3 then tbi.itemid else 0 end as reagent, case when c.mcid=4 then tbi.itemid else 0 end as ayushdrugs
from tbindents tb
inner join masfacilities f on f.facilityid=tb.facilityid " + whDistId + @"
inner JOIN tbindentitems tbi on tbi.indentid=tb.indentid
inner join masitems m on m.itemid=tbi.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
where  issuetype='NO' and tb.indentdate 
      between  ( select startdate from masaccyearsettings where ACCYRSETID="+ whyearid + @") and  ( select enddate from masaccyearsettings where ACCYRSETID="+ whyearid + @") 
      ) group by facilityid
) tbi on tbi.facilityid=f.facilityid

left outer join 
(
select  a.facilityid,count(distinct indentid) as nosindentdrop, round(sum(da)/count(distinct indentid),0) as avgdaystaken
from 
(
select facilityid,indentid,nocdate,deliveredDT,round((to_date(to_char(deliveredDT,'dd-MM-yyyy'),'dd-MM-yyyy'))-nocdate,0) as da
from 
(
select tb.facilityid,tb.indentid,nc.nocid ,nc.nocdate,tb.indentdate ,DROPDATE,rdate,
case when DROPDATE is not null then DROPDATE else case when DROPDATE is null and rdate is not null then rdate else  indentdate end end as deliveredDT 
from mascgmscnoc nc 
inner join tbindents tb on tb.nocid=nc.nocid

inner join masfacilities f on f.facilityid=tb.facilityid  " + whDistId + @"
left outer join
(
SELECT INDENTID AS dropindentid,MIN(ENTRYDATE) AS DROPDATE FROM TBINDENTTRAVALE 
WHERE ISDROP='Y' GROUP BY INDENTID
) di on di.dropindentid=tb.indentid
left outer join 
(
select tr.indentid,max(tr.FACRECEIPTDATE) as rdate from tbfacilityreceipts tr
inner join masfacilities f on f.facilityid=tr.facilityid  " + whDistId + @"
where tr.FACRECEIPTTYPE='NO'
group by tr.indentid
) r on r.indentid=tb.indentid
where  issuetype='NO' and tb.indentdate 
between  ( select startdate from masaccyearsettings where ACCYRSETID="+ whyearid + @") and  ( select enddate from masaccyearsettings where ACCYRSETID="+ whyearid + @") 
)
) a group by  a.facilityid
) b on b.facilityid=f.facilityid

where f.isactive=1  " + whDistId + @"
order by ft.orderdp ";
            var myList = _context.DistFacwiseCGMSCSupplyIndentDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



    }
    }
