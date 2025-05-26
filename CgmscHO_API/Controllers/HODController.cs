using CgmscHO_API.HodDTO;
using CgmscHO_API.HODTO;
using CgmscHO_API.Models;
using CgmscHO_API.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text.RegularExpressions;

namespace CgmscHO_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HODController : ControllerBase
    {
        private readonly OraDbContext _context;

        public HODController(OraDbContext context)
        {
            _context = context;
        }


        [HttpGet("WarehouseInfo")]
        public async Task<ActionResult<IEnumerable<WarehouseInfoDTO>>> WarehouseInfo(string distid)
        {
            string qry = "";


            string whdistid = "";
            if (distid != "0")
            {
                whdistid = " and  f.districtid=" + distid;
            }

            qry = @" select w.WAREHOUSEID,w.WAREHOUSENAME,wf.nosfac,w.ADDRESS1||' '||nvl(w.ADDRESS2,'-')||' '||nvl(ADDRESS3,'-') as address,
w.EMAIL,LATITUDE,LONGITUDE,nosvehicle ,nosdist,am.MOB1
from maswarehouses w
inner join masamgr am on am.amid=w.amid
inner join
(
select count(VID) as nosvehicle,vh.warehouseid from  masvehical vh 
where  ISACTIVE='Y'
group by vh.warehouseid 

) vh on vh.warehouseid=w.warehouseid 
left outer join 
(
select WAREHOUSEID,count(districtid) nosdist from masdistricts where stateid=1
group by WAREHOUSEID
) d on d.warehouseid=w.warehouseid 
left outer join 
(
select count(f.facilityid) as nosfac,wh.warehouseid from masfacilities f
inner join masfacilitywh wh on wh.facilityid=f.facilityid
inner join masfacilitytypes ft on ft.facilitytypeid=f.facilitytypeid
where ISACTIVE=1 and ft.hodid in (2,3,7,5) "+ whdistid + @"
and f.facilitytypeid not in (383,387,375,380) 
group by wh.warehouseid 
) wf on wf.WAREHOUSEID=w.WAREHOUSEID ";

            var myList = _context.WarehouseInfoDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("FacCoverage")]
        public async Task<ActionResult<IEnumerable<FacCoverageDTO>>> FacCoverage(string distid)
        {
            FacOperations f = new FacOperations(_context);


            string whyearid = f.getACCYRSETID();
            string qry = "";

            string whdistid = "";
            if (distid != "0")
            {
                whdistid = " and  f.districtid=" + distid;

            }

//            qry = @"  select ft.facilitytypecode, ft.FACILITYTYPEDESC, count(f.facilityid) as nosfac, ft.orderdp, f.facilitytypeid from masfacilities f
//inner join masfacilitytypes ft on ft.facilitytypeid= f.facilitytypeid
//where ISACTIVE = 1 and ft.hodid in (2,3,7,5)
//and f.facilitytypeid not in (383,387,375,380) "+ whdistid + @"
//group by ft.facilitytypecode,ft.orderdp,FACILITYTYPEDESC,f.facilitytypeid
//order by ft.orderdp  ";


            qry = @"    select ft.facilitytypecode, ft.FACILITYTYPEDESC, count(f.facilityid) as nosfac, ft.orderdp, f.facilitytypeid,nvl(nositems, 0) as nositems,nvl(nosindent, 0) nosindent from masfacilities f
inner join masfacilitytypes ft on ft.facilitytypeid = f.facilitytypeid

left outer join
(

select t.facilitytypeid, count(distinct tbo.ITEMID) nositems, count(distinct tb.nocid) as nosindent from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid = tbo.indentitemid
inner join tbindents tb on tb.indentid = tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where 1 = 1 and tb.status = 'C' 
and tb.indentdate between(select startdate from masaccyearsettings where accyrsetid = "+ whyearid + @" )
and(select enddate from masaccyearsettings where accyrsetid = "+ whyearid + @" ) group by t.facilitytypeid
) iss on iss.facilitytypeid = f.facilitytypeid

where ISACTIVE = 1 and ft.hodid in (2, 3, 7, 5)
and f.facilitytypeid not in (383, 387, 375, 380) " + whdistid + @"
group by ft.facilitytypecode,ft.orderdp,FACILITYTYPEDESC,f.facilitytypeid,nositems,nosindent
order by ft.orderdp  ";

            var myList = _context.FacCoverageDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("SeasonDrugs")]
        public async Task<ActionResult<IEnumerable<SeasonIssueDTO>>> SeasonDrugs(string seasonname,Int64 groupid,Int64 itemtypeid,string storeType)
        {
            FacOperations f = new FacOperations(_context);


           string whyearid = f.getACCYRSETID();
            string qry = "";

            string whgrupid = "";
            if (groupid!=0)
            {
                whgrupid= " and m.groupid ="+ groupid;
            }

            string whitemtypeid = "";
            if (itemtypeid != 0)
            {
                whitemtypeid = " and m.itemtypeid ="+ itemtypeid;
            }

            qry = @"  select ID,IssuedInNosAvgSeason,ISSUEDINLACS as SeasonIssuedLacs,Round((nvl(DHSAI,0)*nvl(m.unitcount,1))/100000,2) as DHSAILacs,round((nvl(IssuedQTY,0)*nvl(m.unitcount,1))/100000,2) as ThisYrIssuedLacs,
round(((nvl(READY,0)+nvl(UQC,0))*nvl(m.unitcount,1))/100000,2) stklacs
,IssueType,season,s.itemid ,m.itemcode,m.itemname,m.strength1,t.itemtypename,
nvl(READY,0)*nvl(m.unitcount,1)ready ,nvl(UQC,0)*nvl(m.unitcount,1) as UQC
from 
SEASONTRANSACTION s
inner join masitems m on m.itemid=s.itemid
inner join masitemtypes t on t.itemtypeid=m.itemtypeid
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = "+ whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid

left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+ whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid= " + whyearid + @" ) group by tbo.ITEMID
) iss on iss.itemid = m.itemid
left outer join
(
select itemid, sum(nvl(ReadyForIssue, 0)) as READY, sum(nvl(Pending, 0)) as UQC from
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))
 else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,    
 case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
 from tbreceiptbatches b
 inner  join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner  join masitems mi on mi.itemid = i.itemid
 inner  join masitemcategories c on c.categoryid = mi.categoryid
