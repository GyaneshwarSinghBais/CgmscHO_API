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
using System.Collections;
using CgmscHO_API.FacilityDTO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Facility : ControllerBase
    {
        private readonly OraDbContext _context;
        public Facility(OraDbContext context)
        {
            _context = context;
        }

        [HttpGet("Facilitiesddl")]
        public async Task<ActionResult<IEnumerable<InitiatedPendingIssueSummaryDTO>>>Facilitiesddl(string userid, string districtid, string hodid, string factypeid)
        {
            FacOperations f = new FacOperations(_context);
            Int64 distid = 0;
            string whdist = "";
            if (districtid == "0")
            {
                distid = f.geFACTDistrictid(userid);
                whdist = " and f.districtid ="+ distid;
            }
            if (districtid == "All")
            {
                whdist = "";
            }


            string whhod = "";
            if (hodid != "0")
            {
                whhod = " and ft.hodid =" + hodid;
            }
            string whfactypeid = "";
            if (factypeid != "0")
            {
                whfactypeid = " and ft.FACILITYTYPEID =" + factypeid;
            }


            string qry = @" select f.facilityname,
f.districtid, f.facilityid from masfacilities f, masfacilitytypes ft where f.facilitytypeid = ft.facilitytypeid
 and f.isactive = 1
 "+ whdist + @" "+ whfactypeid + @"  and ft.FACILITYTYPEID not in (377, 382)  "+ whhod + @"
  order by ft.orderdp ";
                   var myList = _context.facddlDBset
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("DistEDLPostion")]
        public async Task<ActionResult<IEnumerable<StockPostionDTO>>> DistEDLPostion(string facid,string userid,string edltype,string hodid)
        {
            FacOperations f = new FacOperations(_context);
            string qry = "";
            string whfacid = "";
            string yearid = f.getACCYRSETID();
            Int64 whid = 0;

            Int64 did = f.geFACTDistrictid(userid);
            Int64 cmhofacid = 0;
            string strcmhofacid = "";
            if (facid != "0")
            {
                facid = " and f.facilityid=" + facid;
                cmhofacid = f.gecmhoid(facid);
                whid = f.getWHID(facid);
            }
            else
            {
                strcmhofacid=f.getCMHOFacFromDistrict(did.ToString());
                cmhofacid = Convert.ToInt64(strcmhofacid);
                whid = f.getWHID(cmhofacid.ToString());
            }
          
            if (edltype == "EDL")
            {
                qry = @" select facilityname,count(distinct itemid) as nositems,count(distinct itemid)-sum(facstkcnt) as STOCKOUTNOS,
sum(facstkcnt) as facstkcnt,Round(((count(distinct itemid)-sum(facstkcnt))/count(distinct itemid))*100,0) as stockoutP,
sum(WHStkcnt) as WHStkcnt,sum(cmhostkcnt) as cmhostkcnt,
sum(WHUQCStkcnt) as WHUQCStkcnt
,sum(IndentQTYPendingcnt) as Indent_toWH_Pending,sum(WHIssuePending_L180cnt) as WHIssue_Rec_Pending_L180cnt,sum(BalIFT6Month) as BalIFT6Month
,sum(BalLP_L180cnt) as LP_Pipeline180cnt
,sum(BalNOCcnt) as NocTaken_No_LPO,
facilityid,orderdp,facilitytypeid
from 
(

select f.facilityname,f.itemcode,nvl(inhand_qty,0) as facstock,nvl(cmhostk,0) as cmhostk,nvl(READYFORISSUE,0) whReady,
nvl(PENDING,0) whpending,nvl(mbf.BUFFERNOSQTY,0) as WHBuffer,nvl(IndenttoWH,0) IndentQTYPending ,
nvl(whIssueP.intransit,0) as WHIssuePending_L180, 
nvl(IFTTransit.BalIFT,0) as BalIFT_L180,
nvl(lpTransit.BalLP,0) as BalLP_L180 
,nvl(noc.BalNOCAfterLPO,0) as BalNOCAfterLPO, 
sttusEDL,orderdp,f.facilityid,f.districtid,isedl2021,mcid,f.itemid 
,case when nvl(inhand_qty,0)>0 then 1 else 0 end as facstkcnt
,case when nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)=0 and nvl(IFTTransit.BalIFT,0)=0 and nvl(IndenttoWH,0)=0 and nvl(cmhostk,0)>0 then 1 else 0 end as cmhostkcnt
,case when nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)>0 and nvl(PENDING,0)=0 then 1 else 0 end as WHStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(READYFORISSUE,0)=0 and nvl(PENDING,0)>0 then 1 else 0 end as WHUQCStkcnt
,case when nvl(inhand_qty,0)=0 and nvl(whIssueP.intransit,0)>0 then 1 else 0 end WHIssuePending_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(IndenttoWH,0)>0 and nvl(READYFORISSUE,0)>0  then 1 else 0 end IndentQTYPendingcnt
,case when nvl(inhand_qty,0)=0 and nvl(IFTTransit.BalIFT,0)>0 then 1 else 0 end BalIFT6Month
,case when nvl(inhand_qty,0)=0 and nvl(lpTransit.BalLP,0)>0 then 1 else 0 end BalLP_L180cnt
,case when nvl(inhand_qty,0)=0 and nvl(noc.BalNOCAfterLPO,0)>0 then 1 else 0 end BalNOCcnt 
,ISDEDL,ISRFORLP,isfreez_itpr,facilitytypeid,f.isactive
from 
(
select f.facilityname
,itemcode,

m.edlcat,ft.eldcat,m.itemid,m.categoryid
,case when m.edlcat<=ft.eldcat then 1 else 0 end sttusEDL,f.districtid,f.facilityid,ft.facilitytypeid,ft.orderdp,isedl2021,c.mcid,m.ISDEDL,m.ISRFORLP,m.isfreez_itpr,f.isactive
from masitems m, masfacilities f,masitemcategories c,
masfacilitytypes ft where m.isedl2021='Y' 
and ft.hodid=2 and f.isactive=1 and  f.facilitytypeid=ft.facilitytypeid and c.categoryid = m.categoryid
and m.edlcat<=ft.eldcat and  m.isfreez_itpr is  null 
and  m.ISDEDL is null and m.ISRFORLP is null "+ facid + @"
and f.districtid="+did+ @" and ft.facilitytypeid not in (353)
) f
left outer join 
(
select  (case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue)*A.unitcount else 0 end) as READYFORISSUE,
(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end)*A.unitcount PENDING
,A.itemid ,A.warehouseid,A.unitcount
                  from
                 (
                 select t.warehouseid,  b.inwno, 
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
,mi.itemid,nvl(mi.unitcount,1) as unitcount

                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                  inner join masitems mi on mi.itemid = i.itemid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate)
                 And(b.Whissueblock = 0 or b.Whissueblock is null)
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null and t.warehouseid="+whid+@"
                 and  mi.isedl2021='Y'
                 ) A group by warehouseid,A.itemid,unitcount
                 having (sum(nvl(A.ReadyForIssue,0))+sum(nvl(A.Pending,0)))>0
 ) wh on wh.itemid=f.itemid
 
     left outer join 
    (
    select mi.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty ,f.facilityid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = " + did+@" "+facid+@"
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
                 and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
               group by f.facilityid,mi.edlitemcode
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
inner join masfacilities f on f.facilityid=mn.facilityid and f.districtid = "+did+ @" "+facid+ @"
left outer join 
(
select t.indentitemid,tb.facilityid, sum(tbo.issueqty) issueQTYSKU,tb.nocid,t.itemid from tbindentitems t
left outer join tbindents tb on tb.indentid=t.indentid
left outer join tboutwards tbo on tbo.indentitemid=t.indentitemid
where tbo.status='C' and tb.issuetype='NO' and tb.facilityid is not null 
 group by t.indentitemid,tb.facilityid,tb.nocid,t.itemid
) WHI on WHI.facilityid=mn.facilityid  and whi.nocid=mn.nocid and whi.itemid=mni.itemid
where 1=1 and   mn.nocdate>=sysdate-15
and mn.status='C'  and mni.iscancel is null and mni.status='N' and mni.approvedqty=0
and nvl(WHI.issueQTYSKU,0)=0
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
and lp.status='O' and lp.podate>=sysdate-180 and vm.edlitemcode is not null
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
where tbo.status='C' and tb.issuedate>=sysdate-180  and tb.facilityid is not null 
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
where 1=1 and   mn.nocdate>=sysdate-15 and f.districtid=" + did+ @" "+facid+@"
and mn.status='C' and mni.status='Y' and mni.approvedqty>0
and ((mni.approvedqty*nvl(m.unitcount,1))-nvl(absqty,0))>0
group by mni.itemid,mn.facilityid 
            ) noc on noc.itemid=f.itemid and noc.facilityid=f.facilityid
               
               
                    left outer join 
    (
    select mi.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   cmhostk ,f.facilityid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = " + did+ @" "+facid+ @"
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
                 and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null and f.facilityid="+ cmhofacid + @"
               group by f.facilityid,mi.edlitemcode 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
    )stcmho on stcmho.edlitemcode= f.itemcode 
               
                    
