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
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;
using CgmscHO_API.WarehouseDTO;
//using Broadline.Controls;
//using CgmscHO_API.Utility;
namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Warehouse : ControllerBase
    {
        private readonly OraDbContext _context;
        public Warehouse(OraDbContext context)
        {
            _context = context;
        }
        [HttpGet("ReagIndentIssueDetails")]
        public async Task<ActionResult<IEnumerable<ReagentIndentIssueWHDetailsDTO>>> ReagIndentIssueDetails(string mmid)
        {
            string whmmid = "";
            if (mmid != "0")
            {
                whmmid = " and mc.mmid = " + mmid;
            }
            string qrya = @" select nocid, mmid,warehousename,districtname,facilityname,EQPNAME,make,model,indentdt,WhIssueDate,Round(sum(Indentvalue)/100000,2) as Indentvalue,count(distinct itemid) as nositems
from 
(
select d.districtname,f.facilityname,e.EQPNAME,mm.make,mm.model,
m.itemcode,m.itemname,m.unitcount,to_char(mc.nocdate,'dd-MM-yyyy') as indentdt,to_char(tb.indentdate,'dd-MM-yyyy') as WhIssueDate,mci.BOOKEDQTY,case when m.CTRCALB='Y' then 'Control/Cal/Consumable' else 'Reagent' end as CTRCALB 
,nvl(mci.BOOKEDQTY,0)*nvl(m.TESTUNITONREAG,0) as ReagentInTest,
Round(nvl(finalrategst,0)*mci.BOOKEDQTY,0) as Indentvalue,
nvl(finalrategst,0) as SKUFINALRATE,
nvl(tb.issuedQTY,0) as IssuedQTY,nvl(tb.IssueValue,0) as IssueValue,f.facilityid,m.ITEMID,d.districtid,mc.nocid
,mm.mmid,w.warehousename,w.warehouseid
from mascgmscnocitems mci 
inner join masitems m on m.itemid=mci.itemid and m.categoryid=62
inner join mascgmscnoc mc on mc.nocid=mci.nocid and mc.nocdate>'01-Sep-2024' and mc.mmid is not null  " + whmmid + @"
inner join masfacilities f on f.facilityid=mc.facilityid
inner join masdistricts d on d.districtid=f.districtid
inner join maswarehouses w on w.warehouseid=d.warehouseid
inner join masreagentmakemodel mm on mm.mmid = m.mmid
inner join masreagenteqp e on e.PMACHINEID=mm.PMACHINEID
left outer join 
(

 select indentdate,nocid,itemid,facilityid,sum(issuedQTY) as issuedQTY,sum(IssueValue) as IssueValue,finalrategst from
 (
select tb.nocid,tbi.itemid,tb.facilityid,tbo.outwno,nvl(tbo.ISSUEQTY,0) as issuedQTY,
nvl(tbo.ISSUEQTY,0)*aci.finalrategst as IssueValue,tb.indentdate,aci.finalrategst
from tbindents tb
inner join tbindentitems tbi on tbi.indentid=tb.indentid
inner join tboutwards tbo on tbo.INDENTITEMID=tbi.INDENTITEMID
inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
inner join soordereditems si on si.ponoid=rb.ponoid
inner join aoccontractitems aci on aci.contractitemid=si.contractitemid
where tb.status='C' and tb.indentdate>'01-Sep-2024'
)
group by nocid,itemid,facilityid,indentdate,finalrategst
) tb on tb.nocid=mc.nocid and  tb.itemid=mci.itemid

where 1=1 " + whmmid + @"
and nvl(tb.issuedQTY,0)>0  and nvl(mci.BOOKEDQTY,0)>0 and mc.mmid is not null and
 m.categoryid=62  and  mc.status='C' 
 ) group by mmid,districtname,facilityname,EQPNAME,indentdt,WhIssueDate,warehousename,model,make,nocid ";
            var myList = _context.GetReagentIndentIssueWHDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qrya)).ToList();

            return myList;
        }

        [HttpGet("ReagIndentIssueMMID")]
        public async Task<ActionResult<IEnumerable<ReagentIssueSummaryDTO>>> ReagIndentIssueMMID()
        {
            string qrya = @" select mmid, EQPNAME, make, model, count(distinct ITEMID) nosreagent,
count(distinct facilityid) as nosfac,Round(sum(IssueValue)/100000,2) as IssueValueSince3Sep
from
(
select d.districtname, f.facilityname, e.EQPNAME, mm.make, mm.model,
m.itemcode, m.itemname, m.unitcount, to_char(mc.nocdate,'dd-MM-yyyy') as indentdt,to_char(tb.indentdate,'dd-MM-yyyy') as WhIssueDate,mci.BOOKEDQTY,case when m.CTRCALB='Y' then 'Control/Cal/Consumable' else 'Reagent' end as CTRCALB
, nvl(mci.BOOKEDQTY,0)*nvl(m.TESTUNITONREAG,0) as ReagentInTest,
Round(nvl(finalrategst,0)*mci.BOOKEDQTY,0) as Indentvalue,
nvl(finalrategst,0) as SKUFINALRATE,
nvl(tb.issuedQTY,0) as IssuedQTY,nvl(tb.IssueValue,0) as IssueValue,f.facilityid,m.ITEMID,d.districtid,mc.nocid
,mm.mmid
from mascgmscnocitems mci
inner join masitems m on m.itemid=mci.itemid
inner join mascgmscnoc mc on mc.nocid= mci.nocid and mc.nocdate>'01-Sep-2024' and mc.mmid is not null
inner join masfacilities f on f.facilityid= mc.facilityid
inner join masdistricts d on d.districtid= f.districtid
inner join masreagentmakemodel mm on mm.mmid = m.mmid
inner join masreagenteqp e on e.PMACHINEID= mm.PMACHINEID
left outer join
(
 select indentdate, nocid, itemid, facilityid, sum(issuedQTY) as issuedQTY, sum(IssueValue) as IssueValue,finalrategst from (
select tb.nocid, tbi.itemid, tb.facilityid, tbo.outwno, nvl(tbo.ISSUEQTY,0) as issuedQTY,
nvl(tbo.ISSUEQTY,0)*aci.finalrategst as IssueValue,tb.indentdate,aci.finalrategst
from tbindents tb
inner join tbindentitems tbi on tbi.indentid=tb.indentid
inner join tboutwards tbo on tbo.INDENTITEMID= tbi.INDENTITEMID
inner join tbreceiptbatches rb on rb.inwno= tbo.inwno
inner join soordereditems si on si.ponoid= rb.ponoid
inner join aoccontractitems aci on aci.contractitemid= si.contractitemid
where tb.status= 'C' and tb.indentdate>'01-Sep-2024'
)
group by nocid,itemid,facilityid,indentdate,finalrategst
) tb on tb.nocid=mc.nocid and  tb.itemid=mci.itemid

where 1=1 and nvl(tb.issuedQTY,0)>0  and nvl(mci.BOOKEDQTY,0)>0 and mc.mmid is not null and
 m.categoryid= 62  and mc.status= 'C'
 ) group by EQPNAME,make,model,mmid ";
            var myList = _context.GetReagentIssueSummaryDTODbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qrya)).ToList();

            return myList;
        }

        [HttpGet("ReagIndentPendingEQ")]
        public async Task<ActionResult<IEnumerable<WHIndentPendingReagentEQDTO>>> ReagIndentPendingEQ()
        {
            //            string str = @" select mmid,EQPNAME,make,model,sum(indentvalue) as indentvalue,count(distinct facilityid) nosfac,count(distinct warehouseid) as noswh
            //from
            //(
            //select warehousename, districtname, facilityname, EQPNAME, make, model, count(distinct ITEMID) as nositems, round(sum(Indentvalue) / 100000, 2) as indentvalue, districtid, facilityid, nocid, mmid
            //, indentdt, nocdate, nocnumber, warehouseid
            //from
            //(
            //select d.districtname, f.facilityname, e.EQPNAME, mm.make, mm.model,
            //m.itemcode, m.itemname, m.unitcount, to_char(mc.nocdate, 'dd-MM-yyyy') as indentdt, mci.BOOKEDQTY,case when m.CTRCALB = 'Y' then 'Control/Cal/Consumable' else 'Reagent' end as CTRCALB ,nvl(mci.BOOKEDQTY, 0) * nvl(m.TESTUNITONREAG, 0) as ReagentInTest,Round(nvl(SKUFINALRATE, 0) * mci.BOOKEDQTY, 0) as Indentvalue,
            //nvl(tb.issuedQTY, 0) as IssuedQTY,f.facilityid,m.ITEMID,d.districtid,mc.nocid
            //,mm.mmid,mc.nocdate,mc.nocnumber,w.warehousename,w.warehouseid
            //from mascgmscnocitems mci
            //inner join masitems m on m.itemid = mci.itemid
            //inner join mascgmscnoc mc on mc.nocid = mci.nocid--and mc.nocdate > '01-Sept-2024'
            //inner join masfacilities f on f.facilityid = mc.facilityid
            //inner join masdistricts d on d.districtid = f.districtid
            //inner join maswarehouses w on w.warehouseid = d.warehouseid
            //inner join masreagentmakemodel mm on mm.mmid = m.mmid
            //inner join masreagenteqp e on e.PMACHINEID = mm.PMACHINEID
            //left outer join
            //(
            //select tb.nocid, tbi.itemid, tb.facilityid, nvl(sum(tbo.ISSUEQTY), 0) as issuedQTY from tbindents tb
            //inner join tbindentitems tbi on tbi.indentid = tb.indentid
            //inner join tboutwards tbo on tbo.INDENTITEMID = tbi.INDENTITEMID
            //where tb.status = 'C' group by tb.nocid, tbi.itemid, tb.facilityid
            //) tb on tb.nocid = mc.nocid and tb.itemid = mci.itemid
            // left outer join
            //                (
            //select vr.itemid, vr.SKUFINALRATE, m.TESTUNITONREAG as testi, round(vr.SKUFINALRATE / m.TESTUNITONREAG, 2) as singlerate from v_itemrate vr
            //inner join masitems m on m.itemid = vr.itemid
            //where m.categoryid = 62
            // )vr on vr.itemid = m.itemid
            //where 1 = 1 and nvl(tb.issuedQTY,0)= 0  and nvl(mci.BOOKEDQTY,0)> 0 and mc.mmid is not null and
            // m.categoryid = 62  and mc.status = 'C'
            // ) group by districtname, facilityname,EQPNAME,make,model,districtid,facilityid,nocid,mmid,nocdate,indentdt,nocnumber,warehousename,warehouseid
            //) 
            //group by EQPNAME,make,model,mmid ";



            string str = @" select MA.mmid,EQPNAME,make,model,
round(sum(indentvalue)/100000,2) as indentvalue,count(distinct facilityid) nosfac,count(distinct nocid) as noswh
from
(
select  Round(nvl(SKUFINALRATE, 0) * mci.BOOKEDQTY, 0) as Indentvalue,
mc.facilityid,m.ITEMID,mc.nocid
,mc.mmid from mascgmscnoc mc 
inner join mascgmscnocitems mci on mci.nocid=mc.nocid
inner join masitems m on m.itemid = mci.itemid and m.categoryid=62

left outer join
(
select tb.nocid, tbi.itemid, tb.facilityid, nvl(sum(tbo.ISSUEQTY), 0) as issuedQTY from tbindents tb
inner join tbindentitems tbi on tbi.indentid = tb.indentid
inner join tboutwards tbo on tbo.INDENTITEMID = tbi.INDENTITEMID
where tb.status = 'C'   and tb.indentdate > '01-Sep-2024' group by tb.nocid, tbi.itemid, tb.facilityid 
) tb on tb.nocid = mc.nocid and tb.itemid = mci.itemid
inner join 
(
select ci,itemid,FINALRATEGST as SKUFINALRATE,TESTUNITONREAG as testi,round(FINALRATEGST / TESTUNITONREAG, 2) as singlerate from 
(
select max(CONTRACTITEMID) ci,aci.itemid,TESTUNITONREAG from aoccontractitems aci
inner join masitems m on m.itemid =aci.itemid
where m.categoryid=62
group by aci.itemid,m.TESTUNITONREAG
) d 
inner join 
(
select CONTRACTITEMID,FINALRATEGST  from aoccontractitems aci
) ar on ar.CONTRACTITEMID=d.ci

 )vr on vr.itemid = m.itemid
where 1 = 1 and mc.nocdate > '01-Sep-2024' and mc.mmid is not null and nvl(tb.issuedQTY,0)= 0  and nvl(mci.BOOKEDQTY,0)> 0 and
 m.categoryid = 62  and mc.status = 'C'
 ) ma 
inner join masreagentmakemodel mm on mm.mmid = ma.mmid
inner join masreagenteqp e on e.PMACHINEID = mm.PMACHINEID
group by EQPNAME,make,model,MA.mmid ";

            var myList = _context.GetWHIndentPendingReagentEQDTODbSet
.FromSqlInterpolated(FormattableStringFactory.Create(str)).ToList();

            return myList;

        }


        [HttpGet("ReagIndentPending")]
        public async Task<ActionResult<IEnumerable<WHIndentPendingReagentDTO>>> ReagIndentPending(string mmid)
        {
            string whmmid = "";
            if (mmid != "0")
            {
                whmmid = " and mc.mmid = " + mmid;
            }
            string qrya = @"  select warehousename, districtname, facilityname, EQPNAME, make, model, count(distinct ITEMID) as nositems, round(sum(Indentvalue) / 100000, 2) as indentvalue,districtid,facilityid,nocid ,mmid
indentdt, nocdate, nocnumber
from
(
select d.districtname, f.facilityname, e.EQPNAME, mm.make, mm.model,
m.itemcode, m.itemname, m.unitcount, to_char(mc.nocdate, 'dd-MM-yyyy') as indentdt, mci.BOOKEDQTY,case when m.CTRCALB = 'Y' then 'Control/Cal/Consumable' else 'Reagent' end as CTRCALB ,nvl(mci.BOOKEDQTY, 0) * nvl(m.TESTUNITONREAG, 0) as ReagentInTest,Round(nvl(SKUFINALRATE, 0) * mci.BOOKEDQTY, 0) as Indentvalue,
nvl(tb.issuedQTY, 0) as IssuedQTY,f.facilityid,m.ITEMID,d.districtid,mc.nocid
,mm.mmid,mc.nocdate,mc.nocnumber,w.warehousename
from mascgmscnocitems mci
inner join masitems m on m.itemid = mci.itemid
inner join mascgmscnoc mc on mc.nocid = mci.nocid " + whmmid + @"
inner join masfacilities f on f.facilityid = mc.facilityid
inner join masdistricts d on d.districtid = f.districtid
inner join maswarehouses w on w.warehouseid = d.warehouseid
inner join masreagentmakemodel mm on mm.mmid = m.mmid
inner join masreagenteqp e on e.PMACHINEID = mm.PMACHINEID
left outer join
(
select tb.nocid, tbi.itemid, tb.facilityid, nvl(sum(tbo.ISSUEQTY), 0) as issuedQTY from tbindents tb
inner join tbindentitems tbi on tbi.indentid = tb.indentid
inner join tboutwards tbo on tbo.INDENTITEMID = tbi.INDENTITEMID
where tb.status = 'C'
 group by tb.nocid, tbi.itemid, tb.facilityid
) tb on tb.nocid = mc.nocid and tb.itemid = mci.itemid


 inner join 
(
select ci,itemid,FINALRATEGST as SKUFINALRATE,TESTUNITONREAG as testi,round(FINALRATEGST / TESTUNITONREAG, 2) as singlerate from 
(
select max(CONTRACTITEMID) ci,aci.itemid,TESTUNITONREAG from aoccontractitems aci
inner join masitems m on m.itemid =aci.itemid
where m.categoryid=62
group by aci.itemid,m.TESTUNITONREAG
) d 
inner join 
(
select CONTRACTITEMID,FINALRATEGST  from aoccontractitems aci
) ar on ar.CONTRACTITEMID=d.ci

 )vr on vr.itemid = m.itemid
 
where 1 = 1 " + whmmid + @" and nvl(tb.issuedQTY,0)= 0  and nvl(mci.BOOKEDQTY,0)> 0 and mc.mmid is not null and
 m.categoryid = 62  and mc.status = 'C'
 ) group by districtname, facilityname,EQPNAME,make,model,districtid,facilityid,nocid,mmid,nocdate,indentdt,nocnumber,warehousename
 order by nocdate desc ";
            var myList = _context.GetWHIndentPendingReagentDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qrya)).ToList();

            return myList;
        }
        [HttpGet("IndentPending")]
        public async Task<ActionResult<IEnumerable<WHIndentPendingDTO>>> IndentPending(string per, string clause)
        {
            string qry = "";
            if (per == "All")
            {
                qry = @" select warehouseid,warehousename,count(distinct nocnumber) nosIndent
,sum(DMEFAC) as dmefac,sum(AYUSH) as AYUSH,sum(DHS) as dhs,0 as PER
from 
(
select distinct w.warehousename,w.warehouseid,  f.facilityname,mn.nocnumber,to_char(mn.nocdate,'dd-MM-yyyy') nocdate , 
to_date(mn.nocdate,'dd-MM-yyyy') inddt
,round(sysdate-mn.nocdate) as pendingday
,case when (round(sysdate-mn.nocdate))<=7 then 'Under 7 Days'  
else case when (round(sysdate-mn.nocdate))>7 and (round(sysdate-mn.nocdate))<=15   then '7-15 Days'
else '>15 Days' end end as Per
,f.facilitytypeid,
case when f.facilitytypeid in (364,378) then 1 else 0 end as DMEFAC,
case when f.facilitytypeid in (371) then 1 else 0 end as AYUSH,
case when f.facilitytypeid in (364,378,371) then 0 else 1 end as DHS
from mascgmscnoc mn 
inner join mascgmscnocitems mni on mni.nocid=mn.nocid
inner join masfacilities f on f.facilityid=mn.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join masfacilitywh fw on fw.facilityid=f.facilityid
inner join maswarehouses w on w.warehouseid=fw.warehouseid
where mn.status in ('C')  and mni.bookedqty>0
and mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)
and mn.nocdate between SYSDATE -30 and sysdate 
) 
where 1=1
group by warehouseid,warehousename
order by count(distinct nocnumber) desc";
            }
            else
            {
                qry = @"  select warehouseid, warehousename, count(distinct nocnumber) nosIndent,Per
,sum(DMEFAC) as dmefac,sum(AYUSH) as AYUSH,sum(DHS) as dhs
from
                (
select distinct w.warehousename, w.warehouseid, f.facilityname, mn.nocnumber, to_char(mn.nocdate, 'dd-MM-yyyy') nocdate,
to_date(mn.nocdate, 'dd-MM-yyyy') inddt
, round(sysdate - mn.nocdate) as pendingday
,case when(round(sysdate - mn.nocdate))<= 7 then 'Under 7 Days'
else case when(round(sysdate - mn.nocdate))> 7 and(round(sysdate - mn.nocdate)) <= 15   then '7-15 Days'
else '>15 Days' end end as Per
,f.facilitytypeid,
case when f.facilitytypeid in (364, 378) then 1 else 0 end as DMEFAC,
case when f.facilitytypeid in (371) then 1 else 0 end as AYUSH,
case when f.facilitytypeid in (364, 378, 371) then 0 else 1 end as DHS
from mascgmscnoc mn
inner
join mascgmscnocitems mni on mni.nocid = mn.nocid
inner
join masfacilities f on f.facilityid = mn.facilityid
inner
join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
inner
join masfacilitywh fw on fw.facilityid = f.facilityid
inner
join maswarehouses w on w.warehouseid = fw.warehouseid
where mn.status in ('C')  and mni.bookedqty > 0
and mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)
and mn.nocdate between SYSDATE - 30 and sysdate
) 
where 1 = 1 and Per = '" + clause + @"'
group by warehouseid,warehousename,Per
order by count(distinct nocnumber) desc ";

            }

            var myList = _context.WHIndentPendingDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("IndentPendingDetails")]
        public async Task<ActionResult<IEnumerable<WHIndentPendingDetailsDTO>>> IndentPendingDetails(string whid, string clause, string factype)
        {
            string qry = "";
            string whfacttype = "";
            if (factype == "364")
            {
                whfacttype = " and   DMEFAC=1";
            }
            if (factype == "371")
            {
                whfacttype = " and   AYUSH=1";
            }
            if (factype == "367")
            {
                whfacttype = " and   DHS=1";
            }

            string whclauseWHID = "";
            if (whid != "0")
            {
                whclauseWHID = " and w.warehouseid=" + whid + @"";
            }



            string whclause = "";
            if (clause == "2")
            {
                whclause = " and   round(sysdate - mn.nocdate)<=7";
            }
            if (clause == "3")
            {
                whclause = " and   round(sysdate - mn.nocdate)>7 and round(sysdate - mn.nocdate)<=15";
            }
            if (clause == "4")
            {
                whclause = " and   round(sysdate - mn.nocdate)>15";
            }

            qry = @"  select warehousename,FACILITYNAME,NOCNUMBER,nositems,INDDT,PENDINGDAY,PER,DHS,DMEFAC,AYUSH from 
(

select  w.warehousename, w.warehouseid, f.facilityname, mn.nocnumber, to_char(mn.nocdate, 'dd-MM-yyyy') nocdate,
to_date(mn.nocdate, 'dd-MM-yyyy') inddt
, round(sysdate - mn.nocdate) as pendingday
,case when(round(sysdate - mn.nocdate))<= 7 then 'Under 7 Days'
else case when(round(sysdate - mn.nocdate))> 7 and(round(sysdate - mn.nocdate)) <= 15   then '7-15 Days'
else '>15 Days' end end as Per
,f.facilitytypeid,
case when f.facilitytypeid in (364, 378) then 1 else 0 end as DMEFAC,
case when f.facilitytypeid in (371) then 1 else 0 end as AYUSH,
case when f.facilitytypeid in (364, 378, 371) then 0 else 1 end as DHS
,count(distinct mni.ITEMID) as nositems
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid = mn.nocid
inner
join masfacilities f on f.facilityid = mn.facilityid
inner
join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid
inner
join masfacilitywh fw on fw.facilityid = f.facilityid
inner
join maswarehouses w on w.warehouseid = fw.warehouseid
where mn.status in ('C')  and mni.bookedqty > 0
and mn.nocid not in (select nocid from tbindents where status in ('C') and nocid is not null)
and mn.nocdate between SYSDATE - 30 and sysdate " + whclauseWHID + @"
 " + whclause + @"
group by w.warehousename, w.warehouseid, f.facilityname, mn.nocnumber,mn.nocdate,f.facilitytypeid
) where 1=1 " + whfacttype + @" 
order by PENDINGDAY desc";
            var myList = _context.WHIndentPendingDetailsDbSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;

        }

        [HttpGet("MCHAIvsIssuance")]
        public async Task<ActionResult<IEnumerable<MCAIVSIssuanceDTO>>> MCHAIvsIssuance(string faciid)
        {



            FacOperations f = new FacOperations(_context);
            string yearid = f.getACCYRSETID();


            Int64 WHIID = f.getWHID(faciid);

            string qry = @" select mcid,MCATEGORY,count(distinct itemid) as nositems,
Round(sum(appivalue)/10000000,2) as ivalue
,sum(IssuedCount) as nosissued,Round(sum(IssuedValue)/10000000,2) as issueValued
,sum(readystkavailabe) as ReadystkavailableAgainstAI,sum(OnlyPipeline) as NotIssuedOnlyPipeline
,sum(balanccount) as NosBalanceStockAvailable,sum(balanccountNotLift) as NotLiftedBalanceStock
,Round(sum(AvailableValue)/10000000,2) as TotalBAlStockValue,
sum(concernStockoutButAvailableInOTherWH) as concernStockoutButAvailableInOTherWH
,sum(IssuedMorethanAI) as IssuedMorethanAI,Round(sum(IssuedMorethanAIExtraVAlue)/10000000,2) as IssuedMorethanAIExtraVAlue
,sum(TotalNOCTaken) as TotalNOCTaken,Round(sum(NOCValue)/10000000,2) as TotalNOCValue,
sum(NoIssued_andNOCTaken) as NotIssuedStockOutNOCTaken,
sum(LPOGen) as LPOGen
,Round(sum(lpovalue)/10000000,2) as lpovalue from 
(

select itemid,mcid, MCATEGORY,itemcode,itemname,strength1,unit,ai,Round(ai* SKUAprxRate,0) as appivalue,IssuedQTY,IssuedValue,NocQty,NOCValue
,BalancetobeLiftQTY,(case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
else 0 end  end) as AvailableStock,
Round(((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
else 0 end  end))* SKUAprxRate,0)
as AvailableValue,case when ((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
else 0 end  end))>0 then 1 else 0 end balanccount,
case when ((case when BalancetobeLiftQTY>0 and BalancetobeLiftQTY>whready  then whready
else case when whready>BalancetobeLiftQTY then BalancetobeLiftQTY
else 0 end  end))>0 and IssuedQTY=0 then 1 else 0 end balanccountNotLift,
whready,whUQC,WHPipeline,SKUAprxRate
,TotalNOCTaken,RCcount,IssuedCount,IssuedMorethanAI,NoIssued_andNOCTaken,OnlyPipeline,LPOGen,readystkavailabe,lpovalue
,IssuePEr
, case when BalancetobeLiftQTY=0 then 100 
else case when (whready+whUQC+WHPipeline)>0 and BalancetobeLiftQTY>0 then Round((whready+whUQC+WHPipeline)/BalancetobeLiftQTY*100,0)    else
0 end end  as availbleper,
 case when BalancetobeLiftQTY=0 then 100 
else case when (OtherWHReady+OtherWHUQC)>0 and BalancetobeLiftQTY>0 then Round((OtherWHReady+OtherWHUQC)/BalancetobeLiftQTY*100,0)    else
0 end end  as availbleperotherWH
,OtherWHReady+OtherWHUQC otherWHCGMSCStock
,Round(IssuedMorethanAIExtraVAlue,0) as IssuedMorethanAIExtraVAlue
,case when IssuedMorethanAI =0 and (nvl(whready,0)+nvl(whUQC,0))=0 and nvl(OtherWHReady,0)+nvl(OtherWHUQC,0)>0
then 1 else 0 end as concernStockoutButAvailableInOTherWH
from 
(
select mc.mcid,mc.MCATEGORY,ty.itemtypename,
m.itemcode
,m.itemname,m.strength1,m.unit
,a.ai,nvl(iss.ISS_Qty,0) as IssuedQTY
,Round(nvl(IssuedValue,0),0) as IssuedValue
,case when a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 then (a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))) else 0 end as BalancetobeLiftQTY
,case when a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 then Round((a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))*nvl(SKUrate,0),0) else 0 end as BalancetobeLiftValue
,case when ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))>0 and a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 and 
((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))-(a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))>0 then a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))
else ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)) end as BalanceStkAvialbeinWHQTY
,nvl(SKUrate,0) as SKUAprxRate
,Round((case when ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))>0 and a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))>0 and 
((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))-(a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0)))>0 then a.ai-(nvl(iss.ISS_Qty,0)+nvl(NocQty,0))
else ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)) end)*nvl(SKUrate,0),0) as BalanceStkAvialbeinWHValue
,(nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0)) as whready ,nvl(vwh.UQC,0) as whUQC, nvl(vwh.TOTLPIPELINE,0) WHPipeline
,case when (((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0)))>0 and (a.ai-nvl(iss.ISS_Qty,0))>0 and nvl(iss.ISS_Qty,0)=0 then 1 else 0 end as BalanceStkAvialbeinWH,
case when (nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))>0 then 1 else 0 end as readystkavailabe,
case when nvl(IssuedValue,0)=0 and  ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))=0 and nvl(vwh.TOTLPIPELINE,0)>0 then 1 else 0 end as OnlyPipeline,
case when nvl(iss.ISS_Qty,0)>0 then Round(nvl(iss.ISS_Qty,0)/nvl(a.ai,1)*100,2) else 0 end as IssuePEr 
,nvl(NocQty,0) as NocQty,
nvl(NocQty,0)*nvl(SKUrate,0) as NOCValue
,nvl(poqty,0) as poqty,nvl(povalue,0) as lpovalue
,case when  nvl(NocQty,0)>0  then 1 else 0 end as TotalNOCTaken
,case when nvl(NocQty,0)>0 and  nvl(iss.ISS_Qty,0)=0 and   ((nvl(vwh.READY,0)+nvl(vwh.IWHPIPE,0))+nvl(vwh.UQC,0))=0 then 1 else 0 end as NoIssued_andNOCTaken
,case when nvl(poqty,0)>0 then  1 else 0 end as LPOGen
,case when nvl(iss.ISS_Qty,0)>a.ai then  1 else 0 end as IssuedMorethanAI
,case when nvl(iss.ISS_Qty,0)>a.ai then (nvl(iss.ISS_Qty,0)-a.ai)*(Round(nvl(IssuedValue,0),0)/nvl(iss.ISS_Qty,0)) else 0 end IssuedMorethanAIExtraVAlue
,case when nvl(iss.ISS_Qty,0) >0 then 1 else 0 end IssuedCount
,m.unitcount,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as edltype 
,case when rc.itemid is not null then 1 else 0 end as RCcount,
m.itemid,a.facilityid,nvl(OtherWHReady,0) OtherWHReady,nvl(OtherWHUQC,0) as  OtherWHUQC
from  V_InstitutionAI a
inner join masitems m on m.itemid=a.itemid
left outer join 
(
select distinct itemid from v_rcvalid
) rc on rc.itemid=m.itemid