inner  join masitemmaincategory mc on mc.MCID = c.MCID
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null
     and tbi.notindpdmis is null
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid
     Where T.Status = 'C' and mc.MCID =1  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 

     ) group by itemid
) st on st.itemid = m.itemid
where  ISSUETYPE='"+ storeType + "' and SEASON='"+ seasonname + @"' "+ whitemtypeid + @"
order by ISSUEDINLACS desc ";

            var myList = _context.SeasonIssueDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }










        [HttpGet("EdlNonEdlIssuePercentSummary")]
        public async Task<ActionResult<IEnumerable<EdlNonEdlIssuePercentSummaryDTO>>> EdlNonEdlIssuePercentSummary(string  yearid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }

            qry = @"  SELECT case when m.isedl2021 = 'Y' then 'EDL' else 'Non EDL' end as EDLtype, MC.MCID,MC.MCATEGORY,COUNT(M.ITEMID) AI,count(distinct iss.itemid) as nosissue
,Round((count(distinct iss.itemid)/COUNT(M.ITEMID))*100,2) per
from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = "+ whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid="+ whyearid + @") group by tbo.ITEMID
) iss on iss.itemid = m.itemid
where 1=1 and m.ISFREEZ_ITPR is null and mc.mcid in (1,2,3)
group by MC.MCID,MC.MCATEGORY,m.isedl2021
order by mcid
 ";

            var myList = _context.EdlNonEdlIssuePercentSummaryDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }





        [HttpGet("IssuePerWisePerClick")]
        public async Task<ActionResult<IEnumerable<IssuePerWisePerClickDTO>>> IssuePerWisePerClick(string yearid,string orderdp )
        {
            string qry = "";
           
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }

            qry = @"  
select m.itemid,nvl(ty.itemtypename, 'NA') as itemtypename,nvl(g.groupname, 'NA') as groupname ,m.itemcode as ITEMCODE,m.itemname as ITEMNAME,m.STRENGTH1 as STRENGTH1 
,m.unit
,nvl(DHSAI,0) as AI,nvl(IssuedQTY,0) as issued,round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2) as issuP
,case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=100 then '>=100%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<100 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=80 then '80-100%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<80 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=50 then '50-80%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<50 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=20 then '20-50%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<20 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=1 then '0-20%' 
else '0%'
end end end end end as percentage
,case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=100 then 1
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<100 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=80 then 2 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<80 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=50 then 3 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<50 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=20 then 4
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<20 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=1 then 5
else 6
end end end end end as orderdp
,nvl(READY,0) ReadySTK,nvl(UQC,0) as UQCSTK
,nvl(TOTLPIPELINE,0) as TOTLPIPELINE
from masitems m
left outer join masitemgroups g on g.groupid = m.groupid
left outer join masitemtypes ty on ty.itemtypeid = m.itemtypeid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = "+ whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+ whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid="+ whyearid + @") group by tbo.ITEMID
) iss on iss.itemid = m.itemid