left outer join  masbuffer mbf on mbf.itemid=f.itemid and mbf.FACTYPEID=wh.warehouseid
 where isedl2021='Y' and f.isactive='1' and f.ISDEDL is null and f.ISRFORLP is null and f.isfreez_itpr is null 
 and mcid=1 and f.FACILITYTYPEID not in (377,382) "+facid+@"
 ) group by facilityname,facilityid,orderdp,facilitytypeid
 order by orderdp ";
            }
            else
            {
                qry = @"  ";

            }
           
            var myList = _context.StockPostionDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }





        [HttpGet("WHDHSDistStock")]
        public async Task<ActionResult<IEnumerable<CGMSCPublicStockDTO>>> WHDHSDistStock(string mcid,string userid,string hodid)
        {

            FacOperations f = new FacOperations(_context);
            string qry = "";
            string whfacid = "";
            string yearid = f.getACCYRSETID();
            Int64 whid = 0;

            Int64 did = f.geFACTDistrictid(userid);
            Int64 cmhofacid = 0;
            string strcmhofacid = "";
        
           strcmhofacid = f.getCMHOFacFromDistrict(did.ToString());
           cmhofacid = Convert.ToInt64(strcmhofacid);
           whid = f.getWHID(cmhofacid.ToString());


            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.mcid=" + mcid;
            }
             qry = @" select m.itemid as id, m.itemname ||'('||m.itemcode||') '||',Warehouse:'||nvl(st.ready,0)||',DHS Health Facility:'||nvl(stdist.inhand_qty,0)||',Group:'||nvl(g.groupname,'NA') as details
