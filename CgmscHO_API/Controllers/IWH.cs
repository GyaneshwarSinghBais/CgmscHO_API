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
    public class IWH : ControllerBase
    {
        private readonly OraDbContext _context;
        public IWH(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("InitiatedNotIssueSummary")]
        public async Task<ActionResult<IEnumerable<InitiatedPendingIssueSummaryDTO>>> InitiatedNotIssueSummary(string dcflag, string mcid)
        {
            string whclause = "";
            if (dcflag == "Y")
            {
                whclause = " and mc.mcid in (1,2,4) ";
            }
          
            if (dcflag == "N" && mcid != "0")
            {

                whclause = " and mc.mcid =" + mcid;
            }


            string qry = "";      
                qry = @"   select fromwarehousename,count(distinct towarehouseid) nostowh, count(itemid) as nositems,sum(towhstockout) as towhstockout,count(transferno) as nostno,Round(sum(pendingsince)/count(transferno),0) AvgDaysDel  ,fromwarehouseid
from 
(
select m.itemcode,t.transferno,
(select warehousename from maswarehouses where warehouseid = t.fromwarehouseid) fromwarehousename,nvl(st.ReadyForIssue,0) fromwhCstock,nvl(st.Pending,0) fromWHUQC,
(select warehousename from maswarehouses where warehouseid = t.towarehouseid) towarehousename,nvl(stto.ReadyForIssue,0) towhCstock,nvl(stto.Pending,0) toWHUQC,
t.transferdate,i.transferqty,
round(sysdate - t.transferdate) pendingsince,m.categoryid,m.itemid,t.fromwarehouseid ,t.towarehouseid
,case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 1 else 0 end as towhstockout 
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join masitems m on m.itemid = i.itemid
     inner join masitemcategories ic on ic.categoryid = m.categoryid
     inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) st on st.warehouseid=t.fromwarehouseid and st.itemid=m.itemid


left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) stto on stto.warehouseid=t.towarehouseid and stto.itemid=m.itemid

where t.status = 'C' " + whclause + @" and t.transferid not in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferdate between '01-Aug-24' and sysdate and (nvl(st.ReadyForIssue,0)+nvl(st.Pending,0))>0
) group by fromwarehousename,fromwarehouseid ";
            
            var myList = _context.GetInitiatedPendingIssueSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
              [HttpGet("InitiatedNotIssueDetaqils")]
        public async Task<ActionResult<IEnumerable<InitiatedPendingIssueDetailsDTO>>> InitiatedNotIssueDetaqils(string whid,string stkout, string dcflag, string mcid)
        {
            string whclause = "";
            if (whid != "0")
            {
                whclause = " and t.fromwarehouseid=" + whid + @"";
            }
            string whstkout = "";
            if (stkout != "0")
            {
                whstkout = " and (case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 1 else 0 end )=1";
            }

            string whclause1 = "";
            if (dcflag == "Y")
            {
                whclause1 = " and mc.mcid in (1,2,4) ";
            }

            if (dcflag == "N" && mcid != "0")
            {

                whclause1 = " and mc.mcid =" + mcid;
            }

            string qry = "";
            qry = @"  select t.transferdate as InitiatedDT,(select warehousename from maswarehouses where warehouseid = t.fromwarehouseid) fromwarehousename,m.itemcode,m.itemname,m.unit,
i.transferqty,
t.transferno,
nvl(st.ReadyForIssue,0) fromwhCstock,nvl(st.Pending,0) fromWHUQC,
(select warehousename from maswarehouses where warehouseid = t.towarehouseid) towarehousename,nvl(stto.ReadyForIssue,0) towhCstock,nvl(stto.Pending,0) toWHUQC,
round(sysdate - t.transferdate) pendingsince,m.categoryid,m.itemid 
,t.fromwarehouseid ,t.towarehouseid
,case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 'Stock-out' else 'No' end as towhstockout 
,i.TRANSFERITEMID
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join masitems m on m.itemid = i.itemid
     inner join masitemcategories ic on ic.categoryid = m.categoryid
     inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) st on st.warehouseid=t.fromwarehouseid and st.itemid=m.itemid