left outer join
(
select itemid, sum(nvl(ReadyForIssue, 0)) as READY, sum(nvl(Pending, 0)) as UQC from
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))
 else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,    
 case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
 from tbreceiptbatches b
 inner  join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner  join masitems mi on mi.itemid = i.itemid
 inner  join masitemcategories c on c.categoryid = mi.categoryid
inner  join masitemmaincategory mc on mc.MCID = c.MCID
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null
     and tbi.notindpdmis is null
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid
     Where T.Status = 'C' and mc.MCID =1  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 

     ) group by itemid
) st on st.itemid = m.itemid

left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty, 0)receiptabsqty,
receiptdelayexception, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY

from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid
 where op.status  in ('C', 'O') 
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) group by itemid
) pip on pip.itemid = m.itemid

where 1=1 and m.ISFREEZ_ITPR is null and mc.MCID =1 and (case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=100 then 1
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<100 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=80 then 2 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<80 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=50 then 3 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<50 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=20 then 4
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<20 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=1 then 5
else 6
end end end end end)=" + orderdp + @"
order by (nvl(IssuedQTY,0)/nvl(DHSAI,0)) desc  ";

            var myList = _context.IssuePerWisePerClickDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("IssuedPerWise")]
        public async Task<ActionResult<IEnumerable<IssuedPerWiseDTO>>> IssuedPerWise(string yearid)
        {
            string qry = "";

            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }

            qry = @" 
select percentage,count(distinct itemid) nosdrugs,orderdp
from 
(
select m.itemid ,nvl(DHSAI,0) as AI,nvl(IssuedQTY,0) as issued,round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2) as issuP
,case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=100 then '>=100%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<100 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=80 then '80-100%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<80 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=50 then '50-80%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<50 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=20 then '20-50%' 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<20 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=1 then '0-20%' 
else '0%'
end end end end end as percentage
,case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=100 then 1
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<100 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=80 then 2 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<80 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=50 then 3 
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<50 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=20 then 4
else case when round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)<20 and round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2)>=1 then 5
else 6
end end end end end as orderdp

from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = "+ whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+ whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid="+ whyearid + @") group by tbo.ITEMID
) iss on iss.itemid = m.itemid

where 1=1  and m.ISFREEZ_ITPR is null and mc.MCID =1
) group by percentage,orderdp
order by orderdp  ";

            var myList = _context.IssuedPerWiseDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }





        [HttpGet("StockSummaryBalanceIndent")]
        public async Task<ActionResult<IEnumerable<StockPBalanceIndentSummaryDTO>>> StockSummaryBalanceIndent(string yearid,string mcid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "1";
            if (mcid != "1")
            {
                whmcid = mcid;
            }

                qry = @" 
select count(itemid) as nous,btype,btypeorder

from 
(
select m.itemid 
,nvl(DHSAI,0) as AI,nvl(IssuedQTY,0) as issued
,case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end as balanceindent,
round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2) as issuP

,case when (nvl(READY,0)+nvl(UQC,0))=0 and nvl(IssuedQTY,0)=0 then 'Stock-out' else 
case when (nvl(READY,0)+nvl(UQC,0))>0 and nvl(IssuedQTY,0)=0 then 'Non Lifted'
else  case when (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)<0 then 'Excess Lifted'
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))=0
then 'Issued and Stock out'
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))>0
then ( case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<100
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=80 then '80-100%'
else case when 
round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<80
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=50 then '50-80%'
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<50
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=20 then '20-50%'
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<20
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)>0 then '0-20%'

else '>100%'
end end end end  ) 
else 'NA' end end end end end as btype


,nvl(READY,0) ReadySTK,nvl(UQC,0) as UQCSTK
, case when (nvl(READY,0)+nvl(UQC,0))=0 and nvl(IssuedQTY,0)=0 then 8 else 
case when (nvl(READY,0)+nvl(UQC,0))>0 and nvl(IssuedQTY,0)=0 then 1
else  case when (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)<0 then 2
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))=0
then 7
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))>0
then ( case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<100
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=80 then 3
else case when 
round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<80
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=50 then 4
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<50
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=20 then 5
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<20
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)>0 then 6

else 2.5
end end end end  ) 
else 9 end end end end end as btypeorder


from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = "+ whyearid + @" and (case when i.isaireturn ='Y' then 1 else 0 end)=0
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid="+ whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid="+ whyearid + @") group by tbo.ITEMID
) iss on iss.itemid = m.itemid



left outer join
(
select itemid, sum(nvl(ReadyForIssue, 0)) as READY, sum(nvl(Pending, 0)) as UQC from
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))
 else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,    
 case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
 from tbreceiptbatches b
 inner  join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner  join masitems mi on mi.itemid = i.itemid
 inner  join masitemcategories c on c.categoryid = mi.categoryid