,case when m.isedl2021='Y' then 1 else 0 end as edltype,m.itemname as name
from masitems m
left outer join masitemgroups g on g.groupid=m.groupid
left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0))*nvl(unitcount,1) as READY,sum(nvl(Pending,0))*nvl(unitcount,1) as UQC from 
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 ,mi.unitcount
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join masitems mi on mi.itemid= i.itemid
 inner join masitemcategories c on c.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  T.Status = 'C' and mc.mcid=1   And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     and t.warehouseid=" + whid + @"
     ) group by itemid,unitcount
) st on st.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') and soi.warehouseid="+ whid + @"
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid
) pip on pip.itemid = m.itemid


    left outer join 
    (
    select mi.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = " + did + @"
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
                 Where 1=1  and t.hodid="+ hodid + @" and f.isactive=1 
                 and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
               group by mi.edlitemcode
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
    )stdist on  stdist.edlitemcode= m.itemcode

inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
where m.ISFREEZ_ITPR is null and (nvl(st.ready,0)+nvl(st.UQC,0)+nvl(pip.TOTLPIPELINE,0)+nvl(stdist.inhand_qty,0))>0 "+ whmcid + @"
order by (case when m.isedl2021='Y' then 0 else 1 end),g.groupname desc ";
            var myList = _context.CGMSCPublicStockDbSet
             .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("WHDHSDistStockItem")]
        public async Task<ActionResult<IEnumerable<StockPotionItemDTO>>> WHDHSDistStockItem(string itemid, string userid, string hodid)
        {

            FacOperations f = new FacOperations(_context);
            string qry = "";
            string whfacid = "";
            string yearid = f.getACCYRSETID();
            Int64 whid = 0;

            Int64 did = f.geFACTDistrictid(userid);
            Int64 cmhofacid = 0;
            string strcmhofacid = "";

            strcmhofacid = f.getCMHOFacFromDistrict(did.ToString());
            cmhofacid = Convert.ToInt64(strcmhofacid);
            whid = f.getWHID(cmhofacid.ToString());


            string whMitemid = "";
            string whMIitemid = "";
            if (itemid != "0")
            {
                whMitemid = " and m.itemid=" + itemid;
                whMIitemid = " and mi.itemid=" + itemid;
            }
            qry = @" select m.itemcode,e.edl,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDLType,m.itemname,m.strength1,ty.itemtypename,mc.MCATEGORY,nvl(g.groupname,'NA') as gname,nvl(READYFORISSUE,0) readywh,nvl(PENDING,0) qcpendingwh,nvl(TOTLPIPELINE,0) as TOTLPIPELINE,nvl(inhand_qty,0) districtstock,nvl(aiqty,0) cmhoapraiqty,
nvl(whissueqty,0) distfacissueqty,nvl(NOCQTY,0) DISTNOCQTY,nvl(lpoqty,0) distlpoqty,nvl(wissueqty,0) distwardissueqty,
nvl(pissueqty,0) distpatientissueqty,nvl(noofpatients,0) noofpatients from masitems m
left outer join masitemgroups g on g.groupid=m.groupid
inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
left outer join masedl e on e.edlcat=m.edlcat
left outer join 
(
select  (case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue)*A.unitcount else 0 end) as READYFORISSUE,
(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end)*A.unitcount PENDING
,A.itemid ,A.warehouseid,A.unitcount
                  from
                 (
                 select t.warehouseid,  b.inwno, 
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
,mi.itemid,nvl(mi.unitcount,1) as unitcount

                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid                 
                  inner join masitems mi on mi.itemid = i.itemid
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
                 and  mi.isedl2021='Y' "+ whMIitemid + @"
                 ) A group by warehouseid,A.itemid,unitcount
                 having (sum(nvl(A.ReadyForIssue,0))+sum(nvl(A.Pending,0)))>0
 ) wh on wh.itemid=m.itemid
 
 
 left outer join
(
  select itemid, sum(pipelineQTY)*nvl(unitcount,0) TOTLPIPELINE from
(
select m.itemcode,m.unitcount, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty,0)receiptabsqty,
receiptdelayexception ,round(sysdate-op.soissuedate,0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end as pipelineQTY,
oci.finalrategst,
(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end)*oci.finalrategst pipelinevalue
from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID=OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid=OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid= tr.receiptid
where tr.receipttype= 'NO' and tr.status= 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid=OP.PoNoID and rec.itemid=OI.itemid
 where op.status  in ('C','O') "+whMitemid+ @" and soi.warehouseid="+ whid + @"
 group by m.itemcode,m.unitcount, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having (case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is null and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate+1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and  (op.extendeddate+1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty,0) else 0 end end end end) >0
) group by itemid,unitcount
) pip on pip.itemid = m.itemid

 
 
left outer join 
    (
    select mi.edlitemcode as edlitemcode ,sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))   inhand_qty 
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid and f.districtid = "+did+ @"
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
                 Where 1=1  and t.hodid="+ hodid + @" and f.isactive=1
                 and T.Status = 'C' and b.expdate>sysdate  and mi.edlitemcode is not null
               group by mi.edlitemcode 
              having  sum(nvl(b.absrqty,0) - nvl(iq.issueqty,0))>0  
    )st on  st.edlitemcode= m.itemcode
    