left outer join 
(
select itemid,APRXRATE,SKUFINALRATE,case when SKUFINALRATE is not null then SKUFINALRATE else case when APRXRATE is not null then APRXRATE else 0 end end as SKUrate 
from v_itemrate 
) r on r.itemid=m.itemid


 inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
left outer join masitemgroups g on g.groupid =m.groupid
left outer join masitemtypes ty on ty.itemtypeid=m.itemtypeid
left outer join masitemcategories c on c.categoryid=m.categoryid
left outer join vwhstockwithexp vwh on vwh.itemid=m.itemid and vwh.WAREHOUSEID=" + WHIID + @"
left outer join 
(

select facilityid,itemid,sum(ISS_Qty) as iss_qty,sum(finalvalue) as IssuedValue
from 
(
select  f.facilityid,tb.indentid,tbi.itemid,tbi.indentitemid,rb.batchno iss_batchno,tbi.needed indentqty,
        sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty ,sb.acibasicratenew skurate,acicst,acgstpvalue,
        acivat,case when rb.ponoid=1111 then 0 else  sb.acisingleunitprice end  finalrate,
        (sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) * sb.acibasicratenew) as skuvalue, 
        (sum(tbo.issueqty) * sb.acisingleunitprice) as finalvalue,to_char(tb.indentdate,'dd-MM-yyyy') issuedate,tb.indentno,rb.inwno
         from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         left outer join 
                (
                select s.ponoid,tbo.inwno,si.basicrate, a.indentdate,c.batchno,tbo.indentitemid
               ,coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.exciseduty,0))/100) ),2),si.singleunitprice) as supp 
               ,case when  a.indentdate >= '01-Jul-2017' then ( case when  aci.basicratenew is null then aci.basicrate else   aci.basicratenew  end) else aci.basicrate end acibasicratenew
               ,case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then aci.finalrategst else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end) else coalesce(round(((si.basicrate) + ((si.basicrate *si.percentvalue)/100)+((si.basicrate *nvl(si.ExciseDuty,0))/100) ),2),si.singleunitprice)  end   ACIsingleunitprice
               ,case when aci.cstvat ='CST' then aci.percentvalue  else 0 end ACICST,  
               case when aci.cstvat ='VAT' then aci.percentvalue  else 0 end ACIVAT,  
               case when  a.indentDate >= '01-Jul-2017' then  (case when aci.gstflag='Y' then nvl(aci.percentvaluegst,0) else 0 end) else 0 end  ACGSTPvalue   from tbindents a
               inner join tbindentitems b on a.indentid = b.indentid
               inner join tboutwards tbo on tbo.indentitemid=b.indentitemid
               inner join tbreceiptbatches c on  tbo.inwno = c.inwno 
               inner join tbreceiptitems ri on ri.receiptitemid=c.receiptitemid
               inner join tbreceipts r on r.receiptid=ri.receiptid
               left outer join soorderplaced s on s.ponoid=c.ponoid
               inner join soordereditems si on  si.ponoid=s.ponoid and si.itemid=b.itemid   
               INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
               inner join aoccontracts ac on ac.contractid=aci.contractid
               where a.Status='C' and a.notindpdmis is null 
               and b.notindpdmis is null and c.notindpdmis is null 
               and tbo.notindpdmis is null 
               group by s.ponoid,tbo.inwno,si.basicrate,a.indentDate,tbo.indentitemid,
               c.batchno,si.singleunitprice,si.percentvalue,si.ExciseDuty,aci.basicratenew,aci.basicrate,aci.percentvalue,aci.percentvaluegst,aci.cstvat
               ,r.receiptdate,aci.gstflag,aci.singleunitprice,aci.singleunitprice,aci.finalrategst
                 )  sb on sb.inwno=rb.inwno and sb.indentitemid=tbo.indentitemid
        where tb.status = 'C' and tb.issuetype='NO'
       and  tb.indentdate Between ( select startdate from masaccyearsettings where accyrsetid= " + yearid + @" ) and ( select enddate from masaccyearsettings where accyrsetid= " + yearid + @" )  and tb.facilityid=" + faciid + @"
        and tb.notindpdmis is null and tb.notindpdmis is null and tbi.notindpdmis is null   
        and tbo.notindpdmis is null and rb.notindpdmis is null   
        group by tb.indentid,tbi.needed,tbi.itemid,tbi.indentitemid,rb.batchno,
      f.facilityid  ,sb.acisingleunitprice,sb.acibasicratenew,tb.indentdate,rb.ponoid ,acicst,acgstpvalue,acivat,tb.indentdate,tb.indentno,rb.inwno

        ) group by facilityid,itemid
) iss on iss.itemid=m.itemid and iss.facilityid=a.facilityid