left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) stto on stto.warehouseid=t.towarehouseid and stto.itemid=m.itemid

where t.status = 'C'  " + whclause + @"  "+ whstkout + @"  "+ whclause1 + @" and 
t.transferid not in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferdate between '01-Aug-24' and sysdate and (nvl(st.ReadyForIssue,0)+nvl(st.Pending,0))>0
order by t.transferdate ";



            var myList = _context.GetInitiatedPendingIssueDetailsDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }


        [HttpGet("IWHPiplineSummary")]
        public async Task<ActionResult<IEnumerable<IWHPipelineSummaryDTO>>> IWHPiplineSummary(string dcflag,string mcid)
        {
            string whclause = "";
            if (dcflag == "Y")
            {
                whclause = " and mc.mcid in (1,2,4) ";
            }
       
            if (dcflag == "N" && mcid!="0")
            {

                whclause = " and mc.mcid ="+ mcid;
            }



            string qry = "";
            qry = @"   select towarehousename,count(distinct fromwarehouseid) NOSwhiSSUED, count(distinct itemid) as nositems,sum(towhstockout) as towhstockout,Round(sum(pendingsince)/count(transferno),0) AvgDaysDel  ,towarehouseid
from 
(
select m.itemcode,transferno,(select warehousename from maswarehouses where warehouseid = t.fromwarehouseid) fromwarehousename,
nvl(st.ReadyForIssue,0) fromwhCstock,nvl(st.Pending,0) fromWHUQC,
(select warehousename from maswarehouses where warehouseid = t.towarehouseid) towarehousename,
nvl(stto.ReadyForIssue,0) towhCstock,nvl(stto.Pending,0) toWHUQC
,t.transferdate,i.transferqty
,ti.indentdate as WHIssuedDT,nvl(WHIssueQTY,0) whissueQTY,
round(sysdate - ti.indentdate) pendingsince 

,m.itemid,t.fromwarehouseid ,t.towarehouseid
,case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 1 else 0 end as towhstockout
,mc.mcid,mc.MCATEGORY
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join masitems m on m.itemid = i.itemid
     inner join masitemcategories ic on ic.categoryid = m.categoryid
     inner join masitemmaincategory mc on mc.MCID = ic.MCID
inner join 
(
select ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid,sum(nvl(tbo.issueqty,0)) WHIssueQTY   from tbindents ti
inner join tbindentitems tbi on tbi.indentid=ti.indentid
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
where ti.status='C'
group by ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid
)ti on ti.transferid = t.transferid and m.itemid=ti.itemid


left outer join 
(
 select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
from 
(
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) st on st.warehouseid=t.fromwarehouseid and st.itemid=m.itemid


left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) stto on stto.warehouseid=t.towarehouseid and stto.itemid=m.itemid

where t.status = 'C' "+whclause+@"   and t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
and t.transferdate between '01-Apr-24' and sysdate 
)group by towarehousename,towarehouseid ";

            var myList = _context.GetIWHPipelineSummaryDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }



        [HttpGet("IWHPiplineDetails")]
        public async Task<ActionResult<IEnumerable<IWHPipelineDetailsDTO>>> IWHPiplineDetails(string towhid, string stkout, string dcflag, string mcid)
        {
            string whclause = "";
            if (towhid != "0")
            {
                whclause = " and t.towarehouseid=" + towhid + @"";
            }
            string whstkout = "";
            if (stkout != "0")
            {
                whstkout = " and (case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 1 else 0 end )=1";
            }

            string whclause1 = "";
            if (dcflag == "Y")
            {
                whclause1 = " and mc.mcid in (1,2,4) ";
            }

            if (dcflag == "N" && mcid != "0")
            {

                whclause1 = " and mc.mcid =" + mcid;
            }

            string qry = "";
            qry = @"  
select m.itemcode,m.itemname,m.unit,transferno,t.transferdate,i.transferqty,(select warehousename from maswarehouses where warehouseid = t.fromwarehouseid) fromwarehousename,
nvl(st.ReadyForIssue,0) fromwhCstock,nvl(st.Pending,0) fromWHUQC,
(select warehousename from maswarehouses where warehouseid = t.towarehouseid) towarehousename,
nvl(stto.ReadyForIssue,0) towhCstock,nvl(stto.Pending,0) toWHUQC

,ti.indentdate as WHIssuedDT,nvl(WHIssueQTY,0) whissueQTY,
round(sysdate - ti.indentdate) pendingsince 

,m.itemid,t.fromwarehouseid ,t.towarehouseid
,case when (nvl(stto.ReadyForIssue,0) +nvl(stto.Pending,0))=0 then 'Stock-out' else 'No' end as towhstockout 
,mc.mcid,mc.MCATEGORY,i.TRANSFERITEMID
from stktransfers t
inner join stktransferitems i on i.transferid = t.transferid
inner join masitems m on m.itemid = i.itemid
     inner join masitemcategories ic on ic.categoryid = m.categoryid
     inner join masitemmaincategory mc on mc.MCID = ic.MCID
inner join 
(
select ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid,sum(nvl(tbo.issueqty,0)) WHIssueQTY   from tbindents ti
inner join tbindentitems tbi on tbi.indentid=ti.indentid
inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
where ti.status='C'
group by ti.indentdate,ti.warehouseid,ti.transferid,tbi.itemid
)ti on ti.transferid = t.transferid and m.itemid=ti.itemid


left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) st on st.warehouseid=t.fromwarehouseid and st.itemid=m.itemid