left outer join 
(
            select a.itemid,sum(nvl(CMHODISTQTY,0)) aiqty
            from anualindent a 
            inner join masfacilities f  on f.facilityid=a.facilityid  
             inner join masfacilitytypes t on t.facilitytypeid=f.facilitytypeid
            where a.status = 'C' and f.isactive=1 and a.accyrsetid="+ yearid + @" and f.districtid = "+did+ @" and t.hodid="+ hodid + @"
            group by a.itemid having sum(nvl(CMHODISTQTY,0))>0
) ai  on ai.itemid=m.itemid 
left outer join 
(
            select tbi.itemid, sum(tbo.issueqty)*nvl(m.unitcount,1) whissueqty from 
            tbindents tb 
            inner join tbindentitems tbi on tbi.indentid=tb.indentid
            inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
            inner join masfacilities f  on f.facilityid=tb.facilityid 
            inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
            inner join masitems m on m.itemid = tbi.itemid
            where tb.status='C'  " + whMitemid+ @"  and tb.issuetype = 'NO' and tb.issuedate between  (select startdate  from masaccyearsettings where accyrsetid = "+ yearid + @") 
            and  (select enddate  from masaccyearsettings where accyrsetid = " + yearid + @") and f.districtid = " + did+ @" and ft.hodid="+ hodid + @"
            group by tbi.itemid,m.unitcount
) iss on iss.itemid = m.itemid
left outer join 
  (
            select mni.itemid,sum(mni.approvedqty*nvl(m.unitcount,1)) as NOCQTY,sum(nvl(absqty,0)) as lpoqty
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
            where 1=1 and mn.status='C' " + whMitemid+ @" and mni.status='Y' and mni.approvedqty>0
            and mni.iscancel is null and mn.nocdate between  (select startdate  from masaccyearsettings where accyrsetid = "+ yearid + @") 
            and  (select enddate  from masaccyearsettings where accyrsetid = "+ yearid + @") and f.districtid = " + did+ @" and ft.hodid="+ hodid + @"
            group by mni.itemid
) noc on noc.itemid=m.itemid
left outer join 
                 (  
                   select  fsi.itemid,sum(nvl(ftbo.issueqty,0)) wissueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   inner join vmasitems m on m.itemid = fsi.itemid
                   inner join masfacilities f on f.facilityid=fs.facilityid
                   inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
                   where fs.status = 'C' and fs.issuetype = 'NO'  
                   and fs.issueddate between  (select startdate  from masaccyearsettings where accyrsetid = "+ yearid + @") 
                    and  (select enddate  from masaccyearsettings where accyrsetid = "+ yearid + @") and f.districtid = " + did+ @" and ft.hodid="+ hodid + @"
                   group by fsi.itemid                    
                 ) wiq on  wiq.itemid=m.itemid 