left outer join 
(

select itemid,sum(NocQty) as NocQty,nvl(sum(poqty),0) as poqty,nvl(sum(povalue),0) as povalue,nvl(sum(receiptqty),0) as receiptqty
from
(
select mc.MCATEGORY ,m.itemid,m.itemcode,lpitemcode,m.itemname,m.strength1,m.unit,mn.nocid,
mn.nocnumber nocno,To_Char(mn.nocdate,'dd-MM-yyyy') as nocdate,
mni.approvedqty NocQty,
so.tenderno,so.tenderdesc,so.contractno,
Round(so.absqty/nvl(m.unitcount,1),0) as poqty,Round(so.singleunitprice,2)*nvl(m.unitcount,1) as singleunitprice
,round(so.absqty*so.singleunitprice,2) as povalue
,so.suppliername,Round(so.receiptqty/nvl(m.unitcount,1),0) receiptqty,
case when LPODate is not null then To_Char(LPODate,'dd-MM-yyyy') else '-' end as  POdate
,case when RDate is not null then To_Char(RDate,'dd-MM-yyyy') else '-' end as  RDate,LPBUDGETID,BUDGETNAME
from mascgmscnoc mn
inner join mascgmscnocitems mni on mni.nocid=mn.nocid and mni.ISCANCEL is null
inner join masitems m on m.itemid=mni.itemid
 inner join masitemcategories c on c.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = c.MCID
inner join masfacilities f on f.facilityid=mn.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
inner join masitemcategories mic on mic.categoryid = m.categoryid
left outer join
(
select nocid,tenderno,tenderdesc,contractno,lpitemid ,lpitemcode,edlitemcode,sum(nvl(absqty,0)) absqty,singleunitprice,sum(povalue) as povalue, 
sum(receiptqty)  receiptqty,sum(recvalue) as recvalue,suppliername,max(LPODate) as LPODate,max(RDate) as RDate,LPBUDGETID,BUDGETNAME from  
(
select t.tenderno,t.tenderdetails tenderdesc,c.contractno,si.nocid,so.ponoid,si.lpitemid,vp.itemcode as lpitemcode,vp.edlitemcode,
sum(nvl(si.absqty,0)) absqty,si.singleunitprice,sum(nvl(si.itemvalue,0)) as povalue,
nvl(r.receiptqty,0) receiptqty 
,nvl(r.receiptqty,0)*nvl(si.singleunitprice,0) as recvalue,s.suppliername,max(PODATE) as LPODate,max(RDate) as RDate,so.LPBUDGETID,lbg.BUDGETNAME
from lpsoorderplaced so
inner join lpSOORDEREDITEMS si on si.ponoid=so.ponoid
inner join masfacilities f on f.facilityid=so.psaid
inner join vmasitems vp on vp.itemid=si.lpitemid
inner join lpcontracts c on c.contractid = so.contractid
inner join lpmastenders t on t.tenderid = c.schemeid
inner join lpmassuppliers s on s.LPSUPPLIERID = so.LPSUPPLIERID
left outer join maslpbudget lbg on lbg.lpbudgetid=so.LPBUDGETID
left outer join 
(
select tb.ponoid,m.itemid,m.edlitemcode,sum(tbr.absrqty) receiptqty,max(FACRECEIPTDATE) as RDate from tbfacilityreceipts tb
inner join tbfacilityreceiptitems tbi on tbi.facreceiptid=tb.facreceiptid
inner join tbfacilityreceiptbatches tbr on tbr.facreceiptitemid=tbi.facreceiptitemid
inner join masfacilities f on f.facilityid=tb.facilityid
inner join vmasitems m on m.itemid=tbi.itemid
where   tb.ponoid is not null and m.edlitemcode is not null
group by tb.ponoid,m.itemid,m.edlitemcode 
) r on r.ponoid= so.ponoid and r.itemid=si.lpitemid
where  si.nocid is not null and vp.edlitemcode is not null 
group by t.tenderno,t.tenderdetails,c.contractno,si.nocid,si.lpitemid,vp.itemcode,vp.edlitemcode,so.ponoid,r.receiptqty,si.singleunitprice,s.suppliername,so.LPBUDGETID,lbg.BUDGETNAME
) group by  nocid,tenderno,tenderdesc,contractno,lpitemid ,lpitemcode,edlitemcode,singleunitprice,suppliername,LPBUDGETID,BUDGETNAME
) so on so.nocid=mni.nocid and so.edlitemcode=m.itemcode
where 1=1 and mn.status='C' and nvl( mni.approvedqty,0)>0 and mn.nocdate between  ( select startdate from masaccyearsettings where accyrsetid= " + yearid + @" ) and ( select enddate from masaccyearsettings where accyrsetid= " + yearid + @" ) 
  and f.facilityid = " + faciid + @"
) group by itemid
) nc on nc.itemid=m.itemid