inner  join masitemmaincategory mc on mc.MCID = c.MCID
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null
     and tbi.notindpdmis is null
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid
     Where T.Status = 'C' and mc.MCID ="+whmcid+ @"  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 

     ) group by itemid
) st on st.itemid = m.itemid



where 1=1 and m.ISFREEZ_ITPR is null and mc.MCID ="+whmcid+@" 


) group by btype,btypeorder
order by btypeorder ";

            var myList = _context.StockPBalanceIndentSummaryDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("StockSummaryBalanceIndentDetails")]
        public async Task<ActionResult<IEnumerable<StockPBalanceIndentDetailsDTO>>> StockSummaryBalanceIndentDetails(string yearid, string mcid,string orderid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "1";
            if (mcid != "1")
            {
                whmcid = mcid;
            }

            qry = @" 

select m.itemid ,m.itemcode,m.itemname,m.strength1,m.unit
,nvl(DHSAI,0) as AI,nvl(IssuedQTY,0) as issued
,case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end as balanceindent,
to_char(round((nvl(IssuedQTY,0)/nvl(DHSAI,0))*100,2))||'%' as issuP

,case when (nvl(READY,0)+nvl(UQC,0))=0 and nvl(IssuedQTY,0)=0 then 'Stock-out' else 
case when (nvl(READY,0)+nvl(UQC,0))>0 and nvl(IssuedQTY,0)=0 then 'Non Lifted'
else  case when (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)<0 then 'Excess Lifted'
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))=0
then 'Issued and Stock out'
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))>0
then ( case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<100
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=80 then '80-100%'
else case when 
round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<80
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=50 then '50-80%'
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<50
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=20 then '20-50%'
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<20
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)>0 then '0-20%'

else '>100%'
end end end end  ) 
else 'NA' end end end end end as btype


,nvl(READY,0) ReadySTK,nvl(UQC,0) as UQCSTK,nvl(TOTLPIPELINE,0) as TOTLPIPELINE
, case when (nvl(READY,0)+nvl(UQC,0))=0 and nvl(IssuedQTY,0)=0 then 8 else 
case when (nvl(READY,0)+nvl(UQC,0))>0 and nvl(IssuedQTY,0)=0 then 1
else  case when (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)<0 then 2
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))=0
then 7
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))>0
then ( case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<100
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=80 then 3
else case when 
round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<80
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=50 then 4
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<50
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=20 then 5
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<20
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)>0 then 6

else 2.5
end end end end  ) 
else 9 end end end end end as btypeorder

,case when rc.itemid is not null then 'RC Valid till'||to_char(rc.RCEndDT) else 'RC Not Valid' end as rcstatus

, case when (nvl(READY,0)+nvl(UQC,0))>0 and (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)>0 
then to_char(round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0))||'%'
else 'NA' end stockper

from masitems m
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
 inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = " + whyearid + @" and (case when i.isaireturn ='Y' then 1 else 0 end)=0
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid
left outer join 
(

select tbo.ITEMID, sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))  IssuedQTY from tboutwards tbo
inner join tbindentitems tbi on tbi.indentitemid=tbo.indentitemid 
inner join tbindents tb on tb.indentid =tbi.indentid
inner join masfacilities f on f.facilityid = tb.facilityid
inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
where  1=1 and tb.status='C' and t.hodid  =2  and tb.indentdate between (select startdate from masaccyearsettings where accyrsetid=" + whyearid + @" ) 
and (select enddate from masaccyearsettings where accyrsetid=" + whyearid + @") group by tbo.ITEMID
) iss on iss.itemid = m.itemid



left outer join
(
select itemid, sum(nvl(ReadyForIssue, 0)) as READY, sum(nvl(Pending, 0)) as UQC from
(
 select b.batchno, b.expdate, b.inwno, mi.itemid,
 (case when b.qastatus = '1' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))
 else (case when mi.Qctest = 'N' then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0))  end ) end ) ReadyForIssue,    
 case when mi.qctest = 'N' then 0 else (case when b.qastatus = 0 or b.qastatus = 3 then(nvl(b.absrqty, 0) - nvl(iq.issueqty, 0)) end) end Pending
 from tbreceiptbatches b
 inner  join tbreceiptitems i on b.receiptitemid = i.receiptitemid
 inner  join tbreceipts t on t.receiptid = i.receiptid
 inner  join masitems mi on mi.itemid = i.itemid
 inner  join masitemcategories c on c.categoryid = mi.categoryid