left outer join
(
                select m.itemid,sum(o.issueqty) pissueqty,count(distinct t.patid) noofpatients 
                from tbwardissues t 
                inner join tbwardissueitems i on i.issueid = t.issueid
                inner join tbwardoutwards o on o.issueitemid = i.issueitemid
                inner join vmasitems m on m.itemid = i.itemid
                inner join masfacilitywards mfw on mfw.wardid = t.wardid
                inner join masfacilities f on f.facilityid = mfw.facilityid
                inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
                where t.status = 'C' and t.issuetype = 'NO' 
                and t.issuedate between  (select startdate  from masaccyearsettings where accyrsetid = "+ yearid + @") 
                and  (select enddate  from masaccyearsettings where accyrsetid = "+ yearid + @") and f.districtid = " + did+ @" and ft.hodid="+ hodid + @"
                group by m.itemid
) pissue on pissue.itemid = m.itemid                 
 where 1=1  " + whMitemid+@" ";
            var myList = _context.StockPotionItemDbSet
             .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("FacilityIssueDateWise")]
        public async Task<ActionResult<IEnumerable<FacilityIssueDateWiseDTO>>> FacilityIssueDateWise(string StartDt, string EndDt, string facilityId,string distid)
        {
            //FacOperations f = new FacOperations(_context);
            //Int64 distid = 0;
            string whdist = "";
            string whfac = "";
            string whDate = "";

            if (StartDt != "0" && EndDt != "0")
            {
                whDate = "     AND fs.issueddate BETWEEN TO_DATE('"+ StartDt + "', 'YYYY-MM-DD') AND TO_DATE('"+ EndDt + "', 'YYYY-MM-DD') ";
            }
            if (distid != "0")
            {
                whdist = "  and f.districtid = "+ distid + " ";
            }
          
            if (facilityId != "0")
            {
                whfac = "  and fs.facilityid = "+ facilityId + "  ";
            }
           


            string qry = @"  select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty  ,vm.itemcode,vm.itemname,vm.STRENGTH1 ,b.wardname,fs.issueddate,f.districtid
                     from tbfacilityissues fs 
                     Inner join masfacilities f on f.facilityid = fs.facilityid
                       Inner Join masFacilityWards b on (b.WardID=fs.WardID) 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join vmasitems vm on vm.itemid = fsi.itemid
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  "+ whfac + @"  
                   "+ whDate + @"
                   "+ whdist + @"
                   group by fsi.itemid,fs.facilityid,ftbo.inwno  ,vm.itemcode,vm.itemname,vm.STRENGTH1 ,b.wardname,fs.issueddate,f.districtid
                   order by fs.issueddate desc
                    ";
            var myList = _context.FacilityIssueDateWiseDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }




        //        [HttpGet("IndentPendingDetails")]
        //        public async Task<ActionResult<IEnumerable<WHIndentPendingDetailsDTO>>> IndentPendingDetails(string whid, string clause,string factype)
        //        {
        //            string qry = "";
        //            string whfacttype = "";
        //            if (factype == "364")
        //            {
        //                whfacttype = " and   DMEFAC=1";
        //            }
        //            if (factype == "371")
        //            {
        //                whfacttype = " and   AYUSH=1";
        //            }
        //            if (factype == "367")
        //            {
        //                whfacttype = " and   DHS=1";
        //            }


        //            string whclause = "";
        //            if (clause == "2")
        //            {
        //                whclause = " and   round(sysdate - mn.nocdate)<=7";
        //            }
        //            if (clause == "3")
        //            {
        //                whclause = " and   round(sysdate - mn.nocdate)>7 and round(sysdate - mn.nocdate)<=15";
        //            }
        //            if (clause == "4")
        //            {
        //                whclause = " and   round(sysdate - mn.nocdate)>15";
        //            }

        //            qry = @"  select warehousename,FACILITYNAME,NOCNUMBER,nositems,INDDT,PENDINGDAY,PER,DHS,DMEFAC,AYUSH from 
        //(

        //select  w.warehousename, w.warehouseid, f.facilityname, mn.nocnumber, to_char(mn.nocdate, 'dd-MM-yyyy') nocdate,
        //to_date(mn.nocdate, 'dd-MM-yyyy') inddt
        //, round(sysdate - mn.nocdate) as pendingday
        //,case when(round(sysdate - mn.nocdate))<= 7 then 'Under 7 Days'
        //else case when(round(sysdate - mn.nocdate))> 7 and(round(sysdate - mn.nocdate)) <= 15   then '7-15 Days'
        //else '>15 Days' end end as Per
        //,f.facilitytypeid,
        //case when f.facilitytypeid in (364, 378) then 1 else 0 end as DMEFAC,
        //case when f.facilitytypeid in (371) then 1 else 0 end as AYUSH,
        //case when f.facilitytypeid in (364, 378, 371) then 0 else 1 end as DHS
        //,count(distinct mni.ITEMID) as nositems
        //from mascgmscnoc mn
        //inner join mascgmscnocitems mni on mni.nocid = mn.nocid
        //inner
        //join masfacilities f on f.facilityid = mn.facilityid
        //inner
        //join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
        //inner
        //join masfacilitywh fw on fw.facilityid = f.facilityid
        //inner
        //join maswarehouses w on w.warehouseid = fw.warehouseid
        //where mn.status in ('C')  and mni.bookedqty > 0
        //and mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)
        //and mn.nocdate between SYSDATE - 30 and sysdate
        //and w.warehouseid=" + whid + @" "+whclause+@"
        //group by w.warehousename, w.warehouseid, f.facilityname, mn.nocnumber,mn.nocdate,f.facilitytypeid
        //) where 1=1 "+ whfacttype + @" 
        //order by PENDINGDAY desc";
        //        var myList = _context.WHIndentPendingDetailsDbSet
        //       .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;

        //        }

        //    [HttpGet("MCHAIvsIssuance")]
        //        public async Task<ActionResult<IEnumerable<MCAIVSIssuanceDTO>>> MCHAIvsIssuance(string faciid)
        //        {



        //            FacOperations f = new FacOperations(_context);
        //            string yearid = f.getACCYRSETID();


        //            Int64 WHIID = f.getWHID(faciid);

        //            string qry = @" select mcid,MCATEGORY,count(distinct itemid) as nositems,
        //Round(sum(appivalue)/10000000,2) as ivalue
        //,sum(IssuedCount) as nosissued,Round(sum(IssuedValue)/10000000,2) as issueValued
        //,sum(readystkavailabe) as ReadystkavailableAgainstAI,sum(OnlyPipeline) as NotIssuedOnlyPipeline
        //,sum(balanccount) as NosBalanceStockAvailable,sum(balanccountNotLift) as NotLiftedBalanceStock
        //,Round(sum(AvailableValue)/10000000,2) as TotalBAlStockValue,
        //sum(concernStockoutButAvailableInOTherWH) as concernStockoutButAvailableInOTherWH
        //,sum(IssuedMorethanAI) as IssuedMorethanAI,Round(sum(IssuedMorethanAIExtraVAlue)/10000000,2) as IssuedMorethanAIExtraVAlue
        //,sum(TotalNOCTaken) as TotalNOCTaken,Round(sum(NOCValue)/10000000,2) as TotalNOCValue,
        //sum(NoIssued_andNOCTaken) as NotIssuedStockOutNOCTaken,
        //sum(LPOGen) as LPOGen
        //,Round(sum(lpovalue)/10000000,2) as lpovalue from 
        //(

        //select itemid,mcid, MCATEGORY,itemcode,itemname,strength1,unit,ai,Round(ai* SKUAprxRate,0) as appivalue,IssuedQTY,IssuedValue,NocQty,NOCValue
        //,BalancetobeLiftQTY,(case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
        //else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
        //else 0 end  end) as AvailableStock,
        //Round(((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
        //else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
        //else 0 end  end))* SKUAprxRate,0)
        //as AvailableValue,case when ((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
        //else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
        //else 0 end  end))>0 then 1 else 0 end balanccount,
        //case when ((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
        //else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
        //else 0 end  end))>0 and IssuedQTY=0 then 1 else 0 end balanccountNotLift,
        //whready,whUQC,WHPipeline,SKUAprxRate
        //,TotalNOCTaken,RCcount,IssuedCount,IssuedMorethanAI,NoIssued_andNOCTaken,OnlyPipeline,LPOGen,readystkavailabe,lpovalue
        //,IssuePEr
        //, case when BalancetobeLiftQTY=0 then 100 
        //else case when (whready+whUQC+WHPipeline)>0 and BalancetobeLiftQTY>0 then Round((whready+whUQC+WHPipeline)/BalancetobeLiftQTY*100,0)    else
        //0 end end  as availbleper,
        // case when BalancetobeLiftQTY=0 then 100 
        //else case when (OtherWHReady+OtherWHUQC)>0 and BalancetobeLiftQTY>0 then Round((OtherWHReady+OtherWHUQC)/BalancetobeLiftQTY*100,0)    else
        //0 end end  as availbleperotherWH
        //,OtherWHReady+OtherWHUQC otherWHCGMSCStock
        //,Round(IssuedMorethanAIExtraVAlue,0) as IssuedMorethanAIExtraVAlue
        //,case when IssuedMorethanAI =0 and (nvl(whready,0)+nvl(whUQC,0))=0 and nvl(OtherWHReady,0)+nvl(OtherWHUQC,0)>0
        //then 1 else 0 end as concernStockoutButAvailableInOTherWH
        //from 
        //(
        //select mc.mcid,mc.MCATEGORY,ty.itemtypename,
        //m.itemcode
        //,m.itemname,m.strength1,m.unit
        //,a.ai,nvl(iss.ISS_Qty,0) as IssuedQTY
        //,Round(nvl(IssuedValue,0),0) as IssuedValue
        //,case when a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 then (a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))) else 0 end as BalancetobeLiftQTY
        //,case when a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 then Round((a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))*nvl(SKUrate,0),0) else 0 end as BalancetobeLiftValue
        //,case when ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))>0 and a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 and 
        //((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))-(a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))>0 then a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))
        //else ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)) end as BalanceStkAvialbeinWHQTY
        //,nvl(SKUrate,0) as SKUAprxRate
        //,Round((case when ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))>0 and a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 and 
        //((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))-(a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))>0 then a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))
        //else ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)) end)*nvl(SKUrate,0),0) as BalanceStkAvialbeinWHValue
        //,(nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0)) as whready ,nvl(vwh.UQC,0) as whUQC, nvl(vwh.TOTLPIPELINE,0) WHPipeline
        //,case when (((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)))>0 and (a.ai-nvl(iss.ISS_Qty,0))>0 and nvl(iss.ISS_Qty,0)=0 then 1 else 0 end as BalanceStkAvialbeinWH,
        //case when (nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))>0 then 1 else 0 end as readystkavailabe,
        //case when nvl(IssuedValue,0)=0 and  ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))=0 and nvl(vwh.TOTLPIPELINE,0)>0 then 1 else 0 end as OnlyPipeline,
        //case when nvl(iss.ISS_Qty,0)>0 then Round(nvl(iss.ISS_Qty,0)/nvl(a.ai,1)*100,2) else 0 end as IssuePEr 
        //,nvl(NocQty,0) as NocQty,
        //nvl(NocQty,0)*nvl(SKUrate,0) as NOCValue
        //,nvl(poqty,0) as poqty,nvl(povalue,0) as lpovalue
        //,case when  nvl(NocQty,0)>0  then 1 else 0 end as TotalNOCTaken
        //,case when nvl(NocQty,0)>0 and  nvl(iss.ISS_Qty,0)=0 and   ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))=0 then 1 else 0 end as NoIssued_andNOCTaken
        //,case when nvl(poqty,0)>0 then  1 else 0 end as LPOGen
        //,case when nvl(iss.ISS_Qty,0)>a.ai then  1 else 0 end as IssuedMorethanAI
        //,case when nvl(iss.ISS_Qty,0)>a.ai then (nvl(iss.ISS_Qty,0)-a.ai)*(Round(nvl(IssuedValue,0),0)/nvl(iss.ISS_Qty,0)) else 0 end IssuedMorethanAIExtraVAlue
        //,case when nvl(iss.ISS_Qty,0) >0 then 1 else 0 end IssuedCount
        //,m.unitcount,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as edltype 
        //,case when rc.itemid is not null then 1 else 0 end as RCcount,
        //m.itemid,a.facilityid,nvl(OtherWHReady,0) OtherWHReady,nvl(OtherWHUQC,0) as  OtherWHUQC
        //from  V_InstitutionAI a
        //inner join masitems m on m.itemid=a.itemid
        //left outer join 
        //(
        //select distinct itemid from v_rcvalid
        //) rc on rc.itemid=m.itemid

        //left outer join 
        //(
        //select itemid,APRXRATE,SKUFINALRATE,case when SKUFINALRATE is not null then SKUFINALRATE else case when APRXRATE is not null then APRXRATE else 0 end end as SKUrate 
        //from v_itemrate 
        //) r on r.itemid=m.itemid


        // inner join masitemcategories c on c.categoryid = m.categoryid
        //inner join masitemmaincategory mc on mc.MCID = c.MCID
        //left outer join masitemgroups g on g.groupid =m.groupid
        //left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
        //left outer join masitemcategories c on c.categoryid=m.categoryid
        //left outer join vwhstockwithexp vwh on vwh.itemid=m.itemid and vwh.WAREHOUSEID=" + WHIID+ @"
        //left outer join 
        //(

        //select facilityid,itemid,sum(ISS_Qty) as iss_qty,sum(finalvalue) as IssuedValue
        //from 
        //(
        //select  f.facilityid,tb.indentid,tbi.itemid,tbi.indentitemid,rb.batchno iss_batchno,tbi.needed indentqty,
        //        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,sb.acibasicratenew skurate,acicst,acgstpvalue,
        //        acivat,case when rb.ponoid=1111 then 0 else  sb.acisingleunitprice end  finalrate,
        //        (sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * sb.acibasicratenew) as skuvalue, 
        //        (sum(tbo.issueqty) * sb.acisingleunitprice) as finalvalue,to_char(tb.indentdate,'dd-MM-yyyy') issuedate,tb.indentno,rb.inwno
        //         from tbindents tb
        //         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
        //         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
        //         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
        //         inner join masitems m on m.itemid = tbi.itemid
        //         inner join masfacilities f on f.facilityid = tb.facilityid
        //         left outer join 
        //                (
        //                select s.ponoid,tbo.inwno,si.basicrate, a.indentdate,c.batchno,tbo.indentitemid
        //               ,coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.exciseduty,0))/100) ),2),si.singleunitprice) as supp 
        //               ,case when  a.indentdate >= '01-Jul-2017' then ( case when  aci.basicratenew is null then aci.basicrate else   aci.basicratenew  end) else aci.basicrate end acibasicratenew
        //               ,case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then aci.finalrategst else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end) else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end   ACIsingleunitprice
        //               ,case when aci.cstvat ='CST' then aci.percentvalue  else 0 end ACICST,  
        //               case when aci.cstvat ='VAT' then aci.percentvalue  else 0 end ACIVAT,  
        //               case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then nvl(aci.percentvaluegst,0) else 0 end) else 0 end  ACGSTPvalue   from tbindents a
        //               inner join tbindentitems b on a.indentid = b.indentid
        //               inner join tboutwards tbo on tbo.indentitemid=b.indentitemid
        //               inner join tbreceiptbatches c on  tbo.inwno = c.inwno 
        //               inner join tbreceiptitems ri on ri.receiptitemid=c.receiptitemid
        //               inner join tbreceipts r on r.receiptid=ri.receiptid
        //               left outer join soorderplaced s on s.ponoid=c.ponoid
        //               inner join soordereditems si on  si.ponoid=s.ponoid and si.itemid=b.itemid   
        //               INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
        //               inner join aoccontracts ac on ac.contractid=aci.contractid
        //               where a.Status='C' and a.notindpdmis is null 
        //               and b.notindpdmis is null and c.notindpdmis is null 
        //               and tbo.notindpdmis is null 
        //               group by s.ponoid,tbo.inwno,si.basicrate,a.indentDate,tbo.indentitemid,
        //               c.batchno,si.singleunitprice,si.percentvalue,si.ExciseDuty,aci.basicratenew,aci.basicrate,aci.percentvalue,aci.percentvaluegst,aci.cstvat
        //               ,r.receiptdate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst
        //                 )  sb on sb.inwno=rb.inwno and sb.indentitemid=tbo.indentitemid
        //        where tb.status = 'C' and tb.issuetype='NO'
        //       and  tb.indentdate Between ( select startdate from masaccyearsettings where accyrsetid= "+yearid+ @" ) and ( select enddate from masaccyearsettings where accyrsetid= "+yearid+@" )  and tb.facilityid=" + faciid + @"
        //        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        //        and tbo.notindpdmis is null and rb.notindpdmis is null   
        //        group by tb.indentid,tbi.needed,tbi.itemid,tbi.indentitemid,rb.batchno,
        //      f.facilityid  ,sb.acisingleunitprice,sb.acibasicratenew,tb.indentdate,rb.ponoid ,acicst,acgstpvalue,acivat,tb.indentdate,tb.indentno,rb.inwno

        //        ) group by facilityid,itemid
        //) iss on iss.itemid=m.itemid and iss.facilityid=a.facilityid


        //left outer join 
        //(

        //select itemid,sum(NocQty) as NocQty,nvl(sum(poqty),0) as poqty,nvl(sum(povalue),0) as povalue,nvl(sum(receiptqty),0) as receiptqty
        //from
        //(
        //select mc.MCATEGORY ,m.itemid,m.itemcode,lpitemcode,m.itemname,m.strength1,m.unit,mn.nocid,
        //mn.nocnumber nocno,To_Char(mn.nocdate,'dd-MM-yyyy') as nocdate,
        //mni.approvedqty NocQty,
        //so.tenderno,so.tenderdesc,so.contractno,
        //Round(so.absqty/nvl(m.unitcount,1),0) as poqty,Round(so.singleunitprice,2)*nvl(m.unitcount,1) as singleunitprice
        //,round(so.absqty*so.singleunitprice,2) as povalue
        //,so.suppliername,Round(so.receiptqty/nvl(m.unitcount,1),0) receiptqty,
        //case when LPODate is not null then To_Char(LPODate,'dd-MM-yyyy') else '-' end as  POdate
        //,case when RDate is not null then To_Char(RDate,'dd-MM-yyyy') else '-' end as  RDate,LPBUDGETID,BUDGETNAME
        //from mascgmscnoc mn
        //inner join mascgmscnocitems mni on mni.nocid=mn.nocid and mni.ISCANCEL is null
        //inner join masitems m on m.itemid=mni.itemid
        // inner join masitemcategories c on c.categoryid = m.categoryid
        //inner join masitemmaincategory mc on mc.MCID = c.MCID
        //inner join masfacilities f on f.facilityid=mn.facilityid
        //inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
        //inner join masitemcategories mic on mic.categoryid = m.categoryid
        //left outer join
        //(
        //select nocid,tenderno,tenderdesc,contractno,lpitemid ,lpitemcode,edlitemcode,sum(nvl(absqty,0)) absqty,singleunitprice,sum(povalue) as povalue, 
        //sum(receiptqty)  receiptqty,sum(recvalue) as recvalue,suppliername,max(LPODate) as LPODate,max(RDate) as RDate,LPBUDGETID,BUDGETNAME from  
        //(
        //select t.tenderno,t.tenderdetails tenderdesc,c.contractno,si.nocid,so.ponoid,si.lpitemid,vp.itemcode as lpitemcode,vp.edlitemcode,
        //sum(nvl(si.absqty,0)) absqty,si.singleunitprice,sum(nvl(si.itemvalue,0)) as povalue,
        //nvl(r.receiptqty,0) receiptqty 
        //,nvl(r.receiptqty,0)*nvl(si.singleunitprice,0) as recvalue,s.suppliername,max(PODATE) as LPODate,max(RDate) as RDate,so.LPBUDGETID,lbg.BUDGETNAME
        //from lpsoorderplaced so
        //inner join lpSOORDEREDITEMS si on si.ponoid=so.ponoid
        //inner join masfacilities f on f.facilityid=so.psaid
        //inner join vmasitems vp on vp.itemid=si.lpitemid
        //inner join lpcontracts c on c.contractid = so.contractid
        //inner join lpmastenders t on t.tenderid = c.schemeid
        //inner join lpmassuppliers s on s.LPSUPPLIERID = so.LPSUPPLIERID
        //left outer join maslpbudget lbg on lbg.lpbudgetid=so.LPBUDGETID
        //left outer join 
        //(
        //select tb.ponoid,m.itemid,m.edlitemcode,sum(tbr.absrqty) receiptqty,max(FACRECEIPTDATE) as RDate from tbfacilityreceipts tb
        //inner join tbfacilityreceiptitems tbi on tbi.facreceiptid=tb.facreceiptid
        //inner join tbfacilityreceiptbatches tbr on tbr.facreceiptitemid=tbi.facreceiptitemid
        //inner join masfacilities f on f.facilityid=tb.facilityid
        //inner join vmasitems m on m.itemid=tbi.itemid
        //where   tb.ponoid is not null and m.edlitemcode is not null
        //group by tb.ponoid,m.itemid,m.edlitemcode 
        //) r on r.ponoid= so.ponoid and r.itemid=si.lpitemid
        //where  si.nocid is not null and vp.edlitemcode is not null 
        //group by t.tenderno,t.tenderdetails,c.contractno,si.nocid,si.lpitemid,vp.itemcode,vp.edlitemcode,so.ponoid,r.receiptqty,si.singleunitprice,s.suppliername,so.LPBUDGETID,lbg.BUDGETNAME
        //) group by  nocid,tenderno,tenderdesc,contractno,lpitemid ,lpitemcode,edlitemcode,singleunitprice,suppliername,LPBUDGETID,BUDGETNAME
        //) so on so.nocid=mni.nocid and so.edlitemcode=m.itemcode
        //where 1=1 and mn.status='C' and nvl( mni.approvedqty,0)>0 and mn.nocdate between  ( select startdate from masaccyearsettings where accyrsetid= "+yearid+ @" ) and ( select enddate from masaccyearsettings where accyrsetid= "+yearid+@" ) 
        //  and f.facilityid = " + faciid + @"
        //) group by itemid
        //) nc on nc.itemid=m.itemid

        //left outer join 
        //(
        //select itemid,sum(nvl(ReadyForIssue,0)) as OtherWHReady,sum(nvl(Pending,0)) as OtherWHUQC from 
        //(
        // select z.zoneid ,w.warehouseid,b.batchno, b.expdate, b.inwno, mi.itemid,
        // (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
        // else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
        // case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
        // from tbreceiptbatches b
        // inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
        // inner join tbreceipts t on t.receiptid= i.receiptid
        // inner join maswarehouses w on w.warehouseid=t.warehouseid
        // inner join maszone z on z.zoneid=w.zoneid
        // inner join masitems mi on mi.itemid= i.itemid
        // left outer join
        //     (
        //     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
        //     from tboutwards tbo, tbindentitems tbi , tbindents tb
        //     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
        //     and tb.notindpdmis is null and tbo.notindpdmis is null 
        //     and tbi.notindpdmis is null                   
        //     group by tbi.itemid, tb.warehouseid, tbo.inwno
        //     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
        //     Where  t.warehouseid not in ("+ WHIID + @") and T.Status = 'C'  And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
        //     )
        //     group by itemid
        //) stw on stw.itemid=m.itemid
        //where 1=1
        //and a.facilityid="+ faciid + @" 
        //and accyrsetid="+yearid+ @" and AI>0  
        //)
        //)
        //group by MCATEGORY,mcid
        //order by mcid ";
        //            var myList = _context.MCAIVSIssuanceDBSet
        //           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

        //            return myList;
        //        }
    }
}
