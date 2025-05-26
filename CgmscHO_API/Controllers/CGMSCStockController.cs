using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CgmscHO_API.Models;
using System.Runtime.CompilerServices;
using CgmscHO_API.DTO;
using System.Linq;
using System.Drawing;
using System.Data;
using CgmscHO_API.Utility;
using System.Collections;
using Oracle.ManagedDataAccess.Client;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Net.NetworkInformation;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using MessagePack;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CGMSCStockController : ControllerBase
    {
        private const string V = @"  select A.itemcode as ITEMCODE,A.ItemName || '-' || A.strength1 as ItemName, A.SKU,(case when sum( A.ReadyForIssue)>0 then sum( A.ReadyForIssue) else 0 end) as ReadyForIssue,(case when sum(nvl(Pending,0)) >0 then sum(nvl(Pending,0)) else 0 end) Pending,WAREHOUSENAME 
                  from 
                 (  
                 select w.WAREHOUSENAME,mi.ITEMCODE, b.inwno,mi.ITEMNAME , mi.strength1 ,mi.unit as SKU , 
               (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,  
                   case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end  Pending  
             
                  from tbreceiptbatches b  
                  inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid 
                  inner join tbreceipts t on t.receiptid=i.receiptid 
                  inner join masitems mi on mi.itemid=i.itemid 
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid  
                 inner join masfacilitywh fwh on fwh.warehouseid = w.warehouseid
                 left outer join 
                  (  
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty   
                   from tboutwards tbo, tbindentitems tbi , tbindents tb 
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null 
                   group by tbi.itemid,tb.warehouseid,tbo.inwno  
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid 
                 Where  T.Status = 'C' and (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) And (b.Whissueblock = 0 or b.Whissueblock is null)  
                 and  (mi.itemcode like '%@itemcode%' or  mi.ITEMNAME like '%@itemcode%')
                 and fwh.facilityid = '@facid'
                 and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null 
                 ) A group by WAREHOUSENAME,A.itemcode,A.ItemName, A.strength1,A.SKU order by WAREHOUSENAME ";

        private readonly OraDbContext _context;

        public CGMSCStockController(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("concernWhStock")]
        public async Task<ActionResult<IEnumerable<CGMSCitemWiseStock>>> GetStocks(string id, string facid)
        {
            //string v1 = V.Replace("@itemcode", id);
            //v1 = v1.Replace("@warehouseid", whid);

            var context = new CGMSCitemWiseStock();

            var myList = _context.CGMSCStock
            .FromSqlInterpolated(FormattableStringFactory.Create(
                V.Replace("@itemcode", id).Replace("@facid", facid)))
                .ToList();
            return myList;
        }
        [HttpGet("WHstockReport")]
        public async Task<ActionResult<IEnumerable<CGMSCitemWiseStock>>> WHstockReport(string searchp, string facid, string mcatid)
        {
            FacOperations op = new FacOperations(_context);
            Int64 WHIID = op.getWHID(facid);
            Int32 hodid = op.geFACHOID(facid);
            string whmcatid = "";
            if (mcatid != "0")
            {
                if (hodid == 7)
                {
                    whmcatid = " and msc.subcatid in (" + mcatid + ") ";
                }
                else
                {
                    whmcatid = " and mc.MCID in (" + mcatid + ")";
                }
            }
            string whsearchP = "";
            if (searchp != "0")
            {
                whsearchP = " and(mi.itemcode like '%@" + searchp + @"%' or  mi.ITEMNAME like '%" + searchp + @"%') ";
            }
            string qry = @"  select A.itemcode as ITEMCODE,A.ItemName as ITEMNAME,A.strength1 as STRENGTH1, A.SKU,(case when sum(A.ReadyForIssue) > 0 then sum(A.ReadyForIssue) else 0 end) as READYFORISSUE,(case when sum(nvl(Pending, 0)) > 0 then sum(nvl(Pending,0)) else 0 end) PENDING,WAREHOUSENAME
                  from
                 (
                 select w.WAREHOUSENAME, mi.ITEMCODE, b.inwno, mi.ITEMNAME, mi.strength1, mi.unit as SKU,
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending


                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid
                  inner join masitems mi on mi.itemid = i.itemid
                  inner join masitemcategories c on c.categoryid=mi.categoryid
                  inner join masitemmaincategory mc on mc.MCID=c.MCID
                  left outer join massubitemcategory msc on msc.subcatid=mi.subcatid
                 inner join MASWAREHOUSES w  on w.warehouseid = t.warehouseid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
                   from tboutwards tbo, tbindentitems tbi , tbindents tb
                   where 1=1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   and tb.warehouseid=" + WHIID + @" 
                   group by tbi.itemid,tb.warehouseid,tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) And(b.Whissueblock = 0 or b.Whissueblock is null)
                 " + whsearchP + @"
                  and w.warehouseid = " + WHIID + @" " + whmcatid + @"
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
                 ) A group by WAREHOUSENAME, A.itemcode,A.ItemName, A.strength1,A.SKU     having (sum(A.ReadyForIssue)+sum(nvl(Pending, 0)))>0  order by WAREHOUSENAME ";

            var context = new CGMSCitemWiseStock();

            var myList = _context.CGMSCStock
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }
        [HttpGet("stockReport")]
        public async Task<ActionResult<IEnumerable<StockReportFacilityDTO>>> stockReport(string faclityId, string itemid, string catname)
        {
            FacOperations op = new FacOperations(_context);
            Int32 hodid = op.geFACHOID(faclityId);
            string whcatid = "";
            if (catname == "D")
            {
                if (hodid == 7)
                {
                    whcatid = " and c.categoryid in (58,59,60,61)";
                }
                else
                {
                    whcatid = " and c.categoryid in (52)";
                }
            }
            else if (catname == "C")
            {

                whcatid = " and c.categoryid not in (52,58,59,60,61)";

            }
            else
            {

            }

            string whclause = "";
            if (itemid == null)
            {

            }
            else if (itemid == "")
            {

            }
            else if (itemid == "0")
            {

            }
            else
            {
                whclause = " and mi.itemid=" + itemid;
            }
            string qry = @"   select c.categoryname,mi.ITEMCODE,ty.itemtypename,mi.itemname,mi.strength1,  
                 (case when (b.qastatus ='1' or  mi.qctest='N') then (sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))) end) ReadyForIssue                 
                 ,t.facilityid, mi.itemid,c.categoryid, case when mi.ISEDL2021='Y' then 'EDL' else 'Non EDL' end as EDLType
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 left outer join masitemcategories c on c.categoryid=mi.categoryid
                 left outer join masitemtypes ty on ty.itemtypeid=mi.itemtypeid
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid=" + faclityId + @"          
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where 1=1 " + whclause + @" and  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"  " + whcatid + @"
                and (   (case when (b.qastatus ='1' or  mi.qctest='N') then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) > 0
                group by  mi.ITEMCODE, t.facilityid, mi.itemid,b.qastatus,mi.qctest,mi.itemname,mi.strength1,c.categoryname,c.categoryid,itemtypename,mi.ISEDL2021
                order by c.categoryid, mi.itemname ";

            var context = new StockReportFacilityDTO();

            var myList = _context.StockReportFacilityDTOs
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("facstockReportddl")]
        public async Task<ActionResult<IEnumerable<IndentItemsFromWardDTO>>> facstockReportddl(string faclityId)
        {
            string qry = @"   select itemname||'-'||itemcode as name,ItemID   
from 
(
select mi.ITEMCODE,ty.itemtypename,mi.itemname,mi.strength1,  
                 (case when (b.qastatus ='1' or  mi.qctest='N') then (sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))) end) ReadyForIssue                 
                 ,t.facilityid, mi.itemid,c.categoryid, case when mi.ISEDL2021='Y' then 'EDL' else 'Non EDL' end as EDLType
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 left outer join masitemcategories c on c.categoryid=mi.categoryid
                 left outer join masitemtypes ty on ty.itemtypeid=mi.itemtypeid
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid=" + faclityId + @"         
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                and (   (case when (b.qastatus ='1' or  mi.qctest='N') then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) > 0
                group by  mi.ITEMCODE, t.facilityid, mi.itemid,b.qastatus,mi.qctest,mi.itemname,mi.strength1,c.categoryid,itemtypename,mi.ISEDL2021
    ) order by itemname ";

            var context = new IndentItemsFromWardDTO();

            var myList = _context.IndentItemsFromWardDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getFacilityWards")]
        public async Task<ActionResult<IEnumerable<FacilityWardDTO>>> getFacilityWards(string faclityId)
        {
            string qry = @"   Select a.WardID, a.WardName
                 from masFacilityWards a
                 Where a.IsActive=1 and a.FacilityID=" + faclityId + @"
                 Order By a.WardName ";

            var context = new FacilityWardDTO();

            var myList = _context.FacilityWardDTOs
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getIncomplWardIndentMaster")]
        public async Task<ActionResult<IEnumerable<IncompleteWardIndentDTO>>> getIncompleteWardIndent(Int64 faclityId)
        {
            //"+ faclityId+@"
            string qry = @"  select f.wardname WARDNAME ,NOCNumber as WINDENTNO,
to_char(NOCDate,'dd-mm-yyyy') WREQUESTDATE,d.issueno ISSUENO,to_char(d.issuedate,'dd-mm-yyyy') ISSUEDATE,count(b.itemid) as NOS,
case when d.issueid is null then 'Add' else   NVL(d.Status, 'IN') end as DSTATUS,
d.ISSUEID,a.NOCID as INDENTID,a.WARDID,m2.FACILITYID,a.NOCID, '-' as WREQUESTBY
                          from maswardindents a
                          inner join maswardindentitems b on a.nocid =b.nocid 
                           inner join masfacilitywards f on f.wardid=a.wardid  
                          Inner Join masfacilities m2 on (m2.FacilityID=f.FacilityID) 
                           inner join masdistricts md on md.districtid=m2.districtid 
                          left outer join tbfacilityissues d on d.indentid=a.nocid and IssueType='NO' 
                          where a.status='C' and b.bookedflag in ('B','C') and b.BookedQty >0  
                           and NVL(a.Status, 'I') = ('C') 
                           and f.IsActive = 1 and m2.facilityid=" + faclityId + @"
                           and f.IsActive = 1 and  NVL(d.Status, 'IN')='IN'
group by f.wardname  ,NOCNumber ,NOCDate,issuedate,d.Status,d.issueid,a.NOCID,a.wardid,m2.FacilityID,d.issueno";

            var context = new IncompleteWardIndentDTO();

            var myList = _context.IncompleteWardIndentDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getWardIndentItems")]
        public async Task<ActionResult<IEnumerable<IndentItemsFromWardDTO>>> getWardIndentItems(string nocid)
        {
            string qry = @"  select mi.itemname||'-'||mi.itemcode||'-'||to_char(b.BookedQty) as name,mi.ItemID   
from maswardindents a  
inner join maswardindentitems b on a.nocid =b.nocid   
inner join masfacilitywards f on f.wardid=a.wardid 
inner join vmasItems mi on mi.ItemID=b.ItemID  
left outer join tbfacilityissues d on d.indentid=a.NocID and IssueType='NO'  
left outer join tbfacilityissueitems c on c.ItemID=b.ItemID and c.issueid=d.issueid   
where a.status='C' and b.bookedflag in ('B','C')  and b.BookedQty >0  and  a.NOCID = " + nocid + @"
and c.IssueQty is null 
order by mi.itemname ";

            var context = new IndentItemsFromWardDTO();

            var myList = _context.IndentItemsFromWardDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getIncompleteWardIssue")]
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> getIncompleteWardIssue(string faclityId)
        {
            string qry = @"  Select a.WardID,b.WardName,
            a.IssueNo,a.IssueDate,a.WRequestDate,a.WRequestBy, a.Status,a.IssueID, 0 as indentid
             from tbFacilityIssues a
             Inner Join masFacilityWards b on (b.WardID=a.WardID)
             Inner Join masFacilities fac on (fac.FacilityID=b.FacilityID)    
             Inner Join masAccYearSettings m1 on (a.IssueDate Between m1.StartDate and m1.EndDate)
             Where 1=1 and a.FacilityID=" + faclityId + @" and a.status = 'IN'
             Order By a.IssueDate ";

            var context = new IncompleteWardIssueDTO();

            var myList = _context.IncompleteWardIssueDTOs
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getWardIssueItems")]
        public async Task<ActionResult<IEnumerable<WardIssueItemsDTO>>> getWardIssueItems(string faclityId, string issueId)
        {
            string qry = @"   select mi.ITEMCODE ||'-' || mi.itemname || '-' || to_char( (case when (b.qastatus ='1' or  mi.qctest='N') then (sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))) end) ) as name              
                 , mi.itemid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where 1=1  and fs.facilityid=" + faclityId + @"          
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @" and mi.itemid not in (select distinct itemid from tbfacilityissueitems where issueid = " + issueId + @")
                and (   (case when (b.qastatus ='1' or  mi.qctest='N') then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) > 0
                group by  mi.ITEMCODE, mi.itemid,b.qastatus,mi.qctest,mi.itemname
                order by  mi.itemname ";

            var context = new WardIssueItemsDTO();

            var myList = _context.WardIssueItemsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getItemStock")] //checking purpose
        public async Task<ActionResult<IEnumerable<ItemStockDTO>>> getItemStock(string faclityId, string itemid, Int64 indentid = 0)
        {

            string qry = @" select  to_char( (case when (b.qastatus ='1' or  mi.qctest='N') then (sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))) end) ) as stock,mi.itemid,
nvl(mi.multiple,1) as multiple,nvl(INDENTqty,0) as INDENTQTY
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where 1=1  and fs.facilityid=" + faclityId + @"        
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid  
                    LEFT OUTER JOIN (
                                         select b.BookedQty AS INDENTqty ,B.ItemID 
                    from maswardindents a  
                    inner join maswardindentitems b on a.nocid =b.nocid   
                    inner join masfacilitywards f on f.wardid=a.wardid 
                    left outer join tbfacilityissues d on d.indentid=a.NocID and IssueType='NO'  
                    left outer join tbfacilityissueitems c on c.ItemID=b.ItemID and c.issueid=d.issueid   
                    where a.status='C' and b.bookedflag in ('B','C')  and b.BookedQty >0  and  a.NOCID = " + indentid + @"
                    AND B.ITEMID=" + itemid + @" 
                 )  idt on idt.itemid = mi.itemid
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"   and mi.itemid = " + itemid + @"
                and (   (case when (b.qastatus ='1' or  mi.qctest='N') then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) > 0
                group by   mi.itemid,b.qastatus,mi.qctest,mi.multiple,INDENTqty ";

            var context = new ItemStockDTO();

            var myList = _context.ItemStockDBSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpPost("postWardIssue")]
        public IActionResult postWardIssue(tbFacilityIssueItems ObjtbFacilityIssueItems, string facid)
        {
            try
            {
                _context.tbFacilityIssueItems.Add(ObjtbFacilityIssueItems);
                _context.SaveChanges();
                Int64 issueitemid = getIssueItemid(ObjtbFacilityIssueItems.ISSUEID, ObjtbFacilityIssueItems.ITEMID);
                UpdateFacilityAllotQtyOP(facid, ObjtbFacilityIssueItems.ITEMID, ObjtbFacilityIssueItems.ISSUEQTY, issueitemid);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("getIncompleteWardIssueItems")]
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueItemsDTO>>> getIncompleteWardIssueItems(string issueId)
        {
            string qry = @" Select m1.ItemCode,m1.ItemName,
             m1.Strength1 as Strength,sum(tbo.IssueQty) as IssueQty,tbr.batchno,tbr.expdate,r.LOCATIONNO     
             ,a.IssueItemID,
             m1.ItemID
             From vmasItems m1       
             Inner join tbFacilityIssueItems a on (a.ItemID=m1.ItemID)
             Inner join tbFacilityOutwards tbo on tbo.issueItemid = a.issueItemId
             Inner join tbfacilityreceiptbatches tbr on tbr.inwno = tbo.inwno
              Inner join tbFacilityIssues a1 on (a1.IssueID=a.IssueID)
             left outer join masracks r on r.rackid = tbr.STOCKLOCATION and r.warehouseid = a1.facilityid            
             Where 1=1 and a.issueid=" + issueId + @"  
             group by m1.ItemCode,m1.ItemName, m1.Strength1, tbr.batchno,tbr.expdate,r.LOCATIONNO     
             ,a.IssueItemID,
             m1.ItemID
             Order by a.IssueItemid desc ";

            var context = new IncompleteWardIssueItemsDTO();

            var myList = _context.IncompleteWardIssueItemsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        private Int64 getIssueItemid(Int32 issueId, Int32 itemid)
        {
            string qry = @" Select IssueItemID,ISSUEID,ItemID from tbFacilityIssueItems where IssueID=" + issueId + " and ItemID=" + itemid;


            var context = new getIssueItemIdDTO();

            var myList = _context.getIssueItemIdDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            if (myList.Count > 0)
            {
                int issueItemID = myList[0].ISSUEITEMID; // Assuming IssueItemID is an integer
                return Convert.ToInt64(issueItemID.ToString());
            }
            else
            {
                return 0; // Or any other appropriate indication
            }
        }


        private void deleteIssueItemIdIfExist(Int64 IssueItemID)
        {
            string qry = "Delete from tbFacilityOutwards where IssueItemID=" + IssueItemID;

            var context = new getIssueItemIdDTO();

            _context.Database.ExecuteSqlRaw(qry);

            // _context.getIssueItemIdDbSet
            //.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();


        }

        [HttpDelete("deleteIncompleteIssueItems")]
        public IActionResult deleteIncompleteIssueItems(Int64 IssueItemID)
        {
            deleteIssueItemIdIfExist(IssueItemID);
            string qry = "Delete from tbfacilityissueitems where IssueItemID=" + IssueItemID;
            var context = new getIssueItemIdDTO();
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Issue Items Deted from tbfacilityissueitems and tbFacilityOutwards");

        }




        [HttpPost("posttboOutWards")]
        public void posttboOutWards(tbFacilityOutwardsModel ObjtbFacilityOutwardsModel)
        {
            try
            {
                _context.tbFacilityOutwardsDbSet.Add(ObjtbFacilityOutwardsModel);
                _context.SaveChanges();

            }
            catch (Exception ex)
            {

            }
        }



        [HttpPost("postIssueNo")]
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> postWardIssue(string facid, tbFacilityGenIssue objeIssue)
        {
            try
            {
                FacOperations ob = new FacOperations(_context);
                string validIssueDate = ob.FormatDate(objeIssue.ISSUEDATE);
                objeIssue.ISSUEDATE = validIssueDate;
                objeIssue.ISSUEDDATE = validIssueDate;
                objeIssue.WREQUESTDATE = validIssueDate;
                objeIssue.ISSUETYPE = "NO";
                objeIssue.ISUSEAPP = "Y";

                //string DataIDate = objeIssue.ISSUEDATE;
                string issueno = getGeneratedIssueNo(facid);
                objeIssue.ISSUENO = issueno;

                _context.tbFacilityGenIssueDbSet.Add(objeIssue);
                _context.SaveChanges();
                Int64 ffcid = Convert.ToInt64(facid);
                var myObj = getIssueDetails(issueno, ffcid);
                return Ok(myObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }


        [HttpPost("postIssueNoAgainstIndent")]  //gyan
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> postIssueNoAgainstIndent(tbFacilityGenIssue objeIssue)  //indentid is nocid
        {
            //GetMonthName(string date_ddmmyyyy)

            try
            {
                FacOperations ob = new FacOperations(_context);
                string validIssueDate = ob.FormatDate(objeIssue.ISSUEDATE);
                objeIssue.ISSUEDATE = validIssueDate;
                objeIssue.ISSUEDDATE = validIssueDate;

                ob.getIndentData(Convert.ToInt64(objeIssue.INDENTID), out string? nocDate, out Int64? wardid);
                objeIssue.WREQUESTDATE = nocDate;
                objeIssue.WARDID = Convert.ToInt64(wardid);
                objeIssue.ISSUETYPE = "NO";
                objeIssue.ISUSEAPP = "Y";


                string issueno = getGeneratedIssueNo(objeIssue.FACILITYID.ToString());
                objeIssue.ISSUENO = issueno;

                _context.tbFacilityGenIssueDbSet.Add(objeIssue);
                _context.SaveChanges();

                Int64 ffcid = Convert.ToInt64(objeIssue.FACILITYID.ToString());
                var myObj = getIssueDetails(issueno, ffcid);
                return Ok(myObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }


        [HttpGet("getIssueDetails")]
        private async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> getIssueDetails(string Issueno, Int64 facid)
        {
            string qry = @"SELECT a.WardID WARDID, b.WardName WARDNAME, a.IssueNo as ISSUENO, a.IssueDate as ISSUEDATE, a.WRequestDate as WREQUESTDATE, a.WRequestBy as WREQUESTBY, a.STATUS, a.IssueID as ISSUEID,a.INDENTID 
                   FROM tbFacilityIssues a
                   INNER JOIN masFacilityWards b ON b.WardID = a.WardID
                   INNER JOIN masFacilities fac ON fac.FacilityID = b.FacilityID
                   INNER JOIN masAccYearSettings m1 ON a.IssueDate BETWEEN m1.StartDate AND m1.EndDate
                   WHERE a.FacilityID = :facid AND a.status = 'IN' AND a.IssueNo = :Issueno
                   ORDER BY a.IssueDate";



            OracleParameter[] parameters = new OracleParameter[]
            {
        new OracleParameter(":facid", OracleDbType.Int64, facid, ParameterDirection.Input),
        new OracleParameter(":Issueno", OracleDbType.Varchar2, Issueno, ParameterDirection.Input)
            };

            var myList = _context.IncompleteWardIssueDTOs
                .FromSqlRaw(qry, parameters)
                .ToList();

            return myList;
        }


        private string UpdateFacilityAllotQtyOP(string FacilityID, Int64 ItemID, Int64 IssueQty, Int64 IssueItemID)
        {
            string WhereConditions = " and (rb.ExpDate >= SysDate or nvl(rb.ExpDate,SysDate) >= SysDate)";


            deleteIssueItemIdIfExist(IssueItemID);

            Int64 TotAllotQty = IssueQty;

            string qry = @" select distinct rb.FACReceiptItemid FacReceiptItemID,rb.BatchNo,rb.MfgDate,rb.ExpDate,nvl(rb.AbsRqty,0) AbsRQty,nvl(x.issueQty,0) AllotQty , nvl(rb.AbsRqty,0)-nvl(x.issueQty,0) avlQty
 , rb.Inwno ,rb.whinwno
  from tbFacilityReceiptBatches rb  
  inner join tbfacilityreceiptitems i on rb.FACreceiptitemid=i.FACreceiptitemid  
  inner join tbfacilityreceipts r on r.FACreceiptid=i.FACreceiptid 
  left outer join (
    select   '' BatchNo,'' MfgDate,'' ExpDate,sum(nvl(a.IssueQty,0)) issueQty, 0 AbsRQty,  
  a.Inwno   
  from  tbfacilityoutwards a
  inner join tbfacilityissueitems tbi on tbi.issueitemid =a.issueitemid   
  inner join tbfacilityissues tb on tb.issueid=tbi.issueid    
  Where  tb.FacilityID=" + FacilityID + @"   and tbi.itemid =" + ItemID + @"  group by a.Inwno    
  ) x on x.inwno=rb.inwno
  where rb.qastatus=1 and r.status='C' and nvl(rb.whissueblock,0) in (0) and r.FacilityID=" + FacilityID + @"   and i.ItemID= " + ItemID + @"    
   and (rb.ExpDate >= SysDate or nvl(rb.ExpDate,SysDate) >= SysDate)  and (nvl(rb.AbsRqty,0)-nvl(x.issueQty,0))>0
   and Round(rb.ExpDate-sysdate,0) >= 15 order by expdate";


            var context = new getBatchesDTO();

            var myList = _context.getBatchesDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();


            // Assuming IssueItemID is an integer
            int issueItemID = 0; // Default value if not found

            // Loop through myList to find the desired object
            foreach (var item in myList)
            {

                Int64 dblCurAllot = 0;
                Int64 availableQty = Convert.ToInt64(item.avlQty.ToString());

                Int64 issueQty = 0;
                string mIssueItemID = IssueItemID.ToString();
                string mItemID = ItemID.ToString();
                string mFacReceiptItemID = item.FacReceiptItemID.ToString();
                string mInWno = item.Inwno.ToString();
                string mWhInWno = item.whinwno.ToString();
                //string mIssueQty = dblCurAllot.ToString();
                string mIssued = "0";


                if (TotAllotQty <= availableQty)
                {
                    issueQty = TotAllotQty;
                    TotAllotQty -= issueQty;


                    //#region Save Data
                    //strSQL = "Insert into tbFacilityOutwards (IssueItemID,ItemID,FacReceiptItemID,IssueQty,Issued,Inwno,whinwno) values (" +
                    //    mIssueItemID + "," + mItemID + "," + mFacReceiptItemID + "," + issueQty + "," + mIssued + "," + mInWno + ",'" + mWhInWno + "' )";
                    //DBHelper.GetDataTable(strSQL);
                    //#endregion
                    tbFacilityOutwardsModel obj = new tbFacilityOutwardsModel();
                    obj.ISSUEITEMID = Convert.ToInt64(mIssueItemID);
                    obj.ITEMID = Convert.ToInt64(mItemID);
                    obj.FACRECEIPTITEMID = Convert.ToInt64(mFacReceiptItemID);
                    obj.ISSUEQTY = Convert.ToInt64(issueQty);
                    obj.ISSUED = Convert.ToInt64(mIssued);
                    obj.INWNO = Convert.ToInt64(mInWno);
                    obj.WHINWNO = Convert.ToInt64(mWhInWno);

                    posttboOutWards(obj);

                    break;


                }

                else
                {
                    issueQty = availableQty;
                    TotAllotQty -= issueQty;
                    //#region Save Data
                    //strSQL = "Insert into tbFacilityOutwards (IssueItemID,ItemID,FacReceiptItemID,IssueQty,Issued,Inwno,whinwno) values (" +
                    //    mIssueItemID + "," + mItemID + "," + mFacReceiptItemID + "," + issueQty + "," + mIssued + "," + mInWno + ",'" + mWhInWno + "')";
                    //DBHelper.GetDataTable(strSQL);
                    //#endregion

                    tbFacilityOutwardsModel obj = new tbFacilityOutwardsModel();
                    obj.ISSUEITEMID = Convert.ToInt64(mIssueItemID);
                    obj.ITEMID = Convert.ToInt64(mItemID);
                    obj.FACRECEIPTITEMID = Convert.ToInt64(mFacReceiptItemID);
                    obj.ISSUEQTY = Convert.ToInt64(issueQty);
                    obj.ISSUED = Convert.ToInt64(mIssued);
                    obj.INWNO = Convert.ToInt64(mInWno);
                    obj.WHINWNO = Convert.ToInt64(mWhInWno);
                    if (TotAllotQty == 0)
                    {
                        break;
                    }
                    else
                    {
                        posttboOutWards(obj);
                        continue;


                    }


                }

            }

            return "Test";
        }






        [HttpGet("getHoldStock")]
        public async Task<ActionResult<IEnumerable<HoldItemStockDTO>>> getHoldItemStock(string faclityId)
        {
            //faclityId = "23348";
            string qry = @" select ITEMCODE,ITEMNAME,strength1,batchno,expdate,HoldStock,whissueblock ,inwno
   from 
   (
    select mi.ITEMCODE,mi.ITEMNAME, mi.strength1 ,b.batchno,b.expdate,
               (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) as HoldStock, (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) Pending  
              ,case when b.whissueblock=1 then 'Hold' else 'Unhold' end whissueblock,b.inwno   from tbfacilityreceiptbatches b   
                inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                inner join vmasitems mi on mi.itemid=i.itemid 
                inner join masfacilities f  on f.facilityid=t.facilityid 
                left outer join 
                (  
                  select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                    from tbfacilityissues fs 
                  inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                  inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                  where fs.status = 'C'                 
                  group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                Where  T.Status = 'C'  And (b.Whissueblock =1 OR b.QAstatus=2) and b.expdate>sysdate 
                and t.facilityid=" + faclityId + @" ) where HoldStock>0 ";

            var context = new HoldItemStockDTO();

            var myList = _context.HoldItemStockDBSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getNearExpStock")]
        public async Task<ActionResult<IEnumerable<NearExpBatchDTO>>> getNearExpBatchStock(string faclityId, string criteria)
        {
            //string faclityId1 = "23378";
            //string catidID1 = "52";
            //string criteria1 = "60";
            string whclause = ""; string whcatid = "";
            if (criteria != "0")
            {
                whclause = " and Round(b.EXPDATE-Sysdate)<=" + criteria;
            }


            string qry = @" select mi.ITEMCODE,mi.itemname as ITEMNAME,mi.strength1,b.BATCHNO,b.EXPDATE, (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)  as FACSTOCK
   ,case when Round(b.EXPDATE-Sysdate)<30 then 'Under 1 Month'
   else case when Round(b.EXPDATE-Sysdate)>30 and Round(b.EXPDATE-Sysdate)<=60 then 'Under 2 Month'
   else case when Round(b.EXPDATE-Sysdate)>60 and Round(b.EXPDATE-Sysdate)<=90 then 'Under 3 Month'
    else case when Round(b.EXPDATE-Sysdate)>90 and Round(b.EXPDATE-Sysdate)<=120 then 'Under 4 Month'
        else case when Round(b.EXPDATE-Sysdate)>120 and Round(b.EXPDATE-Sysdate)<=150 then 'Under 5 Month'
        else 'Under 6 Month' 
        end end end end end as EXPTIMELINE,b.inwno as INWNO
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid= " + faclityId + @"         
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and f.facilityid= " + faclityId + @" 
                and (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)>0
                and Round(b.EXPDATE-Sysdate)<180
                " + whclause + @"
  order by b.EXPDATE ";

            var context = new NearExpBatchDTO();

            var myList = _context.NearExpBatchDTODBSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getFacilityInfo")]
        public async Task<ActionResult<IEnumerable<FacilityInfoDTO>>> getFacilityInfo(Int64 faclityId)
        {



            string qry = @" select f.FACILITYNAME,f.FACILITYCODE,f.FACILITYID,h.FOOTER1,h.FOOTER2,h.FOOTER3,h.EMAIL,
d.districtid,d.districtname,ft.FACILITYTYPECODE,ft.FACILITYTYPEDESC,ft.ELDCAT,fw.warehouseid,w.WAREHOUSENAME,w.email as whemail,w.phone1 as whcontact  from masfacilities f
                            inner
                                                                                                                                                       join usrusers u on u.FACILITYID = f.facilityid
                            left outer join MASFACHEADERFOOTER h on h.userid = u.userid
                            inner join masfacilitywh fw on fw.facilityid = f.facilityid
                            inner join maswarehouses w on w.WAREHOUSEID = fw.WAREHOUSEID
                            inner join masfacilitytypes ft on ft.FACILITYTYPEID = f.FACILITYTYPEID
                            inner join masdistricts d on d.districtid = f.districtid
                            where f.facilityid =  " + faclityId + "";

            var context = new FacilityInfoDTO();

            var myList = _context.FacilityInfoDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getGeneratedIssueNo")]
        public string getGeneratedIssueNo(string facId)
        {

            FacOperations op = new FacOperations(_context);
            string getNo = op.FacAutoGenerateNumbers(facId, false, "NO");
            return getNo;

        }

        [HttpGet("getReceiptIssueNo")]
        public string getReceiptIssueNo(string facId)
        {

            FacOperations op = new FacOperations(_context);
            string getNo = op.FacAutoGenerateNumbers(facId, true, "NO");
            return getNo;

        }

        [HttpPost("postReceiptMaster")]
        public async Task<ActionResult<IEnumerable<ReceiptMasterDTO>>> postReceiptMaster(string facid, tbFacilityReceiptsModel objeReceipt)
        {

            try
            {
                FacOperations ob = new FacOperations(_context);
                string validRceceiptDate = ob.FormatDate(objeReceipt.FACRECEIPTDATE);
                objeReceipt.FACRECEIPTDATE = validRceceiptDate;
                objeReceipt.FACRECEIPTTYPE = "NO";
                objeReceipt.ISUSEAPP = "Y";
                //objeReceipt.STATUS = "I";

                //string DataIDate = objeIssue.ISSUEDATE;
                string receiptNo = getReceiptIssueNo(facid);
                objeReceipt.FACRECEIPTNO = receiptNo;

                _context.tbFacilityReceiptsDbSet.Add(objeReceipt);
                _context.SaveChanges();
                Int64 ffcid = Convert.ToInt64(facid);
                var myObj = getReceiptDetails(Convert.ToInt64(objeReceipt.INDENTID), ffcid);
                return Ok(myObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpGet("getReceiptDetails")]
        private async Task<ActionResult<IEnumerable<ReceiptMasterDTO>>> getReceiptDetails(Int64 indentId, Int64 facid)
        {


            string qry = @"select faci.nocid,faci.NOCNUMBER as ReqNo ,faci.nocdate as ReqDate, fr.IndentId,tb.INDENTNO WHIssueNo, tb.INDENTDATE as WHIssueDT, FacReceiptNo,FacReceiptDate,fr.FACRECEIPTID from tbFacilityReceipts fr
inner join tbIndents tb on tb.IndentId=fr.IndentId
inner join mascgmscnoc  faci on faci.nocid=tb.NOCID
where fr.facilityid=:facid and tb.IndentID=:indentid";



            OracleParameter[] parameters = new OracleParameter[]
            {
                new OracleParameter(":facid", OracleDbType.Int64, facid, ParameterDirection.Input),
                new OracleParameter(":indentid", OracleDbType.Varchar2, indentId, ParameterDirection.Input)
            };

            var myList = _context.ReceiptMasterDTODbSet
                .FromSqlRaw(qry, parameters)
                .ToList();

            return myList;
        }


        [HttpGet("getItemCategory")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> getCategory(string faclityId)
        {
            string qry = @" select  CATEGORYNAME,CATEGORYID from masitemcategories
where categoryid in (52,54,58,59,60,61,62)
order by categoryid ";

            var context = new CategoryDTO();

            var myList = _context.CategoryDTODbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getMainCategory")]
        public async Task<ActionResult<IEnumerable<CategoryDTO>>> getMainCategory(string faclityId)
        {

            FacOperations op = new FacOperations(_context);
            string qry = "";
            if (faclityId == "HOD")
            {
                qry = @"  select mcid as CATEGORYID, mcategory as CATEGORYNAME from masitemmaincategory where 1=1 ";
            }
            else if (faclityId != "HOD")
            {
                Int32 hodid = op.geFACHOID(faclityId);
                string whcatid = "";

                if (hodid == 7)
                {
                    qry = @"  select subcatid as CATEGORYID,CATNAME  as CATEGORYNAME from massubitemcategory ";

                }
                else
                {

                    qry = @"  select mcid as CATEGORYID, mcategory as CATEGORYNAME from masitemmaincategory where mcid not in (4)";
                }
            }
            else
            {
                qry = @"  select mcid as CATEGORYID, mcategory as CATEGORYNAME from masitemmaincategory where 1=1 ";
            }

            var context = new CategoryDTO();

            var myList = _context.CategoryDTODbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }




        [HttpGet("getReceiptMasterFromWH")]
        public async Task<ActionResult<IEnumerable<ReceiptMasterWHDTO>>> getReceiptMaster(string faclityId)
        {
            string qry = @"  select faci.nocid, faci.nocdate as ReqDate, faci.NOCNUMBER as ReqNo, nositemsReq, tb.INDENTNO WHIssueNo, tb.INDENTDATE as WHIssueDT, nositemsIssued, tb.indentid
, fr.FACRECEIPTNO, fr.FACRECEIPTDATE,case when fr.STATUS='I' then 'IN' else  nvl(fr.STATUS,'IN') end as RStatus
, fr.FACRECEIPTID,faci.FacilityID,tb.warehouseid
from mascgmscnoc faci
inner join
(
select count(distinct ri.itemid) nositemsReq, ri.nocid from mascgmscnocitems ri
where 1=1 and ri.BOOKEDQTY>0
group by ri.nocid
) ri on ri.nocid=faci.nocid
left outer join tbIndents tb on tb.NOCID=faci.nocid
left outer join
(
select count(distinct tbi.itemid) nositemsIssued, tbi.indentid from tbIndentItems tbi
group by tbi.indentid
) tbi on tbi.indentid=tb.indentid
left outer join tbFacilityReceipts fr on fr.FacilityID=faci.FacilityID and fr.IndentId= tb.IndentId

where 1=1 
and faci.ACCYRSETID>=544  and  faci.facilityid =" + faclityId + @"  
--and  faci.facilityid ="" + faclityId+@""   
and nvl(fr.STATUS,'I')='I'
order by faci.nocid desc";
            //    --  and  faci.facilityid in (22480,23417)
            var context = new ReceiptMasterWHDTO();

            var myList = _context.ReceiptMasterDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

            return myList;

        }


        [HttpGet("getReceiptItemsDDL")]
        public async Task<ActionResult<IEnumerable<ReceiptItemsDDL>>> getReceiptItemsDDL(string faclityId, string FACRECEIPTID, string IndentID)
        {
            string qry = @" select itemname||'-'||itemcode||'-Batch-'||batchno||':'||to_char(IssueWH) as name,InwNo 
from 
(
Select distinct m.itemid,m.itemcode,m.itemname,rb.batchno,rb.expdate,tbo.IssueQty*nvl(m.unitcount,1) as IssueWH
,tfr.IndentID,tfr.FACRECEIPTID,tfi.ABSRQTY,rfb.ABSRQTY as BatrchReceiptQTY
,nvl(rc.locationno,'-') locationno,rfb.stocklocation,tfi.facreceiptitemid,rb.InwNo,tfr.status Rstatus,tfi.status RIstatus
             from tbIndentItems tbi
             left outer Join tbOutwards tbo on (tbo.IndentItemID = tbi.IndentItemID) 
             left outer Join tbReceiptBatches rb on (rb.InwNo = tbo.InwNo) 
             Inner Join masItems m on (m.ItemID=tbi.ItemID)
             Inner Join tbIndents tb on (tb.Indentid=tbi.IndentId) 
            left outer join  tbFacilityReceipts tfr on (tfr.IndentID=tb.IndentID) and tfr.facilityid=" + faclityId + @"
            Left Outer Join tbFacilityReceiptItems tfi on  tfi.FacReceiptID=tfr.FacReceiptID and tfi.itemid=tbi.itemid    
            and tfi.INDENTITEMID=tbi.INDENTITEMID
            left outer join tbfacilityreceiptbatches rfb on rfb.facreceiptitemid=tfi.facreceiptitemid
            left outer join masracks rc on rc.rackid=rfb.stocklocation
             Where 1=1  and tb.FacilityID =" + faclityId + @"
             and tb.IndentID =" + IndentID + @" and tfr.FACRECEIPTID=" + FACRECEIPTID + @"  and tbi.ItemID not in (select itemid from tbFacilityReceiptItems where FACRECEIPTID=" + FACRECEIPTID + @" )
)order by itemname ";

            var context = new ReceiptItemsDDL();

            var myList = _context.ReceiptItemsDDLDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }

        [HttpGet("getReceiptItemsDetail")]
        public async Task<ActionResult<IEnumerable<ReceiptItemsDDL>>> getReceiptItemsDetail(string faclityId, string FACRECEIPTID, string IndentID, string inwno)
        {
            string qry = @" select IssueWH  as name,InwNo 
from 
(
Select distinct m.itemid,m.itemcode,m.itemname,rb.batchno,rb.expdate,tbo.IssueQty*nvl(m.unitcount,1) as IssueWH
,tfr.IndentID,tfr.FACRECEIPTID,tfi.ABSRQTY,rfb.ABSRQTY as BatrchReceiptQTY
,nvl(rc.locationno,'-') locationno,rfb.stocklocation,tfi.facreceiptitemid,rb.InwNo,tfr.status Rstatus,tfi.status RIstatus
             from tbIndentItems tbi
             left outer Join tbOutwards tbo on (tbo.IndentItemID = tbi.IndentItemID) 
             left outer Join tbReceiptBatches rb on (rb.InwNo = tbo.InwNo) 
             Inner Join masItems m on (m.ItemID=tbi.ItemID)
             Inner Join tbIndents tb on (tb.Indentid=tbi.IndentId) 
            left outer join  tbFacilityReceipts tfr on (tfr.IndentID=tb.IndentID) and tfr.facilityid=" + faclityId + @"
            Left Outer Join tbFacilityReceiptItems tfi on  tfi.FacReceiptID=tfr.FacReceiptID and tfi.itemid=tbi.itemid    
            and tfi.INDENTITEMID=tbi.INDENTITEMID
            left outer join tbfacilityreceiptbatches rfb on rfb.facreceiptitemid=tfi.facreceiptitemid
            left outer join masracks rc on rc.rackid=rfb.stocklocation
             Where 1=1  and tb.FacilityID =" + faclityId + @"
             and rb.InwNo = " + inwno + @"
             and tb.IndentID =" + IndentID + @" and tfr.FACRECEIPTID=" + FACRECEIPTID + @"  and tbi.ItemID not in (select itemid from tbFacilityReceiptItems where FACRECEIPTID=" + FACRECEIPTID + @" )
) ";

            var context = new ReceiptItemsDDL();

            var myList = _context.ReceiptItemsDDLDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }

        [HttpGet("getRacks")]
        public async Task<ActionResult<IEnumerable<MasRackDTO>>> getRacks(string WH_FACID)
        {
            string qry = @" select LOCATIONNO, RACKID from masracks r where r.WAREHOUSEID=" + WH_FACID + @"
and IsDeactiveated is null
order by LOCATIONNO ";

            var context = new MasRackDTO();

            var myList = _context.MasRackDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }


        [HttpGet("getReceiptDetails")]
        public async Task<ActionResult<IEnumerable<ReceiptDetailsDTO>>> getReceiptDetails(string facilityReceiptType, string facilityId, string facReceiptId)
        {
            string qry = @" select m.itemcode,m.itemname,m.strength1, rfb.batchno,rfb.ABSRQTY,rfb.EXPDATE,nvl(rc.locationno,'-') locationno,rfb.stocklocation ,rfb.INWNO,tri.itemid,tr.FACRECEIPTID,tri.facreceiptitemid  from tbfacilityreceipts tr
inner join tbfacilityreceiptitems tri on tri.FACRECEIPTID=tr.FACRECEIPTID
inner join masitems m on m.itemid = tri.itemid
inner join  tbfacilityreceiptbatches rfb on rfb.facreceiptitemid=tri.facreceiptitemid
inner  join masracks rc on rc.rackid=rfb.stocklocation
where 1=1 and tr.FACRECEIPTTYPE='" + facilityReceiptType + "' and tr.facilityid=" + facilityId + " and tr.FACRECEIPTID=" + facReceiptId + " ";


            var myList = _context.ReceiptDetailsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;


        }





        [HttpGet("getIndentAlert")]
        public async Task<ActionResult<IEnumerable<ReceiptMasterWHDTO>>> getIndentAlert(string faclityId, string CatID, string status)
        {

            string qry = @"  select itemcode,edlCatName,itemtypename, EDL,itemname,strength1, AIFacility,ApprovedAICMHO,FACReqFor3Month,facstock,FACReqFor3Month-facstock as RequiredQTY,WIssueQTYYear,WHReady,Remarks,ITEMTYPEID,edlcat from 
(
select m.itemcode,ty.itemtypename,e.edl as edlCatName,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDL,m.itemname,m.strength1,ai.facilityindentqty AIFacility,ai.cmhodistqty ApprovedAICMHO,nvl(round(ai.cmhodistqty/12,0)*3,0) as FACReqFor3Month 
,nvl(fs.facStock,0) as facstock,nvl(wi.WHIssueYear,0) as WIssueQTYYear,wh.Ready*nvl(m.unitcount,1) as WHReady,wh.warehouseid
,case when  round(nvl(fs.facStock,0),0)< nvl(round(ai.cmhodistqty/12,0)*3,0) then 'Need To Be Indent to CGMSC' else 'Stock Availble For 3 Month' end as Remarks
,ty.ITEMTYPEID,e.edlcat
from masanualindent a
inner join anualindent ai on ai.indentid=a.indentid
inner join masitems m on m.itemid=ai.itemid
left outer join masedl e on e.edlcat=m.edlcat
left outer join masitemtypes ty on ty.ITEMTYPEID=m.ITEMTYPEID
left outer join 
     (  
                 select mi.ITEMCODE, sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid= 23378         
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                group by mi.ITEMCODE
                having (sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
) fs on  fs.ITEMCODE=m.itemcode


left outer join 
(
  select t.warehouseid, i.itemid,
    sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0)) Ready,
    sum(nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending 
    from tbreceiptbatches b    
    inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
    inner join tbreceipts t on t.receiptid=i.receiptid
    inner join masitems mi on mi.itemid=i.itemid  
    left outer join  
    (    
    select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
    from tboutwards tbo, tbindentitems tbi , tbindents tb  
    where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid 
    and tb.status = 'C' and tb.notindpdmis is null 
    and tbo.notindpdmis is null and tbi.notindpdmis is null    
    group by tbi.itemid,tb.warehouseid,tbo.inwno    
    ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
    Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
    and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and mi.categoryid=52
    and   t.warehouseid=2628
    group by t.warehouseid, i.itemid
    having  (sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0))>0)
) wh on wh.itemid=m.itemid

left outer join 
(
select tbi.itemid,sum(tbo.ISSUEQTY*nvl(m.unitcount,1)) as WHIssueYear from  tbIndents tb 
inner join   tbIndentItems tbi on tbi.indentid=tb.indentid
inner join masitems m on m.itemid=tbi.itemid
inner join tboutwards tbo on tbo.INDENTITEMID=tbi.INDENTITEMID
inner  Join masAccYearSettings yr on tb.IndentDate between yr.StartDate and yr.EndDate
where tb.facilityid=" + faclityId + @" and tb.status='C' and tb.issuetype='NO'
and m.categoryid=52
and yr.AccYrSetID =544
group by tbi.itemid,m.itemname,m.itemcode,m.unitcount
) wi on wi.itemid=m.itemid
where 1=1  and  ai.cmhodistqty>0 and a.accyrsetid=544 and a.status='C' and  a.facilityid=" + faclityId + @" 
and m.categoryid=52 and wh.Ready>0
) where Remarks='Need To Be Indent to CGMSC'
order  by itemname ";

            var context = new ReceiptMasterWHDTO();

            var myList = _context.ReceiptMasterDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpPut("completeWardIssues")]
        public IActionResult completeWardIssues(Int64 IssueID)
        {
            string strQuery1 = "Update tbFacilityIssues set Status='C',IssuedDate=sysdate Where IssueID=" + IssueID;
            Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);

            string strQuery2 = @"Update tbFacilityOutwards set Status='C', Issued=1
                              Where IssueItemID in (Select IssueItemID from tbFacilityIssueItems Where IssueID=" + IssueID + ")";
            Int32 result2 = _context.Database.ExecuteSqlRaw(strQuery2);

            string strQuery3 = "Update tbFacilityIssueItems set Status='C',Issued=1 Where IssueID=" + IssueID;
            Int32 result3 = _context.Database.ExecuteSqlRaw(strQuery3);

            Int32 totResult = result1 + result2 + result3;

            if (totResult != 0)
            {
                return Ok("Successfully Updated");
            }
            else
            {
                return BadRequest("Some Error Occuered May be all 3 queries are not excecuted successfully.");
            }

        }

        [HttpDelete("deleteWardIssues")]
        public IActionResult deleteWardIssues(Int64 IssueID)
        {
            string strQuery1 = "Delete tbFacilityIssues  Where IssueID=" + IssueID;
            Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);

            string strQuery2 = @"Delete tbFacilityOutwards Where IssueItemID in (Select IssueItemID from tbFacilityIssueItems Where IssueID=" + IssueID + ")";
            Int32 result2 = _context.Database.ExecuteSqlRaw(strQuery2);

            string strQuery3 = "Delete tbFacilityIssueItems Where IssueID=" + IssueID;
            Int32 result3 = _context.Database.ExecuteSqlRaw(strQuery3);

            Int32 totResult = result1 + result2 + result3;

            if (totResult != 0)
            {
                return Ok("Successfully Deleted");
            }
            else
            {
                return BadRequest("Some Error Occuered May be all 3 queries are not excecuted successfully.");
            }

        }

        [HttpGet("getStockPerEDL")]
        public async Task<ActionResult<IEnumerable<StockOutDTO>>> getStockPerEDL(string faclityId, string Mcatid)
        {
            FacOperations f = new FacOperations(_context);
            Int64 WHIID = f.getWHID(faclityId);
            Int64 EDLCAt = f.geFACEDLCat(faclityId);

            string qry = @" 
select MCID,MCATEGORY, count(itemcode) as totalnos,sum(stockout) as stockout,Round(sum(stockout)/count(itemcode)*100,2) as stockoutPer,sum(StockinWH) as StockinWH from 
(
select mc.MCID,MCATEGORY,itemtypename, m.itemcode,e.EDL,m.itemname,nvl(fs.facStock,0) as facstock,m.strength1,case when nvl(fs.facStock,0)=0 then 1 else 0 end as stockout
,case when nvl(fs.facStock,0)=0 and wh.Ready*nvl(m.unitcount,1) >0 then 1 else 0 end as StockinWH
from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join  masedl e on e.edlcat=m.edlcat
left outer join masitemtypes ty on ty.ITEMTYPEID=m.ITEMTYPEID
left outer join 
     (  
     select ITEMCODE,sum(facStock) facStock from 
     (
                 select case when mi.edlitemcode is null then  mi.ITEMCODE else  mi.edlitemcode end as ITEMCODE,
                 sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 

                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid=" + faclityId + @"    
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                group by mi.ITEMCODE,mi.edlitemcode
                having (sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
        ) group by ITEMCODE
) fs on  fs.ITEMCODE=m.itemcode

left outer join 
(
  select t.warehouseid, i.itemid,
    sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0)) Ready,
    sum(nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending 
    from tbreceiptbatches b    
    inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
    inner join tbreceipts t on t.receiptid=i.receiptid
    inner join masitems mi on mi.itemid=i.itemid  
    inner join masitemcategories c on c.categoryid=mi.categoryid
   inner join masitemmaincategory mc on mc.MCID=c.MCID
    left outer join  
    (    
    select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
    from tboutwards tbo, tbindentitems tbi , tbindents tb  
    where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid 
    and tb.status = 'C' and tb.notindpdmis is null 
    and tbo.notindpdmis is null and tbi.notindpdmis is null    
    group by tbi.itemid,tb.warehouseid,tbo.inwno    
    ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
    Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
    and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and mc.MCID = " + Mcatid + @"
    and   t.warehouseid=" + WHIID + @"
    group by t.warehouseid, i.itemid
    having  (sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0))>0)
) wh on wh.itemid=m.itemid
where isedl2021='Y' and mc.MCID=" + Mcatid + @"
and m.isfreez_itpr is null and m.edlcat<=" + EDLCAt + @"
) group by MCATEGORY,MCID";
            //    and  faci.facilityid =" + faclityId+@"   
            var context = new StockOutDTO();

            var myList = _context.StockOutDTODbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getStockPerNonEDLAg_ApprovedAI")]
        public async Task<ActionResult<IEnumerable<StockOutDTO>>> getStockPerNonEDLAg_ApprovedAI(string faclityId, string Mcatid)
        {
            FacOperations f = new FacOperations(_context);
            Int64 WHIID = f.getWHID(faclityId);
            Int64 EDLCAt = f.geFACEDLCat(faclityId);
            string yearid = f.getACCYRSETID();
            string qry = @" 


select MCID,MCATEGORY,count(itemcode) as totalnos,sum(stockout) as stockout,Round(sum(stockout)/count(itemcode)*100,2) as stockoutPer,sum(StockinWH) as StockinWH from 
(
select mc.MCID,MCATEGORY,m.itemcode,m.itemname ,nvl(fs.facStock,0) as facstock,case when nvl(fs.facStock,0)=0 then 1 else 0 end as stockout
,case when nvl(fs.facStock,0)=0 and wh.Ready*nvl(m.unitcount,1) >0 then 1 else 0 end as StockinWH
,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDL
from masanualindent a
inner join anualindent ai on ai.indentid=a.indentid
inner join masitems m on m.itemid=ai.itemid
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join 
     (  
     select ITEMCODE,sum(facStock) facStock from 
     (
                 select case when mi.edlitemcode is null then  mi.ITEMCODE else  mi.edlitemcode end as ITEMCODE,
                 sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid= " + faclityId + @"         
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                group by mi.ITEMCODE,mi.edlitemcode
                having (sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
        ) group by ITEMCODE
) fs on  fs.ITEMCODE=m.itemcode

left outer join 
(
  select t.warehouseid, i.itemid,
    sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0)) Ready,
    sum(nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending 
    from tbreceiptbatches b    
    inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
    inner join tbreceipts t on t.receiptid=i.receiptid
    inner join masitems mi on mi.itemid=i.itemid  
    inner join masitemcategories c on c.categoryid=mi.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
    left outer join  
    (    
    select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
    from tboutwards tbo, tbindentitems tbi , tbindents tb  
    where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid 
    and tb.status = 'C' and tb.notindpdmis is null 
    and tbo.notindpdmis is null and tbi.notindpdmis is null    
    group by tbi.itemid,tb.warehouseid,tbo.inwno    
    ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
    Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
    and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and mc.MCID= " + Mcatid + @" 
    and   t.warehouseid= " + WHIID + @"
    group by t.warehouseid, i.itemid
    having  (sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0))>0)
) wh on wh.itemid=m.itemid
where 1=1 and m.isfreez_itpr is null  and  ai.cmhodistqty>0 and a.accyrsetid=" + yearid + @" and a.status='C' and  a.facilityid=" + faclityId + @"  
 and mc.MCID= " + Mcatid + @" and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='Non EDL'
) group by MCID,MCATEGORY ";
            //    and  faci.facilityid =" + faclityId+@"   
            var context = new StockOutDTO();

            var myList = _context.StockOutDTODbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        //Int64 facilityId, Int64 inwardNo, Int64 facReceiptId, out Int64? indentItemId, out Int64? itemId, out Int64? batchQty, out Int64? facReceiptItemid

        [HttpPost("postReceiptItems")]  //gyan
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> postReceiptItems(Int64 rackID, Int64 facid, Int64 facReceiptId, Int64 whinwno, tbFacilityReceiptItemsModel ObjRItems)  //indentid is nocid
        {
            //GetMonthName(string date_ddmmyyyy)

            try
            {
                FacOperations ob = new FacOperations(_context);

                ob.getWhIssuedItemData(facid, whinwno, facReceiptId, out Int64? indentItemId, out Int64? itemId, out Int64? batchQty, out Int64? facReceiptItemid
                                        , out string? MfgDate, out Int64? ponoid, out Int32? qastatus, out string? whissueblock, out string? expiryDate, out string? batchno);
                ObjRItems.INDENTITEMID = Convert.ToInt64(indentItemId);
                ObjRItems.ITEMID = Convert.ToInt64(itemId);
                ObjRItems.FACRECEIPTID = facReceiptId;
                ObjRItems.ABSRQTY = Convert.ToInt64(batchQty);

                ob.isFacilityReceiptItemsExist(Convert.ToInt64(Convert.ToInt64(itemId)), facReceiptId, out Int64? batchQty1, out Int64? facilityReceiptItemId);
                Int64 FacReceiptitemid = 0;
                if (facilityReceiptItemId != null)
                {
                    Int64 total = Convert.ToInt64(batchQty1) + Convert.ToInt64(batchQty);
                    string strQuery1 = @"update tbFacilityReceiptItems set ABSRQTY= " + total + " where itemid=" + Convert.ToInt64(itemId) + "" +
                        " and FACRECEIPTID=" + facReceiptId + " and  FACRECEIPTITEMID = " + facilityReceiptItemId + "";
                    Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);
                    FacReceiptitemid = Convert.ToInt64(facilityReceiptItemId);
                }
                else
                {

                    _context.tbFacilityReceiptItemsDbSet.Add(ObjRItems);  //
                    _context.SaveChanges();
                    FacReceiptitemid = Convert.ToInt64(ObjRItems.FACRECEIPTITEMID);
                }



                tbFacilityReceiptBatchesModel objRBatches = new tbFacilityReceiptBatchesModel();

                objRBatches.MFGDATE = MfgDate;
                objRBatches.EXPDATE = expiryDate;
                objRBatches.WHISSUEBLOCK = whissueblock;
                objRBatches.BATCHNO = batchno;
                objRBatches.ITEMID = Convert.ToInt64(itemId);
                objRBatches.FACRECEIPTITEMID = Convert.ToInt64(FacReceiptitemid);
                objRBatches.ABSRQTY = Convert.ToInt64(batchQty);
                objRBatches.STOCKLOCATION = Convert.ToInt64(rackID);
                objRBatches.QASTATUS = Convert.ToInt32(qastatus);
                objRBatches.WHINWNO = Convert.ToInt64(whinwno);
                objRBatches.PONOID = Convert.ToInt64(ponoid);

                _context.tbFacilityReceiptBatchesDbSet.Add(objRBatches);
                _context.SaveChanges();


                return Ok(objRBatches);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }

        [HttpPut("completeReceipts")]
        public IActionResult completeReceipts(Int64 receiptId)
        {
            string strQuery1 = "Update tbFacilityReceipts set Status = 'C' Where FacReceiptID = " + receiptId;
            Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);

            string strQuery2 = @"Update tbFacilityReceiptItems set Status = 'C' Where FacReceiptID =" + receiptId;
            Int32 result2 = _context.Database.ExecuteSqlRaw(strQuery2);



            Int32 totResult = result1 + result2;

            if (totResult != 0)
            {
                return Ok("Successfully Updated");
            }
            else
            {
                return BadRequest("Some Error Occuered May be all 3 queries are not excecuted successfully.");
            }

        }

        [HttpDelete("deleteReceipts")]
        public IActionResult deleteReceipts(Int64 receiptId)
        {
            string strQuery1 = "Delete from tbFacilityReceiptItems Where FacReceiptID = " + receiptId;
            Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);

            string strQuery2 = @"Delete from tbFacilityReceipts where FacReceiptID =" + receiptId + ")";
            Int32 result2 = _context.Database.ExecuteSqlRaw(strQuery2);

            string strQuery3 = "delete from tbfacilityreceiptbatches where FACReceiptitemid in (select facreceiptitemid from tbfacilityreceiptitems where facreceiptid=" + receiptId + ")";
            Int32 result3 = _context.Database.ExecuteSqlRaw(strQuery3);

            Int32 totResult = result1 + result2 + result3;

            if (totResult != 0)
            {
                return Ok("Successfully Deleted");
            }
            else
            {
                return BadRequest("Some Error Occuered May be all 3 queries are not excecuted successfully.");
            }

        }


        [HttpDelete("deleteReceiptItems")]
        public IActionResult deleteReceiptItems(Int64 inwno, Int64 facReceiptItemId, Int64 itemid, Int64 receiptId, Int64 deletedBatchQty)
        {
            Int32 resultdel1 = 0;
            Int32 resultdel2 = 0;
            Int32 resultdel3 = 0;
            Int32 resultdel4 = 0;

            FacOperations ob = new FacOperations(_context);
            ob.isFacilityReceiptItemsExist(Convert.ToInt64(Convert.ToInt64(itemid)), receiptId, out Int64? batchQty1, out Int64? facilityReceiptItemId);
            Int64 FacReceiptitemid = 0;
            if (deletedBatchQty == batchQty1)
            {
                //Int64 total = Convert.ToInt64(batchQty1) + Convert.ToInt64(batchQty);
                //string strQuery1 = @"update tbFacilityReceiptItems set ABSRQTY= " + total + " where itemid=" + Convert.ToInt64(itemId) + "" +
                //    " and FACRECEIPTID=" + facReceiptId + " and  FACRECEIPTITEMID = " + facilityReceiptItemId + "";
                //Int32 result1 = _context.Database.ExecuteSqlRaw(strQuery1);
                //FacReceiptitemid = Convert.ToInt64(facilityReceiptItemId);
                string strQuery5 = "Delete from tbFacilityReceiptItems Where FACRECEIPTITEMID = " + facReceiptItemId;
                resultdel1 = _context.Database.ExecuteSqlRaw(strQuery5);
                string strQuery6 = " Delete from TBFACILITYRECEIPTBATCHES where FACRECEIPTITEMID =" + facReceiptItemId + @" and inwno = " + inwno;
                resultdel2 = _context.Database.ExecuteSqlRaw(strQuery5);


            }
            else
            {

                string strQuery5 = "Update  tbFacilityReceiptItems set ABSRQTY = " + (batchQty1 - deletedBatchQty) + " Where FACRECEIPTITEMID = " + facReceiptItemId;
                resultdel3 = _context.Database.ExecuteSqlRaw(strQuery5);
                string strQuery6 = " Delete from TBFACILITYRECEIPTBATCHES where FACRECEIPTITEMID =" + facReceiptItemId + @" and inwno = " + inwno;
                resultdel4 = _context.Database.ExecuteSqlRaw(strQuery5);
            }




            Int32 totResult = resultdel1 + resultdel2 + resultdel3 + resultdel4;

            if (totResult != 0)
            {
                return Ok("Successfully Deleted");
            }
            else
            {
                return BadRequest("Some Error Occuered May be all 3 queries are not excecuted successfully.");
            }

        }



        [HttpGet("getStockOutDrillDown")]
        public async Task<ActionResult<IEnumerable<StockOutDetailDTO>>> getStockOutDrillDown(string faclityId, string Mcatid, string isEDL)
        {
            FacOperations f = new FacOperations(_context);
            Int64 WHIID = f.getWHID(faclityId);
            Int64 EDLCAt = f.geFACEDLCat(faclityId);
            string yearid = f.getACCYRSETID();
            string qry = "";

            if (isEDL == "Y")
            {
                qry = @" 
select  m.itemcode,e.EDL,m.itemname,m.strength1
,nvl(wh.Ready,0)*nvl(m.unitcount,1)  StockinWH
from masitems m
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
inner join  masedl e on e.edlcat=m.edlcat
left outer join masitemtypes ty on ty.ITEMTYPEID=m.ITEMTYPEID
left outer join 
     (  
     select ITEMCODE,sum(facStock) facStock from 
     (
                 select case when mi.edlitemcode is null then  mi.ITEMCODE else  mi.edlitemcode end as ITEMCODE,
                 sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 

                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid=" + faclityId + @"
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                group by mi.ITEMCODE,mi.edlitemcode
                having (sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
        ) group by ITEMCODE
) fs on  fs.ITEMCODE=m.itemcode

left outer join 
(
  select t.warehouseid, i.itemid,
    sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0)) Ready,
    sum(nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending 
    from tbreceiptbatches b    
    inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
    inner join tbreceipts t on t.receiptid=i.receiptid
    inner join masitems mi on mi.itemid=i.itemid  
    inner join masitemcategories c on c.categoryid=mi.categoryid
   inner join masitemmaincategory mc on mc.MCID=c.MCID
    left outer join  
    (    
    select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
    from tboutwards tbo, tbindentitems tbi , tbindents tb  
    where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid 
    and tb.status = 'C' and tb.notindpdmis is null 
    and tbo.notindpdmis is null and tbi.notindpdmis is null    
    group by tbi.itemid,tb.warehouseid,tbo.inwno    
    ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
    Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
    and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and mc.MCID = " + Mcatid + @"
    and   t.warehouseid=" + WHIID + @"
    group by t.warehouseid, i.itemid
    having  (sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0))>0)
) wh on wh.itemid=m.itemid
where 1=1 and isedl2021='Y' and mc.MCID=" + Mcatid + @"
and m.isfreez_itpr is null and m.edlcat<=2
and nvl(fs.facStock,0)=0
order by wh.Ready ";
            }
            else
            {
                qry = @" 
select m.itemcode,e.edl,m.itemname ,m.strength1,nvl(wh.Ready,0)*nvl(m.unitcount,1)  StockinWH
from masanualindent a
inner join anualindent ai on ai.indentid=a.indentid
inner join masitems m on m.itemid=ai.itemid
inner join  masedl e on e.edlcat=m.edlcat
inner join masitemcategories c on c.categoryid=m.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
left outer join 
     (  
     select ITEMCODE,sum(facStock) facStock from 
     (
                 select case when mi.edlitemcode is null then  mi.ITEMCODE else  mi.edlitemcode end as ITEMCODE,
                 sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid= " + faclityId + @"    
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid                 
                 Where  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"
                group by mi.ITEMCODE,mi.edlitemcode
                having (sum((case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
        ) group by ITEMCODE
) fs on  fs.ITEMCODE=m.itemcode

left outer join 
(
  select t.warehouseid, i.itemid,
    sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0)) Ready,
    sum(nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending 
    from tbreceiptbatches b    
    inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
    inner join tbreceipts t on t.receiptid=i.receiptid
    inner join masitems mi on mi.itemid=i.itemid  
    inner join masitemcategories c on c.categoryid=mi.categoryid
inner join masitemmaincategory mc on mc.MCID=c.MCID
    left outer join  
    (    
    select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
    from tboutwards tbo, tbindentitems tbi , tbindents tb  
    where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid 
    and tb.status = 'C' and tb.notindpdmis is null 
    and tbo.notindpdmis is null and tbi.notindpdmis is null    
    group by tbi.itemid,tb.warehouseid,tbo.inwno    
    ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
    Where  T.Status = 'C'  And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
    and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null and mc.MCID= " + Mcatid + @"
    and   t.warehouseid=" + WHIID + @"
    group by t.warehouseid, i.itemid
    having  (sum(nvl(case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
    (case when mi.Qctest ='N' and b.qastatus=2 then 0 
    else case when mi.Qctest ='N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end  end ) end,0))>0)
) wh on wh.itemid=m.itemid
where 1=1 and m.isfreez_itpr is null  and  ai.cmhodistqty>0
and a.accyrsetid=544 and a.status='C' and  a.facilityid=" + faclityId + @"
 and mc.MCID=" + Mcatid + @" and (case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end)='Non EDL'
and nvl(fs.facStock,0)=0
order by wh.Ready ";
            }

            var myList = _context.StockOutDetailDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getFacilityIndentToWH")]
        public async Task<ActionResult<IEnumerable<FacilityIndentToWHDTO>>> getFacilityIndentToWH(string faclityId, string yrid, string itemid)
        {
            string qry = @" select mni.itemid,sum(BOOKEDQTY *nvl(m.unitcount,1)) as FacIndentToWH from mascgmscnoc mn
                            inner join mascgmscnocitems mni on mni.nocid=mn.nocid
                            inner join masitems m on m.itemid=mni.itemid
                            where mn.status='C' and mn.facilityid=" + faclityId + @"
                            and mn.nocdate between (select STARTDATE from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @"  )
                            and mni.itemid=" + itemid + @"
                            group by mni.itemid ";

            var myList = _context.FacilityIndentToWHDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getWHissueToFacility")]
        public async Task<ActionResult<IEnumerable<FacilityIndentToWHDTO>>> getWHissueToFacility(string faclityId, string yrid, string itemid)
        {
            string qry = @" select tbi.itemid,sum(tbo.issueqty*nvl(m.unitcount,1) )as FacIndentToWH 
                            from
                            tbindents tb
                            inner join tbindentitems tbi on tbi.indentid=tb.indentid
                            inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                            inner join masitems m on m.itemid=tbi.itemid
                            where tb.status='C' and tb.facilityid=" + faclityId + @"
                            and tb.indentdate between (select STARTDATE from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and tbi.itemid=" + itemid + @"
                            group by tbi.itemid ";

            var myList = _context.FacilityIndentToWHDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getFacilityReceiptAgainstIndent")]
        public async Task<ActionResult<IEnumerable<FacilityReceiptAgainstIndentDTO>>> getFacilityReceiptAgainstIndent(string faclityId, string yrid, string itemid)
        {
            string qry = @"select itemid,sum(nvl(WhIssueNOS,0)) WhIssueToFac,sum(nvl(FacReceiptAgnIndnet,0) )FacReceiptAgnIndnet from 
                         (
                         select fi.indentid,fi.itemid,fi.WhIssueNOS,fr.FacReceiptAgnIndnet from(
                         select tb.indentid,tbi.itemid,sum(tbo.issueqty) as WhIssueSKU 
                         ,sum(tbo.issueqty*nvl(m.unitcount,1) )as WhIssueNOS 
                         from
                         tbindents tb
                         inner join tbindentitems tbi on tbi.indentid=tb.indentid
                         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                         inner join masitems m on m.itemid=tbi.itemid
                         where tb.status='C' and tb.facilityid=" + faclityId + @"
                         and tb.indentdate between (select STARTDATE from masaccyearsettings where accyrsetid=544 )
                         and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                         and tbi.itemid=" + itemid + @"
                         group by tbi.itemid,tb.indentid
                         )fi
                         left outer join
                         (
                         select r.indentid,ri.itemid,sum(rb.absrqty) as FacReceiptAgnIndnet 
                         from
                         tbfacilityreceipts r
                         inner join tbfacilityreceiptitems ri on ri.facreceiptid=r.facreceiptid
                         inner join tbfacilityreceiptbatches rb on rb.facreceiptitemid =ri.facreceiptitemid
                         where r.status='C' and r.facilityid=" + faclityId + @"
                         and r.facreceiptdate between (select STARTDATE from masaccyearsettings where accyrsetid=544 )
                         and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                         and ri.itemid=" + itemid + @"
                         group by ri.itemid,r.indentid
                         
                         )fr on fr.indentid=fi.indentid and fr.itemid=fi.itemid
                         ) group by itemid ";

            var myList = _context.FacilityReceiptAgainstIndentDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getFacilityReceiptFromOtherFacilityOrLP")]
        public async Task<ActionResult<IEnumerable<FacilityReceiptFromOtherFacilityOrLP_DTO>>> getFacilityReceiptFromOtherFacilityOrLP(string faclityId, string yrid, string itemid)
        {
            string qry = @" select tbi.itemid,sum(tbo.issueqty*nvl(m.unitcount,1) )as FacIndentToWH 
                            from
                            tbindents tb
                            inner join tbindentitems tbi on tbi.indentid=tb.indentid
                            inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                            inner join masitems m on m.itemid=tbi.itemid
                            where tb.status='C' and tb.facilityid=" + faclityId + @"
                            and tb.indentdate between (select STARTDATE from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and tbi.itemid=" + itemid + @"
                            group by tbi.itemid ";

            var myList = _context.FacilityReceiptFromOtherFacilityOrLP_DbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getFacilityWardIssue")]
        public async Task<ActionResult<IEnumerable<FacilityReceiptFromOtherFacilityOrLP_DTO>>> getFacilityWardIssue(string faclityId, string yrid, string itemid)
        {
            string qry = @" select tbi.itemid,sum(tbo.issueqty*nvl(m.unitcount,1) )as FacIndentToWH 
                            from
                            tbfacilityissues tb
                            inner join tbfacilityissueitems tbi on tbi.issueid=tb.issueid
                            inner join tbfacilityoutwards tbo on tbo.issueitemid=tbi.issueitemid
                            inner join masfacilitywards wr on wr.wardid=tb.wardid
                            inner join vmasitems m on m.itemid=tbi.itemid
                            where tb.status='C' and tb.facilityid=" + faclityId + @"
                            and tb.issuedate between (select STARTDATE from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and tbi.itemid=" + itemid + @"
                            group by tbi.itemid ";

            var myList = _context.FacilityReceiptFromOtherFacilityOrLP_DbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getFacilityIssueToOtherFacility")]
        public async Task<ActionResult<IEnumerable<FacilityReceiptFromOtherFacilityOrLP_DTO>>> getFacilityIssueToOtherFacility(string faclityId, string yrid, string itemid)
        {
            string qry = @"select tbi.itemid,sum(tbo.issueqty*nvl(m.unitcount,1) )as FacIndentToWH 
                            from
                            tbfacilityissues tb
                            inner join tbfacilityissueitems tbi on tbi.issueid=tb.issueid
                            inner join tbfacilityoutwards tbo on tbo.issueitemid=tbi.issueitemid
                            inner join vmasitems m on m.itemid=tbi.itemid
                            where tb.status='C' and tb.facilityid=" + faclityId + @"
                            and tb.issuedate between (select STARTDATE from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=" + yrid + @" )
                            and tbi.itemid=" + itemid + @"
                            and tb.TOFACILITYID is not null
                            group by tbi.itemid ";

            var myList = _context.FacilityReceiptFromOtherFacilityOrLP_DbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("getYear")]
        public async Task<ActionResult<IEnumerable<masAccYearSettingsModel>>> getYear()
        {
            string qry = @"SELECT
                                masaccyearsettings.accyrsetid,
                                masaccyearsettings.accyear,
                                masaccyearsettings.yearorder,
                                masaccyearsettings.startdate,
                                masaccyearsettings.enddate,
                                masaccyearsettings.shaccyear,
                                masaccyearsettings.entrydt,
                                masaccyearsettings.lastupdatedt
                            FROM
                                masaccyearsettings
                            WHERE
                                masaccyearsettings.accyrsetid >=539
                            ORDER BY
                                masaccyearsettings.yearorder DESC ";

            var myList = _context.masAccYearSettingsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


        [HttpGet("getFacilityAvailableItem")]
        public async Task<ActionResult<IEnumerable<IndentItemsFromWardDTO>>> getFacilityAvailableItem(Int32 facilityId)
        {
            string qry = @" select itemid,itemname||' '||itemcode as name from masitems where 1=1 and itemid in (
                            select distinct tbi.itemid 
                            from
                            tbindents tb
                            inner join tbindentitems tbi on tbi.indentid=tb.indentid
                            inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
                            inner join masitems m on m.itemid=tbi.itemid
                            where tb.status='C' and tb.facilityid=" + facilityId + @"
                            and tb.indentdate between (select STARTDATE from masaccyearsettings where accyrsetid=544 )
                            and (select ENDDATE+1 from masaccyearsettings where accyrsetid=544 )) or itemid  in (select distinct ai.itemid from masanualindent a
                            inner join anualindent ai on ai.indentid=a.indentid
                            where a.accyrsetid=544) order by  itemcode ";

            var myList = _context.IndentItemsFromWardDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }



        [HttpGet("getItemDetail")]
        public async Task<ActionResult<IEnumerable<ItemDetailDTO>>> getItemDetail(string faclityId, string yrid, string itemid)
        {
            string qry = @" select m.itemcode,m.itemname,m.strength1,ty.itemtypename,e.edl as edlCatName,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as EDL
                           , ai.facilityindentqty AIFacility,ai.cmhodistqty ApprovedAICMHO from masanualindent a
                            inner join anualindent ai on ai.indentid=a.indentid
                            inner join masitems m on m.itemid=ai.itemid
                            left outer join masedl e on e.edlcat=m.edlcat
                            left outer join masitemtypes ty on ty.ITEMTYPEID=m.ITEMTYPEID
                            where  a.accyrsetid=" + yrid + @" and a.status='C' and  a.facilityid=" + faclityId + @"
                            and m.itemid = " + itemid + @" ";

            var myList = _context.ItemDetailDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }
        [HttpGet("FacIndentAlert")]

        public async Task<ActionResult<IEnumerable<IndentAlertNewDTO>>> FacIndentAlert(string facid, string mcatid, string isEDL, string itemid)
        {
            FacOperations op = new FacOperations(_context);
            Int64 WHIID = op.getWHID(facid);
            Int32 hodid = op.geFACHOID(facid);
            string yearid = op.getACCYRSETID();
            string whmcatid = "";
            if (mcatid != "0")
            {
                if (hodid == 7)
                {
                    whmcatid = " and msc.subcatid in (" + mcatid + ") ";
                }
                else
                {
                    whmcatid = " and mc.MCID in (" + mcatid + ")";
                }
            }
            string edlcaluse = "";

            if (isEDL == "Y")
            {
                edlcaluse = " and m.isedl2021 = 'Y' ";
            }
            if (isEDL == "N")
            {
                edlcaluse = " and (case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end)= 'Non EDL' ";
            }


            string whitemcase = "";
            if (itemid != "0")
            {
                whitemcase = " and itemid = " + itemid;
            }
            else
            {
                whitemcase = " and Remarks='Need To Be Indent to CGMSC'";
            }

            string qry = @" select itemid,MCID, MCATEGORY, itemcode, edlCatName, itemtypename, EDL, itemname, strength1, AIFacility, ApprovedAICMHO, FACReqFor3Month, facstock, FACReqFor3Month-facstock as RequiredQTY,WIssueQTYYear,WHReady,Remarks,ITEMTYPEID,edlcat,unitcount, nvl(ApprovedAICMHO,0)-nvl(WIssueQTYYear,0) as BalAI  from
                (
        select m.itemcode, ty.itemtypename, e.edl as edlCatName,case when m.isedl2021= 'Y' then 'EDL' else 'Non EDL' end as EDL, m.itemname, m.strength1, ai.facilityindentqty AIFacility, ai.cmhodistqty ApprovedAICMHO, nvl(round(ai.cmhodistqty/12,0)*3,0) as FACReqFor3Month 
        ,nvl(fs.facStock,0) as facstock,nvl(wi.WHIssueYear,0) as WIssueQTYYear,wh.Ready* nvl(m.unitcount,1) as WHReady,wh.warehouseid
        ,case when round(nvl(fs.facStock,0),0)< nvl(round(ai.cmhodistqty/12,0)*3,0) then 'Need To Be Indent to CGMSC' else 'Stock Availble For 3 Month' end as Remarks
        ,ty.ITEMTYPEID,e.edlcat, mc.MCID,mc.MCATEGORY,m.itemid,nvl(m.unitcount,1) as unitcount
        from masanualindent a
        inner join anualindent ai on ai.indentid=a.indentid
        inner join masitems m on m.itemid= ai.itemid
        inner join masitemcategories c on c.categoryid= m.categoryid
        inner join masitemmaincategory mc on mc.MCID= c.MCID
        left outer join massubitemcategory msc on msc.categoryid= c.categoryid
        left outer join masedl e on e.edlcat= m.edlcat
        left outer join masitemtypes ty on ty.ITEMTYPEID= m.ITEMTYPEID
        left outer join
             (
                         select mi.ITEMCODE, sum((case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) facStock
                         from tbfacilityreceiptbatches b
                         inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid
                         inner join tbfacilityreceipts t on t.facreceiptid= i.facreceiptid
                         inner join vmasitems mi on mi.itemid= i.itemid
                         inner join masfacilities f  on f.facilityid= t.facilityid
                         left outer join
                         (
                           select fs.facilityid, fsi.itemid, ftbo.inwno, sum(nvl(ftbo.issueqty,0)) issueqty
                             from tbfacilityissues fs
                           inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid
                           inner join tbfacilityoutwards ftbo on ftbo.issueitemid= fsi.issueitemid
                           where fs.status = 'C'  and fs.facilityid= " + facid + @"
                           group by fsi.itemid, fs.facilityid, ftbo.inwno
                         ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid
                         Where  T.Status = 'C'  And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
                        and t.facilityid= " + facid + @"
                        group by mi.ITEMCODE
                        having (sum((case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)))>0
        ) fs on  fs.ITEMCODE=m.itemcode


        left outer join
        (
          select t.warehouseid, i.itemid,
            sum(nvl(case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
            (case when mi.Qctest ='N' and b.qastatus= 2 then 0 
            else case when mi.Qctest = 'N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end end ) end,0)) Ready,
            sum(nvl(case when mi.qctest= 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)) end) end,0))  Pending
            from tbreceiptbatches b
            inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
            inner join tbreceipts t on t.receiptid= i.receiptid
            inner join masitems mi on mi.itemid= i.itemid
            inner join masitemcategories c on c.categoryid= mi.categoryid
            left outer join
            (
            select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
            from tboutwards tbo, tbindentitems tbi , tbindents tb
            where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid
            and tb.status = 'C' and tb.notindpdmis is null 
            and tbo.notindpdmis is null and tbi.notindpdmis is null    
            group by tbi.itemid, tb.warehouseid, tbo.inwno
            ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid
            Where  T.Status = 'C'  And(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) and(b.Whissueblock = 0 or b.Whissueblock is null)
            and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null
            and t.warehouseid= " + WHIID + @" 
            group by t.warehouseid, i.itemid
            having  (sum(nvl(case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else 
            (case when mi.Qctest ='N' and b.qastatus= 2 then 0 
            else case when mi.Qctest = 'N' then (nvl(b.absrqty,0)-nvl(iq.issueqty,0)) end end ) end,0))>0)
        ) wh on wh.itemid=m.itemid

        left outer join
        (
        select tbi.itemid, sum(tbo.ISSUEQTY* nvl(m.unitcount,1)) as WHIssueYear from  tbIndents tb
        inner join   tbIndentItems tbi on tbi.indentid=tb.indentid
        inner join masitems m on m.itemid= tbi.itemid
        inner join masitemcategories c on c.categoryid= m.categoryid
        inner join tboutwards tbo on tbo.INDENTITEMID= tbi.INDENTITEMID
        inner Join masAccYearSettings yr on tb.IndentDate between yr.StartDate and yr.EndDate
        where tb.facilityid= " + facid + @" and tb.status= 'C' and tb.issuetype= 'NO'
        and yr.AccYrSetID = " + yearid + @"
        group by tbi.itemid, m.itemname, m.itemcode, m.unitcount
        ) wi on wi.itemid=m.itemid
        where 1=1 " + whmcatid + @" 
        and ai.cmhodistqty>0 and a.accyrsetid= " + yearid + @" and a.status= 'C' and a.facilityid= " + facid + @"
        and wh.Ready>0 " + edlcaluse + @"
        ) where 1=1 " + whitemcase + @"
        order  by itemname ";
            var myList = _context.IndentAlertNewDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("FacMonthIndent")]

        public async Task<ActionResult<IEnumerable<FacMonthIndentDTO>>> FacMonthIndent(string facid, string istatus)
        {
            string qry = @"  select faci.nocid, faci.nocdate as ReqDate, faci.NOCNUMBER as ReqNo, nvl(nositemsReq,0) as nositemsReq, tb.INDENTNO WHIssueNo, tb.INDENTDATE as WHIssueDT, nvl(nositemsIssued,0) as NosIssued,nvl(tb.indentid,0) indentid
, fr.FACRECEIPTNO, fr.FACRECEIPTDATE,case when fr.STATUS='I' then 'IN' else  nvl(fr.STATUS,'IN') end as RStatus
, fr.FACRECEIPTID,faci.FacilityID,nvl(tb.warehouseid,0) warehouseid
,nvl(faci.STATUS,'I') as IStatus
from mascgmscnoc faci
left outer join
(
select count(distinct ri.itemid) nositemsReq, ri.nocid from mascgmscnocitems ri
where 1=1 and ri.BOOKEDQTY>0
group by ri.nocid
) ri on ri.nocid=faci.nocid
left outer join tbIndents tb on tb.NOCID=faci.nocid
left outer join
(
select count(distinct tbi.itemid) nositemsIssued, tbi.indentid from tbIndentItems tbi
group by tbi.indentid
) tbi on tbi.indentid=tb.indentid
left outer join tbFacilityReceipts fr on fr.FacilityID=faci.FacilityID and fr.IndentId= tb.IndentId

where 1=1 
and faci.ACCYRSETID>=544  and faci.facilityid =" + facid + @"
and nvl(faci.STATUS,'I')='I'
order by faci.nocid desc ";
            //(23413)
            var myList = _context.FacMonthIndentDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("getFacMonthIndentItems")]
        public async Task<ActionResult<IEnumerable<IndentItemsFromWardDTO>>> getFacMonthIndentItems(string faclityId, string Mcatid, string indendid)
        {

            FacOperations f = new FacOperations(_context);
            Int64 WHIID = f.getWHID(faclityId);
            Int64 EDLCAt = f.geFACEDLCat(faclityId);
            Int32 hodid = f.geFACHOID(faclityId);
            string whmcatid = "";
            if (Mcatid != "0")
            {
                if (hodid == 7)
                {
                    whmcatid = " and msc.subcatid in (" + Mcatid + ") ";
                }
                else
                {
                    whmcatid = " and mc.MCID in (" + Mcatid + ")";
                }
            }

            string qry = @" select A.itemname || '-' || A.itemcode as name,ItemID
                  from
                 (
                 select e.edlcat, e.edl, w.WAREHOUSENAME, mi.ITEMCODE, b.inwno, mi.ITEMNAME, mi.strength1, mi.unit as SKU,
               (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,  
                   case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
, mi.unitcount,mi.itemid
                  from tbreceiptbatches b
                  inner join tbreceiptitems i on b.receiptitemid = i.receiptitemid
                  inner join tbreceipts t on t.receiptid = i.receiptid
                  inner join masitems mi on mi.itemid = i.itemid
                          left outer join masedl e on e.edlcat = mi.edlcat
                  inner join masitemcategories c on c.categoryid = mi.categoryid
                  inner join masitemmaincategory mc on mc.MCID = c.MCID
                  left outer join massubitemcategory msc on msc.categoryid = c.categoryid
                 inner join MASWAREHOUSES w  on w.warehouseid = t.warehouseid
                 left outer join
                (
                   select tb.warehouseid, tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty, 0)) issueqty
                   from tboutwards tbo, tbindentitems tbi, tbindents tb
                   where 1 = 1 and  tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null
                   and tb.warehouseid = " + WHIID + @"
                   group by tbi.itemid, tb.warehouseid, tbo.inwno
                 ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid and iq.warehouseid = t.warehouseid
                 Where T.Status = 'C' and(b.ExpDate >= SysDate or nvl(b.ExpDate, SysDate) >= SysDate) And(b.Whissueblock = 0 or b.Whissueblock is null)
              and mc.MCID in (1) and nvl(mi.edlcat,0)<= " + EDLCAt + @"
                  and w.warehouseid = " + WHIID + @" and((case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) )> 0
                 and t.notindpdmis is null and b.notindpdmis is null and i.notindpdmis is null
                 ) A
                where 1 = 1
                and A.itemid not in (select ci.itemid from mascgmscnoc c
inner join mascgmscnocitems ci on ci.nocid = c.nocid
where c.facilityid = " + faclityId + @" and ci.nocid = " + indendid + @")
                 group by WAREHOUSENAME, A.itemcode,A.ItemName, A.strength1,A.SKU ,A.unitcount,A.Itemid,A.edlcat,A.edl
                  having  sum(A.ReadyForIssue) > 0
                 order by A.ItemName ";
            var myList = _context.IndentItemsFromWardDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpGet("getMonthIndentProgram")]
        public async Task<ActionResult<IEnumerable<getIndentProgramDTO>>> getMonthIndentProgram()
        {
            string qry = @"select programid,program from masprogram where Isactive is null order by programid ";
            var myList = _context.getIndentProgramDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpPost("postNOCitems")]
        public IActionResult postNOCitems(mascgmscnocitems objmascgmscnocitems)
        {
            objmascgmscnocitems.CGMSCLREMARKS = "Indent Placed";
            objmascgmscnocitems.STATUS = "C";
            objmascgmscnocitems.BOOKEDFLAG = "B";
            objmascgmscnocitems.APPROVEDQTY = 0;
            objmascgmscnocitems.BOOKEDQTY = objmascgmscnocitems.REQUESTEDQTY;

            try
            {
                _context.mascgmscnocitemsDbSet.Add(objmascgmscnocitems);
                _context.SaveChanges();
                return Ok("Success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("getSavedFacIndentItems")]
        public async Task<ActionResult<IEnumerable<SavedFacIndentItemsDTO>>> getSavedFacIndentItems(Int64 nocid)
        {
            string qry = @"select SR,b.ItemID,mi.itemname,NVL(STOCKINHAND ,0) STOCKINHAND ,requestedqty*nvl(mi.unitcount,1) whindentQTY,
                a.status
                from MASCGMSCNOC a,MASCGMSCNOCITEMS b 
                inner join masitems mi on mi.ItemID=b.ItemID 
                where a.nocid=b.nocid and a.NOCID=" + nocid + @"  order by b.sr desc ";
            var myList = _context.SavedFacIndentItemsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpDelete("deleteCgmscNOCitems")]
        public IActionResult deleteCgmscNOCitems(Int64 sr)
        {

            string qry = " delete from MASCGMSCNOCITEMS where SR = " + sr;
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Deleted from MASCGMSCNOCITEMS");

        }

        [HttpDelete("deleteCgmscNOCitemsALL")]
        public IActionResult deleteCgmscNOCitemsALL(Int64 nocid)
        {

            string qry = "  delete from MASCGMSCNOCITEMS where NOCID =  " + nocid;
            _context.Database.ExecuteSqlRaw(qry);

            string qry1 = "   delete from MASCGMSCNOC where NOCID = " + nocid;
            _context.Database.ExecuteSqlRaw(qry1);

            return Ok("Successfully Deleted from MASCGMSCNOCITEMS & MASCGMSCNOC");

        }

        [HttpPut("completemascgmscnoc")]
        public IActionResult completemascgmscnoc(Int64 nocid)
        {

            string qry = "  update MASCGMSCNOC set STATUS = 'C' where nocid = " + nocid;
            _context.Database.ExecuteSqlRaw(qry);


            return Ok("Successfully Update MASCGMSCNOC status C");

        }

        [HttpGet("FacMonthIndentNo")]

        public async Task<ActionResult<IEnumerable<FacMonthIndentDTO>>> FacMonthIndentNo(string facid, string NOCNumber)
        {
            string qry = @"  select faci.nocid, faci.nocdate as ReqDate, faci.NOCNUMBER as ReqNo, nvl(nositemsReq,0) as nositemsReq, tb.INDENTNO WHIssueNo, tb.INDENTDATE as WHIssueDT, nvl(nositemsIssued,0) as NosIssued,nvl(tb.indentid,0) indentid
, fr.FACRECEIPTNO, fr.FACRECEIPTDATE,case when fr.STATUS='I' then 'IN' else  nvl(fr.STATUS,'IN') end as RStatus
, fr.FACRECEIPTID,faci.FacilityID,nvl(tb.warehouseid,0) warehouseid
,nvl(faci.STATUS,'I') as IStatus
from mascgmscnoc faci
left outer join
(
select count(distinct ri.itemid) nositemsReq, ri.nocid from mascgmscnocitems ri
where 1=1 and ri.BOOKEDQTY>0
group by ri.nocid
) ri on ri.nocid=faci.nocid
left outer join tbIndents tb on tb.NOCID=faci.nocid
left outer join
(
select count(distinct tbi.itemid) nositemsIssued, tbi.indentid from tbIndentItems tbi
group by tbi.indentid
) tbi on tbi.indentid=tb.indentid
left outer join tbFacilityReceipts fr on fr.FacilityID=faci.FacilityID and fr.IndentId= tb.IndentId

where 1=1 
and faci.ACCYRSETID>=544  and faci.facilityid =" + facid + @"
and faci.NOCNUMBER='" + NOCNumber + "' ";
            //(23413)
            var myList = _context.FacMonthIndentDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpPost("postWhIndentNo")]
        public async Task<ActionResult<IEnumerable<IncompleteWardIssueDTO>>> postWhIndentNo(Int64 facid, string indentDt, Int64 programid)
        {
            try
            {
                FacOperations ob = new FacOperations(_context);
                tbGenIndent objSaveIndent = new tbGenIndent();
                string indentDate = ob.FormatDate(indentDt);

                string faccode = ob.getFacCodeForIndent(facid);
                string yearcode = ob.getSHAccYear();
                string yearId = ob.getACCYRSETID();
                string autono = facid.ToString() + "/NC" + faccode + "/" + yearcode;
                //objeIssue.ISSUENO = issueno;
                objSaveIndent.NOCDATE = indentDate;
                objSaveIndent.NOCNUMBER = autono;
                objSaveIndent.FACILITYID = facid;
                objSaveIndent.PROGRAMID = programid;
                objSaveIndent.ACCYRSETID = Convert.ToInt64(yearId);
                objSaveIndent.ISUSEAPP = "Y";
                objSaveIndent.STATUS = "I";
                objSaveIndent.AUTO_NCCODE = faccode;


                _context.tbGenIndentDbSet.Add(objSaveIndent);
                _context.SaveChanges();


                var myObj = FacMonthIndentNo(facid.ToString(), autono);
                return Ok(myObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message.ToString());
            }
        }



        [HttpGet("getAyushItems")]
        public async Task<ActionResult<IEnumerable<GetAyushItemsDTO>>> getAyushItems(string catid)
        {
            string wh = "";
            if (catid == "" || catid == "0")
            {
                wh = " and c.categoryid in (58,59,60)";
            }
            else
            {
                wh = " and c.categoryid=" + catid;
            }
            string qry = @"select distinct m.itemid,m.itemname||'-'||m.itemcode as itemnamePar,m.itemname from  vmasitems m 
left outer join  masitemcategories c on c.categoryid=m.categoryid
left outer join  masitems mm on mm.itemid=m.itemid
where  m.itemcode not in ('T1') and mm.isfreez_itpr is null  " + wh + @" and ( case when  SUBSTR(m.itemcode,1,2) ='LP' and m.edlitemcode is not null then 'N' else 'Y' end)='Y'
 order by m.itemname
  ";
            var myList = _context.GetAyushItemsDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("getFileStorageLocation")]
        public async Task<ActionResult<IEnumerable<GetFileStorageLocationDTO>>> getFileStorageLocation(string whid)
        {

            string qry = @"Select nvl(RackID,0) RackID,nvl(locationno,'Null') locationno from masracks where warehouseid=" + whid + " order by locationno  ";
            var myList = _context.GetFileStorageLocationDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("getOpeningStocksReport")]
        public async Task<ActionResult<IEnumerable<GetOpeningStocksRptDTO>>> getOpeningStocksReport(String itemid, Int64 usrFacilityID)
        {
            string whcluase = "";
            if (itemid != "" && itemid != "0")
            {
                whcluase = " and mi.itemid=" + itemid;
            }

            string qry = @"  select  row_number() over (order by bt.inwno) as id ,bt.inwno,nvl(i.absrqty,0) opening_qty,to_char(FACreceiptdate,'dd/MM/yyyy') as receiptdate ,r.facilityid,i.itemid,mi.itemcode, 
                   mi.unit,mi.strength1,mi.itemname,bt.batchno,to_char(bt.mfgdate,'dd/MM/yyyy') as mfgdate,to_char(bt.expdate,'dd/MM/yyyy') as expdate,bt.absrqty 
,      r.facreceiptid as  ReceiptID,i.FACreceiptitemid as ReceiptItemID              
from tbfacilityreceipts r 
                    inner join tbfacilityreceiptitems i on r.FACreceiptid=i.FACreceiptid 
                 inner join tbFacilityReceiptBatches bt on bt.FACreceiptitemid=i.FACreceiptitemid 
                   inner join vmasitems mi on mi.itemid=i.itemid  where 1=1 " + whcluase + @" and  FACreceipttype='FC' and r.facilityid=" + usrFacilityID + @"
              Order By mi.ItemID, bt.BatchNo  ";

            var myList = _context.GetOpeningStocksRptDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }

        [HttpPut("SaveBatchRecord")]
        public IActionResult SaveBatchRecord(Int64 mItemID, String mBatchNo, String mStockLocation, String mMfgDate, String mExpDate, String mAbsRQty, string facilityId)
        {

            master msterObj = new master(_context);

            string mShortQty = "0";
            string mReceiptQty = "";

            double dblAbsRQty = double.Parse(mAbsRQty);
            double dblShortQty = double.Parse(mShortQty);
            double dblReceiptQty = dblAbsRQty - dblShortQty;
            mReceiptQty = dblReceiptQty.ToString();

            string mReceiptItemID = msterObj.getOPReceiptItemidID(facilityId, mItemID.ToString());
            mReceiptItemID = "111";

            //if (mReceiptItemID != "0" && mReceiptItemID != "")
            //{
            //    if (msterObj.getSameBatchInItems(mBatchNo, mReceiptItemID, mItemID.ToString()))
            //    {
                    string strQuery = "Insert into tbFacilityReceiptBatches (FACReceiptItemID,ItemID,BatchNo,StockLocation,MfgDate,ExpDate,AbsRQty,ShortQty,qastatus)" +
                        " values (" + mReceiptItemID + ", " + mItemID + ", '" + mBatchNo + "', '" + mStockLocation + "', '" + mMfgDate + "', '" + mExpDate + "', " + mReceiptQty + ", " + mShortQty + ",'1')";

                    _context.Database.ExecuteSqlRaw(strQuery);

                    string strQuery2 = "update  tbfacilityreceiptitems set AbsRQty=(AbsRQty+" + mReceiptQty + ")  where FACReceiptItemID=" + mReceiptItemID;
                    _context.Database.ExecuteSqlRaw(strQuery2);

                    return Ok("Added Successfully");

            //    }
            //    else
            //    {
            //        return BadRequest("Your Batch No is Duplicate for this Items");
            //    }

            //}
            //else
            //{
            //    return BadRequest("mReceiptItemID is not null or zero");
            //}

        }




        [HttpPut("FreezeOpeningStock")]
        public IActionResult FreezeOpeningStock(Int64 mReceiptID)
        {

            master msterObj = new master(_context);

            bool IsReceipt = msterObj.isFacilityReceipt(mReceiptID.ToString());

            if (!IsReceipt)
            {
                return BadRequest("All the entries must be done before complete freezing the opening stock.");
            }

            // check all total qty vs sum of batch qty      


            if (msterObj.checkstock(mReceiptID.ToString()) == "True")
            {
                string strQuery = " update tbfacilityreceipts set Status='C' where facreceiptid=" + mReceiptID;
                _context.Database.ExecuteSqlRaw(strQuery);
                return Ok("Freezed Successfully");
            }
            else
            {
                //msg 
                string msg = "";
                if (msterObj.checkstock(mReceiptID.ToString()) == "False")
                {
                    msg = "Please Enter Opening Quantity along with batches";
                }
                else
                {
                    msg = "Please Enter Remaining batch quantity of Drugcode " + msterObj.checkstock(mReceiptID.ToString());
                }
                return BadRequest(msg);

            }

        }



        [HttpPut("UpdateHeaderInfo")]
        public IActionResult UpdateHeaderInfo(Int64 usrFacilityID, Int64 receiptID, String receiptNo, String receiptDate)
        {
            master msterObj = new master(_context);

            string mSourceID = "716";
            string mFacilityID = usrFacilityID.ToString();
            string mReceiptID = receiptID.ToString();
            string mReceiptNo = receiptNo.ToUpper();
            if (mReceiptNo == "AUTO GENERATED")

                mReceiptNo = msterObj.FacAutoGenerateNumbers(usrFacilityID.ToString(), true, "FC");  //string ID, bool IsWarehouse, bool IsReceipt, string mType
            string mReceiptDate = receiptDate.Trim();



            //#region Validate Data
            //string ErrMsg = "";
            //ErrMsg += GenFunctions.CheckDateGreaterThanToday(mReceiptDate, "Receipt date");
            //ErrMsg += GenFunctions.CheckDuplicate(mReceiptNo, "Receipt number", "tbfacilityreceipts", "FACReceiptNo", mReceiptID, "FACReceiptID", " and FacilityID = " + mFacilityID);
            //ErrMsg += GenFunctions.CheckStringForEmpty(ref mReceiptNo, "Receipt number", false);
            //ErrMsg += GenFunctions.CheckStringForEmpty(ref mSourceID, "Source", false);
            //ErrMsg += GenFunctions.CheckDate(ref mReceiptDate, "Receipt date", false);
            //if (ErrMsg != "")
            //{
            //    GenFunctions.AlertOnLoad(this.Page, ErrMsg);
            //    lblMsg.Text = ErrMsg;
            //    lblMsg.ForeColor = Color.Red;
            //    return;
            //}
            //else
            //    lblMsg.Text = "";
            //#endregion





            if (mReceiptID == "" || mReceiptID == "0")
            {
                String ReceiptID = "";
                if (msterObj.checkAlreadyFC(usrFacilityID))
                {
                    string strSQL = "Insert into tbFacilityReceipts (FacilityID,FacReceiptNo,FacReceiptDate,FACReceiptType)" +
                        " values (" + mFacilityID + "," +
                        mReceiptNo + "," + mReceiptDate + ",'FC')";
                    _context.Database.ExecuteSqlRaw(strSQL);


                    strSQL = "Select FACRECEIPTID from tbFacilityReceipts where FACReceiptType='FC'" +
                        " and FacReceiptNo=" + mReceiptNo + " and FacReceiptDate=" + mReceiptDate;
                    var myList = _context.GetFacilityReceiptIdDbSet
                        .FromSqlInterpolated(FormattableStringFactory.Create(strSQL)).ToList();

                    // DataTable dtID = DBHelper.GetDataTable(strSQL);

                    if (myList.Count > 0)
                        ReceiptID = myList[0].FACRECEIPTID.ToString();
                    return Ok("Updated Successfully");
                    //lblReceiptID.Text = dtID.Rows[0]["FacReceiptID"].ToString();
                    //lblMsg.Text = "Updated Successfully";
                    //lblMsg.ForeColor = Color.Green;
                    // PopulateHeaderInfo(RC4Engine.Decrypter(ReceiptID.Value, EncryptionKey));
                }
            }
            else
            {
                string strSQL = "Update tbFacilityReceipts set FacReceiptNo=" + mReceiptNo + ",FacReceiptDate=" + mReceiptDate + " " +
                    " Where FacReceiptID = " + mReceiptID;
                _context.Database.ExecuteSqlRaw(strSQL);
                return Ok("Updated Successfully");
                //lblMsg.Text = "Updated Successfully";
                //lblMsg.ForeColor = Color.Green;
                //PopulateHeaderInfo(RC4Engine.Decrypter(ReceiptID.Value, EncryptionKey));
            }

            return BadRequest(mReceiptID);

        }


        [HttpGet("GetHeaderInfo")]
        public async Task<ActionResult<IEnumerable<GetHeaderInfoDTO>>> GetHeaderInfo(String ReceiptID)
        {

            string qry = @" Select  facreceiptid,sourceid,schemeid,facilityid,warehouseid,indentid,issueid,facreceiptno,facreceiptdate,facreceipttype,facreceiptvalue,remarks,
    status,wardid,supplierid,reasonflag,isedl,tofacilityid,ponoid,stkregno,stkregdate,mrcnumber,mrcdate,sptype,splocationid,invoiceno,invoicedate,voucherstatus,isnicshare,compentrydt,
    receiptby,recbydate,receivedby,recbymobileno,entrydate,isuseapp from tbfacilityreceipts where FACReceiptID = " + ReceiptID;


            var myList = _context.GetHeaderInfoDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;
        }


        [HttpGet("FacilityIssueCurrectstock")]
        public async Task<ActionResult<IEnumerable<FacilityIssueCurrentStockDTO>>> FacilityIssueCurrectstock(string faclityId, string itemid, string catname,string issueid)
        {
            FacOperations op = new FacOperations(_context);
            Int32 hodid = op.geFACHOID(faclityId);
            string whcatid = "";
            if (catname == "D")
            {
                if (hodid == 7)
                {
                    whcatid = " and c.categoryid in (58,59,60,61)";
                }
                else
                {
                    whcatid = " and c.categoryid in (52)";
                }
            }
            else if (catname == "C")
            {

                whcatid = " and c.categoryid not in (52,58,59,60,61)";

            }
            else
            {

            }

            string whclause = "";
            if (itemid == null)
            {

            }
            else if (itemid == "")
            {

            }
            else if (itemid == "0")
            {

            }
            else
            {
                whclause = " and mi.itemid=" + itemid;
            }
            string qry = @"   select c.categoryname,mi.ITEMCODE,ty.itemtypename,mi.itemname,mi.strength1,  
                 (case when (b.qastatus ='1' or  mi.qctest='N') then (sum(nvl(b.absrqty,0)) - sum(nvl(iq.issueqty,0))) end) ReadyForIssue                 
                 ,t.facilityid, mi.itemid,c.categoryid, case when mi.ISEDL2021='Y' then 'EDL' else 'Non EDL' end as EDLType
   ,nvl(fiss.facissueqty,0) as facissueqty,nvl(fiss.issueitemid,0) as  issueitemid
                 from tbfacilityreceiptbatches b   
                 inner join tbfacilityreceiptitems i on b.facreceiptitemid=i.facreceiptitemid 
                 inner join tbfacilityreceipts t on t.facreceiptid=i.facreceiptid  
                 inner join vmasitems mi on mi.itemid=i.itemid 
                 left outer join masitemcategories c on c.categoryid=mi.categoryid
                 left outer join masitemtypes ty on ty.itemtypeid=mi.itemtypeid
                 inner join masfacilities f  on f.facilityid=t.facilityid 
                 left outer join 
                 (  
                   select  fs.facilityid,fsi.itemid,ftbo.inwno,sum(nvl(ftbo.issueqty,0)) issueqty   
                     from tbfacilityissues fs 
                   inner join tbfacilityissueitems fsi on fsi.issueid=fs.issueid 
                   inner join tbfacilityoutwards ftbo on ftbo.issueitemid=fsi.issueitemid 
                   where fs.status = 'C'  and fs.facilityid=" + faclityId + @"          
                   group by fsi.itemid,fs.facilityid,ftbo.inwno                     
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.facilityid=t.facilityid 
left outer join
                 (
                 select tb.ISSUEID,tbi.issueitemid,tbi.itemid,sum(tbo.issueqty) as facissueqty from tbfacilityissues tb
                            inner join tbfacilityissueitems tbi on tbi.ISSUEID=tb.ISSUEID
                            inner join tbfacilityoutwards tbo on tbo.issueitemid=tbi.issueitemid
                            where tb.ISSUEID="+ issueid + @" and facilityid=" + faclityId + @"
                            group by tb.ISSUEID,tbi.issueitemid,tbi.itemid
                 )fiss on fiss.itemid=mi.itemid
                 Where 1=1 " + whclause + @" and  T.Status = 'C'  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate 
                and t.facilityid= " + faclityId + @"  " + whcatid + @"
                and (   (case when (b.qastatus ='1' or  mi.qctest='N') then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) end)) > 0
                group by  fiss.facissueqty,fiss.issueitemid,  mi.ITEMCODE, t.facilityid, mi.itemid,b.qastatus,mi.qctest,mi.itemname,mi.strength1,c.categoryname,c.categoryid,itemtypename,mi.ISEDL2021
                order by c.categoryid, mi.itemname ";

           // var context = new StockReportFacilityDTO();

            var myList = _context.FacilityIssueCurrentStockDbSet
            .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }


    }
}