inner  join masitemmaincategory mc on mc.MCID = c.MCID
 left outer join
     (
     select tbi.itemid, tbo.inwno, sum(nvl(tbo.issueqty,0)) issueqty
     from tboutwards tbo, tbindentitems tbi , tbindents tb
     where tbo.indentitemid = tbi.indentitemid and tbi.indentid = tb.indentid and tb.status = 'C'
     and tb.notindpdmis is null and tbo.notindpdmis is null
     and tbi.notindpdmis is null
     group by tbi.itemid, tb.warehouseid, tbo.inwno
     ) iq on b.inwno = Iq.inwno and iq.itemid = i.itemid
     Where T.Status = 'C' and mc.MCID =" + whmcid + @"  And (b.Whissueblock = 0 or b.Whissueblock is null) and b.expdate > sysdate 

     ) group by itemid
) st on st.itemid = m.itemid

left outer join 
(
select   itemid,max(RCEndDT) as RCEndDT
from (
select to_char(a.ContractStartDate,'dd-MM-yyyy') rcstart,a.ContractEndDate,
case when (ci.isextended='Y') then to_char(ci.rcextendedupto,'dd-MM-yyyy') else to_char(ContractEndDate,'dd-MM-yyyy') end RCEndDT,
 case when ci.basicratenew is null then ci.basicrate else basicratenew end basicrate,
 ci.finalrategst
 ,case when   ci.percentvaluegst is null then   ci.percentvalue  else ci.percentvaluegst end Tax   
, ci.itemid,ms.schemeid,s.supplierid  
, case when sysdate between s.blacklistfdate and  s.blacklisttdate then 'Yes' else 'No' end as blacklisted,
case when (ci.isextended='Y') then round(rcextendedupto-sysdate,0) else  round(ContractEndDate-sysdate,0)  end as DayRemaining
from aoccontractitems ci
inner join aoccontracts a on a.contractid=ci.contractid
inner join massuppliers s on s.supplierid=a.supplierid
inner join masschemes ms on ms.schemeid=a.schemeid
inner join masitems m on m.itemid=ci.itemid
where  ForceCloseRC is null  and a.status = 'C' 
and  (sysdate between a.contractstartdate and a.contractenddate or 
sysdate between a.contractstartdate and  (case when ci.isextended='Y' then ci.rcextendedupto else a.contractenddate end))
and round(Months_Between(to_date(a.ContractEndDate),to_date(a.ContractStartDate)),0)<=nvl(( case when m.categoryid=62 then  nvl(ms.ISREAGDURATION,36) else 36 end) ,36)
) where blacklisted='No' 
group  by itemid
) rc on rc.itemid=m.itemid


left outer join
(
  select itemid, sum(pipelineQTY) TOTLPIPELINE from
(
select m.itemcode, OI.itemid, op.ponoid, m.nablreq, op.soissuedate, op.extendeddate, sum(soi.ABSQTY) as absqty, nvl(rec.receiptabsqty, 0)receiptabsqty,
receiptdelayexception, round(sysdate - op.soissuedate, 0) as days,
case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end as pipelineQTY

from soOrderPlaced OP
inner join SoOrderedItems OI on OI.PoNoID = OP.PoNoID
inner join soorderdistribution soi on soi.orderitemid = OI.orderitemid
inner join masitems m on m.itemid = oi.itemid
inner join aoccontractitems oci on oci.contractitemid = oi.contractitemid
left outer join
(
select tr.ponoid, tri.itemid, sum(tri.receiptabsqty) receiptabsqty from tbreceipts tr
inner join tbreceiptitems tri on tri.receiptid = tr.receiptid
where tr.receipttype = 'NO' and tr.status = 'C' and tr.notindpdmis is null and tri.notindpdmis is null
group by tr.ponoid, tri.itemid
) rec on rec.ponoid = OP.PoNoID and rec.itemid = OI.itemid
 where op.status  in ('C', 'O') and op.deptid=367
 group by m.itemcode, m.nablreq, op.ponoid, op.soissuedate, op.extendeddate, OI.itemid , rec.receiptabsqty,
 op.soissuedate, op.extendeddate , receiptdelayexception, oci.finalrategst
 having(case when m.nablreq = 'Y' and round(sysdate-op.soissuedate,0) <= 150 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is null and round(sysdate - op.soissuedate, 0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.receiptdelayexception = 1 and sysdate <= op.extendeddate + 1 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0)
else case when op.extendeddate is not null and op.receiptdelayexception = 1 and(op.extendeddate + 1) <= op.soissuedate
and round(sysdate-op.soissuedate,0) <= 90 then sum(soi.ABSQTY)-nvl(rec.receiptabsqty, 0) else 0 end end end end) > 0
) group by itemid
) pip on pip.itemid = m.itemid

where 1=1 and m.ISFREEZ_ITPR is null and mc.MCID =" + whmcid + @"