left outer join 
(
         select warehouseid,itemid,sum(ReadyForIssue) as ReadyForIssue,sum(Pending) as Pending
         from 
         (
            select w.warehouseid,mi.itemid, b.inwno,  
   (case when b.qastatus ='1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0)) else (case when mi.Qctest ='N' and b.qastatus=2 then 0 else case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) ) end  end ) end ) ReadyForIssue,    
                    nvl(case when  mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end,0)  Pending    
,mi.categoryid
                 from tbreceiptbatches b    
                 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid  
                 inner join tbreceipts t on t.receiptid=i.receiptid  
                 inner join masitems mi on mi.itemid=i.itemid  
                 inner join MASWAREHOUSES w  on w.warehouseid=t.warehouseid    
                 left outer join  
                 (    
                   select  tb.warehouseid,tbi.itemid,tbo.inwno,sum(nvl(tbo.issueqty,0)) issueqty    
                   from tboutwards tbo, tbindentitems tbi , tbindents tb  
                   where  tbo.indentitemid=tbi.indentitemid and tbi.indentid=tb.indentid and tb.status = 'C' and tb.notindpdmis is null and tbo.notindpdmis is null and tbi.notindpdmis is null    
                   group by tbi.itemid,tb.warehouseid,tbo.inwno    
                 ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid and iq.warehouseid=t.warehouseid    
                 Where  T.Status = 'C'  
                 And (b.ExpDate >= SysDate or nvl(b.ExpDate,SysDate) >= SysDate) and (b.Whissueblock = 0 or b.Whissueblock is null)
and t.notindpdmis is null and b.notindpdmis is null  and i.notindpdmis is null  
) group by warehouseid,itemid
) stto on stto.warehouseid=t.towarehouseid and stto.itemid=m.itemid

where t.status = 'C' " + whclause + @" "+ whstkout + @"  "+ whclause1 + @" and 
t.transferid in (select transferid from tbindents where status = 'C' and transferid is not null)
and t.transferid not in (select transferid from tbreceipts where status = 'C' and transferid is not null)
and t.transferdate between '01-Apr-24' and sysdate 
order by ti.indentdate desc,m.itemname ";



            var myList = _context.GetIWHPipelineDetailsDTODbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }
    }
}