left outer join 
(
select itemid,sum(nvl(ReadyForIssue,0)) as OtherWHReady,sum(nvl(Pending,0)) as OtherWHUQC from 
(
 select z.zoneid ,w.warehouseid,b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0))    
 else (case when mi.Qctest ='N' then (nvl(b.absrqty,0) - nvl(iq.issueqty,0) )  end ) end ) ReadyForIssue,    
 case when mi.qctest='N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then (nvl(b.absrqty,0)- nvl(iq.issueqty,0)) end) end Pending
 from tbreceiptbatches b
 inner join tbreceiptitems i on b.receiptitemid=i.receiptitemid
 inner join tbreceipts t on t.receiptid= i.receiptid
 inner join maswarehouses w on w.warehouseid=t.warehouseid
 inner join maszone z on z.zoneid=w.zoneid
 inner join masitems mi on mi.itemid= i.itemid
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid=tbi.indentitemid and tbi.indentid= tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null 
     and tbi.notindpdmis is null                   
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid=i.itemid
     Where  t.warehouseid not in (" + WHIID + @") and T.Status = 'C'  And(b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate>sysdate
     )
     group by itemid
) stw on stw.itemid=m.itemid
where 1=1
and a.facilityid=" + faciid + @" 
and accyrsetid=" + yearid + @" and AI>0  
)
)
group by MCATEGORY,mcid
order by mcid ";
            var myList = _context.MCAIVSIssuanceDBSet
           .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();

            return myList;
        }

        [HttpGet("TravelVouchers")]
        public async Task<ActionResult<IEnumerable<TransportVoucherDTO>>> TravelVouchers(string vid, string indentId)
        {


            string whremid = "";
            string whIndentId = "";

            if (indentId != "0")
            {
                whIndentId = " and tbb.indentid = " + indentId;
            }

            if (vid == "0")
            {

            }
            else if (vid == "Null")
            {

            }
            else
            {
                whremid = " and tb.VID=" + vid;
            }





            string qry = @" select f.facilityid, f.facilityname,d.districtname,tbb.indentno as IssueVoucher,to_char(tbb.indentdate,'dd-MM-yyyy') IssueVoucherDT,nositems
,tbi.ENTRYDATE as travelvoucherIssueDT,
case when tbi.IspartialRelease ='Y' then 'Partialy Dispatch of Voucher' else 'Fully Dispatch of Voucher' end IspartialRelease
,noc.nocnumber as IndentNo,to_char(noc.nocdate,'dd-MM-yyyy') IndenDT
,tb.VID,tbi.travaleid,tb.voucherid,tbb.indentid,f.facilityname||'-'||tbb.indentno as details
,f.LONGITUDE,f.LATITUDE
from TBTRAVALEMASTER tb 
inner join TBINDENTTRAVALE tbi on tbi.voucherid=tb.voucherid
inner join tbindents tbb on tbb.indentid=tbi.indentid
inner join 
(
select tbi.INDENTID,count(distinct ITEMID) as nositems from tbindentitems tbi
group by tbi.INDENTID
) it on it.INDENTID=tbb.indentid
left outer join mascgmscnoc noc on noc.nocid=tbb.nocid and noc.status='C'
inner join masfacilities f on f.facilityid=tbb.facilityid
inner join masdistricts d on d.districtid=f.districtid
where tbb.status='C' and tb.status='C' " + whremid + @" " + whIndentId + @"
and tbi.ENTRYDATE > '01-Dec-2024'
and tbi.ISDROP is null order by tbi.ENTRYDATE desc  ";
            var myList = _context.TransportVoucherDBSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpPut("updateTBIndentTravaleWH")]
        public IActionResult UpdateTBIndentTravaleWH(long travelId, string? latitude, string? longitude, string dt1)
        {
            // Validate the date and time format (dd-MM-yyyy HH:mm:ss)
            if (!DateTime.TryParseExact(dt1, "dd-MM-yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
            {
                return BadRequest("Invalid date format. Please use dd-MM-yyyy HH:mm:ss.");
            }

            // Convert the date to a format Oracle understands
            string formattedDate = parsedDate.ToString("yyyy-MM-dd HH:mm:ss");

            // Create the query string
            string qry = $@"
        UPDATE TBINDENTTRAVALE
        SET ISDROP = 'Y',
            DROPDATE = TO_TIMESTAMP('{formattedDate}', 'YYYY-MM-DD HH24:MI:SS'),
            DROPLONGITUDE = '{longitude}',
            DROPLATITUDE = '{latitude}',
            isDropbyAPL = 'Y'
        WHERE TRAVALEID = {travelId}";

            // Execute the query
            _context.Database.ExecuteSqlRaw(qry);

            return Ok("Successfully Saved");
        }


        //[HttpPut("updateTBIndentTravaleWH")]
        //public IActionResult updateTBIndentTravaleWH(long travelId, string? latitude, string? longitude, string dt1)
        //{
        //    // Validate the date format (dd-MM-yyyy)
        //    if (!DateTime.TryParseExact(dt1, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
        //    {
        //        return BadRequest("Invalid date format. Please use dd-MM-yyyy.");
        //    }

        //    // Convert date to Oracle's preferred format (e.g., DD-MON-YYYY)
        //    string formattedDate = parsedDate.ToString("dd-MMM-yyyy").ToUpper();

        //    string qry = $@"
        //UPDATE TBINDENTTRAVALE
        //SET ISDROP = 'Y',
        //    DROPDATE = TO_DATE('{formattedDate}', 'DD-MON-YYYY'),
        //    DROPLONGITUDE = '{longitude}',
        //    DROPLATITUDE = '{latitude}'
        //WHERE TRAVALEID = {travelId}";

        //    _context.Database.ExecuteSqlRaw(qry);

        //    return Ok("Successfully Saved");
        //}



        [HttpPut("updateTBIndentTravale")]
        public IActionResult updateTBIndentTravale(Int64 travelId, string? latitude, string? longitude)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");    

            
            string qry = @" update TBINDENTTRAVALE set ISDROP = 'Y' ,DROPDATE = '" + dt1 + "',DROPLONGITUDE = '" + longitude + "',DROPLATITUDE = '" + latitude + @"' 
                        where travaleid = " + travelId + "  ";
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Saved");

        }

        [HttpPut("insertFistLatLontToFacility")]
        public IActionResult insertFistLatLontToFacility(Int64 facilityId, string latitude, string longitude)
        {
            string dt1 = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

            string qry = @" update masfacilities set isDriverAddedPosition = 'Y' ,PositionEntryByDriver = '" + dt1 + "',LONGITUDE = '" + longitude + "',LATITUDE = '" + latitude + @"' 
                        where FACILITYID = " + facilityId + "  ";
            _context.Database.ExecuteSqlRaw(qry);
            return Ok("Successfully Saved");

        }

        [HttpGet("GetWarehouseInfo")]
        public async Task<ActionResult<IEnumerable<WarehousesModel>>> GetWarehouseInfo(Int64 whid)
        {
            string qry = "";


            qry = @"  SELECT
                         warehouseid,
                         warehousecode,
                         warehousename,
                         warehousetypeid,
                         address1,
                         address2,
                         address3,
                         city,
                         zip,
                         phone1,
                         phone2,
                         fax,
                         email,
                         isactive,
                         isgovernment,
                         stateid,
                         districtid,
                         locationid,
                         remarks,
                         contactpersonname,
                         amid,
                         soid,
                         parentid,
                         gdt_entry_date,
                         facilitytypeid,
                         facilitytype,
                         poper,
                         entry_date,
                         zoneid,
                         latitude,
                         longitude,
                         isanpractive
                    FROM
                        maswarehouses
                    WHERE
                    WAREHOUSEID = " + whid + " ";

            var myList = _context.WarehousesDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("GetVehicleEntriesExits")]
        public async Task<ActionResult<IEnumerable<GetEnterExitVhicleDTO>>> GetVehicleEntriesExits(Int64 whid, Int64 previousNDays, Int64 tranId, string plateNo)
        {
            string qry = "";
            string whwhid = "";
            string whPreviousNDays = "";
            string whTranId = "";
            string whPlateNo = "";

            if (whid != 0)
            {
                whwhid = " AND w.warehouseid = " + whid + @" ";
            }

            if (tranId != 0)
            {
                whTranId = "  AND v.tranid = " + tranId + " ";
            }

            if (previousNDays != 0)
            {
                whPreviousNDays = " AND v.ENTRYDATE >= TRUNC(SYSDATE) - " + previousNDays + @"  AND v.ENTRYDATE < TRUNC(SYSDATE) ";
            }

            if (plateNo != "0")
            {
                whPlateNo = "  AND v.VPLATENO = '"+ plateNo + "' ";
            }

            qry = @" SELECT v.TRANID,
       v.VPLATENO,
       v.DIRECTION,
       TO_CHAR(v.VDATE, 'dd-MM-yyyy') AS VDATE,
       v.ENTRYDATE,
       v.camid,
       vh.WAREHOUSEID,
       w.WAREHOUSENAME
FROM maswhvehicletransport v
LEFT OUTER JOIN masvehical vh ON vh.VEHICALNO = v.VPLATENO
LEFT OUTER JOIN maswarehouses w ON w.warehouseid = vh.warehouseid
WHERE 1 = 1 
  " + whwhid + @"
 " + whTranId + @"
  "+ whPlateNo + @"
  " + whPreviousNDays + @" ";

            var myList = _context.GetEnterExitVhicleDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetVehicleNo")]
        public async Task<ActionResult<IEnumerable<GetVehicleNoDTO>>> GetVehicleNo()
        {
            string qry = "";
            //String whWhid = "";

            //if(whid != "0")
            //{
            //    whWhid = " and warehouseid = "+ whid + " ";
            //}

            qry = @" select vid, (vehicalno || '-' || VEHICALMODEL) as  vehicalno from masvehical where isactive = 'Y' ";

            var myList = _context.GetVehicleNoDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetVehicleNoByWh")]
        public async Task<ActionResult<IEnumerable<GetVehicleNoDTO>>> GetVehicleNoByWh(string whid)
        {
            string qry = "";
            String whWhid = "";

            if (whid != "0")
            {
                whWhid = " and warehouseid = " + whid + " ";
            }

            qry = @" select vid, (vehicalno || '-' || VEHICALMODEL) as  vehicalno from masvehical where isactive = 'Y' " + whWhid + " ";

            var myList = _context.GetVehicleNoDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("GetLatLong")]
        public async Task<ActionResult<IEnumerable<GetLatLongDTO>>> GetLatLong(Int32 indentId)
        {
            string qry = "";
            string whIndent = "";

            if(indentId != 0)
            {
                whIndent = " and ti.indentid = "+ indentId + " ";
            }

            qry = @" select f.latitude, f.longitude,case when (tld.DROPLATITUDE in ('-00.0000','null','00.0000')or (tld.DROPLATITUDE is null )) and f.latitude is not null then f.latitude else tld.DROPLATITUDE end as lat
,case when (tld.DROPLONGITUDE in ('-00.0000','null','00.0000')or (tld.DROPLONGITUDE is null )) and f.longitude is not null then f.longitude else tld.DROPLONGITUDE end as lon
, f.facilityid 
,f.facilityname
from masfacilities f 
inner join tbindents ti on ti.facilityid = f.facilityid 
left outer join 
(
    select max(td.TRAVALEID) as TRAVALEID, f.facilityid from TBINDENTTRAVALE td
    inner join tbindents ti on ti.INDENTID = td.INDENTID 
    inner join masfacilities f on f.facilityid = ti.facilityid
    where (DROPLONGITUDE is not null and DROPLATITUDE is not null)
    group by f.facilityid
) td on td.facilityid = f.facilityid 
left outer join TBINDENTTRAVALE tld on tld.TRAVALEID = td.TRAVALEID
where 1=1 and facilitytypeid not in (377,371) and f.isactive = 1  " + whIndent + @"
 ";

            var myList = _context.GetLatLongDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

    }
}