and (case when (nvl(READY,0)+nvl(UQC,0))=0 and nvl(IssuedQTY,0)=0 then 8 else 
case when (nvl(READY,0)+nvl(UQC,0))>0 and nvl(IssuedQTY,0)=0 then 1
else  case when (case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end)<0 then 2
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))=0
then 7
else  case when nvl(IssuedQTY,0)>0 and (nvl(DHSAI,0)-nvl(IssuedQTY,0) )>0  and (nvl(READY,0)+nvl(UQC,0))>0
then ( case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<100
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=80 then 3
else case when 
round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<80
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=50 then 4
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)<50
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,0)>=20 then 5
else case when round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)<20
and  round(((nvl(READY,0)+nvl(UQC,0))/(case when nvl(IssuedQTY,0)>0 then nvl(DHSAI,0)-nvl(IssuedQTY,0) else nvl(DHSAI,0) end))*100,4)>0 then 6

else 2.5
end end end end  ) 
else 9 end end end end end)="+orderid+@"

order by m.itemname";



            var myList = _context.StockPBalanceIndentDetailsDbSet
    .FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


        [HttpGet("DirectorateAIDetails")]
        public async Task<ActionResult<IEnumerable<DirectorateIndentPOStatusDTO>>> DirectorateAIDetails(string yearid, string mcid, string hodid, string groupid, string itemtypeid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.MCID =" + mcid;
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and op.deptid =" + hodid;
            }
            string whgroupid = "";
            if (groupid != "0")
            {
                whgroupid = " and m.groupid =" + groupid;
            }
            string whitemtypeid = "";
            if (itemtypeid != "0")
            {
                whitemtypeid = " and m.itemtypeid =" + itemtypeid;
            }


            qry = @" select g.groupid, g.groupname,
m.itemid,m.itemcode,nvl(DHSAI,0) as DHSAI,nvl(POQTY,0) as POQTY,nvl(povalue,0) as povalue ,nvl(RQTY,0) as RQTY,nvl(rvalue,0) as rvalue,case when nvl(RQTY,0)>0 then to_char(round((nvl(RQTY,0)/nvl(POQTY,0))*100,2))||'%' else '0' end as rPercentage,
t.itemtypename,m.itemname,m.strength1,m.unit,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as edltype,e.edl from  masitems m 
inner join masitemtypes t on t.itemtypeid=m.itemtypeid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join masitemgroups g on g.groupid=m.groupid
left outer join masedl e on e.edlcat=m.edlcat
inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = " + whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid

left outer join 
(


select sum(orderedqty) as POQTY,sum(dhsrqty) as RQTY,a.itemid,round(sum(povalue)/10000000,2) as povalue, round(sum(rvalue)/10000000,2) as rvalue from 
(
select oi.absqty as orderedqty,nvl(dhsrqty,0) as dhsrqty ,oi.itemid,op.ponoid,oi.absqty*c.finalrategst as povalue,nvl(dhsrqty,0)*c.finalrategst rvalue from soorderplaced op
inner join soOrderedItems OI on(OI.ponoid = op.ponoid)
    inner join aoccontractitems c on c.contractitemid = oi.contractitemid
inner join masitems mi on mi.itemid=oi.itemid
inner join masitemcategories ic on ic.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join  
(
                                
select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
from tbreceipts t 
inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
where T.Status = 'C' and  T.receipttype = 'NO' 
and t.ponoid in (select ponoid from soorderplaced op where 1=1 "+ whhodid + @")
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
where op.AIFINYEAR= "+ whyearid + @"  "+ whhodid + @"   and  op.status not in ( 'OC','WA1','I' ) 
) a group by a.itemid
) po on po.itemid=m.itemid
where 1=1 and m.ISFREEZ_ITPR is null "+ whmcid + @"  "+ whgroupid + @"  "+ whitemtypeid + @" order by g.groupname  ";

            var myList = _context.DirectorateIndentPOStatusDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        public Int64? groupid { get; set; }

      

        [HttpGet("GroupWiseAI_PODetails")]
        public async Task<ActionResult<IEnumerable<DirectorateGroupAI_PODTO>>> GroupWiseAI_PODetails(string yearid, string mcid, string hodid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.MCID =" + mcid;
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and op.deptid =" + hodid;
            }
         
            qry = @" 
select groupid,groupname,count(itemid) as nosIndent,sum(POgiven) as POgiven,sum(povalue) as povalue,sum(Received) as itemsreceived,sum(rvalue) as rvalue
from 
(
select g.groupid, g.groupname,
m.itemid,m.itemcode,nvl(DHSAI,0) as DHSAI,nvl(POQTY,0) as POQTY,nvl(povalue,0) as povalue ,nvl(RQTY,0) as RQTY,nvl(rvalue,0) as rvalue,case when nvl(RQTY,0)>0 then to_char(round((nvl(RQTY,0)/nvl(POQTY,0))*100,2))||'%' else '0' end as rPercentage,
t.itemtypename,m.itemname,m.strength1,m.unit,case when m.isedl2021='Y' then 'EDL' else 'Non EDL' end as edltype,e.edl 

,case when nvl(POQTY,0) >0 then 1 else 0 end as POgiven,case when nvl(POQTY,0) >0 and nvl(RQTY,0)>0 then 1 else 0 end as Received
from  masitems m 
inner join masitemtypes t on t.itemtypeid=m.itemtypeid
inner join masitemcategories ic on ic.categoryid = m.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join masitemgroups g on g.groupid=m.groupid
left outer join masedl e on e.edlcat=m.edlcat
inner join 
(
select itemid,sum(nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0)) as DHSAI,case when i.isaireturn ='Y' then 1 else 0 end as AIReturn from itemindent i  where 1 = 1 
and (nvl(i.DHS_INDENTQTY, 0) + nvl(i.mitanin, 0))>0 and
accyrsetid = " + whyearid + @"
group by  itemid,i.isaireturn
) ai on ai.itemid = m.itemid

left outer join 
(


select sum(orderedqty) as POQTY,sum(dhsrqty) as RQTY,a.itemid,round(sum(povalue)/10000000,2) as povalue, round(sum(rvalue)/10000000,2) as rvalue from 
(
select oi.absqty as orderedqty,nvl(dhsrqty,0) as dhsrqty ,oi.itemid,op.ponoid,oi.absqty*c.finalrategst as povalue,nvl(dhsrqty,0)*c.finalrategst rvalue from soorderplaced op
inner join soOrderedItems OI on(OI.ponoid = op.ponoid)
    inner join aoccontractitems c on c.contractitemid = oi.contractitemid
inner join masitems mi on mi.itemid=oi.itemid
inner join masitemcategories ic on ic.categoryid = mi.categoryid
inner join masitemmaincategory mc on mc.MCID = ic.MCID
left outer join  
(
                                
select i.itemid, t.ponoid, nvl(sum(tb.absrqty),0) as dhsrqty                  
from tbreceipts t 
inner join tbreceiptitems i on (i.receiptid = t.receiptid) 
 inner join tbreceiptbatches tb on (i.receiptitemid =tb.receiptitemid)
where T.Status = 'C' and  T.receipttype = 'NO' 
and t.ponoid in (select ponoid from soorderplaced op where 1=1 " + whhodid + @")
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
where op.AIFINYEAR= " + whyearid + @"  " + whhodid + @"   and  op.status not in ( 'OC','WA1','I' ) 
) a group by a.itemid
) po on po.itemid=m.itemid
where 1=1 and m.ISFREEZ_ITPR is null    " + whmcid + @"

) group by groupid,groupname
order by groupname  ";

            var myList = _context.DirectorateGroupAI_PODbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }






        [HttpGet("Diswise_Issuance")]
        public async Task<ActionResult<IEnumerable<DisWiseIssueDTO>>> CFYDiswiseIssuance(string yearid, string mcid, string hodid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.MCID =" + mcid;
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid  =" + hodid;
            }

            qry = @" select count(distinct itemid) as TOTALISSUEITEMS,Round(sum(TotalVal)/10000000,2) as TOTALISSUEVALUE,Round(sum(DHSValue)/10000000,2) DHSISSUEVALUE,
  sum(DHSItem) as DHSISSUEITEMS,Round(sum(DMEVal)/10000000,2) as DMEISSUEVALUE, sum(DMEItem) as DMEISSUEITEMS
 ,Round(sum(AYUSHVal)/10000000,2) AYIssueval,  sum(AYUSHItem) as AYIssueitems
 ,districtname,districtid
  from 
  (
  select mcid,mcategory,itemid,hodid,
  case when hodid=2 and sum(DHSQTY) >0 then 1 else 0 end DHSItem,  case when  hodid=3 and sum(DMEQTY)>0 then 1 else 0 end DMEItem
  ,case when  hodid=7 and sum(AYUSHQTY)>0 then 1 else 0 end AYUSHItem
  ,sum(TotalVal) as TotalVal,sum(DHSValue) as DHSValue,sum(DMEVal) as DMEVal,sum(AYUSHVal) as AYUSHVal
  ,districtname,districtid
  from 
  (
  select   t.hodid,mc.mcid,mc.mcategory, m.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty,
  case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DHSQTY,  
    case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DMEQTY,  
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end AYUSHQTY,     
     case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end SKURate
      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
        ,case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DHSValue,

case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DMEVal,
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end AYUSHVal
      ,d.districtid,d.districtname
  from tbindents tb
         inner join tbindentitems tbi on tbi.indentid=tb.indentid 
         inner join tboutwards tbo on tbo.indentitemid=tbi.indentitemid
         inner join tbreceiptbatches rb on rb.inwno=tbo.inwno
          inner join tbreceiptitems ri on ri.receiptitemid=rb.receiptitemid
 inner join tbreceipts r on r.receiptid=ri.receiptid
         inner join masitems m on m.itemid = tbi.itemid
         inner join masfacilities f on f.facilityid = tb.facilityid
         inner join masdistricts d on d.districtid=f.districtid
         inner join masfacilitytypes t on t.facilitytypeid = f.facilitytypeid
          inner join masitemcategories c on c.categoryid=m.categoryid
       inner join masitemmaincategory mc on mc.MCID=c.MCID
           inner join soordereditems si on  si.ponoid=rb.ponoid and si.itemid=rb.itemid   
    INNER JOIN aoccontractitems aci on aci.contractitemid=si.contractitemid
       where 1=1  and tb.status = 'C'  "+ whmcid + @"
       and 
       tb.issuetype='NO' "+ whhodid + @"
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID="+ whyearid + @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID="+ whyearid + @")  
        group by  mc.mcid,t.hodid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,d.districtid,d.districtname
        ) group by mcid,mcategory,itemid,hodid,districtname,districtid
)
        group by districtname,districtid
order by districtname ";

            var myList = _context.DisWiseIssueDTODbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }

        [HttpGet("Monthwise_Issuance")]
        public async Task<ActionResult<IEnumerable<MonthwiseIssueDTO>>> Monthwise_Issuance(string yearid, string mcid, string hodid)
        {
            string qry = "";
            string whyearid = "";
            if (yearid == "0")
            {
                FacOperations f = new FacOperations(_context);


                whyearid = f.getACCYRSETID();
            }
            else
            {
                whyearid = yearid;
            }
            string whmcid = "";
            if (mcid != "0")
            {
                whmcid = " and mc.MCID =" + mcid;
            }

            string whhodid = "";
            if (hodid != "0")
            {
                whhodid = " and t.hodid  =" + hodid;
            }

            qry = @" select count(distinct itemid) as TOTALISSUEITEMS,Round(sum(TotalVal)/10000000,2) as TOTALISSUEVALUE,Round(sum(DHSValue)/10000000,2) DHSISSUEVALUE,
  sum(DHSItem) as DHSISSUEITEMS,Round(sum(DMEVal)/10000000,2) as DMEISSUEVALUE, sum(DMEItem) as DMEISSUEITEMS
 ,Round(sum(AYUSHVal)/10000000,2) AYIssueval,  sum(AYUSHItem) as AYIssueitems
 ,a.mon,fm.MonthName,fm.id
  from 
  (
  select mcid,mcategory,itemid,hodid,
  case when hodid=2 and sum(DHSQTY) >0 then 1 else 0 end DHSItem,  case when  hodid=3 and sum(DMEQTY)>0 then 1 else 0 end DMEItem
  ,case when  hodid=7 and sum(AYUSHQTY)>0 then 1 else 0 end AYUSHItem
  ,sum(TotalVal) as TotalVal,sum(DHSValue) as DHSValue,sum(DMEVal) as DMEVal,sum(AYUSHVal) as AYUSHVal
,mon
  from 
  (
  select   t.hodid,mc.mcid,mc.mcategory, m.itemid,sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) iss_qty,
  case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DHSQTY,  
    case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end DMEQTY,  
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0)) else 0 end AYUSHQTY,     
     case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end SKURate
      ,round(sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))* (case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end),0) TotalVal
        ,case when  t.hodid=2 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DHSValue,

case when  t.hodid=3 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end DMEVal,
      case when  t.hodid=7 then sum(tbo.issueqty + nvl(tbo.reconcile_qty,0))*(case when aci.finalrategst is null then si.singleunitprice else aci.finalrategst end) else 0 end AYUSHVal


      ,extract(month from indentdate) mon
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
       where 1=1  and tb.status = 'C' "+ whmcid + @"
       and 
       tb.issuetype='NO'  "+ whhodid + @" 
           and tb.indentdate between (select STARTDATE from masaccyearsettings  where ACCYRSETID="+ whyearid + @") and  (select ENDDATE from masaccyearsettings  where ACCYRSETID="+ whyearid + @")  
        group by  mc.mcid,t.hodid,m.itemid,aci.finalrategst,si.singleunitprice,mc.mcategory,indentdate
        ) group by mcid,mcategory,itemid,hodid,mon
)a
inner join MASFYMONTH fm on fm.MonthId=a.mon
        group by a.mon,fm.MonthName,fm.id
order by fm.ID ";

            var myList = _context.MonthwiseIssueDbSet
.FromSqlInterpolated(FormattableStringFactory.Create(qry)).ToList();
            return myList;

        }


    }
}
